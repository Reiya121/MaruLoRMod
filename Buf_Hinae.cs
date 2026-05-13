using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace Maru_Mod
{
    /// <summary>
    /// 叡智 [崩壊] Lv1
    /// [味方1名]
    /// ラウンド開始時、
    /// HP50%未満のとき、さらに1枚引く
    /// HP25%未満のとき、さらに1枚引く
    /// HP10%未満のとき、さらに1枚引く
    /// </summary>
    public class BattleUnitBuf_Wisdom : BattleUnitBuf
    {
        protected override string keywordId => "Wisdom";
        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 残っているHPの割合を取得
            float hpRate = (float)_owner.hp / _owner.MaxHp;

            // 最終的なドロー枚数
            int drawCount = 0;

            // HP50%未満のとき、さらに1枚引く
            if (hpRate < 0.5f)
                drawCount++;

            // HP25%未満のとき、さらに1枚引く
            if (hpRate < 0.25f)
                drawCount++;

            // HP10%未満のとき、さらに1枚引く
            if (hpRate < 0.1f)
                drawCount++;

            // 貯まったカウントの分だけドローする
            if (drawCount > 0)
            {
                _owner.allyCardDetail.DrawCards(drawCount);
            }
        }
    }

    /// <summary>
    /// 過保護 [覚醒] Lv1
    /// [味方1名]
    /// ラウンド開始時
    /// 手札が4枚以下の場合、さらに1枚引く
    /// </summary>
    public class BattleUnitBuf_Overprotective : BattleUnitBuf
    {
        protected override string keywordId => "Overprotective";
        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 現在の手札枚数を取得
            int handCount = _owner.allyCardDetail.GetHand().Count;

            // 最終的なドロー枚数
            int drawCount = 0;

            if (handCount <= 4)
            {
                drawCount++;
            }

            // 貯まったカウントの分だけドローする
            if (drawCount > 0)
            {
                _owner.allyCardDetail.DrawCards(drawCount);
            }
        }
    }

    /// <summary>
    /// 見守るもの [崩壊] Lv1
    /// [味方1名]
    /// 防御ダイス + 1
    /// パワー - 1
    /// </summary>
    public class BattleUnitBuf_TheGuardian : BattleUnitBuf
    {
        protected override string keywordId => "TheGuardian";

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (behavior == null)
                return;

            if (behavior.Detail == BehaviourDetail.Guard)
            {
                // 防御ダイス +1
                behavior.ApplyDiceStatBonus(new DiceStatBonus() { power = 1 });
            }
            else
            {
                // それ以外のダイスは -1
                behavior.ApplyDiceStatBonus(new DiceStatBonus() { power = -1 });
            }
        }
    }
}
