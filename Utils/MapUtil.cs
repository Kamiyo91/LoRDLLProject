using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.StageManagers;
using CustomMapUtility;

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
            return Singleton<StageController>.Instance.EnemyStageManager is
                EnemyTeamStageManager_RushBattleWithCMUOnly_DLL4221 || (ModParameters.PackageIds.Contains(
                                                                            Singleton<StageController>.Instance
                                                                                .GetStageModel().ClassInfo.id
                                                                                .packageId) &&
                                                                        ids.Contains(Singleton<StageController>.Instance
                                                                            .GetStageModel().ClassInfo.id));
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
            return (bool)typeof(MapUtil).GetMethod("ChangeMapGeneric").MakeGenericMethod(model.Component)
                .Invoke(model, new object[] { cmh, model, faction });
        }

        public static bool ChangeMapGeneric<T>(CustomMapHandler cmh, MapModel model,
            Faction faction = Faction.Player) where T : MapManager, ICMU, new()
        {
            if (CheckStageMap(model.OriginalMapStageIds) || SingletonBehavior<BattleSceneRoot>
                    .Instance.currentMapObject.isEgo ||
                Singleton<StageController>.Instance.GetStageModel().ClassInfo.stageType == StageType.Creature)
                return false;
            cmh.InitCustomMap<T>(model.Stage, model.IsPlayer, model.InitBgm, model.Bgx,
                model.Bgy, model.Fx, model.Fy, model.UnderX, model.UnderY);
            if (model.IsPlayer && !model.OneTurnEgo)
            {
                cmh.ChangeToCustomEgoMapByAssimilation<T>(model.Stage, faction);
                return true;
            }

            cmh.ChangeToCustomEgoMap<T>(model.Stage, faction);
            MapChangedValue(true);
            return true;
        }

        public static void InitSephirahMap(string packageId, MapModel model, SephirahType sephirah)
        {
            typeof(MapUtil).GetMethod("InitSephirahMapGeneric").MakeGenericMethod(model.Component)
                .Invoke(model, new object[] { packageId, model, sephirah });
        }

        public static void InitSephirahMapGeneric<T>(string packageId, MapModel model, SephirahType sephirah)
            where T : MapManager, ICMU, new()
        {
            var cmh = CustomMapHandler.GetCMU(packageId);
            cmh.InitCustomSephirahMap<T>(sephirah, model.Stage, false, model.InitBgm, model.Bgx,
                model.Bgy, model.Fx, model.Fy, model.UnderX, model.UnderY);
        }

        public static void ChangeToSephirahMap(string packageId, MapModel model, SephirahType sephirah, bool playEffect)
        {
            typeof(MapUtil).GetMethod("ChangeToSephirahMapGeneric").MakeGenericMethod(model.Component)
                .Invoke(model, new object[] { packageId, model, sephirah, playEffect });
        }

        public static void ChangeToSephirahMapGeneric<T>(string packageId, MapModel model,
            SephirahType sephirah, bool playEffect) where T : MapManager, ICMU, new()
        {
            var cmh = CustomMapHandler.GetCMU(packageId);
            cmh.ChangeToCustomSephirahMap<T>(sephirah, model.Stage, Faction.Player, false, playEffect);
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

        public static void PrepareEnemyMaps(CustomMapHandler cmh, List<MapModel> mapModels)
        {
            foreach (var mapModel in mapModels.Where(x => x != null))
                typeof(MapUtil).GetMethod("InitEnemyMap").MakeGenericMethod(mapModel.Component)
                    .Invoke(mapModel, new object[] { cmh, mapModel });
        }

        public static void PrepareEnemyMapsMultiCmu(Dictionary<string, List<MapModel>> mapModels)
        {
            foreach (var item in mapModels.Where(x => !string.IsNullOrEmpty(x.Key)))
            {
                var cmh = CustomMapHandler.GetCMU(item.Key);
                foreach (var mapModel in item.Value)
                    typeof(MapUtil).GetMethod("InitEnemyMap").MakeGenericMethod(mapModel.Component)
                        .Invoke(mapModel, new object[] { cmh, mapModel });
            }
        }

        public static void InitEnemyMap<T>(CustomMapHandler cmh, MapModel model)
            where T : MapManager, ICMU, new()
        {
            cmh.InitCustomMap<T>(model.Stage, false, true, model.Bgx,
                model.Bgy, model.Fx, model.Fy, model.UnderX, model.UnderY);
        }
    }
}