using System.Collections.Generic;
using System.Linq;
using BigDLL4221.BaseClass;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using LOR_DiceSystem;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_NpcMechBase_DLL4221 : PassiveAbilityBase
    {
        public NpcMechUtilBase Util;

        public void SetUtil(NpcMechUtilBase util)
        {
            if (Util != null) return;
            Util = util;
            Util.Model.Owner = owner;
            Util.Model.ThisPassiveId = id;
        }

        public override void OnWaveStart()
        {
            if (Util.Model.AdditionalStartDraw > 0) owner.allyCardDetail.DrawCards(Util.Model.AdditionalStartDraw);
            Util.MechBuff();
            Util.PermanentBuffs();
            if (!Util.Restart()) Util.InitStartMech();
        }

        public override void OnRoundStart()
        {
            Util.MechBuff();
            Util.PermanentBuffs();
            Util.UseSpecialBuffCard();
            Util.RoundStartBuffs();
            Util.ExtraRecovery();
            if (!Util.EgoCheck()) return;
            Util.EgoActive();
        }

        public override int SpeedDiceNumAdder()
        {
            return !Util.Model.MechOptions.TryGetValue(Util.Model.Phase, out var mechOptions)
                ? 0
                : mechOptions.SpeedDieAdder;
        }

        public override int ChangeTargetSlot(BattleDiceCardModel card, BattleUnitModel target, int currentSlot,
            int targetSlot, bool teamkill)
        {
            return Util.AlwaysAimToTheSlowestDice(target, targetSlot);
        }

        public override void OnRoundEnd()
        {
            Util.ExhaustMechBufAttackCards();
            Util.ExhaustEgoAttackCards();
            Util.SetOneTurnCard(false);
            Util.RaiseCounter();
        }

        public override void OnBattleEnd()
        {
            if (Util.CheckSkinChangeIsActive())
                if (ModParameters.KeypageOptions.TryGetValue(owner.Book.BookId.packageId, out var bookOptions))
                {
                    var bookOption = bookOptions.FirstOrDefault(x => x.KeypageId == owner.Book.BookId.id);
                    if (bookOption?.BookCustomOptions != null)
                        owner.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                            new List<string> { bookOption.BookCustomOptions.OriginalSkin };
                }

            Util.OnEndBattle();
        }

        public override BattleUnitModel ChangeAttackTarget(BattleDiceCardModel card, int idx)
        {
            var unit = Util.IgnoreSephiraSelectionTarget(card.GetID());
            return unit ?? base.ChangeAttackTarget(card, idx);
        }

        public override BattleDiceCardModel OnSelectCardAuto(BattleDiceCardModel origin, int currentDiceSlotIdx)
        {
            Util.OnSelectCardPutMassAttack(ref origin);
            return base.OnSelectCardAuto(origin, currentDiceSlotIdx);
        }

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {
            Util.MechHpCheck(dmg);
            Util.SurviveCheck(dmg);
            return base.BeforeTakeDamage(attacker, dmg);
        }

        public override void OnStartBattle()
        {
            UnitUtil.RemoveImmortalBuff(owner);
        }

        public override void OnDie()
        {
            if (!Util.Model.MechOptions.TryGetValue(Util.Model.Phase, out var mechOptions)) return;
            if (mechOptions.MechOnDeath)
                Util.Model.PhaseChanging = true;
            foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction)
                         .Where(x => mechOptions.UnitsThatDieTogetherByPassive.Contains(x.Book.BookId)))
                unit.DieFake();
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            Util.OnUseCardResetCount(curCard);
            Util.OnUseMechBuffAttackCard(curCard.card.GetID());
            Util.ChangeToEgoMap(curCard.card.GetID());
        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            Util.ReviveCheck();
            Util.CheckPhase();
            Util.ReturnFromEgoMap();
            if (owner.IsDead()) Util.ReturnFromEgoAssimilationMap();
        }

        public override int GetDamageReduction(BattleDiceBehavior behavior)
        {
            if (!Util.Model.MechOptions.TryGetValue(Util.Model.Phase, out var mechOptions))
                return base.GetDamageReduction(behavior);
            if (mechOptions.DamageOptions == null) return base.GetDamageReduction(behavior);
            switch (behavior.card.card.XmlData.Spec.Ranged)
            {
                case CardRange.FarArea:
                    return mechOptions.DamageOptions.LessMassAttackDamage;
                case CardRange.FarAreaEach:
                    return mechOptions.DamageOptions.LessMassAttackIndividualDamage;
                case CardRange.Near:
                    return mechOptions.DamageOptions.LessMeleeAttackDamage;
                case CardRange.Far:
                    return mechOptions.DamageOptions.LessRangedAttackDamage;
                case CardRange.Special:
                    return mechOptions.DamageOptions.LessSpecialRangeAttackDamage;
            }

            switch (behavior.Detail)
            {
                case BehaviourDetail.Slash:
                    return mechOptions.DamageOptions.LessSlashAttackDamage;
                case BehaviourDetail.Penetrate:
                    return mechOptions.DamageOptions.LessPierceAttackDamage;
                case BehaviourDetail.Hit:
                    return mechOptions.DamageOptions.LessHitAttackDamage;
            }

            return base.GetDamageReduction(behavior);
        }

        public override int GetBreakDamageReduction(BattleDiceBehavior behavior)
        {
            if (!Util.Model.MechOptions.TryGetValue(Util.Model.Phase, out var mechOptions))
                return base.GetDamageReduction(behavior);
            if (mechOptions.DamageOptions == null) return base.GetDamageReduction(behavior);
            switch (behavior.card.card.XmlData.Spec.Ranged)
            {
                case CardRange.FarArea:
                    return mechOptions.DamageOptions.LessMassAttackBreakDamage;
                case CardRange.FarAreaEach:
                    return mechOptions.DamageOptions.LessMassAttackIndividualBreakDamage;
                case CardRange.Near:
                    return mechOptions.DamageOptions.LessMeleeAttackBreakDamage;
                case CardRange.Far:
                    return mechOptions.DamageOptions.LessRangedAttackBreakDamage;
                case CardRange.Special:
                    return mechOptions.DamageOptions.LessSpecialRangeAttackBreakDamage;
            }

            switch (behavior.Detail)
            {
                case BehaviourDetail.Slash:
                    return mechOptions.DamageOptions.LessSlashAttackBreakDamage;
                case BehaviourDetail.Penetrate:
                    return mechOptions.DamageOptions.LessPierceAttackBreakDamage;
                case BehaviourDetail.Hit:
                    return mechOptions.DamageOptions.LessHitAttackBreakDamage;
            }

            return base.GetDamageReduction(behavior);
        }
    }
}