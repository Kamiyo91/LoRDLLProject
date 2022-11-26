using BigDLL4221.Models;

namespace BigDLL4221.CardAbility
{
    public class DiceCardSelfAbility_EmotionScreenUI_DLL4221 : DiceCardSelfAbilityBase
    {
        public virtual string PoolName { get; set; }
        public virtual bool OnlyForUser { get; set; } = false;
        public virtual bool IncreaseEmotionSelectLevel { get; set; } = false;

        public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
        {
            StaticModsInfo.EmotionCardPullCode = PoolName;
            if (OnlyForUser) StaticModsInfo.OnPlayEmotionCardUsedBy = unit.Book.BookId;
            if (!IncreaseEmotionSelectLevel) StaticModsInfo.OnPlayCardEmotion = true;
            Activate();
            self.exhaust = true;
        }

        private static void Activate()
        {
            var currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            //SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            currentStageFloorModel.StartPickEmotionCard();
        }

        public override bool IsTargetableSelf()
        {
            return true;
        }
    }
}