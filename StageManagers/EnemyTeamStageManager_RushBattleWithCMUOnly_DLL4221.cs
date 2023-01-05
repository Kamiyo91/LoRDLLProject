using System;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using CustomMapUtility;
using UnityEngine;

namespace BigDLL4221.StageManagers
{
    public class EnemyTeamStageManager_RushBattleWithCMUOnly_DLL4221 : EnemyTeamStageManager
    {
        private readonly Dictionary<int, RushBattlePhaseOptions> _clonedPhases =
            new Dictionary<int, RushBattlePhaseOptions>();

        private bool _isLastWave;
        public RushBattlePhaseOptions ActualPhase;
        public int ActualPhaseInt;
        public CustomMapHandler Cmh;
        public bool ForcedChanged;
        public List<int> FoughtWaves = new List<int>();
        public bool IsInfinite;
        public bool IsRandom;
        public int MapPhase;
        public Dictionary<int, RushBattlePhaseOptions> Phases = new Dictionary<int, RushBattlePhaseOptions>();

        public void SetParameter(List<RushBattlePhaseOptions> phases, bool isInfinite = false, bool isRandom = false)
        {
            try
            {
                IsInfinite = isInfinite;
                IsRandom = isRandom;
                foreach (var phase in phases.Select((x, i) => (i, x)))
                {
                    Phases.Add(phase.i, phase.x);
                    _clonedPhases.Add(phase.i, phase.x);
                }

                if (!Singleton<StageController>.Instance.GetStageModel()
                        .GetStageStorageData("RushBattlePhaseSave4221", out ActualPhaseInt))
                    ActualPhaseInt = StaticModsInfo.RandomWaveStart;
                ActualPhase = Phases[ActualPhaseInt];
                MapPhase = ActualPhase.StarterMapPhase;
                if (!Singleton<StageController>.Instance.GetStageModel()
                        .GetStageStorageData("FoughtPhaseSave4221", out FoughtWaves)) FoughtWaves = new List<int>();
                if (FoughtWaves != null && FoughtWaves.Any())
                    foreach (var key in FoughtWaves)
                        Phases.Remove(key);
                if (!string.IsNullOrEmpty(ActualPhase.CmhPackageId))
                    Cmh = CustomMapHandler.GetCMU(ActualPhase.CmhPackageId);
                var stageModel = Singleton<StageController>.Instance.GetStageModel();
                stageModel.ClassInfo.mapInfo = new List<string>();
                foreach (var map in ActualPhase.Maps.SelectMany(x => x.Value))
                    stageModel.ClassInfo.mapInfo.Add(map.Stage);
                _isLastWave = Phases.Count < 2;
                if (!IsInfinite && _isLastWave) return;
                stageModel._waveList.Add(new StageWaveModel());
                stageModel._waveList.ElementAt(stageModel._waveList.Count - 1).Init(stageModel,
                    Singleton<StageController>.Instance.GetCurrentWaveModel()._stageWaveInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.InnerException);
            }
        }

        public void ChangeParameters(List<RushBattlePhaseOptions> phases, bool isInfinite = false,
            bool isRandom = false)
        {
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            IsInfinite = isInfinite;
            IsRandom = isRandom;
            Phases.Clear();
            _clonedPhases.Clear();
            FoughtWaves.Clear();
            foreach (var phase in phases.Select((x, i) => (i, x)))
            {
                Phases.Add(phase.i, phase.x);
                _clonedPhases.Add(phase.i, phase.x);
            }

            _isLastWave = Phases.Count < 2;
            if (_isLastWave && !IsInfinite) stageModel._waveList.RemoveAt(stageModel._waveList.Count - 1);
            ForcedChanged = true;
        }

        public void SetMapPhase(int value)
        {
            MapPhase = value;
        }

        public override void OnWaveStart()
        {
            try
            {
                if (FoughtWaves != null && FoughtWaves.Any())
                    foreach (var key in FoughtWaves)
                        Phases.Remove(key);
                if (ActualPhase.StartEmotionLevel != 0)
                    foreach (var unit in BattleObjectManager.instance.GetList(Faction.Enemy))
                        UnitUtil.LevelUpEmotion(unit, ActualPhase.StartEmotionLevel);
                if (Cmh == null) return;
                MapUtil.PrepareEnemyMapsMultiCmu(ActualPhase.Maps);
                if (MapPhase == -1) return;
                Cmh.EnforceMap(MapPhase);
                MapUtil.MapChangedValue(false);
                Singleton<StageController>.Instance.CheckMapChange();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.InnerException);
            }
        }

