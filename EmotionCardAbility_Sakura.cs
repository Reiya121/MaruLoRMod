using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    /// <summary>
    /// 小さな奇跡
    /// </summary>
    public class EmotionCardAbility_ASmallMiraclePage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;
            foreach (BattleUnitModel unit in
                BattleObjectManager.instance.GetAliveList(_owner.faction))
            {
                _owner.bufListDetail.AddBuf(new BattleUnitBuf_ASmallMiracle());
            }
        }
    }

    /// <summary>
    /// 願いの形
    /// </summary>
    public class EmotionCardAbility_TheShapeOfAWishPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            foreach (BattleUnitModel unit in
                BattleObjectManager.instance.GetAliveList(_owner.faction))
            {
                _owner.bufListDetail.AddBuf(new BattleUnitBuf_TheShapeOfAWish());
            }
        }
    }

    /// <summary>
    /// 信じる心
    /// </summary>
    public class EmotionCardAbility_ABelievingHeartPage : EmotionCardAbilityBase
    {
        public override void OnSelectEmotionOnce()
        {
            if (_owner == null)
                return;

            _owner.bufListDetail.AddBuf(new BattleUnitBuf_ABelievingHeart());
        }
    }
}