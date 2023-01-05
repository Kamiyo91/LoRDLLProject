using System;
using System.Collections.Generic;
using BigDLL4221.Models;
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
        [HarmonyPatch(typeof(UIBattleSettingWaveList), "SetData")]
        [HarmonyPrefix]
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

        [HarmonyPatch(typeof(BattleUnitInfoManagerUI), "Initialize")]
        [HarmonyPrefix]
        private static bool BattleUnitInfoManagerUI_Initialize_Pre(BattleUnitInfoManagerUI __instance,
            IList<BattleUnitModel> unitList)
        {
            try
            {
                if (UnitLimitParameters.AllyProfileArray2.Count < 9)
                {
                    UnitLimitParameters.AllyProfileArray2.Clear();
                    for (var i = 0; i < 9; i++)
                        if (__instance.allyProfileArray.Length > i)
                        {
                            UnitLimitParameters.AllyProfileArray2.Add(__instance.allyProfileArray[i]);
                        }
                        else
                        {
                            UnitLimitParameters.AllyProfileArray2.Add(Object.Instantiate(
                                UnitLimitParameters.AllyProfileArray2[4],
                                UnitLimitParameters.AllyProfileArray2[4].transform.parent));
                            UnitLimitParameters.AllyProfileArray2[i].gameObject.transform.localPosition +=
                                new Vector3(0f, (i - 4) * 64f, 0f);
                        }
                }

                if (UnitLimitParameters.EnemyProfileArray2.Count < 9)
                {
                    UnitLimitParameters.EnemyProfileArray2.Clear();
                    for (var j = 0; j < 9; j++)
                        if (__instance.enemyProfileArray.Length > j)
                        {
                            UnitLimitParameters.EnemyProfileArray2.Add(__instance.enemyProfileArray[j]);
                        }
                        else
                        {
                            UnitLimitParameters.EnemyProfileArray2.Add(Object.Instantiate(
                                UnitLimitParameters.EnemyProfileArray2[4],
                                UnitLimitParameters.EnemyProfileArray2[4].transform.parent));
                            UnitLimitParameters.EnemyProfileArray2[j].gameObject.transform.localPosition +=
                                new Vector3(0f, (j - 4) * 64f, 0f);
                        }
                }

                __instance.allyDirection = Singleton<StageController>.Instance.AllyFormationDirection;
                foreach (var t in __instance.allyProfileArray) t.gameObject.SetActive(false);
                foreach (var t in __instance.enemyProfileArray) t.gameObject.SetActive(false);
                foreach (var t in UnitLimitParameters.AllyProfileArray2) t.gameObject.SetActive(false);
                foreach (var t in UnitLimitParameters.EnemyProfileArray2) t.gameObject.SetActive(false);
                __instance.enemyarray = __instance.allyDirection == Direction.RIGHT
                    ? UnitLimitParameters.EnemyProfileArray2.ToArray()
                    : UnitLimitParameters.AllyProfileArray2.ToArray();
                __instance.allyarray = __instance.allyDirection == Direction.RIGHT
                    ? UnitLimitParameters.AllyProfileArray2.ToArray()
                    : UnitLimitParameters.EnemyProfileArray2.ToArray();
                foreach (var battleUnitModel in unitList)
                {
                    var index = battleUnitModel.index;
                    if (index >= 9) continue;
                    if (battleUnitModel.faction == Faction.Enemy)
                    {
                        __instance.enemyarray[index].gameObject.SetActive(true);
                        __instance.enemyarray[index].Initialize();
                        __instance.enemyarray[index].SetUnitModel(battleUnitModel);
                    }
                    else
                    {
                        __instance.allyarray[index].gameObject.SetActive(true);
                        __instance.allyarray[index].Initialize();
                        __instance.allyarray[index].SetUnitModel(battleUnitModel);
                    }
                }

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

        [HarmonyPatch(typeof(BattleEmotionCoinUI), "Init")]
        [HarmonyPrefix]
        private static bool BattleEmotionCoinUI_Init_Pre(BattleEmotionCoinUI __instance)
        {
            try
            {
                __instance._librarian_lib.Clear();
                __instance._enemy_lib.Clear();
                __instance._lib_queue.Clear();
                __instance._ene_queue.Clear();
                var aliveList = BattleObjectManager.instance.GetAliveList();
                var num = 0;
                var num2 = 0;
                var allyFormationDirection = Singleton<StageController>.Instance.AllyFormationDirection;
                if (UnitLimitParameters.Librarian2.Count < 9)
                {
                    UnitLimitParameters.Librarian2.Clear();
                    for (var i = 0; i < 9; i++)
                        if (__instance.librarian.Length > i)
                        {
                            UnitLimitParameters.Librarian2.Add(__instance.librarian[i]);
                        }
                        else
                        {
                            UnitLimitParameters.Librarian2.Add(new BattleEmotionCoinUI.BattleEmotionCoinData
                            {
                                cosFactor = 1f,
                                sinFactor = 1f,
                                target = Object.Instantiate(__instance.librarian[4].target,
                                    __instance.librarian[4].target)
                            });
                            UnitLimitParameters.Librarian2[i].target.localPosition +=
                                new Vector3(0f, (i - 4) * 64f, 0f);
                        }
                }

                if (UnitLimitParameters.Enemy2.Count < 9)
                {
                    UnitLimitParameters.Enemy2.Clear();
                    for (var j = 0; j < 9; j++)
                        if (__instance.enermy.Length > j)
                        {
                            UnitLimitParameters.Enemy2.Add(__instance.enermy[j]);
                        }
                        else
                        {
                            UnitLimitParameters.Enemy2.Add(new BattleEmotionCoinUI.BattleEmotionCoinData
                            {
                                cosFactor = 1f,
                                sinFactor = 1f,
                                target = Object.Instantiate(__instance.enermy[4].target, __instance.enermy[4].target)
                            });
                            UnitLimitParameters.Enemy2[j].target.localPosition += new Vector3(0f, (j - 4) * 64f, 0f);
                        }
                }

                __instance.librarian = UnitLimitParameters.Librarian2.ToArray();
                __instance.enermy = UnitLimitParameters.Enemy2.ToArray();
                var array = allyFormationDirection == Direction.RIGHT ? __instance.librarian : __instance.enermy;
                var array2 = allyFormationDirection == Direction.RIGHT ? __instance.enermy : __instance.librarian;
                foreach (var battleUnitModel in aliveList)
                    if (battleUnitModel.faction == Faction.Enemy)
                    {
                        if (num2 <= 8) __instance._enemy_lib.Add(battleUnitModel.id, array2[num2++]);
                    }
                    else
                    {
                        if (num <= 8) __instance._librarian_lib.Add(battleUnitModel.id, array[num++]);
                    }

                __instance._init = true;
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
        }

        [HarmonyPatch(typeof(BattleEmotionCoinUI), "Acquisition")]
        [HarmonyPrefix]
        private static bool BattleEmotionCoinUI_Acquisition_Pre(BattleUnitModel unit)
        {
            try
            {
                if (SingletonBehavior<BattleManagerUI>.Instance.ui_unitListInfoSummary.GetProfileUI(unit) == null)
                    return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
        }

        [HarmonyPatch(typeof(UICharacterRenderer), "SetCharacter")]
        [HarmonyPrefix]
        private static bool UICharacterRenderer_SetCharacter_Pre(UICharacterRenderer __instance, UnitDataModel unit,
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

            if (index < 11) return true;
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

            if (uicharacter.unitAppearance != null) return true;
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
                if (unitAppearance != null) unitAppearance.Initialize("");


                var unitAppearance2 = uicharacter.unitAppearance;
                if (unitAppearance2 != null)
                    unitAppearance2.InitCustomData(unit.customizeData, unit.defaultBook.GetBookClassInfoId());


                var unitAppearance3 = uicharacter.unitAppearance;
                if (unitAppearance3 != null) unitAppearance3.InitGiftDataAll(unit.giftInventory.GetEquippedList());


                var unitAppearance4 = uicharacter.unitAppearance;
                if (unitAppearance4 != null) unitAppearance4.ChangeMotion(ActionDetail.Standing);


                var unitAppearance5 = uicharacter.unitAppearance;
                if (unitAppearance5 != null) unitAppearance5.ChangeLayer("CharacterAppearance_UI");

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
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
        }

        [HarmonyPatch(typeof(BattleEmotionRewardInfoUI), "SetData")]
        [HarmonyPrefix]
        private static bool BattleEmotionRewardInfoUI_SetData_Pre(BattleEmotionRewardInfoUI __instance,
            List<UnitBattleDataModel> units, Faction faction)
        {
            try
            {
                while (units.Count > __instance.slots.Count && __instance.slots.Count < 9)
                {
                    var item = Object.Instantiate(__instance.slots[0]);
                    __instance.slots.Add(item);
                }

                foreach (var battleEmotionRewardSlotUI in __instance.slots)
                    battleEmotionRewardSlotUI.gameObject.SetActive(false);
                for (var i = 0; i < units.Count; i++)
                {
                    if (i > 8) break;
                    __instance.slots[i].gameObject.SetActive(true);
                    __instance.slots[i].SetData(units[i], faction);
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
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
    }
}