        public void ChangeMusic(string packageId, string musicFileName, string mapName)
        {
            CustomMapHandler.GetCMU(packageId).SetMapBgm(musicFileName, true, mapName);
        }

        public override void OnRoundStart()
        {
            if (MapPhase == -1 || Cmh == null) return;
            Cmh.EnforceMap(MapPhase);
            MapUtil.MapChangedValue(false);
        }

        public override void OnRoundEndTheLast()
        {
            if (BattleObjectManager.instance.GetAliveList(Faction.Enemy).Count < 1) ChangeInnerPhase();
        }

        public void ChangeInnerPhase()
        {
            if (!ActualPhase.InnerPhases.TryGetValue(ActualPhase.ActualInnerPhase, out var units)) return;
            ActualPhase.ActualInnerPhase++;
            foreach (var unit in BattleObjectManager.instance.GetList(Faction.Enemy))
                BattleObjectManager.instance.UnregisterUnit(unit);
            foreach (var model in units.Select((x, i) => (i, x)))
                UnitUtil.AddNewUnitWithDefaultData(model.x, model.i);
        }

        public override void OnEndBattle()
        {
            if (_isLastWave && !IsInfinite) return;
            if (ActualPhase.RecoverPlayerUnits)
                foreach (var unit in BattleObjectManager.instance.GetList(Faction.Player))
                    UnitUtil.UnitReviveAndRecovery(unit, unit.MaxHp, true);
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var nextWaveModel = stageModel._waveList.ElementAt(Singleton<StageController>.Instance._currentWave);
            if (nextWaveModel == null) return;
            if (!ForcedChanged)
            {
                FoughtWaves.Add(ActualPhaseInt);
                Phases.Remove(ActualPhaseInt);
                stageModel.SetStageStorgeData("FoughtPhaseSave4221", FoughtWaves);
            }

            ActualPhaseInt = !IsRandom
                ? ActualPhaseInt++
                : Phases.Any()
                    ? Phases.Keys.ElementAt(RandomUtil.Range(0, Phases.Count - 1))
                    : 0;
            stageModel.SetStageStorgeData("RushBattlePhaseSave4221", ActualPhaseInt);
            if (!Phases.TryGetValue(ActualPhaseInt, out ActualPhase))
            {
                if (!IsInfinite) return;
                stageModel.SetStageStorgeData("FoughtPhaseSave4221", new List<int>());
                ActualPhaseInt = !IsRandom ? 0 : _clonedPhases.Keys.ElementAt(RandomUtil.Range(0, Phases.Count - 1));
                stageModel.SetStageStorgeData("RushBattlePhaseSave4221", ActualPhaseInt);
                if (!_clonedPhases.TryGetValue(ActualPhaseInt, out ActualPhase))
                {
                    Debug.LogError("Infinite Battle Error - Next Phase not found, Ending the battle");
                    return;
                }
            }

            var list = new List<UnitBattleDataModel>();
            UnitUtil.PreparePreBattleEnemyUnits(ActualPhase.UnitModels, stageModel, list);
            nextWaveModel.ResetUnitBattleDataList(list);
        }
    }

    public class RushBattlePhaseOptions
    {
        public RushBattlePhaseOptions(List<UnitModel> unitModels, Dictionary<string, List<MapModel>> maps = null,
            Dictionary<int, List<UnitModel>> innerPhases = null, bool recoverPlayerUnits = true,
            int actualInnerPhase = 0, int starterMapPhase = 0, int startEmotionLevel = 0, string cmhPackageId = "")
        {
            Maps = maps ?? new Dictionary<string, List<MapModel>>();
            UnitModels = unitModels;
            InnerPhases = innerPhases ?? new Dictionary<int, List<UnitModel>>();
            RecoverPlayerUnits = recoverPlayerUnits;
            ActualInnerPhase = actualInnerPhase;
            StarterMapPhase = starterMapPhase;
            StartEmotionLevel = startEmotionLevel;
            CmhPackageId = cmhPackageId;
        }

        public Dictionary<string, List<MapModel>> Maps { get; set; }
        public List<UnitModel> UnitModels { get; set; }
        public Dictionary<int, List<UnitModel>> InnerPhases { get; set; }
        public bool RecoverPlayerUnits { get; set; }
        public int ActualInnerPhase { get; set; }
        public int StarterMapPhase { get; set; }
        public int StartEmotionLevel { get; set; }
        public string CmhPackageId { get; set; }
    }
}