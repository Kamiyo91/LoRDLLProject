using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using LOR_BattleUnit_UI;
using UnityEngine;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class SpeedDiceColorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpeedDiceUI), "Init")]
        public static void Init(SpeedDiceUI __instance, Faction faction)
        {
            if (ModParameters.KeypageOptions.TryGetValue(__instance._view.model.Book.BookId.packageId,
                    out var keypageOptions))
            {
                var keypageOption =
                    keypageOptions.FirstOrDefault(x => x.KeypageId == __instance._view.model.Book.BookId.id);
                if (keypageOption?.CustomDiceColorOptions != null)
                {
                    ArtUtil.ChangeSpeedDiceColor(__instance, keypageOption.CustomDiceColorOptions);
                    return;
                }
            }

            if (faction == Faction.Player)
            {
                var currentFloor = Singleton<StageController>.Instance.CurrentFloor;
                if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(currentFloor, out var floorItem)) return;
                if (floorItem.IsActive && floorItem.FloorOptions.CustomDiceColorOptions != null)
                    ArtUtil.ChangeSpeedDiceColor(__instance, floorItem.FloorOptions.CustomDiceColorOptions);
            }

            var stageId = Singleton<StageController>.Instance.GetStageModel().ClassInfo.id;
            if (!ModParameters.StageOptions.TryGetValue(stageId.packageId, out var stageOptions)) return;
            var stageItem =
                stageOptions.FirstOrDefault(x => x.StageId == stageId.id);
            if (stageItem?.CustomDiceColorOptions != null)
                ArtUtil.ChangeSpeedDiceColor(__instance, stageItem.CustomDiceColorOptions);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpeedDiceUI), "ChangeSprite")]
        public static void ChangeSprite(SpeedDiceUI __instance, int value)
        {
            if (value < 999 || !ModParameters.SpeedDieArtWorks.TryGetValue("Infinite", out var sprite)) return;
            __instance._txtSpeedMax.gameObject.SetActive(false);
            __instance.img_tensNum.sprite = sprite;
            __instance.img_tensNum.gameObject.transform.localPosition += new Vector3(13f, 0f, 0f);
            __instance.img_tensNum.color = __instance.img_unitsNum.color;
            __instance.img_tensNum.gameObject.transform.localScale = new Vector3(0.95f, 0.85f, 0f);
            __instance.img_tensNum.gameObject.SetActive(true);
        }
    }
}