using System;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.Utils;

namespace BigDLL4221.StageManagers
{
    public class EnemyTeamStageManager_RushBattle_DLL4221 : EnemyTeamStageManager
    {
        private RushBattleModelSubRoot _actualWave;
        private int _actualWaveIndex;
        private bool _isRandom;
        private bool _lastWave;
        private RushBattleModelSubRoot _nextWave;

        public void SetParameters()
        {
            StaticModsInfo.ChangedFormation = new Tuple<bool, int>(false, 0);
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var rushBattleOptions = BattleRushStaticInfo.RushBattleModels.FirstOrDefault(x =>
                x.PackageId == stageModel.ClassInfo.id.packageId && x.Id == stageModel.ClassInfo.id.id);
            if (rushBattleOptions == null) return;
            _isRandom = rushBattleOptions.IsRandom;
            if (!Singleton<StageController>.Instance.GetStageModel()
                    .GetStageStorageData("RushBattlePhaseSave23421", out _actualWaveIndex))
                _actualWaveIndex = StaticModsInfo.StartWaveIndex;
            _actualWave = rushBattleOptions.Waves[_actualWaveIndex];
            _lastWave = !rushBattleOptions.IsInfinite && !_actualWave.LastOneInfinite &&
                        rushBattleOptions.Waves.Where(x => x != _actualWave).All(x => x.Fought);
            if (!_lastWave) stageModel._waveList.Add(new StageWaveModel());
        }

        public override void OnWaveStart()
        {
            SetParameters();
        }

        public override void OnEndBattle()
        {
            if (_lastWave || (!BattleObjectManager.instance.GetAliveList(Faction.Player).Any() &&
                              BattleObjectManager.instance.GetAliveList(Faction.Enemy).Any())) return;
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var rushBattleOptions = BattleRushStaticInfo.RushBattleModels.FirstOrDefault(x =>
                x.PackageId == stageModel.ClassInfo.id.packageId && x.Id == stageModel.ClassInfo.id.id);
            if (rushBattleOptions == null) return;
            foreach (var wave in rushBattleOptions.Waves.Where(x => x == _actualWave))
                wave.Fought = true;
            if (!_lastWave && rushBattleOptions.IsRandom &&
                rushBattleOptions.Waves.Where(x =>
                    (string.IsNullOrEmpty(x.WaveCode) && string.IsNullOrEmpty(_actualWave.WaveCode)) ||
                    x.WaveCode == _actualWave.WaveCode).All(x => x.Fought))
                foreach (var wave in rushBattleOptions.Waves.Where(x => x.WaveCode == _actualWave.WaveCode))
                    wave.Fought = false;
            if (!_lastWave && rushBattleOptions.IsRandom && !string.IsNullOrEmpty(_actualWave.SwitchWaveCode) &&
                rushBattleOptions.Waves.Where(x => x.WaveCode == _actualWave.SwitchWaveCode).All(x => x.Fought))
                foreach (var wave in rushBattleOptions.Waves.Where(x => x.WaveCode == _actualWave.SwitchWaveCode))
                    wave.Fought = false;
            if (!string.IsNullOrEmpty(_actualWave.WaveCode))
                stageModel.SetStageStorgeData("FoughtSwitchSaved23421", _actualWave.WaveCode);
            if (!string.IsNullOrEmpty(_actualWave.SwitchWaveCode))
                stageModel.SetStageStorgeData("FoughtSwitchSaved23421", _actualWave.SwitchWaveCode);
            if (!Singleton<StageController>.Instance.GetStageModel()
                    .GetStageStorageData("FoughtSwitchSaved23421", out string waveCode)) waveCode = string.Empty;
            if (_isRandom)
            {
                var nextWaves = rushBattleOptions.Waves.Where(x =>
                    (string.IsNullOrEmpty(waveCode) || x.WaveCode == waveCode) && !x.Fought).ToList();
                _nextWave = RandomUtil.SelectOne(nextWaves);
                var nextWaveIndex = rushBattleOptions.Waves.IndexOf(_nextWave);
                stageModel.SetStageStorgeData("RushBattlePhaseSave23421", nextWaveIndex);
            }
            else
            {
                var order = string.IsNullOrEmpty(_actualWave.SwitchWaveCode) ? _actualWave.WaveOrder + 1 : 0;
                _nextWave = rushBattleOptions.Waves.FirstOrDefault(x =>
                    (string.IsNullOrEmpty(waveCode) || x.WaveCode == waveCode) && x.WaveOrder == order);
                var nextWaveIndex = rushBattleOptions.Waves.IndexOf(_nextWave);
                stageModel.SetStageStorgeData("RushBattlePhaseSave23421", nextWaveIndex);
            }

            if (_nextWave == null) return;
            var stageWaveInfo = Singleton<StageController>.Instance.GetCurrentWaveModel()._stageWaveInfo;
            StaticModsInfo.ChangedFormation = new Tuple<bool, int>(true, _nextWave.FormationId);
            stageWaveInfo.availableNumber = _nextWave.UnitAllowed;
            var mapList = new List<string>();
            mapList.AddRange(_nextWave.MapNames);
            StaticModsInfo.ChangingAct = true;
            StaticModsInfo.NextActManager =
                new Tuple<string, List<string>>(_nextWave.StageManagerName, mapList);
            var nextWaveModel = stageModel._waveList.ElementAt(stageModel._waveList.Count - 1);
            nextWaveModel.Init(stageModel, stageWaveInfo);
            nextWaveModel._unitList.Clear();
            foreach (var unitModel in _nextWave.UnitModels)
                nextWaveModel._unitList.Add(UnitBattleDataModel.CreateUnitBattleDataByEnemyUnitId(stageModel,
                    new LorId(unitModel.PackageId, unitModel.Id)));
            if (_actualWave.RecoverPlayerUnits)
                foreach (var unit in BattleObjectManager.instance.GetAliveList(Faction.Player))
                    UnitUtil.UnitReviveAndRecovery(unit, unit.MaxHp, false);
            foreach (var stageFloor in stageModel.GetAvailableFloorList())
            {
                var playerUnits = _actualWave.PlayerUnitModels.Where(x => x.Floor == stageFloor.Sephirah)
                    .SelectMany(x => x.UnitModels).ToList();
                if (playerUnits.Any())
                {
                    stageFloor._unitList.Clear();
                    UnitUtil.PreparePreBattleAllyUnits(stageFloor, playerUnits, stageModel, stageFloor._unitList);
                }

                if (!_actualWave.ReloadOriginalPlayerUnits.Contains(stageFloor.Sephirah)) continue;
                foreach (var unitDataModel in stageFloor._floorModel.GetUnitDataList()
                             .Where(unitDataModel => stageFloor._unitList.Count < 5))
                    stageFloor._unitList.Add(UnitUtil.InitUnitDefault(stageModel, unitDataModel));
            }
        }
    }
}