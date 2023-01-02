using System;
using CustomMapUtility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigDLL4221.Extensions
{
    public static class CMUExtensions
    {
        public static T InitCustomSephirahMap<T>(this CustomMapHandler cmh, SephirahType sephirah,
            string stageName,
            bool isEgo = false)
            where T : MapManager, ICMU, new()
        {
            var initBGMs = true;
            Offsets offsets;
            if (cmh.mapOffsetsCache.TryGetValue(stageName, out offsets))
            {
                initBGMs = cmh.mapAutoBgmCache[stageName];
                Debug.Log("CustomMapUtility: Loaded offsets from cache");
            }
            else
            {
                offsets = new Offsets();
            }

            return new SephirahMapManager<T>(cmh, sephirah).Init(stageName, offsets, isEgo, initBGMs);
        }

        public static T InitCustomSephirahMap<T>(this CustomMapHandler cmh, SephirahType sephirah,
            string stageName,
            bool isEgo = false,
            bool initBGMs = true,
            float bgx = 0.5f,
            float bgy = 0.5f,
            float floorx = 0.5f,
            float floory = 0.3773148f,
            float underx = 0.5f,
            float undery = 0.2777778f)
            where T : MapManager, ICMU, new()
        {
            var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
            return new SephirahMapManager<T>(cmh, sephirah).Init(stageName, offsets, isEgo, initBGMs);
        }

        public static void ChangeToCustomSephirahMap<T>(this CustomMapHandler cmh, SephirahType sephirah,
            string mapName, Faction faction = Faction.Player, bool byAssimilationFlag = false, bool playEffect = false)
            where T : MapManager, ICMU, new()
        {
            var instance = SingletonBehavior<BattleSceneRoot>.Instance;
            if (string.IsNullOrEmpty(mapName))
            {
                Debug.LogError("CustomMapUtility: Sephirah map not specified");
            }
            else
            {
                var addedMapList = instance.mapList;
                var mapChangeFilter = instance._mapChangeFilter;
                var mapManager = addedMapList?.Find(x => x.name.Contains(mapName));
                if (mapManager == null)
                {
                    Debug.LogWarning("CustomMapUtility: Reinitializing Ego map");
                    cmh.InitCustomSephirahMap<T>(sephirah, mapName, false);
                    mapManager = addedMapList?.Find(x => x.name.Contains(mapName));
                }

                if (playEffect)
                    mapChangeFilter.StartMapChangingEffect((Direction)faction);
                if (mapManager == instance.currentMapObject)
                    return;
                instance.currentMapObject.EnableMap(false);
                instance.currentMapObject = mapManager;
                if (!byAssimilationFlag)
                {
                    instance.currentMapObject.ActiveMap(true);
                    instance.currentMapObject.InitializeMap();
                }
                else
                {
                    if (!instance.currentMapObject.IsMapInitialized)
                        instance.currentMapObject.InitializeMap();
                    instance.currentMapObject.EnableMap(true);
                    instance.currentMapObject.PlayMapChangedSound();
                    SingletonBehavior<BattleCamManager>.Instance.SetVignetteColorBgCam(instance.currentMapObject
                        .sephirahColor);
                    foreach (var battleUnitModel in BattleObjectManager.instance.GetList())
                        battleUnitModel.view.ChangeScale(instance.currentMapObject.mapSize);
                }
            }
        }
    }

    public class SephirahMapManager<T> : MapInstance<T> where T : MapManager, ICMU, new()
    {
        private readonly SephirahType _sephirah;

        public SephirahMapManager(CustomMapHandler handler, SephirahType sephirah) : base(handler)
        {
            _sephirah = sephirah;
        }

        public override T Init(string stageName, Offsets offsets, bool isEgo, bool initBGMs)
        {
            var mapManager = SingletonBehavior<BattleSceneRoot>.Instance.mapList?.Find(x => x.name.Contains(stageName));
            if (mapManager != null)
            {
                if (mapManager is T obj)
                {
                    Debug.LogWarning("CustomMapUtility: A map with an overlapping name and manager is already loaded");
                    return obj;
                }

                Debug.LogError(
                    "CustomMapUtility: A map with an overlapping name and a different manager is already loaded");
                return default;
            }

            this.stageName = stageName;
            this.offsets = offsets;
            var mapObject = !MapTemplateExists
                ? Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1",
                    SingletonBehavior<BattleSceneRoot>.Instance.transform)
                : Object.Instantiate(MapTemplateObject, SingletonBehavior<BattleSceneRoot>.Instance.transform);
            mapObject.name = "InvitationMap_" + stageName;
            var manager = InitManager(mapObject);
            manager.sephirahType = _sephirah;
            if (manager is IAsyncMapInit asyncMapInit)
            {
                imageInitTask = ImageInit_Async();
            }
            else
            {
                asyncMapInit = null;
                Debug.LogWarning($"CustomMapUtility: {typeof(T)} does not implement IAsyncMapInit");
                ImageInit();
            }

            if (initBGMs)
                try
                {
                    if (manager is IBGM bgm)
                    {
                        var bgms = bgm.GetCustomBGMs();
                        if (bgms != null && bgms.Length != 0)
                        {
                            if (asyncMapInit == null)
                            {
                                manager.mapBgm = AudioHandler.CustomBgmParse(bgms);
                            }
                            else
                            {
                                AudioHandler.CustomBgmParseAsync(bgms);
                                asyncMapInit.FirstLoad += (sender, e) =>
                                {
                                    var audioClip = AudioHandler.GetAudioClip(bgms);
                                    if (!isEgo)
                                        CustomMapHandler.AntiEardrumDamage_Checked(true, audioClip);
                                    manager.mapBgm = audioClip;
                                };
                            }
                        }
                        else
                        {
                            Debug.Log("CustomMapUtility: CustomBGMs is null or empty, enabling AutoBGM");
                            bgm.AutoBGM = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("CustomMapUtility: MapManager does not implement IBGM");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("CustomMapUtility: Failed to get BGMs");
                    Debug.LogException(ex);
                }
            else
                Debug.Log("CustomMapUtility: BGM initialization is disabled");

            manager._bMapInitialized = false;
            SingletonBehavior<BattleSceneRoot>.Instance.mapList?.Add(manager);
            Debug.Log("CustomMapUtility: Sephirah Map Added.");
            handler.mapOffsetsCache[stageName] = offsets;
            handler.mapAutoBgmCache[stageName] = initBGMs;
            return manager;
        }
    }
}