using HarmonyLib;
using LOR_BattleUnit_UI;
using LOR_DiceSystem;
using LOR_XML;
using Mod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Maru_Mod
{
    internal class PatchClass // パッチをまとめて置くためのフォルダ
    {
        // 下のパッチは専用ページパッチの1例。ざっくり言うと「このModのコアぺに専用ページが設定されているなら、このModの同番号のページも使えるようにする」パッチ
        [HarmonyPatch(typeof(BookModel), "SetXmlInfo")]
        public class BookModel_SetXmlInfo
        {
            public static void Postfix(BookModel __instance, BookXmlInfo ____classInfo, ref List<DiceCardXmlInfo> ____onlyCards)
            {
                bool flag = __instance.BookId.packageId == ModUtil.packageId;
                if (flag) // このModのコアぺなら
                {
                    foreach (int id in ____classInfo.EquipEffect.OnlyCard)// 全ての専用ページに対して
                    {
                        ____onlyCards.Add(ItemXmlDataList.instance.GetCardItem(new LorId(ModUtil.packageId, id), false));
                        // このModのその番号のページを専用ページとして追加する。
                    }
                }
            }
        }

        // =========================================================
        // ラウンド終了時、幻想体ページ選択の処理を上書きする
        // =========================================================
        [HarmonyPatch(typeof(StageController), "RoundEndPhase_ChoiceEmotionCard")]
        public class StageController_RoundEndPhase_ChoiceEmotionCard
        {
            // Token: 0x0600005E RID: 94 RVA: 0x000047F0 File Offset: 0x000029F0
            public static void Postfix(ref bool __result)
            {
                bool flag = __result && BattleObjectManager.instance.GetAliveListWithAvailable(Faction.Player).Count > 0 && BattleObjectManager.instance.GetAliveListWithAvailable(Faction.Enemy).Count > 0;
                bool flag2 = flag;
                if (flag2)
                {
                    BattleUnitModel emoCard = EmotionSelectControl.GetEmoCard();
                    bool flag3 = emoCard != null && EmotionSelectControl.NextSelectEmotionLevel <= EmotionSelectControl.MaxEmotionLevel && EmotionSelectControl.NextSelectEmotionLevel <= emoCard.emotionDetail.EmotionLevel;
                    bool flag4 = flag3;
                    if (flag4)
                    {
                        bool flag5 = emoCard.passiveDetail.HasPassive<PassiveAbility_MaruExclusiveEmotion>();
                        bool flag6 = flag5;
                        if (flag6)
                        {
                            Debug.Log("Thank You マッシローsan!!");
                            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
                            PatchClass.StageController_RoundEndPhase_ChoiceEmotionCard.StartExEmotionCardSelection(emoCard);
                            __result = false;
                        }
                    }
                }
            }

            private static void StartExEmotionCardSelection(BattleUnitModel binah)
            {
                int nextSelectEmotionLevel = EmotionSelectControl.NextSelectEmotionLevel;
                int num = binah.emotionDetail.EmotionLevel - nextSelectEmotionLevel + 1;
                bool flag = num > 0;
                bool flag2 = flag;
                if (flag2)
                {
                    List<EmotionCardXmlInfo> list =　EmotionSelectControl.ExEmotionCardsList.FindAll(x => x.Level == EmotionSelectControl.NextSelectEmotionLevel);
                    bool flag3 = list.Count == 0;
                    bool flag4 = flag3;
                    if (flag4)
                    {
                        Debug.LogError("ERROR!! : Can'tFindExEmotionCardsData");
                    }
                    else
                    {
                        foreach (EmotionCardXmlInfo item in EmotionSelectControl.SelectedCardsList)
                        {
                            list.Remove(item);
                        }
                        List<EmotionCardXmlInfo> list2 = new List<EmotionCardXmlInfo>(list);
                        bool flag5 = list2.Count > 0;
                        bool flag6 = flag5;
                        if (flag6)
                        {
                            EmotionSelectControl.ChangeForExtraSelection = true;
                            int num2 = EmotionSelectControl.SelectedCardsList.Count;
                            bool flag7 = num2 == 0;
                            bool flag8 = flag7;
                            if (flag8)
                            {
                                num2 = 1;
                            }
                            else
                            {
                                bool flag9 = num2 == 1;
                                bool flag10 = flag9;
                                if (flag10)
                                {
                                    num2 = 3;
                                }
                            }
                            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.Init(num2, list2);
                        }
                        else
                        {
                            EmotionSelectControl.ChangeForExtraSelection = false;

                            SingletonBehavior<BattleManagerUI>
                                .Instance
                                .ui_levelup
                                .SetRootCanvas(false);
                        }
                    }
                }
            }
        }

        // =========================================================
        // 幻想体ページが選ばれたとき
        // =========================================================
        [HarmonyPatch(typeof(StageLibraryFloorModel), "OnPickPassiveCard")]
        public class StageLibraryFloorModel_OnPickPassiveCard
        {
            // Token: 0x06000063 RID: 99 RVA: 0x000049FC File Offset: 0x00002BFC
            public static bool Prefix(EmotionCardXmlInfo card, BattleUnitModel target)
            {
                bool flag = !EmotionSelectControl.ChangeForExtraSelection;
                bool flag2 = flag;
                bool result;
                if (flag2)
                {
                    result = true;
                }
                else
                {
                    EmotionSelectControl.ChangeForExtraSelection = false;

                    bool flag3 = card == null;
                    bool flag4 = flag3;
                    if (flag4)
                    {
                        result = false;
                    }
                    else
                    {
                        EmotionSelectControl.SetSelectedCard(card);

                        bool flag5 =
                            card.TargetType == EmotionTargetType.All ||
                            card.TargetType == EmotionTargetType.AllIncludingEnemy;

                        bool flag6 = flag5;

                        if (flag6)
                        {
                            foreach (BattleUnitModel battleUnitModel in BattleObjectManager.instance.GetAliveList(Faction.Player))
                            {
                                battleUnitModel.emotionDetail.ApplyEmotionCard(card, true);
                            }

                            if (card.TargetType == EmotionTargetType.AllIncludingEnemy)
                            {
                                foreach (BattleUnitModel battleUnitModel in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
                                {
                                    battleUnitModel.emotionDetail.ApplyEmotionCard(card, true);
                                }
                            }
                        }
                        else
                        {
                            bool flag7 = target != null;
                            bool flag8 = flag7;

                            if (flag8)
                            {
                                target.emotionDetail.ApplyEmotionCard(card, true);
                            }
                            else
                            {
                                Debug.LogError("target is null");
                            }
                        }

                        EmotionSelectControl.SetNextSelectionLevel();

                        result = false;
                    }
                }
                return result;
            }
        }

        // =========================================================
        // MaruEmotionSelectControlの初期化
        // =========================================================
        [HarmonyPatch(typeof(StageLibraryFloorModel), "Init")]
        public class StageLibraryFloorModel_Init
        {
            // Token: 0x06000061 RID: 97 RVA: 0x000049E9 File Offset: 0x00002BE9
            public static void Postfix()
            {
                EmotionSelectControl.Initialize();
            }
        }

        // =========================================================
        // 各ページが選ばれたときの効果を選ぶ
        // =========================================================
        // Token: 0x02000025 RID: 37
        [HarmonyPatch(typeof(BattleEmotionCardModel), MethodType.Constructor, new Type[]
        {
    typeof(EmotionCardXmlInfo),
    typeof(BattleUnitModel)
        })]
        public class BattleEmotionCardModel_New
        {
            // Token: 0x06000069 RID: 105 RVA: 0x00004B7C File Offset: 0x00002D7C
            public static bool Prefix(BattleEmotionCardModel __instance, EmotionCardXmlInfo xmlInfo, BattleUnitModel owner, ref EmotionCardXmlInfo ____xmlInfo, ref BattleUnitModel ____owner, ref List<EmotionCardAbilityBase> ____abilityList)
            {
                string name = xmlInfo.Name;
                Type typeFromHandle;
                // =====================================================
                // Lv1
                // =====================================================
                if (name == "Wisdom_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_WisdomPage);
                }
                else if (name == "Overprotective_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_OverprotectivePage);
                }
                else if (name == "TheGuardian_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_TheGuardianPage);
                }
                else if (name == "WaveringResolve_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_WaveringResolvePage);
                }
                else if (name == "SenseOfDuty_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_SenseOfDutyPage);
                }
                else if (name == "Diamond_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_DiamondPage);
                }

                // =====================================================
                // Lv2 覚醒
                // =====================================================
                else if (name == "ASmallMiracle_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_ASmallMiraclePage);
                }
                else if (name == "TheShapeOfAWish_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_TheShapeOfAWishPage);
                }
                else if (name == "ABelievingHeart_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_ABelievingHeartPage);
                }

                // =====================================================
                // Lv2 崩壊
                // =====================================================
                else if (name == "FalseWisdom_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_FalseWisdomPage);
                }
                else if (name == "ATwistedWish_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_ATwistedWishPage);
                }
                else if (name == "Chaos_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_ChaosPage);
                }

                // =====================================================
                // Lv3
                // =====================================================
                else if (name == "Resolve_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_ResolvePage);
                }
                else if (name == "Adventure_FormerlyCycle_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_Adventure_FormerlyCyclePage);
                }
                else if (name == "Leadership_Page")
                {
                    typeFromHandle = typeof(EmotionCardAbility_LeadershipPage);
                }

                else
                {
                    return true;
                }

                ____xmlInfo = xmlInfo;
                ____owner = owner;

                try
                {
                    ____abilityList = new List<EmotionCardAbilityBase>();

                    EmotionCardAbilityBase emotionCardAbilityBase =
                        Activator.CreateInstance(typeFromHandle) as EmotionCardAbilityBase;

                    emotionCardAbilityBase.SetEmotionCard(__instance);

                    ____abilityList.Add(emotionCardAbilityBase);
                }
                catch (Exception message)
                {
                    Debug.LogError(message);
                }

                return false;
            }
        }

        // =========================================================
        // 各ページにアートワークを適用する
        // =========================================================
        [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
        public class EmotionPassiveCardUI_SetSprites
        {
            public static void Postfix(
                EmotionCardXmlInfo ____card,
                ref Image ____artwork)
            {
                switch (____card.Name)
                {
                    // =====================================================
                    // Lv1
                    // =====================================================

                    case "Wisdom_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Wisdom"];
                        break;

                    case "Overprotective_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Overprotective"];
                        break;

                    case "TheGuardian_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_TheGuardian"];
                        break;

                    case "WaveringResolve_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_WaveringResolve"];
                        break;

                    case "SenseOfDuty_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_SenseOfDuty"];
                        break;

                    case "Diamond_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Diamond"];
                        break;

                    // =====================================================
                    // Lv2 覚醒
                    // =====================================================

                    case "ASmallMiracle_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_ASmallMiracle"];
                        break;

                    case "TheShapeOfAWish_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_TheShapeOfAWish"];
                        break;

                    case "ABelievingHeart_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_ABelievingHeart"];
                        break;

                    // =====================================================
                    // Lv2 崩壊
                    // =====================================================

                    case "FalseWisdom_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_FalseWisdom"];
                        break;

                    case "ATwistedWish_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_ATwistedWish"];
                        break;

                    case "Chaos_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Chaos"];
                        break;

                    // =====================================================
                    // Lv3
                    // =====================================================

                    case "Resolve_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Resolve"];
                        break;

                    case "Adventure_FormerlyCycle_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Adventure_FormerlyCycle"];
                        break;

                    case "Leadership_Page":
                        ____artwork.sprite = ModUtil.ArtWorks["Emotion_Leadership"];
                        break;
                }
            }
        }

        /// <summary>
        /// 幻想体ページ選択時の階層アイコン変更
        /// </summary>
        [HarmonyPatch(typeof(LevelUpUI), "Init")]
        public class LevelUpUI_Init
        {
            // Token: 0x06000065 RID: 101 RVA: 0x00004A80 File Offset: 0x00002C80
            public static void Postfix(LevelUpUI __instance, Image ___FloorIconImage, TextMeshProUGUI ___txt_SelectDesc, TextMeshProUGUI ___txt_BtnSelectDesc)
            {
                bool changeForExtraSelection = EmotionSelectControl.ChangeForExtraSelection;
                bool flag = changeForExtraSelection;
                if (flag)
                {
                    ___FloorIconImage.sprite = ModUtil.ArtWorks["MaruAbnorma"];
                    string text = "都城治虫の冒険と覚悟";
                    ___txt_SelectDesc.text = text;
                    ___txt_BtnSelectDesc.text = text;
                    BattleUnitModel emoCard = EmotionSelectControl.GetEmoCard();
                    bool flag2 = emoCard != null;
                    bool flag3 = flag2;
                    if (flag3)
                    {
                        __instance.SetEmotionPerDataUI((float)emoCard.emotionDetail.totalPositiveCoins.Count, (float)emoCard.emotionDetail.totalNegativeCoins.Count);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LevelUpUI), "HideSelectAbRoutine")]
        public class LevelUpUI_HideSelectAbRoutine
        {
            // Token: 0x06000067 RID: 103 RVA: 0x00004B1C File Offset: 0x00002D1C
            public static void Postfix(LevelUpUI __instance, Image ___NeedSelectAb_FloorIconImage, ref IEnumerator __result)
            {
                bool changeForExtraSelection = EmotionSelectControl.ChangeForExtraSelection;
                bool flag = changeForExtraSelection;
                if (flag)
                {
                    Action<object> postItemAction = delegate (object item)
                    {
                        Sprite sprite = ModUtil.ArtWorks["MaruAbnorma"];
                        bool flag2 = ___NeedSelectAb_FloorIconImage.sprite != sprite;
                        bool flag3 = flag2;
                        bool flag4 = flag3;
                        if (flag4)
                        {
                            ___NeedSelectAb_FloorIconImage.sprite = sprite;
                        }
                    };
                    PatchClass.LevelUpUI_HideSelectAbRoutine.FixEnumerator fixEnumerator = new PatchClass.LevelUpUI_HideSelectAbRoutine.FixEnumerator
                    {
                        enumerator = __result,
                        postItemAction = postItemAction
                    };
                    __result = fixEnumerator.GetEnumerator();
                }
            }

            // Token: 0x02000026 RID: 38
            private class FixEnumerator : IEnumerable
            {
                // Token: 0x0600006B RID: 107 RVA: 0x00004C70 File Offset: 0x00002E70
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                // Token: 0x0600006C RID: 108 RVA: 0x00004C88 File Offset: 0x00002E88
                public IEnumerator GetEnumerator()
                {
                    while (this.enumerator.MoveNext())
                    {
                        object item = this.enumerator.Current;
                        yield return item;
                        this.postItemAction(item);
                        item = null;
                        item = null;
                        item = null;
                    }
                    yield break;
                }

                // Token: 0x04000029 RID: 41
                public IEnumerator enumerator;

                // Token: 0x0400002A RID: 42
                public Action<object> postItemAction;
            }
        }

        /// <summary>
        /// 専用の本をゲーム開始時に獲得する
        /// </summary>
        // Token: 0x0200002A RID: 42
        [HarmonyPatch(typeof(GameSceneManager), "ActivateUIController")]
        public class GameSceneManager_ActivateUIController
        {
            // Token: 0x060000A2 RID: 162 RVA: 0x0000576C File Offset: 0x0000396C
            public static void Postfix()
            {
                string language = GlobalGameManager.Instance.CurrentOption.language;
                bool flag = Singleton<BookInventoryModel>.Instance.GetBookCount(new LorId(ModUtil.packageId, 20090711)) < 1;
                if (flag)
                {
                    Singleton<BookInventoryModel>.Instance.CreateBook(new LorId(ModUtil.packageId, 20090711));
                    bool flag2 = language == "jp";
                    if (flag2)
                    {
                        UIAlarmPopup.instance.SetAlarmText("都城治虫の冒険と覚悟を入手しました。");
                    }
                }
            }
        }

        // =========================================================
        // 光回復時HP回復
        // =========================================================
        [HarmonyPatch(typeof(BattlePlayingCardSlotDetail), "RecoverPlayPoint")]
        public class BattleUnitBuf_ASmallMiracle_Patch
        {
            public static void Postfix(
                BattlePlayingCardSlotDetail __instance,
                int value)
            {
                if (value <= 0)
                    return;

                BattleUnitModel owner =
                    AccessTools.Field(
                        typeof(BattlePlayingCardSlotDetail),
                        "_self")
                    .GetValue(__instance)
                    as BattleUnitModel;

                if (owner == null || owner.IsDead())
                    return;

                BattleUnitBuf_ASmallMiracle buf =
                    owner.bufListDetail
                    .GetBuf<BattleUnitBuf_ASmallMiracle>();

                if (buf == null)
                    return;

                owner.RecoverHP(2);
            }
        }
    }


    public class KaedeInitializer : ModInitializer //この関数名を書き換えるときは必ず全部Ctrl + Hで置き換えること
    {
        public override void OnInitializeMod() //Modを読み込むときにしたいことをここに。大体のパッチはここで何かしないと動かないぞ！
        {
            Harmony harmony = new Harmony(ModUtil.packageId);
            // パッチ読み込みの一番初めにする定義
            //ModUtil.packageIdでなくても被らないstringならなんでも可。(毎回手作業で変えるの面倒だからModUtil.packageIdでいいけど)
            foreach (Type type in typeof(PatchClass).GetNestedTypes(AccessTools.all))
            {
                Debug.Log(ModUtil.packageId + " HarmonyPatch：" + type.Name);
                harmony.CreateClassProcessor(type).Patch();
            }

            // 各種xmlとリソースの読み込み処理
            ModUtil.path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
             LoadArtworks(new DirectoryInfo(ModUtil.path + "/ArtWork"));
            LoadEmotionalCards();
            LoadAbnormalityCardLocalize();

            // PatchClassにあるパッチを全部読みこんでデバッグ用のlogを出してる
            // このModの「同じアセンブリあるよ」っていうエラーを全部消す
            Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll((string errorLog) => errorLog.Contains(ModUtil.packageId) && errorLog.Contains("The same assembly name already exists"));

        }

        // =========================================================
        // 幻想体ページのXMLの読み込み
        // =========================================================
        private static void LoadEmotionalCards()
        {
            foreach (FileInfo file in new DirectoryInfo(ModUtil.path + "/EmotionalCard").GetFiles())
            {
                try
                {
                    using (StringReader reader = new StringReader(File.ReadAllText(file.FullName)))
                    {
                        EmotionCardXmlRoot root =
                            (EmotionCardXmlRoot)new XmlSerializer(typeof(EmotionCardXmlRoot)).Deserialize(reader);

                        EmotionSelectControl.SetExEmotionCardsList(root.emotionCardXmlList);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        // =========================================================
        // 幻想体ページの翻訳ファイルを読み込み
        // =========================================================
        private static void LoadAbnormalityCardLocalize()
        {
            try
            {
                string path = ModUtil.path + "/Localize/" + GlobalGameManager.Instance.CurrentOption.language + "/AbnormalityCards";

                if (!Directory.Exists(path))
                    return;

                FileInfo[] files = new DirectoryInfo(path).GetFiles();

                foreach (FileInfo file in files)
                {
                    using (StringReader reader = new StringReader(File.ReadAllText(file.FullName)))
                    {
                        AbnormalityCardsRoot root =
                            (AbnormalityCardsRoot)new XmlSerializer(typeof(AbnormalityCardsRoot)).Deserialize(reader);

                        Dictionary<string, AbnormalityCard> dictionary =
                            typeof(AbnormalityCardDescXmlList)
                            .GetField("_dictionary", AccessTools.all)
                            .GetValue(Singleton<AbnormalityCardDescXmlList>.Instance)
                            as Dictionary<string, AbnormalityCard>;

                        foreach (AbnormalityCard card in root.sephirahList.SelectMany(x => x.list))
                        {
                            string key = card.id;

                            dictionary[key] = card;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        // =========================================================
        // 幻想体ページのアートワークファイルを読み込み
        // =========================================================
        private static void LoadArtworks(DirectoryInfo dir)
        {
            foreach (DirectoryInfo sub in dir.GetDirectories())
            {
                LoadArtworks(sub);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                Texture2D tex = new Texture2D(2, 2);

                tex.LoadImage(File.ReadAllBytes(file.FullName));

                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    Vector2.zero);

                ModUtil.ArtWorks[Path.GetFileNameWithoutExtension(file.Name)] = sprite;
            }
        }
    }
}
