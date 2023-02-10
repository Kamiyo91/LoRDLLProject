using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BattleCharacterProfile;
using BigDLL4221.Utils;
using HarmonyLib;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class UnitLimitPatch
    {
        private const int MIN_EMOTION_SLOTS = 9;
        private const float Y_SHIFT = 64f;

        [HarmonyPatch(typeof(UIBattleSettingWaveList), "SetData")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        public static void UIBattleSettingWaveList_SetData(UIBattleSettingWaveList __instance, StageModel stage)
        {
            try
            {
                while (stage.waveList.Count > __instance.waveSlots.Count)
                    __instance.waveSlots.Add(__instance.waveSlots[__instance.waveSlots.Count - 1]);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        [HarmonyPatch(typeof(LibraryFloorModel), "Init")]
        [HarmonyPostfix]
        private static void LibraryFloorModel_Init_Post(LibraryFloorModel __instance)
        {
            try
            {
                UnitLimitUtil.AddIndexes(__instance._formationIndex, 99);
                UnitLimitUtil.AddFormationPosition(__instance._defaultFormation);
                if (__instance._formation == null)
                    __instance._formation = __instance._defaultFormation;
                else
                    UnitLimitUtil.AddFormationPosition(__instance._formation);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        [HarmonyPatch(typeof(StageWaveModel), "Init")]
        [HarmonyPostfix]
        private static void StageWaveModel_Init_Post(StageWaveModel __instance)
        {
            try
            {
                UnitLimitUtil.AddIndexes(__instance._formationIndex, 100);
                UnitLimitUtil.AddFormationPositionForEnemy(__instance._formation);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        [HarmonyPatch(typeof(StageWaveModel), nameof(StageWaveModel.GetUnitBattleDataListByFormation))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> StageWaveModel_GetUnitBattleDataListByFormation_In(
            IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode != OpCodes.Ldc_I4_5) continue;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(StageWaveModel), nameof(StageWaveModel._unitList)));
                yield return new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertyGetter(typeof(List<UnitBattleDataModel>),
                        nameof(List<UnitBattleDataModel>.Count)));
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Math), nameof(Math.Max), new[] { typeof(int), typeof(int) }));
            }
        }


        [HarmonyPatch(typeof(BattleUnitInfoManagerUI), "UpdateCharacterProfile")]
        [HarmonyPostfix]
        private static void BattleUnitInfoManagerUI_UpdateCharacterProfile_Post(BattleUnitModel unit, float hp, int bp,
            BattleBufUIDataList bufDataList = null)
        {
            try
            {
                unit.view.unitBottomStatUI.UpdateStatUI(hp, bp, bufDataList);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        [HarmonyPatch(typeof(BattleUnitInfoManagerUI), nameof(BattleUnitInfoManagerUI.Initialize))]
        [HarmonyPrefix]
        private static void BattleUnitInfoManagerUI_Initialize_Pre(BattleUnitInfoManagerUI __instance)
        {
            var lastAlly = __instance.allyProfileArray.Length - 1;
            if (lastAlly < MIN_EMOTION_SLOTS - 1)
            {
                var allyProfileArray2 = new List<BattleCharacterProfileUI>(__instance.allyProfileArray);
                for (var i = lastAlly + 1; i < MIN_EMOTION_SLOTS; i++)
                {
                    allyProfileArray2.Add(Object.Instantiate(allyProfileArray2[lastAlly],
                        allyProfileArray2[lastAlly].transform.parent));
                    allyProfileArray2[i].transform.localPosition = allyProfileArray2[lastAlly].transform.localPosition +
                                                                   new Vector3(0f, (i - lastAlly) * Y_SHIFT, 0f);
                }

                __instance.allyProfileArray = allyProfileArray2.ToArray();
            }

            var lastEnemy = __instance.enemyProfileArray.Length - 1;
            if (lastEnemy >= MIN_EMOTION_SLOTS - 1) return;
            var enemyProfileArray2 = new List<BattleCharacterProfileUI>(__instance.enemyProfileArray);
            for (var i = lastEnemy + 1; i < MIN_EMOTION_SLOTS; i++)
            {
                enemyProfileArray2.Add(Object.Instantiate(enemyProfileArray2[lastEnemy],
                    enemyProfileArray2[lastEnemy].transform.parent));
                enemyProfileArray2[i].transform.localPosition = enemyProfileArray2[lastEnemy].transform.localPosition +
                                                                new Vector3(0f, (i - lastEnemy) * Y_SHIFT, 0f);
            }

            __instance.enemyProfileArray = enemyProfileArray2.ToArray();
        }

        [HarmonyPatch(typeof(BattleUnitInfoManagerUI), nameof(BattleUnitInfoManagerUI.Initialize))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void BattleUnitInfoManagerUI_Initialize_Pre2(BattleUnitInfoManagerUI __instance,
            ref IList<BattleUnitModel> unitList)
        {
            var allyDirection = StageController.Instance.AllyFormationDirection;
            var enemyProfilesCount = allyDirection == Direction.RIGHT
                ? __instance.enemyProfileArray.Length
                : __instance.allyProfileArray.Length;
            var allyProfilesCount = allyDirection == Direction.RIGHT
                ? __instance.allyProfileArray.Length
                : __instance.enemyProfileArray.Length;
            unitList = unitList.Where(model =>
                    model.faction == Faction.Enemy ? model.index < enemyProfilesCount : model.index < allyProfilesCount)
                .ToList();
        }

        [HarmonyPatch(typeof(BattleEmotionCoinUI), nameof(BattleEmotionCoinUI.Init))]
        [HarmonyPrefix]
        private static void BattleEmotionCoinUI_Init_Pre(BattleEmotionCoinUI __instance)
        {
            var unitList = BattleManagerUI.Instance.ui_unitListInfoSummary;
            var lastLibrarian = __instance.librarian.Length - 1;
            if (lastLibrarian < MIN_EMOTION_SLOTS - 1)
            {
                var librarian = new List<BattleEmotionCoinUI.BattleEmotionCoinData>(__instance.librarian);
                for (var i = lastLibrarian + 1; i < MIN_EMOTION_SLOTS; i++)
                {
                    librarian.Add(new BattleEmotionCoinUI.BattleEmotionCoinData
                    {
                        cosFactor = 1f,
                        sinFactor = 1f,
                        target = Object.Instantiate(librarian[lastLibrarian].target,
                            librarian[lastLibrarian].target.parent)
                    });
                    librarian[i].target.localPosition = librarian[lastLibrarian].target.localPosition +
                                                        unitList.allyProfileArray[i].transform.localPosition -
                                                        unitList.allyProfileArray[lastLibrarian].transform
                                                            .localPosition;
                }

                __instance.librarian = librarian.ToArray();
            }

            var lastEnermy = __instance.enermy.Length - 1;
            if (lastEnermy >= MIN_EMOTION_SLOTS - 1) return;
            var enermy = new List<BattleEmotionCoinUI.BattleEmotionCoinData>(__instance.enermy);
            for (var i = lastEnermy + 1; i < MIN_EMOTION_SLOTS; i++)
            {
                enermy.Add(new BattleEmotionCoinUI.BattleEmotionCoinData
                {
                    cosFactor = 1f,
                    sinFactor = 1f,
                    target = Object.Instantiate(enermy[lastEnermy].target, enermy[lastEnermy].target.parent)
                });
                enermy[i].target.localPosition = enermy[lastEnermy].target.localPosition +
                                                 unitList.enemyProfileArray[i].transform.localPosition -
                                                 unitList.enemyProfileArray[lastEnermy].transform.localPosition;
            }

            __instance.enermy = enermy.ToArray();
        }

        [HarmonyPatch(typeof(BattleEmotionCoinUI), nameof(BattleEmotionCoinUI.Init))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BattleEmotionCoinUI_Init_In(
            IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.Method(typeof(BattleObjectManager), nameof(BattleObjectManager.GetAliveList),
                new[] { typeof(bool) });
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (!instruction.Is(OpCodes.Callvirt, method)) continue;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(UnitLimitUtil), nameof(UnitLimitUtil.BattleEmotionCoinUI_Init_Helper)));
            }
        }

        [HarmonyPatch(typeof(BattleEmotionCoinUI), nameof(BattleEmotionCoinUI.Acquisition))]
        [HarmonyPrefix]
        private static bool BattleEmotionCoinUI_Acquisition_Pre(BattleUnitModel unit)
        {
            return BattleManagerUI.Instance.ui_unitListInfoSummary.GetProfileUI(unit) != null;
        }

        [HarmonyPatch(typeof(BattleEmotionRewardInfoUI), nameof(BattleEmotionRewardInfoUI.SetData))]
        [HarmonyPrefix]
        private static void BattleEmotionRewardInfoUI_SetData_Pre(BattleEmotionRewardInfoUI __instance,
            List<UnitBattleDataModel> units)
        {
            while (units.Count > __instance.slots.Count && __instance.slots.Count < MIN_EMOTION_SLOTS)
            {
                var newUI = Object.Instantiate(__instance.slots[0]);
                __instance.slots.Add(newUI);
            }
        }

        [HarmonyPatch(typeof(BattleEmotionRewardInfoUI), nameof(BattleEmotionRewardInfoUI.SetData))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BattleEmotionRewardInfoUI_SetData_In(
            IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.PropertyGetter(typeof(List<UnitBattleDataModel>),
                nameof(List<UnitBattleDataModel>.Count));
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (!instruction.Is(OpCodes.Callvirt, method)) continue;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(BattleEmotionRewardInfoUI), nameof(BattleEmotionRewardInfoUI.slots)));
                yield return new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertyGetter(typeof(List<BattleEmotionRewardSlotUI>),
                        nameof(List<BattleEmotionRewardSlotUI>.Count)));
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Math), nameof(Math.Min), new[] { typeof(int), typeof(int) }));
            }
        }

        [HarmonyPatch(typeof(UICharacterRenderer), nameof(UICharacterRenderer.GetRenderTextureByIndexAndSize))]
        [HarmonyPatch(typeof(UICharacterRenderer), nameof(UICharacterRenderer.GetRenderTextureByIndex))]
        [HarmonyPatch(typeof(UICharacterRenderer), nameof(UICharacterRenderer.GetUICharacterByIndex))]
        [HarmonyPatch(typeof(UICharacterRenderer), nameof(UICharacterRenderer.Redraw))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UICharacterRenderer_LimitRaisingTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
                if (instruction.LoadsConstant(11L))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(UICharacterRenderer), nameof(UICharacterRenderer.characterList)));
                    yield return new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(List<UICharacter>), nameof(List<UICharacter>.Count)));
                }
                else
                {
                    yield return instruction;
                }
        }

        [HarmonyPatch(typeof(UICharacterRenderer), nameof(UICharacterRenderer.SetCharacter))]
        [HarmonyPrefix]
        private static void UICharacterRenderer_SetCharacter_Prefix(UICharacterRenderer __instance, int index)
        {
            try
            {
                var fixedIndex = UnitLimitUtil.UICharacterRenderer_SetCharacter_GetIndexWithSkip(index);
                var minCharCount = Math.Max(199, fixedIndex);
                while (__instance.characterList.Count < minCharCount)
                    __instance.characterList.Add(new UICharacter(null, null));
                while (__instance.cameraList.Count <= fixedIndex)
                {
                    var camera = Object.Instantiate(__instance.cameraList[0],
                        __instance.cameraList[0].transform.parent);
                    camera.name = "[Camera]" + __instance.cameraList.Count;
                    camera.targetTexture = Object.Instantiate(__instance.cameraList[0].targetTexture);
                    camera.targetTexture.name = "RT_Character_" + __instance.cameraList.Count;
                    camera.transform.position += new Vector3(10f * __instance.cameraList.Count, 0f, 0f);
                    __instance.cameraList.Add(camera);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        [HarmonyPatch(typeof(UICharacterRenderer), nameof(UICharacterRenderer.SetCharacter))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UICharacterRenderer_SetCharacter_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var passedTextureIndexInit = false;
            var storedFixedIndex = false;
            var customBookProperty =
                AccessTools.PropertyGetter(typeof(UnitDataModel), nameof(UnitDataModel.CustomBookItem));
            var textureIndexField = AccessTools.Field(typeof(UnitDataModel), nameof(UnitDataModel.textureIndex));
            var getMaxForCheck = AccessTools.Method(typeof(UnitLimitUtil),
                nameof(UnitLimitUtil.UICharacterRenderer_SetCharacter_GetMaxWithoutSkip));
            var getIndex = AccessTools.Method(typeof(UnitLimitUtil),
                nameof(UnitLimitUtil.UICharacterRenderer_SetCharacter_GetIndexWithSkip));
            foreach (var instruction in instructions)
                if (instruction.LoadsConstant(11L))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, getMaxForCheck);
                }
                else
                {
                    if (instruction.operand as MethodInfo == customBookProperty)
                    {
                        passedTextureIndexInit = true;
                        yield return instruction;
                    }
                    else
                    {
                        if (passedTextureIndexInit)
                        {
                            if (instruction.opcode == OpCodes.Ldarg_2)
                            {
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldfld, textureIndexField);
                            }
                            else
                            {
                                yield return instruction;
                            }
                        }
                        else
                        {
                            yield return instruction;
                            if (!instruction.Is(OpCodes.Stfld, textureIndexField) || storedFixedIndex) continue;
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, getIndex);
                            yield return new CodeInstruction(OpCodes.Stfld, textureIndexField);
                            storedFixedIndex = true;
                        }
                    }
                }
        }
    }
}