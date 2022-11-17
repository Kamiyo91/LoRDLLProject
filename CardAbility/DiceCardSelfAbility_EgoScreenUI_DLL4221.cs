using BigDLL4221.Models;

namespace BigDLL4221.CardAbility
{
    public class DiceCardSelfAbility_EgoScreenUI_DLL4221 : DiceCardSelfAbilityBase
    {
        public virtual string PoolName { get; set; }

        public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
        {
            ModParameters.EgoCardPullCode = PoolName;
            Activate(unit);
            self.exhaust = true;
        }

        private static void Activate(BattleUnitModel unit)
        {
            var currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            unit.view.speedDiceSetterUI.gameObject.SetActive(false);
            Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.egoSelectionPoint++;
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            currentStageFloorModel.StartPickEmotionCard();
        }

        public override bool IsTargetableSelf()
        {
            return true;
        }
    }
}