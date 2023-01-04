using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using CustomMapUtility;

namespace BigDLL4221.StageManagers
{
    public class EnemyTeamStageManager_RushBattleWithCMUOnly_DLL4221 : EnemyTeamStageManager
    {
        private readonly Dictionary<int, RushBattlePhaseOptions> _clonedPhases =
            new Dictionary<int, RushBattlePhaseOptions>();

        public RushBattlePhaseOptions ActualPhase;
        public int ActualPhaseInt;
        public CustomMapHandler Cmh;
        public List<int> FoughtWaves = new List<int>();
        public bool IsInfinite;
        public bool IsRandom;
        public int MapPhase;
        public Dictionary<int, RushBattlePhaseOptions> Phases = new Dictionary<int, RushBattlePhaseOptions>();

        public void SetParameter(List<RushBattlePhaseOptions> phases, bool isInfinite, bool isRandom,
            CustomMapHandler cmh = null)
        {
            Cmh = cmh;
            IsInfinite = isInfinite;
            IsRandom = isRandom;
            foreach (var phase in phases.Select((x, i) => (i, x)))
            {
                Phases.Add(phase.i, phase.x);
                _clonedPhases.Add(phase.i, phase.x);
            }

            ActualPhase = Phases[0];
            MapPhase = ActualPhase.StarterMapPhase;
        }

        public void SetMapPhase(int value)
        {
            MapPhase = value;
        }

        public override void OnWaveStart()
        {
            Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData("RushBattlePhaseSave4221", out ActualPhaseInt);
            Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData("FoughtPhaseSave4221", out FoughtWaves);
            if (FoughtWaves != null && FoughtWaves.Any())
                foreach (var key in FoughtWaves)
                    Phases.Remove(key);
            if (ActualPhase.StartEmotionLevel != 0)
                foreach (var unit in BattleObjectManager.instance.GetList(Faction.Enemy))
                    UnitUtil.LevelUpEmotion(unit, ActualPhase.StartEmotionLevel);
            if (Cmh == null) return;
            MapUtil.PrepareEnemyMaps(Cmh, ActualPhase.Maps);
            if (MapPhase == -1) return;
            Cmh.EnforceMap(MapPhase);
            MapUtil.MapChangedValue(false);
            Singleton<StageController>.Instance.CheckMapChange();
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
            if (ActualPhase.RecoverPlayerUnits)
                foreach (var unit in BattleObjectManager.instance.GetList(Faction.Player))
                    UnitUtil.UnitReviveAndRecovery(unit, unit.MaxHp, true);
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var currentWaveModel = Singleton<StageController>.Instance.GetCurrentWaveModel();
            //if (currentWaveModel == null || currentWaveModel.IsUnavailable()) return;
            FoughtWaves.Add(ActualPhaseInt);
            Phases.Remove(ActualPhaseInt);
            stageModel.SetStageStorgeData("FoughtPhaseSave4221", FoughtWaves);
            ActualPhaseInt = !IsRandom
                ? ActualPhaseInt++
                : _clonedPhases.Keys.ElementAt(RandomUtil.Range(0, Phases.Count - 1));
            stageModel.SetStageStorgeData("RushBattlePhaseSave4221", ActualPhaseInt);
            if (!Phases.TryGetValue(ActualPhaseInt, out ActualPhase))
            {
                if (!IsInfinite) return;
                stageModel.SetStageStorgeData("FoughtPhaseSave4221", new List<int>());
                ActualPhaseInt = !IsRandom ? 0 : _clonedPhases.Keys.ElementAt(RandomUtil.Range(0, Phases.Count - 1));
                stageModel.SetStageStorgeData("RushBattlePhaseSave4221", ActualPhaseInt);
                if (!_clonedPhases.TryGetValue(ActualPhaseInt, out ActualPhase)) return;
            }

            var list = new List<UnitBattleDataModel>();
            UnitUtil.PreparePreBattleEnemyUnits(ActualPhase.UnitModels, stageModel, list);
            currentWaveModel.ResetUnitBattleDataList(list);
        }
    }

    public class RushBattlePhaseOptions
    {
        public List<MapModel> Maps { get; set; }
        public List<UnitModel> UnitModels { get; set; }
        public Dictionary<int, List<UnitModel>> InnerPhases { get; set; }
        public bool RecoverPlayerUnits { get; set; }
        public int ActualInnerPhase { get; set; }
        public int StarterMapPhase { get; set; }
        public int StartEmotionLevel { get; set; }
    }
}