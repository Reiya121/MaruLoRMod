using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace Maru_Mod
{
    /// <summary>
    /// 偽りの叡智 [崩壊] Lv2
    /// 幕開始時、光2回復
    /// 最大体力10%ダメージ
    /// </summary>
    public class BattleUnitBuf_FalseWisdom : BattleUnitBuf
    {
        protected override string keywordId => "FalseWisdom";

        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 光+2
            _owner.cardSlotDetail.RecoverPlayPoint(2);

            // 最大HPの10%ダメージ（最低1保証）
            int dmg = Mathf.Max(1, Mathf.RoundToInt(_owner.MaxHp * 0.1f));

            _owner.TakeDamage(dmg);
        }
    }

    /// <summary>
    /// 歪んだ願い [崩壊] Lv2
    /// パワー+2
    /// 幕終了時、最大体力10%ダメージ
    /// </summary>
    public class BattleUnitBuf_ATwistedWish : BattleUnitBuf
    {
        protected override string keywordId => "ATwistedWish";

        // 常時パワー+2
        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            behavior.ApplyDiceStatBonus(new DiceStatBonus()
            {
                power = 2
            });
        }

        // 幕終了時：最大HP10%ダメージ
        public override void OnRoundEnd()
        {
            if (_owner == null || _owner.IsDead())
                return;

            int dmg = Mathf.Max(1, Mathf.RoundToInt(_owner.MaxHp * 0.1f));

            _owner.TakeDamage(dmg);
        }
    }

    /// <summary>
    /// 混沌 [崩壊] Lv2
    /// 与ダメージ時、追加で2ダメージ
    /// 攻撃後、自身に1ダメージ
    /// </summary>
    public class BattleUnitBuf_Chaos : BattleUnitBuf
    {
        protected override string keywordId => "Chaos";

        public override void OnSuccessAttack(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            // 対象取得
            BattleUnitModel target = behavior.card?.target;

            if (target == null || target.IsDead())
                return;

            // 攻撃ダイスのみ対象
            if (!IsAttackDice(behavior.Detail))
                return;

            // 追加ダメージ +2
            target.TakeDamage(2);

            // 反動ダメージ（自分に1）
            _owner.TakeDamage(1);
        }
    }
}
