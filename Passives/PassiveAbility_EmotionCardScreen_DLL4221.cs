using BigDLL4221.Models;
using BigDLL4221.Utils;
using UnityEngine;

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

        public override async void OnRoundEnd()
        {
            await GenericUtil.PutTaskDelay(1000);
            owner.emotionDetail.CheckLevelUp();
            if (owner.emotionDetail.EmotionLevel > SelectionMaxNumber + 1 ||
                EmotionCards >= owner.emotionDetail.EmotionLevel) return;
            EmotionCards++;
            var emotionList = CardUtil.CustomCreateSelectableList(owner.emotionDetail.EmotionLevel, PoolName);
            StaticModsInfo.OnPlayCardEmotion = true;
            if (emotionList.Count <= 0) return;
            if (!SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.IsEnabled)
                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            StaticModsInfo.EmotionCardPullCode = PoolName;
            if (OnlyForUser) StaticModsInfo.OnPlayEmotionCardUsedBy = owner.Book.BookId;
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.Init(
                Mathf.Clamp(owner.emotionDetail.EmotionLevel - 1, 0, 4), emotionList);
        }

        public override void OnBattleEnd()
        {
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            stageModel.SetStageStorgeData($"EmotionUnit{PoolName}", EmotionCards);
        }
    }
}