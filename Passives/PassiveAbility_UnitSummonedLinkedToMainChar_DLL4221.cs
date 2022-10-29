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

        public void SetParameters(SummonedUnitStatModelLinked model)
        {
            Model = model;
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

        public override void OnBattleEnd()
        {
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
            if ((!Model.LowerOrHigherRange && _mainChar.hp < Model.MainCharHpForRevive) ||
                (Model.LowerOrHigherRange && _mainChar.hp > Model.MainCharHpForRevive)) return;
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
    }
}