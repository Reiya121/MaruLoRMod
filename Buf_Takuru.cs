using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace Maru_Mod
{
    /// <summary>
    /// 揺らぐ決意 [崩壊] Lv1
    /// 幕の開始時
    /// 手札3枚以下：ダイスの威力-1
    /// 手札5枚以上：ダイスの威力+2
    /// </summary>
    public class BattleUnitBuf_WaveringResolve : BattleUnitBuf
    {
        protected override string keywordId => "WaveringResolve";

        private int _powerBonus = 0;

        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 手札枚数を計算
            int handCount = _owner.allyCardDetail.GetHand().Count;

            // リセット
            _powerBonus = 0;

            // ハンド枚数に応じたボーナスを取得
            if (handCount <= 3)
            {
                _powerBonus = -1;
            }
            else if (handCount >= 5)
            {
                _powerBonus = 2;
            }
        }

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (behavior == null)
                return;

            if (_powerBonus != 0)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus() { power = _powerBonus });
            }
        }
    }

    /// <summary>
    /// 義務感で戦う [覚醒] Lv1
    /// 光が自身より2多い相手に命中するたび、混乱ダメージ2〜5
    /// </summary>
    public class BattleUnitBuf_SenseOfDuty : BattleUnitBuf
    {
        protected override string keywordId => "SenseOfDuty";

        public override void OnSuccessAttack(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            BattleUnitModel target = behavior?.card?.target;
            if (target == null || target.IsDead())
                return;

            // 互いの光を取得
            int myLight = _owner.cardSlotDetail.PlayPoint;
            int targetLight = target.cardSlotDetail.PlayPoint;

            if (targetLight >= myLight + 2)
            {
                int dmg = UnityEngine.Random.Range(2, 6);
                target.TakeBreakDamage(dmg, DamageType.Buf, _owner);
            }
        }
    }

    /// <summary>
    /// ダイヤモンド [覚醒] Lv1
    /// 一幕に1回攻撃命中時、手札を1枚引く
    /// </summary>
    public class BattleUnitBuf_Diamond : BattleUnitBuf
    {
        protected override string keywordId => "Diamond";

        private bool _activatedThisRound = false;

        public override void OnRoundStart()
        {
            // 幕開始時にリセット
            _activatedThisRound = false;
        }

        public override void OnSuccessAttack(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 既に発動していたら何もしない
            if (_activatedThisRound)
                return;

            // 攻撃ダイスのみ（任意だけど推奨）
            if (!IsAttackDice(behavior.Detail))
                return;

            // 1回だけ発動
            _activatedThisRound = true;

            // 1ドロー
            _owner.allyCardDetail.DrawCards(1);
        }
    }
}
