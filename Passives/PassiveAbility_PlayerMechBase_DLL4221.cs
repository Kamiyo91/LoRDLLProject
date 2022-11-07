using System.Collections.Generic;
using System.Linq;
using BigDLL4221.BaseClass;
using BigDLL4221.Utils;
using LOR_DiceSystem;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_PlayerMechBase_DLL4221 : PassiveAbilityBase
    {
        public MechUtilBase Util;

        public void SetUtil(MechUtilBase util)
        {
            if (Util != null) return;
            Util = util;
            Util.Model.Owner = owner;
            Util.Model.ThisPassiveId = id;
        }

        public override void OnBattleEnd()
        {
            if (Util.CheckSkinChangeIsActive() && !string.IsNullOrEmpty(Util.Model.OriginalSkinName))
                owner.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                    new List<string> { Util.Model.OriginalSkinName };
            if (owner.faction == Faction.Player && Util.Model.CustomData) owner.Book.owner = null;
        }

        public override void OnBreakState()
        {
            Util.DeactiveEgoOnBreak();
        }

        public override void OnWaveStart()
        {
            UnitUtil.CheckSkinProjection(owner);
            if (Util.Model.AdditionalStartDraw > 0) owner.allyCardDetail.DrawCards(Util.Model.AdditionalStartDraw);
            Util.AddExpireCards();
            Util.PermanentBuffs();
            if (UnitUtil.CheckSkinProjection(owner))
                Util.DoNotChangeSkinOnEgo();
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            Util.OnUseExpireCard(curCard);
            Util.ChangeToEgoMap(curCard.card.GetID());
        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            Util.ExtraMethodOnRoundEndTheLastIgnoreDead();
            Util.ReviveCheck();
            Util.DeactiveEgoDuration();
            Util.ReturnFromEgoMap();
            if (owner.IsDead()) Util.ReturnFromEgoAssimilationMap();
            if (!Util.Model.EgoOptions.TryGetValue(Util.Model.EgoPhase, out var egoOptions)) return;
            if (!egoOptions.RemoveEgoWhenSolo) return;
            if (BattleObjectManager.instance.GetAliveList(owner.faction).Count(x =>
                    x.passiveDetail.PassiveList.Exists(y => egoOptions.UnitsThatDieTogetherByPassive.Contains(y.id))) >
                0) return;
            Util.ForcedDeactiveEgo();
        }

        public override void OnRoundEndTheLast()
        {
            Util.ExtraMethodOnRoundEndTheLast();
            if (Util.EgoCheck()) Util.EgoActive();
        }

        public override void OnRoundEnd()
        {
            if (owner.faction != Faction.Enemy) return;
            if (Util.Model.EgoOptions == null) return;
            if (!UnitUtil.SpecialCaseEgo(owner.faction, Util.Model.ThisPassiveId,
                    Util.Model.EgoOptions, out var egoPhase)) return;
            Util.Model.EgoPhase = egoPhase;
            Util.ForcedEgo();
        }

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {
            Util.SurviveCheck(dmg);
            return base.BeforeTakeDamage(attacker, dmg);
        }

        public override void OnRoundStartAfter()
        {
            Util.PermanentBuffs();
            Util.EgoRoundStartBuffs();
            Util.ReAddOnPlayCard();
            Util.InitDeactiveEgoAfter();
        }

        public override void OnStartBattle()
        {
            UnitUtil.RemoveImmortalBuff(owner);
        }

        public override void OnRoundStart()
        {
            Util.EgoDurationCount();
            if (Util.EgoCheck()) Util.EgoActive();
        }

        public override void OnKill(BattleUnitModel target)
        {
            Util.ExtraMethodOnKill(target);
            if (Util.CheckOnDieAtFightEnd())
                owner.Die();
        }

        public override void OnDie()
        {
            if (!Util.Model.EgoOptions.TryGetValue(Util.Model.EgoPhase, out var egoOptions)) return;
            foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction)
                         .Where(x => egoOptions.UnitsThatDieTogetherByPassive.Contains(x.Book.BookId)))
                unit.Die();
        }

        public void ForcedEgo(int egoPhase)
        {
            if (egoPhase != 0)
            {
                var egoCard =
                    Util.Model.PersonalCards.FirstOrDefault(x => x.Value.ActiveEgoCard && x.Value.EgoPhase == egoPhase);
                if (egoCard.Key == null) return;
                if (!Util.Model.EgoOptions.TryGetValue(egoCard.Value.EgoPhase, out _)) return;
                owner.personalEgoDetail.RemoveCard(egoCard.Key);
            }
            else
            {
                owner.personalEgoDetail.RemoveCard(Util.Model.FirstEgoFormCard);
            }

            Util.Model.EgoPhase = egoPhase;
            Util.ForcedEgo();
        }

        public void SetDieAtEnd()
        {
            Util.TurnOnDieAtFightEnd();
        }

        public override int GetDamageReduction(BattleDiceBehavior behavior)
        {
            if (Util.Model.DamageOptions == null) return base.GetDamageReduction(behavior);
            switch (behavior.card.card.XmlData.Spec.Ranged)
            {
                case CardRange.FarArea:
                    return Util.Model.DamageOptions.LessMassAttackDamage;
                case CardRange.FarAreaEach:
                    return Util.Model.DamageOptions.LessMassAttackIndividualDamage;
                case CardRange.Near:
                    return Util.Model.DamageOptions.LessMeleeAttackDamage;
                case CardRange.Far:
                    return Util.Model.DamageOptions.LessRangedAttackDamage;
                case CardRange.Special:
                    return Util.Model.DamageOptions.LessSpecialRangeAttackDamage;
            }

            switch (behavior.Detail)
            {
                case BehaviourDetail.Slash:
                    return Util.Model.DamageOptions.LessSlashAttackDamage;
                case BehaviourDetail.Penetrate:
                    return Util.Model.DamageOptions.LessPierceAttackDamage;
                case BehaviourDetail.Hit:
                    return Util.Model.DamageOptions.LessHitAttackDamage;
            }

            return base.GetDamageReduction(behavior);
        }

        public override void OnDieOtherUnit(BattleUnitModel unit)
        {
            Util.ExtraMethodOnOtherUnitDie(unit);
        }

        public override int GetBreakDamageReduction(BattleDiceBehavior behavior)
        {
            if (Util.Model.DamageOptions == null) return base.GetDamageReduction(behavior);
            switch (behavior.card.card.XmlData.Spec.Ranged)
            {
                case CardRange.FarArea:
                    return Util.Model.DamageOptions.LessMassAttackBreakDamage;
                case CardRange.FarAreaEach:
                    return Util.Model.DamageOptions.LessMassAttackIndividualBreakDamage;
                case CardRange.Near:
                    return Util.Model.DamageOptions.LessMeleeAttackBreakDamage;
                case CardRange.Far:
                    return Util.Model.DamageOptions.LessRangedAttackBreakDamage;
                case CardRange.Special:
                    return Util.Model.DamageOptions.LessSpecialRangeAttackBreakDamage;
            }

            switch (behavior.Detail)
            {
                case BehaviourDetail.Slash:
                    return Util.Model.DamageOptions.LessSlashAttackBreakDamage;
                case BehaviourDetail.Penetrate:
                    return Util.Model.DamageOptions.LessPierceAttackBreakDamage;
                case BehaviourDetail.Hit:
                    return Util.Model.DamageOptions.LessHitAttackBreakDamage;
            }

            return base.GetDamageReduction(behavior);
        }

        public override int GetMaxHpBonus()
        {
            if (Util == null) return base.GetMaxHpBonus();
            if (!Util.Model.EgoOptions.TryGetValue(Util.Model.EgoPhase, out var egoOptions))
                return base.GetMaxHpBonus();
            return egoOptions.ExtraMaxHp != 0 ? egoOptions.ExtraMaxHp : base.GetMaxBpBonus();
        }

        public override int GetMaxBpBonus()
        {
            if (Util == null) return base.GetMaxBpBonus();
            if (!Util.Model.EgoOptions.TryGetValue(Util.Model.EgoPhase, out var egoOptions))
                return base.GetMaxBpBonus();
            return egoOptions.ExtraMaxStagger != 0 ? egoOptions.ExtraMaxStagger : base.GetMaxBpBonus();
        }
    }
}