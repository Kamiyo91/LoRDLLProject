using BigDLL4221.Models;

namespace BigDLL4221.CardAbility
{
    public class DiceCardSelfAbility_EgoScreenUI_DLL4221 : DiceCardSelfAbilityBase
    {
        public virtual string PoolName { get; set; }

        public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
        {
            StaticModsInfo.EgoCardPullCode = PoolName;
            Activate();
            self.exhaust = true;
        }

        private static void Activate()
        {
            var currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.egoSelectionPoint++;
            //SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            currentStageFloorModel.StartPickEmotionCard();
        }

        public override bool IsTargetableSelf()
        {
            return true;
        }
    }
}