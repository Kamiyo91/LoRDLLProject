using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using LOR_DiceSystem;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_UnitSummonedLinkedToMainChar_DLL4221 : PassiveAbilityBase
    {
        private BattleUnitModel _mainChar;
        private PassiveAbilityBase _mainCharPassive;
        public SummonedUnitStatModelLinked Model;

        public override void OnWaveStart()
        {
            if (Model.LoweredCardCost != 0)
                UnitUtil.ChangeCardCostByValue(owner, Model.LoweredCardCost, Model.MaxCardCost, false);
            if (Model.EgoOptions != null && Model.EgoOptions.ActiveEgoOnStart) Model.EgoOptions.EgoActivated = true;
        }

        public void SetParameters(SummonedUnitStatModelLinked model,
            bool removeFromUIAfterDeath = false)
        {
            Model = model;
            Model.RemoveFromUIAfterDeath = removeFromUIAfterDeath;
            _mainChar = BattleObjectManager.instance.GetAliveList(owner.faction).FirstOrDefault(x =>
                x.HasPassive(model.LinkedCharByPassive.GetType(), out _mainCharPassive));
        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            if ((owner.faction == Faction.Player && Model.DieAtSceneEndForPlayer) ||
                (owner.faction == Faction.Enemy && Model.DieAtSceneEndForNpc)) owner.Die();
            ReviveMech(owner.faction == Faction.Player ? Model.ReviveAfterScenesPlayer : Model.ReviveAfterScenesNpc);
            if (!owner.IsDead() || !Model.RemoveFromUIAfterDeath) return;
            if (owner.faction == Faction.Player && Model.UseCustomData) owner.Book.owner = null;
            BattleObjectManager.instance.UnregisterUnit(owner);
            UnitUtil.RefreshCombatUI();
        }

        public override int SpeedDiceNumAdder()
        {
            return Model.AdditionalSpeedDie;
        }

        public override void OnBattleEnd()
        {
            if (!string.IsNullOrEmpty(Model.OriginalSkinName))
                owner.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                    new List<string> { Model.OriginalSkinName };
            if (owner.faction == Faction.Player && Model.UseCustomData) owner.Book.owner = null;
        }

        public void TurnReviveOff()
        {
            Model.ReviveAfterScenesPlayer = -1;
            Model.ReviveAfterScenesNpc = -1;
        }

        private void ReviveMech(int reviveAfterScenes)
        {
            if (_mainChar.IsDead()) return;
            if ((!Model.LowerOrHigherRange && _mainChar.hp <= Model.MainCharHpForRevive) ||
                (Model.LowerOrHigherRange && _mainChar.hp >= Model.MainCharHpForRevive)) return;
            if (reviveAfterScenes < 0) return;
            if (owner.IsDead() && reviveAfterScenes == Model.ReviveCount)
            {
                Model.ReviveCount = 0;
                owner.Revive(Model.HpRecoveredWithRevive);
                owner.view.EnableView(true);
                owner.view.EnableStatNumber(true);
            }

            if (owner.IsDead()) Model.ReviveCount++;
        }

        public override int GetDamageReduction(BattleDiceBehavior behavior)
        {
            if (Model.DamageOptions == null) return base.GetDamageReduction(behavior);
            switch (behavior.card.card.XmlData.Spec.Ranged)
            {
                case CardRange.FarArea:
                    return Model.DamageOptions.LessMassAttackDamage;
                case CardRange.FarAreaEach:
                    return Model.DamageOptions.LessMassAttackIndividualDamage;
                case CardRange.Near:
                    return Model.DamageOptions.LessMeleeAttackDamage;
                case CardRange.Far:
                    return Model.DamageOptions.LessRangedAttackDamage;
                case CardRange.Special:
                    return Model.DamageOptions.LessSpecialRangeAttackDamage;
            }

            switch (behavior.Detail)
            {
                case BehaviourDetail.Slash:
                    return Model.DamageOptions.LessSlashAttackDamage;
                case BehaviourDetail.Penetrate:
                    return Model.DamageOptions.LessPierceAttackDamage;
                case BehaviourDetail.Hit:
                    return Model.DamageOptions.LessHitAttackDamage;
            }

            return base.GetDamageReduction(behavior);
        }

        public override int GetBreakDamageReduction(BattleDiceBehavior behavior)
        {
            if (Model.DamageOptions == null) return base.GetDamageReduction(behavior);
            switch (behavior.card.card.XmlData.Spec.Ranged)
            {
                case CardRange.FarArea:
                    return Model.DamageOptions.LessMassAttackBreakDamage;
                case CardRange.FarAreaEach:
                    return Model.DamageOptions.LessMassAttackIndividualBreakDamage;
                case CardRange.Near:
                    return Model.DamageOptions.LessMeleeAttackBreakDamage;
                case CardRange.Far:
                    return Model.DamageOptions.LessRangedAttackBreakDamage;
                case CardRange.Special:
                    return Model.DamageOptions.LessSpecialRangeAttackBreakDamage;
            }

            switch (behavior.Detail)
            {
                case BehaviourDetail.Slash:
                    return Model.DamageOptions.LessSlashAttackBreakDamage;
                case BehaviourDetail.Penetrate:
                    return Model.DamageOptions.LessPierceAttackBreakDamage;
                case BehaviourDetail.Hit:
                    return Model.DamageOptions.LessHitAttackBreakDamage;
            }

            return base.GetDamageReduction(behavior);
        }

        public override BattleUnitModel ChangeAttackTarget(BattleDiceCardModel card, int idx)
        {
            var unit = UnitUtil.IgnoreSephiraSelectionTarget(Model.IgnoreSephirah);
            return unit ?? base.ChangeAttackTarget(card, idx);
        }

        public override int ChangeTargetSlot(BattleDiceCardModel card, BattleUnitModel target, int currentSlot,
            int targetSlot, bool teamkill)
        {
            return UnitUtil.AlwaysAimToTheSlowestDice(target, targetSlot, Model.AimAtTheSlowestDie);
        }

        public override BattleDiceCardModel OnSelectCardAuto(BattleDiceCardModel origin, int currentDiceSlotIdx)
        {
            if (Model.OneTurnCard || Model.MaxCounter < 0 || !Model.MassAttackCards.Any())
                return base.OnSelectCardAuto(origin, currentDiceSlotIdx);
            if (Model.Counter < Model.MaxCounter) return base.OnSelectCardAuto(origin, currentDiceSlotIdx);
            origin = BattleDiceCardModel.CreatePlayingCard(
                ItemXmlDataList.instance.GetCardItem(RandomUtil.SelectOne(Model.MassAttackCards)));
            Model.OneTurnCard = true;
            return base.OnSelectCardAuto(origin, currentDiceSlotIdx);
        }

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {
            if (Model.EgoActivated || Model.EgoOptions == null || Model.EgoOptions.EgoActivated)
                return base.BeforeTakeDamage(attacker, dmg);
            if (Model.EgoOptions.ActiveEgoOnHpRange == 0 || owner.hp - dmg > Model.EgoOptions.ActiveEgoOnHpRange)
                return base.BeforeTakeDamage(attacker, dmg);
            Model.EgoOptions.EgoActivated = true;
            return base.BeforeTakeDamage(attacker, dmg);
        }

        public override void OnRoundStart()
        {
            if (Model.EgoOptions != null && Model.EgoOptions.EgoActivated && !Model.EgoActivated) EgoActive();
            if (Model.MaxCounter > 0) Model.Counter++;
            Model.OneTurnCard = false;
        }

        public virtual void EgoActive()
        {
            Model.EgoOptions.EgoActivated = false;
            Model.EgoActivated = true;
            if (!string.IsNullOrEmpty(Model.EgoOptions.EgoSkinName))
                owner.view.SetAltSkin(Model.EgoOptions.EgoSkinName);
            if (Model.EgoOptions.EgoType != null)
                owner.bufListDetail.AddBufWithoutDuplication(Model.EgoOptions.EgoType);
            owner.cardSlotDetail.RecoverPlayPoint(owner.cardSlotDetail.GetMaxPlayPoint());
            if (Model.EgoOptions.RecoverHpOnEgo != 0)
                UnitUtil.UnitReviveAndRecovery(owner, Model.EgoOptions.RecoverHpOnEgo, false);
            if (Model.EgoOptions.RefreshUI) UnitUtil.RefreshCombatUI();
            if (Model.EgoOptions.EgoAbDialogList.Any())
                UnitUtil.BattleAbDialog(owner.view.dialogUI, Model.EgoOptions.EgoAbDialogList,
                    Model.EgoOptions.EgoAbColorColor);
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            if (!Model.MassAttackCards.Contains(curCard.card.GetID())) return;
            Model.Counter = 0;
            owner.allyCardDetail.ExhaustACardAnywhere(curCard.card);
            Model.OneTurnCard = false;
        }

        public virtual void SetCounter(int value)
        {
            Model.Counter = value;
        }
    }
}