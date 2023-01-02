using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using CustomMapUtility;
using UnityEngine;

namespace BigDLL4221.Utils
{
#pragma warning disable
    public static class MapUtil
    {
        public static void ActiveCreatureBattleCamFilterComponent(bool value = true)
        {
            var battleCamera = SingletonBehavior<BattleCamManager>.Instance._effectCam;
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
            var mapList = BattleSceneRoot.Instance._addedMapList;
            if (removeAll)
                mapList?.Clear();
            else
                mapList?.RemoveAll(x => x.name.Contains(name));
        }

        public static void MapChangedValue(bool value)
        {
            Singleton<StageController>.Instance._mapChanged = value;
        }

        public static bool ChangeMap(CustomMapHandler cmh, MapModel model, Faction faction = Faction.Player)
        {
            if (CheckStageMap(model.OriginalMapStageIds) || SingletonBehavior<BattleSceneRoot>
                    .Instance.currentMapObject.isEgo ||
                Singleton<StageController>.Instance.GetStageModel().ClassInfo.stageType == StageType.Creature)
                return false;
            typeof(CustomMapHandler).GetMethods().FirstOrDefault(info =>
                    info.Name == "InitCustomMap" && info.IsGenericMethod && info.GetParameters().Length > 8)
                .MakeGenericMethod(model.Component).Invoke(cmh,
                    new object[]
                    {
                        model.Stage, model.IsPlayer, model.InitBgm, model.Bgx,
                        model.Bgy, model.Fx, model.Fy, model.UnderX, model.UnderY
                    });
            if (model.IsPlayer && !model.OneTurnEgo)
            {
                typeof(CustomMapHandler).GetMethod("ChangeToCustomEgoMapByAssimilation")
                    .MakeGenericMethod(model.Component).Invoke(cmh, new object[]
                    {
                        model.Stage, faction
                    });
                return true;
            }

            typeof(CustomMapHandler).GetMethod("ChangeToCustomEgoMap").MakeGenericMethod(model.Component).Invoke(cmh,
                new object[]
                {
                    model.Stage, faction, false
                });
            MapChangedValue(true);
            return true;
        }

        public static void InitSephirahMap(string packageId, MapModel model, SephirahType sephirah)
        {
            var cmh = CustomMapHandler.GetCMU(packageId);
            typeof(CMUExtensions).GetMethods()
                .FirstOrDefault(info =>
                    info.Name == "InitCustomSephirahMap" && info.IsGenericMethod && info.GetParameters().Length > 9)
                .MakeGenericMethod(model.Component).Invoke(cmh,
                    new object[]
                    {
                        cmh, sephirah, model.Stage, false, model.InitBgm, model.Bgx,
                        model.Bgy, model.Fx, model.Fy, model.UnderX, model.UnderY
                    });
        }

        public static void ChangeToSephirahMap(string packageId, MapModel model, SephirahType sephirah)
        {
            Debug.LogError("Entry Changing Sephirah");
            var cmh = CustomMapHandler.GetCMU(packageId);
            typeof(CMUExtensions).GetMethod("ChangeToCustomSephirahMap")
                .MakeGenericMethod(model.Component).Invoke(cmh,
                    new object[]
                    {
                        cmh, sephirah, model.Stage, Faction.Player, false
                    });
        }

        public static void ReturnFromEgoMap(CustomMapHandler cmh, string mapName, List<LorId> ids,
            bool isAssimilationMap = false)
        {
            if (CheckStageMap(ids) ||
                Singleton<StageController>.Instance.GetStageModel().ClassInfo.stageType ==
                StageType.Creature) return;
            cmh.RemoveCustomEgoMapByAssimilation(mapName);
            RemoveValueInAddedMap(mapName);
            if (!isAssimilationMap)
            {
                Singleton<StageController>.Instance.CheckMapChange();
                return;
            }

            MapChangedValue(true);
            if (!string.IsNullOrEmpty(Singleton<StageController>.Instance.GetStageModel().GetCurrentMapInfo()))
                cmh.EnforceTheme();
            Singleton<StageController>.Instance.CheckMapChange();
            SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(SingletonBehavior<BattleSceneRoot>
                .Instance.currentMapObject.mapBgm);
            SingletonBehavior<BattleSoundManager>.Instance.CheckTheme();
        }
        //Need to rework it
        //public static void PrepareEnemyMaps(CustomMapHandler cmh, List<MapModel> mapModels)
        //{
        //    foreach (var mapModel in mapModels.Where(x => x != null))
        //        mapModel.PrepareEnemyMap(cmh);
        //}
    }
}