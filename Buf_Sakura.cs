using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace Maru_Mod
{
    /// <summary>
    /// 小さな奇跡 [覚醒] Lv2
    /// 光回復時、HP2回復
    /// </summary>
    public class BattleUnitBuf_ASmallMiracle : BattleUnitBuf
    {
        protected override string keywordId => "ASmallMiracle";

    }

    /// <summary>
    /// 願いの形 [覚醒] Lv2
    /// 光3以上のとき、パワー+1
    /// </summary>
    public class BattleUnitBuf_TheShapeOfAWish : BattleUnitBuf
    {
        protected override string keywordId => "TheShapeOfAWish";

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            // 現在の光を取得
            int light = _owner.cardSlotDetail.PlayPoint;

            // 光3以上ならパワー+1
            if (light >= 3)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus()
                {
                    power = 1
                });
            }
        }
    }

    /// <summary>
    /// 信じる心 [覚醒] Lv2
    /// 幕開始時、光1回復
    /// 手札5枚以上ならさらに+1
    /// </summary>
    public class BattleUnitBuf_ABelievingHeart : BattleUnitBuf
    {
        protected override string keywordId => "ABelievingHeart";

        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 基本回復量
            int recover = 1;

            // 手札枚数取得
            int handCount = _owner.allyCardDetail.GetHand().Count;

            // 手札5枚以上ならさらに+1
            if (handCount >= 5)
            {
                recover += 1;
            }

            // 光回復
            _owner.cardSlotDetail.RecoverPlayPoint(recover);
        }
    }
}
