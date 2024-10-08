﻿using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using LOR_BattleUnit_UI;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class SpeedDiceColorPatchWithPattyMod
    {
        [HarmonyPostfix]
        [HarmonyAfter("Patty_SpeedDiceColorChange_MOD")]
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
    }
}