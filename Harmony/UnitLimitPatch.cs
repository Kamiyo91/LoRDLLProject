using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleCharacterProfile;
using BigDLL4221.Utils;
using HarmonyLib;
using UI;
using UnityEngine;
using Workshop;
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
                __instance._formationIndex = new List<int>();
                for (var i = 0; i < 99; i++) __instance._formationIndex.Add(i);
                __instance._formationIndex[0] = 1;
                __instance._formationIndex[1] = 0;
                for (var j = 3; j < 99; j++) __instance._formationIndex[j] = j;
                __instance._defaultFormation = new FormationModel(Singleton<FormationXmlList>.Instance.GetData(1));
                UnitLimitUtil.AddFormationPosition(__instance._defaultFormation);
                __instance._formation = __instance._defaultFormation;
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
                for (var i = 5; i < 100; i++) __instance._formationIndex.Add(i);
                UnitLimitUtil.AddFormationPositionForEnemy(__instance._formation);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        [HarmonyPatch(typeof(StageWaveModel), "GetUnitBattleDataListByFormation")]
        [HarmonyPrefix]
        private static bool StageWaveModel_GetUnitBattleDataListByFormation_Pre(StageWaveModel __instance,
            ref List<UnitBattleDataModel> __result)
        {
            try
            {
                var list = new List<UnitBattleDataModel>();
                var num = Math.Max(__instance._unitList.Count, 5);
                for (var i = 0; i < num; i++)
                {
                    var formationIndex = __instance.GetFormationIndex(i);
                    if (formationIndex < __instance._unitList.Count) list.Add(__instance._unitList[formationIndex]);
                }

                __result = list;
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
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

        [HarmonyPatch(typeof(UICharacterRenderer), "SetCharacter")]
        [HarmonyPostfix]
        private static void UICharacterRenderer_SetCharacter(UICharacterRenderer __instance, UnitDataModel unit,
            int index, bool forcelyReload = false)
        {
            if (__instance.characterList.Capacity < 199)
            {
                var list = new List<UICharacter>(199);
                list.AddRange(__instance.characterList);
                while (list.Count < 199) list.Add(new UICharacter(null, null));
                __instance.characterList = list;
            }

            while (__instance.cameraList.Count <= index + 1)
            {
                var camera = Object.Instantiate(__instance.cameraList[0], __instance.cameraList[0].transform.parent);
                camera.name = "[Camera]" + (index + 1);
                camera.targetTexture = Object.Instantiate(__instance.cameraList[0].targetTexture);
                camera.targetTexture.name = "RT_Character_" + (index + 1);
                camera.transform.position += new Vector3(10f * index, 0f, 0f);
                __instance.cameraList.Add(camera);
            }

            if (index < 11) return;
            unit.textureIndex = index;
            if (Singleton<StageController>.Instance.State == StageController.StageState.Battle &&
                GameSceneManager.Instance.battleScene.gameObject.activeSelf) unit.textureIndex++;
            var customBookItem = unit.CustomBookItem;
            var uicharacter = __instance.characterList[index];
            if (forcelyReload || (uicharacter.unitAppearance != null && uicharacter.unitModel != unit))
            {
                if (uicharacter.unitAppearance != null)
                {
                    Singleton<AssetBundleManagerRemake>.Instance.ReleaseSdObject(
                        uicharacter.unitAppearance.resourceName);
                    if (uicharacter.unitAppearance.gameObject != null)
                        Object.Destroy(uicharacter.unitAppearance.gameObject);
                }

                uicharacter.unitAppearance = null;
                uicharacter.unitModel = null;
                if (uicharacter.resName != "")
                    Singleton<AssetBundleManagerRemake>.Instance.ReleaseSdObject(uicharacter.resName);

                uicharacter.resName = "";
            }

            if (uicharacter.unitAppearance != null) return;
            var num = 10 * index;
            var characterName = customBookItem.GetCharacterName();
            try
            {
                var isWorkshopSkin = false;
                var num2 = 0;
                if (!string.IsNullOrEmpty(unit.workshopSkin))
                    num2 = 1;
                else if (unit.bookItem.IsWorkshop) num2 = customBookItem.ClassInfo.skinType == "Custom" ? 2 : 0;

                switch (num2)
                {
                    case 1:
                    {
                        var workshopSkinData =
                            Singleton<CustomizingResourceLoader>.Instance.GetWorkshopSkinData(unit.workshopSkin);
                        var gameObject = (GameObject)Resources.Load("Prefabs/Characters/[Prefab]Appearance_Custom");
                        uicharacter.unitModel = unit;
                        uicharacter.unitAppearance = Object.Instantiate(gameObject, __instance.characterRoot)
                            .GetComponent<CharacterAppearance>();
                        uicharacter.unitAppearance.transform.localPosition = new Vector3(num, -2f, 10f);
                        uicharacter.unitAppearance.GetComponent<WorkshopSkinDataSetter>().SetData(workshopSkinData);
                        isWorkshopSkin = true;
                        break;
                    }
                    case 2:
                    {
                        var workshopBookSkinData =
                            Singleton<CustomizingBookSkinLoader>.Instance.GetWorkshopBookSkinData(
                                unit.bookItem.BookId.packageId, unit.bookItem.ClassInfo.GetCharacterSkin());
                        var gameObject = (GameObject)Resources.Load("Prefabs/Characters/[Prefab]Appearance_Custom");
                        uicharacter.unitModel = unit;
                        uicharacter.unitAppearance = Object.Instantiate(gameObject, __instance.characterRoot)
                            .GetComponent<CharacterAppearance>();
                        uicharacter.unitAppearance.transform.localPosition = new Vector3(num, -2f, 10f);
                        var component = uicharacter.unitAppearance.GetComponent<WorkshopSkinDataSetter>();
                        if (component != null && workshopBookSkinData != null) component.SetData(workshopBookSkinData);

                        break;
                    }
                    default:
                    {
                        GameObject gameObject;
                        var text = "";
                        switch (unit.gender)
                        {
                            case Gender.Creature:
                                text = characterName;
                                gameObject = Singleton<AssetBundleManagerRemake>.Instance.LoadSdPrefab(text);
                                break;
                            case Gender.EGO:
                                text = "[Prefab]" + characterName;
                                gameObject = Singleton<AssetBundleManagerRemake>.Instance.LoadSdPrefab(text);
                                break;
                            default:
                                string s;
                                switch (unit.appearanceType)
                                {
                                    case Gender.F:
                                        s = "_F";
                                        break;
                                    case Gender.M:
                                        s = "_M";
                                        break;
                                    default:
                                        s = "_N";
                                        break;
                                }

                                gameObject =
                                    Singleton<AssetBundleManagerRemake>.Instance.LoadCharacterPrefab_DefaultMotion(
                                        characterName, s, out text);
                                break;
                        }

                        if (gameObject != null)
                        {
                            uicharacter.unitModel = unit;
                            uicharacter.unitAppearance = Object.Instantiate(gameObject, __instance.characterRoot)
                                .GetComponent<CharacterAppearance>();
                            uicharacter.unitAppearance.transform.localPosition = new Vector3(num, -2f, 10f);
                            uicharacter.resName = text;
                        }

                        break;
                    }
                }

                var unitAppearance = uicharacter.unitAppearance;
                unitAppearance?.Initialize("");
                var unitAppearance2 = uicharacter.unitAppearance;
                unitAppearance2?.InitCustomData(unit.customizeData, unit.defaultBook.GetBookClassInfoId());
                var unitAppearance3 = uicharacter.unitAppearance;
                unitAppearance3?.InitGiftDataAll(unit.giftInventory.GetEquippedList());
                var unitAppearance4 = uicharacter.unitAppearance;
                unitAppearance4?.ChangeMotion(ActionDetail.Standing);
                var unitAppearance5 = uicharacter.unitAppearance;
                unitAppearance5?.ChangeLayer("CharacterAppearance_UI");
                if (isWorkshopSkin) uicharacter.unitAppearance.GetComponent<WorkshopSkinDataSetter>().LateInit();
                if (unit.EnemyUnitId != -1)
                    if (uicharacter.unitAppearance != null)
                    {
                        var transform = uicharacter.unitAppearance.cameraPivot;
                        if (transform != null)
                        {
                            var characterMotion = uicharacter.unitAppearance.GetCharacterMotion(ActionDetail.Standing);
                            if (characterMotion != null)
                            {
                                var b = characterMotion.transform.position - transform.position;
                                b.z = 0f;
                                characterMotion.transform.localPosition += b;
                            }
                        }
                    }

                __instance.StartCoroutine(UnitLimitUtil.RenderCam_2(unit.textureIndex, __instance));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }


        [HarmonyPatch(typeof(UICharacterRenderer), "GetRenderTextureByIndexAndSize")]
        [HarmonyPrefix]
        private static bool UICharacterRenderer_GetRenderTextureByIndexAndSize_Pre(UICharacterRenderer __instance,
            ref Texture __result, int index)
        {
            try
            {
                var v = Vector2.one;
                var uicharacter = __instance.characterList[index];
                if (uicharacter.unitModel != null)
                {
                    var d = (float)uicharacter.unitModel.customizeData.height;
                    if (uicharacter.unitAppearance != null)
                    {
                        v = Vector2.one * d * 0.005f;
                        uicharacter.unitAppearance.transform.localScale = v;
                    }

                    __result = __instance.cameraList[index].targetTexture;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
        }

        [HarmonyPatch(typeof(UICharacterRenderer), "GetRenderTextureByIndex")]
        [HarmonyPrefix]
        private static bool UICharacterRenderer_GetRenderTextureByIndex_Pre(UICharacterRenderer __instance,
            ref Texture __result, int index)
        {
            try
            {
                if (index < __instance.characterList.Count)
                {
                    var uicharacter = __instance.characterList[index];
                    if (uicharacter.unitAppearance != null)
                        uicharacter.unitAppearance.transform.localScale = Vector2.one;
                    __result = __instance.cameraList[index].targetTexture;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
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
                    AccessTools.Method(typeof(UnitLimitPatch), nameof(BattleEmotionCoinUI_Init_Helper)));
            }
        }

        private static List<BattleUnitModel> BattleEmotionCoinUI_Init_Helper(List<BattleUnitModel> unitList,
            BattleEmotionCoinUI __instance)
        {
            var allyDirection = StageController.Instance.AllyFormationDirection;
            var enemyDataCount = allyDirection == Direction.RIGHT
                ? __instance.enermy.Length
                : __instance.librarian.Length;
            var allyDataCount = allyDirection == Direction.RIGHT
                ? __instance.librarian.Length
                : __instance.enermy.Length;
            var allyUnits = 0;
            var enemyUnits = 0;
            var filteredList = unitList.Where(model =>
                model.faction == Faction.Enemy ? enemyUnits++ < enemyDataCount : allyUnits++ < allyDataCount).ToList();
            unitList.Clear();
            unitList.AddRange(filteredList);
            return unitList;
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
    }
}