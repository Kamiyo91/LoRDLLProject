using System.Collections.Generic;
using System.Linq;
using BigDLL4221.BaseClass;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
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
            if (!Util.CheckSkinChangeIsActive()) return;
            if (!ModParameters.KeypageOptions.TryGetValue(owner.Book.BookId.packageId, out var bookOptions)) return;
            var bookOption = bookOptions.FirstOrDefault(x => x.KeypageId == owner.Book.BookId.id);
            if (bookOption?.BookCustomOptions != null)
                owner.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                    new List<string> { bookOption.BookCustomOptions.OriginalSkin };
        }

        public override void OnWaveStart()
        {
            if(Util.Model.AdditionalStartDraw > 0) owner.allyCardDetail.DrawCards(Util.Model.AdditionalStartDraw);
            Util.AddExpireCards();
            Util.PermanentBuffs();
            if (UnitUtil.CheckSkinProjection(owner))
                Util.DoNotChangeSkinOnEgo();
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            Util.OnUseExpireCard(curCard.card.GetID());
            Util.ChangeToEgoMap(curCard.card.GetID());
        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            Util.DeactiveEgo();
            Util.ReturnFromEgoMap();
        }

        public override void OnRoundEndTheLast()
        {
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
            if (Util.CheckOnDieAtFightEnd())
                owner.Die();
        }

        public void ForcedEgo(int egoPhase)
        {
            var egoCard = Util.Model.PersonalCards.FirstOrDefault(x => x.Value.ActiveEgoCard && x.Value.EgoPhase == egoPhase);
            if (egoCard.Key == null) return;
            if (!Util.Model.EgoOptions.TryGetValue(egoCard.Value.EgoPhase, out var egoOptions)) return;
            Util.Model.EgoPhase = egoPhase;
            owner.personalEgoDetail.RemoveCard(egoCard.Key);
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
    }
}