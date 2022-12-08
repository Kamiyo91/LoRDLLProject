using BigDLL4221.Models;
using BigDLL4221.Utils;
using UnityEngine;

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
            Activate(unit);
            self.exhaust = true;
        }

        private static void Activate(BattleUnitModel unit)
        {
            var emotionList =
                CardUtil.CustomCreateSelectableList(unit.emotionDetail.EmotionLevel,
                    StaticModsInfo.EmotionCardPullCode);
            StaticModsInfo.EmotionCardPullCode = string.Empty;
            if (emotionList.Count <= 0) return;
            if (!SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.IsEnabled)
                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.Init(
                Mathf.Clamp(unit.emotionDetail.EmotionLevel - 1, 0, 4), emotionList);
        }

        public override bool IsTargetableSelf()
        {
            return true;
        }
    }
}