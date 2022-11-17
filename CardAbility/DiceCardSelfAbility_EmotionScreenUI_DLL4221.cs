using BigDLL4221.Models;

namespace BigDLL4221.CardAbility
{
    public class DiceCardSelfAbility_EmotionScreenUI_DLL4221 : DiceCardSelfAbilityBase
    {
        public virtual string PoolName { get; set; }

        public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
        {
            StaticBoolChecks.EmotionCardPullCode = PoolName;
            Activate(unit);
            self.exhaust = true;
        }

        private static void Activate(BattleUnitModel unit)
        {
            var currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            unit.view.speedDiceSetterUI.gameObject.SetActive(false);
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            currentStageFloorModel.StartPickEmotionCard();
        }

        public override bool IsTargetableSelf()
        {
            return true;
        }
    }
}