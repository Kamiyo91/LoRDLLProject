﻿using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigDLL4221.Utils
{
    public static class UnitLimitUtil
    {
        public static void AddFormationPosition(FormationModel Formation)
        {
            var list = Formation._postionList;
            if (list == null) return;
            for (var i = list.Count; i < 99; i++)
            {
                var info = new FormationPositionXmlData
                {
                    name = "E" + i,
                    vector = new XmlVector2
                    {
                        x = GetVector2X(i - 4),
                        y = GetVector2Y(i - 4)
                    },
                    eventList = null
                };
                var item = new FormationPosition(info)
                {
                    eventList = new List<FormationPositionEvent>(),
                    index = i
                };
                list.Add(item);
            }
        }

        public static int GetVector2X(int i)
        {
            int result;
            switch (i)
            {
                case 1:
                    result = 12;
                    break;
                case 2:
                    result = 12;
                    break;
                case 3:
                    result = 9;
                    break;
                case 4:
                    result = 9;
                    break;
                case 5:
                    result = 8;
                    break;
                case 6:
                    result = 8;
                    break;
                case 7:
                    result = 21;
                    break;
                case 8:
                    result = 21;
                    break;
                case 9:
                    result = 20;
                    break;
                case 10:
                    result = 20;
                    break;
                case 11:
                    result = 2;
                    break;
                case 12:
                    result = 2;
                    break;
                case 13:
                    result = 22;
                    break;
                case 14:
                    result = 22;
                    break;
                case 15:
                    result = 22;
                    break;
                default:
                    result = 12;
                    break;
            }

            return result;
        }

        public static int GetVector2Y(int i)
        {
            int result;
            switch (i)
            {
                case 1:
                    result = 7;
                    break;
                case 2:
                    result = -9;
                    break;
                case 3:
                    result = -5;
                    break;
                case 4:
                    result = -15;
                    break;
                case 5:
                    result = 19;
                    break;
                case 6:
                    result = 9;
                    break;
                case 7:
                    result = 19;
                    break;
                case 8:
                    result = 9;
                    break;
                case 9:
                    result = -5;
                    break;
                case 10:
                    result = -15;
                    break;
                case 11:
                    result = -14;
                    break;
                case 12:
                    result = 14;
                    break;
                case 13:
                    result = -16;
                    break;
                case 14:
                    result = 0;
                    break;
                case 15:
                    result = 16;
                    break;
                default:
                    result = 0;
                    break;
            }

            return result;
        }

        public static void AddFormationPositionForEnemy(FormationModel Formation)
        {
            var list = Formation._postionList;
            var num = -23;
            var num2 = 18;
            if (list == null) return;
            for (var i = list.Count; i < 99; i++)
            {
                var info = new FormationPositionXmlData
                {
                    name = "E" + i,
                    vector = new XmlVector2
                    {
                        x = num,
                        y = num2
                    },
                    eventList = null
                };
                num += 5;
                if (num > -3)
                {
                    num2 -= 7;
                    num = -23;
                }

                if (num2 < -17)
                {
                    num = -12;
                    num2 = 0;
                }

                var item = new FormationPosition(info)
                {
                    eventList = new List<FormationPositionEvent>(),
                    index = i
                };
                list.Add(item);
            }
        }

        public static void AddIndexes(ICollection<int> indexes, int targetCount)
        {
            var sortedIndexes = new List<int>(indexes);
            sortedIndexes.Sort();
            var i = 0;
            for (var j = 0; indexes.Count < targetCount;)
                if (i < sortedIndexes.Count && j == sortedIndexes[i])
                {
                    i++;
                }
                else
                {
                    indexes.Add(j);
                    j++;
                }
        }

        public static int UICharacterRenderer_SetCharacter_GetMaxWithoutSkip(UICharacterRenderer renderer)
        {
            var count = renderer.characterList.Count;
            if (count > 11 && SkipCustomizationIndex()) count--;
            return count;
        }

        public static int UICharacterRenderer_SetCharacter_GetIndexWithSkip(int index)
        {
            if (index >= 10 && SkipCustomizationIndex()) index++;
            return index;
        }

        public static bool SkipCustomizationIndex()
        {
            return StageController.Instance.State == StageController.StageState.Battle &&
                   GameSceneManager.Instance.battleScene.gameObject.activeSelf;
        }

        public static List<BattleUnitModel> BattleEmotionCoinUI_Init_Helper(List<BattleUnitModel> unitList,
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

        public static void SetActiveCharacters()
        {
            var unitBattleDataList =
                Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetUnitBattleDataList();
            foreach (var unitBattleDataModel in unitBattleDataList) unitBattleDataModel.IsAddedBattle = false;
            var num = 0;
            var num2 = 0;
            while (num < Singleton<StageController>.Instance.GetCurrentWaveModel().AvailableUnitNumber &&
                   num2 < unitBattleDataList.Count)
            {
                if (!unitBattleDataList[num2].unitData.IsLockUnit() && !unitBattleDataList[num2].isDead)
                {
                    unitBattleDataList[num2].IsAddedBattle = true;
                    num++;
                }

                num2++;
            }
        }

        public static void UICharacterRenderer_EnsureCounts(UICharacterRenderer __instance, int index)
        {
            try
            {
                var num = Math.Max(199, index + 1);
                while (__instance.characterList.Count < num) __instance.characterList.Add(new UICharacter(null, null));
                while (__instance.cameraList.Count <= index + 1)
                {
                    var camera = Object.Instantiate(__instance.cameraList[0],
                        __instance.cameraList[0].transform.parent);
                    camera.name = "[Camera]" + (index + 1);
                    camera.targetTexture = Object.Instantiate(__instance.cameraList[0].targetTexture);
                    camera.targetTexture.name = "RT_Character_" + (index + 1);
                    camera.transform.position += new Vector3(10f * index, 0f, 0f);
                    __instance.cameraList.Add(camera);
                }
            }
            catch
            {
                // Ignored
            }
        }
    }
}