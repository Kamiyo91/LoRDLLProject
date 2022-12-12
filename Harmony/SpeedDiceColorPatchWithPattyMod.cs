using HarmonyLib;
using LOR_BattleUnit_UI;
using UnityEngine;

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
            Debug.LogError("Test");
        }
    }
}