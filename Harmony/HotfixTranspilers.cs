using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class HotfixTranspilers
    {
        //Cya Hotfix for UI Limits
        [HarmonyPatch(typeof(BattleUnitInformationUI_BuffList), nameof(BattleUnitInformationUI_BuffList.SetData))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BattleUnitInformationUI_BuffList__SetData__Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.PropertyGetter(typeof(List<BattleUnitInformationUI_BuffSlot>),
                nameof(List<BattleUnitInformationUI_BuffSlot>.Count));
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
                if ((codes[i].opcode == OpCodes.Ble_S || codes[i].opcode == OpCodes.Ble) &&
                    codes[i - 1].Is(OpCodes.Callvirt, method))
                    yield return new CodeInstruction(OpCodes.Blt, codes[i].operand);
                else
                    yield return codes[i];
        }

        //Cya Hotfix for Keywords Limits
        [HarmonyPatch(typeof(KeywordListUI), nameof(KeywordListUI.Init))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> KeywordListUI_Init_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var ineq = AccessTools.Method(typeof(string), "op_Inequality");
            var keylist = AccessTools.Field(typeof(KeywordListUI), nameof(KeywordListUI.keywordList));
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!codes[i].Is(OpCodes.Call, ineq) || !codes[i - 1].Is(OpCodes.Ldstr, "")) continue;
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, keylist);
                yield return new CodeInstruction(OpCodes.Ldlen);
                yield return new CodeInstruction(OpCodes.Conv_I4);
                yield return new CodeInstruction(OpCodes.Clt);
                yield return new CodeInstruction(OpCodes.And);
            }
        }

        //remove error logs about saved formations being too big
        [HarmonyPatch(typeof(LibraryFloorModel), nameof(LibraryFloorModel.LoadFromSaveData))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> LibraryFloorModel_LoadFromSaveData_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.Method(typeof(Debug), nameof(Debug.Log), new[] { typeof(object) });
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count - 2; i++)
            {
                if (!codes[i].Is(OpCodes.Ldstr, "formation index length is too high") ||
                    !codes[i + 1].Is(OpCodes.Call, method)) continue;
                codes.RemoveRange(i, 2);
                break;
            }

            return codes;
        }
    }
}