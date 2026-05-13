using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maru_Mod
{
    /// <summary>
    /// 覚悟 [覚醒] Lv3
    /// 手札が4枚以上の味方は威力+1
    /// 光が4以上の味方は受けるダメージ-1
    /// ラウンド終了時、手札が5枚以上または光が5以上の味方は2ダメージを受ける
    /// [篠原桜を取得している場合]
    /// ダメージを受けた味方は次の幕、保護+1
    /// [ヌルラトホテプを取得している場合]
    /// ダメージを受けた味方は次の幕、攻撃ダイスの最低値+1 
    /// </summary>
    public class BattleUnitBuf_Resolve : BattleUnitBuf
    {
        protected override string keywordId => "Resolve";

        /// <summary>
        /// 味方に「桜系」バフ持ちがいるか
        /// </summary>
        private bool HasSakuraAlly()
        {
            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            return allies.Exists(x =>
                x.bufListDetail.GetBuf<BattleUnitBuf_ASmallMiracle>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_TheShapeOfAWish>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_ABelievingHeart>() != null);
        }

        /// <summary>
        /// 味方に「ヌルラトホテプ系」バフ持ちがいるか
        /// </summary>
        private bool HasNurratAlly()
        {
            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            return allies.Exists(x =>
                x.bufListDetail.GetBuf<BattleUnitBuf_FalseWisdom>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_ATwistedWish>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_Chaos>() != null);
        }

        /// <summary>
        /// 手札4枚以上なら威力+1
        /// </summary>
        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            int handCount = _owner.allyCardDetail.GetHand().Count;

            if (handCount >= 4)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus()
                {
                    power = 1
                });
            }
        }

        /// <summary>
        /// 光4以上なら被ダメ-1
        /// </summary>
        public override int GetDamageReduction(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return 0;

            int light = _owner.cardSlotDetail.PlayPoint;

            if (light >= 4)
                return 1;

            return 0;
        }

        /// <summary>
        /// ダメージを受けた時
        /// </summary>
        public override void OnTakeDamageByAttack(BattleDiceBehavior atkDice, int dmg)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (dmg <= 0)
                return;

            // 次の幕 保護+1
            if (HasSakuraAlly())
            {
                _owner.bufListDetail.AddKeywordBufByEtc(
                    KeywordBuf.Protection,
                    1,
                    _owner
                );
            }

            // 次の幕 攻撃ダイス最低値+1
            if (HasNurratAlly())
            {
                _owner.bufListDetail.AddReadyBuf(new BattleUnitBuf_MinDiceUp());
            }
        }

        /// <summary>
        /// 手札5枚以上 または 光5以上なら2ダメージ
        /// </summary>
        public override void OnRoundEnd()
        {
            if (_owner == null || _owner.IsDead())
                return;

            int handCount = _owner.allyCardDetail.GetHand().Count;
            int light = _owner.cardSlotDetail.PlayPoint;

            if (handCount >= 5 || light >= 5)
            {
                _owner.TakeDamage(2);
            }
        }
    }

    /// <summary>
    /// 次の幕、攻撃ダイスの最低値+1
    /// </summary>
    public class BattleUnitBuf_MinDiceUp : BattleUnitBuf
    {
        protected override string keywordId => "MinDiceUp";

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            // 攻撃ダイスのみ
            if (!IsAttackDice(behavior.Detail))
                return;

            behavior.ApplyDiceStatBonus(new DiceStatBonus()
            {
                min = 1
            });
        }

        public override void OnRoundEnd()
        {
            Destroy();
        }
    }

    /// <summary>
    /// 冒険(旧:循環) [覚醒] Lv3
    /// ラウンド開始時
    /// 手札が最も少ない味方はカードを1枚引く
    /// 光が最も少ない味方は光を1回復
    /// [篠原桜を取得している場合]
    /// カードを引いた味方は、混乱保護+1（この幕）
    /// [ヌルラトホテフを取得している場合]
    /// 光が最も多い味方は、次に使用するページのコスト-1（最低1） 
    /// </summary>
    public class BattleUnitBuf_Adventure_FormerlyCycle : BattleUnitBuf
    {
        protected override string keywordId => "Adventure_FormerlyCycle";

        /// <summary>
        /// 味方に桜系バフ持ちが居るか
        /// </summary>
        private bool HasSakuraAlly()
        {
            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            return allies.Exists(x =>
                x.bufListDetail.GetBuf<BattleUnitBuf_ASmallMiracle>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_TheShapeOfAWish>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_ABelievingHeart>() != null);
        }

        /// <summary>
        /// 味方にヌルラトホテプ系バフ持ちが居るか
        /// </summary>
        private bool HasNurratAlly()
        {
            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            return allies.Exists(x =>
                x.bufListDetail.GetBuf<BattleUnitBuf_FalseWisdom>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_ATwistedWish>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_Chaos>() != null);
        }

        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            if (allies == null || allies.Count == 0)
                return;

            // =========================
            // 手札が最も少ない味方
            // =========================
            int minHand = allies.Min(x => x.allyCardDetail.GetHand().Count);

            List<BattleUnitModel> minHandUnits = allies.FindAll(
                x => x.allyCardDetail.GetHand().Count == minHand);

            BattleUnitModel drawTarget =
                RandomUtil.SelectOne(minHandUnits);

            if (drawTarget != null)
            {
                // 1ドロー
                drawTarget.allyCardDetail.DrawCards(1);

                // 桜系が居るなら混乱保護+1（この幕）
                if (HasSakuraAlly())
                {
                    drawTarget.bufListDetail.AddKeywordBufThisRoundByEtc(
                        KeywordBuf.BreakProtection,
                        1,
                        _owner
                    );
                }
            }

            // =========================
            // 光が最も少ない味方
            // =========================
            int minLight = allies.Min(x => x.cardSlotDetail.PlayPoint);

            List<BattleUnitModel> minLightUnits = allies.FindAll(
                x => x.cardSlotDetail.PlayPoint == minLight);

            BattleUnitModel lightTarget =
                RandomUtil.SelectOne(minLightUnits);

            if (lightTarget != null)
            {
                lightTarget.cardSlotDetail.RecoverPlayPoint(1);
            }

            // =========================
            // 光が最も多い味方
            // 次の使用ページコスト-1
            // =========================
            if (HasNurratAlly())
            {
                // 既存のコスト減少バフを削除
                foreach (BattleUnitModel unit in allies)
                {
                    BattleUnitBuf_NextCardCostDown oldBuf =
                        unit.bufListDetail.GetBuf<BattleUnitBuf_NextCardCostDown>();

                    if (oldBuf != null)
                    {
                        oldBuf.Destroy();
                    }
                }

                int maxLight = allies.Max(x => x.cardSlotDetail.PlayPoint);

                List<BattleUnitModel> maxLightUnits = allies.FindAll(
                    x => x.cardSlotDetail.PlayPoint == maxLight);

                BattleUnitModel costTarget =
                    RandomUtil.SelectOne(maxLightUnits);

                if (costTarget != null)
                {
                    costTarget.bufListDetail.AddBuf(
                        new BattleUnitBuf_NextCardCostDown());
                }
            }
        }
    }

    /// <summary>
    /// 次に使用するページのコスト-1（最低1）
    /// </summary>
    public class BattleUnitBuf_NextCardCostDown : BattleUnitBuf
    {
        protected override string keywordId => "NextCardCostDown";

        private bool _used = false;

        public override int GetCardCostAdder(BattleDiceCardModel card)
        {
            if (_used)
                return 0;

            if (card == null)
                return 0;

            // 現在コスト取得
            int cost = card.CurCost;

            // 最低1
            if (cost <= 1)
                return 0;

            return -1;
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            _used = true;
            Destroy();
        }

        public override void OnRoundEnd()
        {
            Destroy();
        }
    }

    /// <summary>
    /// 統率 [覚醒] Lv3
    /// 手札が3枚以上の味方はパワー+1
    /// 光が3以上の味方は最初のダイス威力+1
    /// （両方満たす場合、両方適用）
    /// [篠原桜を取得している場合]
    /// ラウンド開始時、光+1
    /// [ヌルラトホテプを取得している場合]
    /// 光が4以上の味方は、攻撃命中時、出血1付与
    /// </summary>
    public class BattleUnitBuf_Leadership : BattleUnitBuf
    {
        protected override string keywordId => "Leadership";

        private bool _firstDiceUsed = false;

        /// <summary>
        /// 味方に桜系バフ持ちが居るか
        /// </summary>
        private bool HasSakuraAlly()
        {
            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            return allies.Exists(x =>
                x.bufListDetail.GetBuf<BattleUnitBuf_ASmallMiracle>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_TheShapeOfAWish>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_ABelievingHeart>() != null);
        }

        /// <summary>
        /// 味方にヌルラトホテプ系バフ持ちが居るか
        /// </summary>
        private bool HasNurratAlly()
        {
            List<BattleUnitModel> allies =
                BattleObjectManager.instance.GetAliveList(_owner.faction);

            return allies.Exists(x =>
                x.bufListDetail.GetBuf<BattleUnitBuf_FalseWisdom>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_ATwistedWish>() != null ||
                x.bufListDetail.GetBuf<BattleUnitBuf_Chaos>() != null);
        }

        public override void OnRoundStart()
        {
            if (_owner == null || _owner.IsDead())
                return;

            // 最初のダイスフラグをリセット
            _firstDiceUsed = false;

            // 桜系が居るなら光+1
            if (HasSakuraAlly())
            {
                _owner.cardSlotDetail.RecoverPlayPoint(1);
            }
        }

        /// <summary>
        /// ダイス威力補正
        /// </summary>
        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            // =========================
            // 手札3枚以上ならパワー+1
            // =========================
            int handCount = _owner.allyCardDetail.GetHand().Count;

            if (handCount >= 3)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus()
                {
                    power = 1
                });
            }

            // =========================
            // 光3以上なら最初のダイス威力+1
            // =========================
            int light = _owner.cardSlotDetail.PlayPoint;

            if (!_firstDiceUsed && light >= 3)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus()
                {
                    power = 1
                });

                _firstDiceUsed = true;
            }
        }

        /// <summary>
        /// 光4以上なら攻撃命中時、出血1付与
        /// </summary>
        public override void OnSuccessAttack(BattleDiceBehavior behavior)
        {
            if (_owner == null || _owner.IsDead())
                return;

            if (behavior == null)
                return;

            // ヌルラトホテプ系が居ないなら無効
            if (!HasNurratAlly())
                return;

            // 攻撃ダイスのみ
            if (!IsAttackDice(behavior.Detail))
                return;

            // 光4以上
            int light = _owner.cardSlotDetail.PlayPoint;

            if (light < 4)
                return;

            BattleUnitModel target = behavior.card?.target;

            if (target == null || target.IsDead())
                return;

            // 出血1付与
            target.bufListDetail.AddKeywordBufByEtc(
                KeywordBuf.Bleeding,
                1,
                _owner
            );
        }
    }
}
