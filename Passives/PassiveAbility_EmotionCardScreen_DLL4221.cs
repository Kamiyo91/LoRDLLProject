using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_EmotionCardScreen_DLL4221 : PassiveAbilityBase
    {
        public int EmotionCards;
        public virtual string PoolName { get; set; }
        public virtual bool OnlyForUser { get; set; } = true;
        public virtual int SelectionMaxNumber { get; set; } = 5;

        public override void OnWaveStart()
        {
            EmotionCards = Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData<int>($"EmotionUnit{PoolName}", out var usedEmotionCards)
                ? usedEmotionCards
                : 0;
        }

        public override /*async*/ void OnRoundEndTheLast()
        {
            //await GenericUtil.PutTaskDelay(1000);
            owner.emotionDetail.CheckLevelUp();
            if (owner.emotionDetail.EmotionLevel > SelectionMaxNumber + 1 ||
                EmotionCards >= owner.emotionDetail.EmotionLevel) return;
            CustomEmotionTool.SetParameters(new CustomEmotionParameters
            {
                PoolName = PoolName,
                BookId = owner.Book.BookId,
                IsOnlyForUser = OnlyForUser,
                EmotionLevel = owner.emotionDetail.EmotionLevel
            });
        }

        public override void OnBattleEnd()
        {
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            stageModel.SetStageStorgeData($"EmotionUnit{PoolName}", EmotionCards);
        }
    }
}