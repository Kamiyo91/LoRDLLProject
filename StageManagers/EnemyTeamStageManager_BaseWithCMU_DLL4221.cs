using System.Collections.Generic;
using System.Linq;
using BigDLL4221.BaseClass;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Passives;
using BigDLL4221.Utils;

namespace BigDLL4221.StageManagers
{
    public class EnemyTeamStageManager_BaseWithCMU_DLL4221 : EnemyTeamStageManager
    {
        private int _index;
        private List<MapModel> _mapModels;
        private int _mapPhase;
        private int _phase;
        private NpcMechUtilBase _util;

        public void SetParameters(List<MapModel> mapModels = null, int index = 0)
        {
            _mapModels = mapModels ?? new List<MapModel>();
            _index = index;
        }

        public override void OnWaveStart()
        {
            _util = BattleObjectManager.instance.GetAliveList(Faction.Enemy).FirstOrDefault(x => x.index == _index)
                .GetActivePassive<PassiveAbility_NpcMechBase_DLL4221>().Util;
            var phaseChanged = Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData(_util.Model.SaveDataId, out _phase);
            MapUtil.PrepareEnemyMaps(_mapModels);
            _phase = phaseChanged ? _phase : 0;
            _mapPhase = GetMapPhase();
            if (_mapPhase == -1) return;
            CustomMapHandler.EnforceMap(_mapPhase);
            Singleton<StageController>.Instance.CheckMapChange();
        }

        public override void OnRoundEndTheLast()
        {
            CheckPhase();
        }

        public override void OnRoundStart()
        {
            _mapPhase = GetMapPhase();
            if (_mapPhase == -1) return;
            CustomMapHandler.EnforceMap(_mapPhase);
        }

        private void CheckPhase()
        {
            _phase = _util.Model.Phase;
            if (!_util.Model.MechOptions.TryGetValue(_phase, out var mechPhaseOptions)) return;
            if (!mechPhaseOptions.HasCustomMap) return;
            CustomMapHandler.EnforceMap(_phase);
            Singleton<StageController>.Instance.CheckMapChange();
        }

        private int GetMapPhase()
        {
            if (!_util.Model.MechOptions.TryGetValue(_phase, out var mechPhaseOptions)) return -1;
            if (_phase == 0 && !mechPhaseOptions.HasCustomMap) return -1;
            var phase = !mechPhaseOptions.HasCustomMap
                ? (from phaseOption in _util.Model.MechOptions.Where(x => x.Key < _phase).Reverse()
                    where phaseOption.Value.HasCustomMap
                    select phaseOption.Key).FirstOrDefault()
                : _phase;
            return phase == 0 ? -1 : _phase;
        }
    }
}