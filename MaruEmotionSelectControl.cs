using HarmonyLib;
using LOR_XML;
using System.Collections.Generic;

namespace Maru_Mod
{
    public static class MaruEmotionSelectControl
    {
        public static List<EmotionCardXmlInfo> ExEmotionCardsList = new List<EmotionCardXmlInfo>();

        public static List<EmotionCardXmlInfo> SelectedCardsList = new List<EmotionCardXmlInfo>();

        public static bool ChangeForExtraSelection = false;

        public static int NextSelectEmotionLevel = 0;

        public static void Initialize()
        {
            SelectedCardsList.Clear();
            ChangeForExtraSelection = false;
            NextSelectEmotionLevel = 0;
        }
        [HarmonyPatch(typeof(BattleUnitView), "OnMouseDown")]

        public static void SetExEmotionCardsList(List<EmotionCardXmlInfo> list)
        {
            ExEmotionCardsList.AddRange(list);
        }

        public static void SetSelectedCard(EmotionCardXmlInfo card)
        {
            if (!SelectedCardsList.Contains(card))
                SelectedCardsList.Add(card);
        }

        public static void SetNextSelectionLevel()
        {
            NextSelectEmotionLevel++;
        }

        public static BattleUnitModel GetEmotionUnit()
        {
            foreach (BattleUnitModel unit in BattleObjectManager.instance.GetAliveList(Faction.Player))
            {
                if (unit.passiveDetail.HasPassive<PassiveAbility_MaruExclusiveEmotion>())
                    return unit;
            }

            return null;
        }
    }
}