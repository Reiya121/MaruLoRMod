using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    /// <summary>
    /// 偽りの叡智
    /// </summary>
    public class EmotionCardAbility_FalseWisdomPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_FalseWisdom());
        }
    }

    /// <summary>
    /// 歪んだ願い
    /// </summary>
    public class EmotionCardAbility_ATwistedWishPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_ATwistedWish());
        }
    }

    /// <summary>
    /// 混沌
    /// </summary>
    public class EmotionCardAbility_ChaosPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            foreach (BattleUnitModel unit in
                BattleObjectManager.instance.GetAliveList(_owner.faction))
            {
                _owner.bufListDetail.AddBuf(new BattleUnitBuf_Chaos());
            }
        }
    }
}