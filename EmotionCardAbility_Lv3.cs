using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    /// <summary>
    /// 覚悟
    /// </summary>
    public class EmotionCardAbility_ResolvePage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_Resolve());
        }
    }

    /// <summary>
    /// 冒険(旧:循環)
    /// </summary>
    public class EmotionCardAbility_Adventure_FormerlyCyclePage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_Adventure_FormerlyCycle());
        }
    }

    /// <summary>
    /// 統率
    /// </summary>
    public class EmotionCardAbility_LeadershipPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_Leadership());
        }
    }
}