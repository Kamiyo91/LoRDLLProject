using BigDLL4221.Models;
using BigDLL4221.Utils;
using UnityEngine;

namespace BigDLL4221.CardAbility
{
    public class DiceCardSelfAbility_EgoScreenUI_DLL4221 : DiceCardSelfAbilityBase
    {
        public virtual string PoolName { get; set; }

        public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
        {
            StaticModsInfo.EgoCardPullCode = PoolName;
            Activate(unit);
            self.exhaust = true;
        }

        private static void Activate(BattleUnitModel unit)
        {
            var egoList = CardUtil.CustomCreateSelectableEgoList(StaticModsInfo.EgoCardPullCode);
            StaticModsInfo.EgoCardPullCode = string.Empty;
            Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.egoSelectionPoint++;
            if (egoList.Count <= 0) return;
            if (!SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.IsEnabled)
                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.InitEgo(
                Mathf.Clamp(unit.emotionDetail.EmotionLevel - 1, 0, 4), egoList);
        }

        public override bool IsTargetableSelf()
        {
            return true;
        }
    }
}