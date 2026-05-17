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
            public static void Postfix(ref bool __result)
            {
                if (!__result)
                    return;

                BattleUnitModel unit = MaruEmotionSelectControl.GetEmotionUnit();

                if (unit == null)
                    return;

                if (unit.emotionDetail.EmotionLevel < MaruEmotionSelectControl.NextSelectEmotionLevel)
                    return;

                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);

                StartExtraSelection(unit);

                __result = false;
            }

            private static void StartExtraSelection(BattleUnitModel unit)
            {
                List<EmotionCardXmlInfo> list =
                    new List<EmotionCardXmlInfo>(MaruEmotionSelectControl.ExEmotionCardsList);

                foreach (EmotionCardXmlInfo card in MaruEmotionSelectControl.SelectedCardsList)
                    list.Remove(card);

                if (list.Count <= 0)
                {
                    SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(false);
                    return;
                }

                MaruEmotionSelectControl.ChangeForExtraSelection = true;

                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.Init(MaruEmotionSelectControl.NextSelectEmotionLevel,list);
            }
        }

        // =========================================================
        // 幻想体ページが選ばれたとき
        // =========================================================
        [HarmonyPatch(typeof(StageLibraryFloorModel), "OnPickPassiveCard")]
        public class StageLibraryFloorModel_OnPickPassiveCard
        {
            public static bool Prefix(EmotionCardXmlInfo card, BattleUnitModel target)
            {
                if (!MaruEmotionSelectControl.ChangeForExtraSelection)
                    return true;

                MaruEmotionSelectControl.ChangeForExtraSelection = false;

                if (card == null)
                    return false;

                MaruEmotionSelectControl.SetSelectedCard(card);

                if (card.TargetType == EmotionTargetType.All || card.TargetType == EmotionTargetType.AllIncludingEnemy)
                {
                    foreach (var unit in BattleObjectManager.instance.GetAliveList(Faction.Player))
                    {
                        unit.emotionDetail.ApplyEmotionCard(card, true);
                    }
                }
                else
                {
                    if (target != null)
                    {
                        target.emotionDetail.ApplyEmotionCard(card, true);
                    }
                    else
                    {
                        Debug.LogError("target is null");
                    }
                }


                MaruEmotionSelectControl.SetNextSelectionLevel();

                return false;
            }
        }

        // =========================================================
        // MaruEmotionSelectControlの初期化
        // =========================================================
        [HarmonyPatch(typeof(StageLibraryFloorModel), "Init")]
        public class StageLibraryFloorModel_Init
        {
            public static void Postfix()
            {
                MaruEmotionSelectControl.Initialize();
            }
        }

        // =========================================================
        // 各ページが選ばれたときの効果を選ぶ
        // =========================================================
        [HarmonyPatch(typeof(BattleEmotionCardModel), MethodType.Constructor, new Type[]
        {
    typeof(EmotionCardXmlInfo),
    typeof(BattleUnitModel)
        })]
        public class BattleEmotionCardModel_New
        {
            public static bool Prefix(
                BattleEmotionCardModel __instance,
                EmotionCardXmlInfo xmlInfo,
                BattleUnitModel owner,
                ref EmotionCardXmlInfo ____xmlInfo,
                ref BattleUnitModel ____owner,
                ref List<EmotionCardAbilityBase> ____abilityList)
            {
                Type type = null;

                switch (xmlInfo.Name)
                {
                    // =====================================================
                    // Lv1
                    // =====================================================

                    case "Wisdom_Page":
                        type = typeof(EmotionCardAbility_WisdomPage);
                        break;

                    case "Overprotective_Page":
                        type = typeof(EmotionCardAbility_OverprotectivePage);
                        break;

                    case "TheGuardian_Page":
                        type = typeof(EmotionCardAbility_TheGuardianPage);
                        break;

                    case "WaveringResolve_Page":
                        type = typeof(EmotionCardAbility_WaveringResolvePage);
                        break;

                    case "SenseOfDuty_Page":
                        type = typeof(EmotionCardAbility_SenseOfDutyPage);
                        break;

                    case "Diamond_Page":
                        type = typeof(EmotionCardAbility_DiamondPage);
                        break;

                    // =====================================================
                    // Lv2 覚醒
                    // =====================================================

                    case "ASmallMiracle_Page":
                        type = typeof(EmotionCardAbility_ASmallMiraclePage);
                        break;

                    case "TheShapeOfAWish_Page":
                        type = typeof(EmotionCardAbility_TheShapeOfAWishPage);
                        break;

                    case "ABelievingHeart_Page":
                        type = typeof(EmotionCardAbility_ABelievingHeartPage);
                        break;

                    // =====================================================
                    // Lv2 崩壊
                    // =====================================================

                    case "FalseWisdom_Page":
                        type = typeof(EmotionCardAbility_FalseWisdomPage);
                        break;

                    case "ATwistedWish_Page":
                        type = typeof(EmotionCardAbility_ATwistedWishPage);
                        break;

                    case "Chaos_Page":
                        type = typeof(EmotionCardAbility_ChaosPage);
                        break;

                    // =====================================================
                    // Lv3
                    // =====================================================

                    case "Resolve_Page":
                        type = typeof(EmotionCardAbility_ResolvePage);
                        break;

                    case "Adventure_FormerlyCycle_Page":
                        type = typeof(EmotionCardAbility_Adventure_FormerlyCyclePage);
                        break;

                    case "Leadership_Page":
                        type = typeof(EmotionCardAbility_LeadershipPage);
                        break;
                }

                if (type == null)
                    return true;

                ____xmlInfo = xmlInfo;
                ____owner = owner;

                try
                {
                    ____abilityList = new List<EmotionCardAbilityBase>();

                    EmotionCardAbilityBase ability =
                        Activator.CreateInstance(type) as EmotionCardAbilityBase;

                    ability.SetEmotionCard(__instance);

                    ____abilityList.Add(ability);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
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
            LoadEmotionalCards();
            LoadAbnormalityCardLocalize();
            LoadArtworks(new DirectoryInfo(ModUtil.path + "/ArtWork"));

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

                        MaruEmotionSelectControl.SetExEmotionCardsList(root.emotionCardXmlList);
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