using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    /// <summary>
    /// 揺らぐ決意
    /// </summary>
    public class EmotionCardAbility_WaveringResolvePage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_WaveringResolve());
        }
    }

    /// <summary>
    /// 義務感で戦う
    /// </summary>
    public class EmotionCardAbility_SenseOfDutyPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_SenseOfDuty());
        }
    }

    /// <summary>
    /// ダイヤモンド
    /// </summary>
    public class EmotionCardAbility_DiamondPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_Diamond());
        }
    }
}