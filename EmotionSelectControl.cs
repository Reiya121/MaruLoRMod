using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    internal class EmotionSelectControl
    {
        // Token: 0x0600000E RID: 14 RVA: 0x00002421 File Offset: 0x00000621
        public static void Initialize()
        {
            EmotionSelectControl._selectedCards = new List<EmotionCardXmlInfo>();
            EmotionSelectControl._nextSelectEmotionLevel = 1;
            EmotionSelectControl.ChangeForExtraSelection = false;
        }

        // Token: 0x0600000F RID: 15 RVA: 0x0000243A File Offset: 0x0000063A
        internal static void SetNextSelectionLevel()
        {
            EmotionSelectControl._nextSelectEmotionLevel += 1;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x0000244A File Offset: 0x0000064A
        internal static void SetSelectedCard(EmotionCardXmlInfo card)
        {
            EmotionSelectControl._selectedCards.Add(card);
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000011 RID: 17 RVA: 0x0000245C File Offset: 0x0000065C
        internal static int NextSelectEmotionLevel
        {
            get
            {
                return EmotionSelectControl._nextSelectEmotionLevel;
            }
        }

        internal static int MaxEmotionLevel
        {
            get
            {
                if (_extraEmotionCardList == null || _extraEmotionCardList.Count == 0)
                    return 0;

                return _extraEmotionCardList.Max(x => x.Level);
            }
        }

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000012 RID: 18 RVA: 0x00002474 File Offset: 0x00000674
        internal static List<EmotionCardXmlInfo> ExEmotionCardsList
        {
            get
            {
                return new List<EmotionCardXmlInfo>(EmotionSelectControl._extraEmotionCardList);
            }
        }

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x06000013 RID: 19 RVA: 0x00002490 File Offset: 0x00000690
        internal static List<EmotionCardXmlInfo> SelectedCardsList
        {
            get
            {
                return new List<EmotionCardXmlInfo>(EmotionSelectControl._selectedCards);
            }
        }

        // Token: 0x06000014 RID: 20 RVA: 0x000024AC File Offset: 0x000006AC
        internal static void SetExEmotionCardsList(List<EmotionCardXmlInfo> cardList)
        {
            EmotionSelectControl._extraEmotionCardList = new List<EmotionCardXmlInfo>();
            EmotionSelectControl._extraEmotionCardList.AddRange(cardList);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000024C8 File Offset: 0x000006C8
        internal static BattleUnitModel GetEmoCard()
        {
            return BattleObjectManager.instance.GetAliveList(Faction.Player).Find((BattleUnitModel x) => x.passiveDetail.HasPassive<PassiveAbility_MaruExclusiveEmotion>());
        }

        // Token: 0x04000002 RID: 2
        private static List<EmotionCardXmlInfo> _extraEmotionCardList;

        // Token: 0x04000003 RID: 3
        private static List<EmotionCardXmlInfo> _selectedCards;

        // Token: 0x04000004 RID: 4
        private static int _nextSelectEmotionLevel;

        // Token: 0x04000005 RID: 5
        internal static bool ChangeForExtraSelection;
    }
}
