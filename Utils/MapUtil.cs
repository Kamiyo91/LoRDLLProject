using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using HarmonyLib;
using UnityEngine;

namespace BigDLL4221.Utils
{
#pragma warning disable
    public static class MapUtil
    {
        public static void ActiveCreatureBattleCamFilterComponent(bool value = true)
        {
            var battleCamera = (Camera)typeof(BattleCamManager).GetField("_effectCam",
                AccessTools.all)?.GetValue(SingletonBehavior<BattleCamManager>.Instance);
            if (!(battleCamera is null)) battleCamera.GetComponent<CameraFilterPack_Drawing_Paper3>().enabled = value;
        }

        public static bool CheckStageMap(List<LorId> ids)
        {
            return ModParameters.PackageIds.Contains(Singleton<StageController>.Instance.GetStageModel().ClassInfo.id
                       .packageId) &&
                   ids.Contains(Singleton<StageController>.Instance.GetStageModel().ClassInfo.id);
        }

        public static void RemoveValueInAddedMap(string name, bool removeAll = false)
        {
            var mapList = (List<MapManager>)typeof(BattleSceneRoot).GetField("_addedMapList",
                AccessTools.all)?.GetValue(SingletonBehavior<BattleSceneRoot>.Instance);
            if (removeAll)
                mapList?.Clear();
            else
                mapList?.RemoveAll(x => x.name.Contains(name));
        }

        public static void MapChangedValue(bool value)
        {
            typeof(StageController).GetField("_mapChanged", AccessTools.all)
                ?.SetValue(Singleton<StageController>.Instance, value);
        }

        public static void ChangeMap(MapModel model, Faction faction = Faction.Player)
        {
            if (CheckStageMap(model.OriginalMapStageIds) || SingletonBehavior<BattleSceneRoot>
                    .Instance.currentMapObject.isEgo ||
                Singleton<StageController>.Instance.GetStageModel().ClassInfo.stageType == StageType.Creature) return;
            CustomMapHandler.InitCustomMap(model.Stage, model.Component, model.IsPlayer, model.InitBgm, model.Bgx,
                model.Bgy, model.Fx, model.Fy);
            if (model.IsPlayer && !model.OneTurnEgo)
            {
                CustomMapHandler.ChangeToCustomEgoMapByAssimilation(model.Stage, faction);
                return;
            }

            CustomMapHandler.ChangeToCustomEgoMap(model.Stage, faction);
            MapChangedValue(true);
        }

        public static void ReturnFromEgoMap(string mapName, List<LorId> ids, bool isAssimilationMap = false)
        {
            if (CheckStageMap(ids) ||
                Singleton<StageController>.Instance.GetStageModel().ClassInfo.stageType ==
                StageType.Creature) return;
            CustomMapHandler.RemoveCustomEgoMapByAssimilation(mapName);
            RemoveValueInAddedMap(mapName);
            if (!isAssimilationMap)
            {
                Singleton<StageController>.Instance.CheckMapChange();
                return;
            }

            MapChangedValue(true);
            if (!string.IsNullOrEmpty(Singleton<StageController>.Instance.GetStageModel().GetCurrentMapInfo()))
                CustomMapHandler.EnforceTheme();
            Singleton<StageController>.Instance.CheckMapChange();
            SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(SingletonBehavior<BattleSceneRoot>
                .Instance.currentMapObject.mapBgm);
            SingletonBehavior<BattleSoundManager>.Instance.CheckTheme();
        }

        public static void PrepareEnemyMaps(List<MapModel> mapModels)
        {
            foreach (var mapModel in mapModels.Where(x => x != null))
                CustomMapHandler.InitCustomMap(mapModel.Stage, mapModel.Component, false, true, mapModel.Bgx,
                    mapModel.Bgy, mapModel.Fx, mapModel.Fy);
        }
    }
}