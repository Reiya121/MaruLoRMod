using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    /// <summary>
    /// 叡智
    /// </summary>
    public class EmotionCardAbility_WisdomPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_Wisdom());
        }
    }

    /// <summary>
    /// 過保護
    /// </summary>
    public class EmotionCardAbility_OverprotectivePage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_Overprotective());
        }
    }

    /// <summary>
    /// 見守るもの
    /// </summary>
    public class EmotionCardAbility_TheGuardianPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_TheGuardian());
        }
    }
}
