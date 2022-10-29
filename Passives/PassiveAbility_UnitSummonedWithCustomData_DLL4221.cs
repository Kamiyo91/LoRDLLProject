using BigDLL4221.Models;
using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_UnitSummonedWithCustomData_DLL4221 : PassiveAbilityBase
    {
        public SummonedUnitStatModel Model;

        public void SetParameters(SummonedUnitStatModel model)
        {
            Model = model;
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
    }
}