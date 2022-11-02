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
        private List<MapModel> _mapModels;
        private int _mapPhase;
        private int _phase;
        private NpcMechUtilBase _util;

        public void SetParameters(NpcMechUtilBase util, List<MapModel> mapModels = null)
        {
            _mapModels = mapModels ?? new List<MapModel>();
            _util = util;
        }

        public override void OnWaveStart()
        {
            MapUtil.PrepareEnemyMaps(_mapModels);
            PrepareUtil();
            Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData(_util.Model.SaveDataId, out _phase);
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
            if (!_util.Model.MechOptions.TryGetValue(_phase, out var mechOptions)) return;
            if (mechOptions.MechHp == 0 || _util.Model.Owner.hp > mechOptions.MechHp) return;
            _phase++;
            ChangeMap();
        }

        public override void OnRoundStart_After()
        {
            if (!_util.Model.MechOptions.TryGetValue(_phase, out var mechOptions)) return;
            if (mechOptions.CreatureFilter) MapUtil.ActiveCreatureBattleCamFilterComponent();
        }

        private int GetMapPhase()
        {
            if (_util.Model.MechOptions == null ||
                !_util.Model.MechOptions.TryGetValue(_phase, out var mechPhaseOptions)) return -1;
            if (_phase == 0 && !mechPhaseOptions.HasCustomMap) return -1;
            var subPhase = _util.Model.MechOptions.Where(x => x.Key < _phase).Reverse().Any(x => x.Value.HasCustomMap);
            return !mechPhaseOptions.HasCustomMap && subPhase
                ? (from phaseOption in _util.Model.MechOptions.Where(x => x.Key < _phase).Reverse()
                    where phaseOption.Value.HasCustomMap
                    select phaseOption.Value.MapOrderIndex).FirstOrDefault()
                : mechPhaseOptions.HasCustomMap
                    ? mechPhaseOptions.MapOrderIndex
                    : -1;
        }

        private void PrepareUtil()
        {
            var mainEnemy = BattleObjectManager.instance.GetAliveList(Faction.Enemy).FirstOrDefault(x =>
                x.passiveDetail.HasPassive<PassiveAbility_NpcMechBase_DLL4221>());
            var mainPassive = mainEnemy
                .GetActivePassive<PassiveAbility_NpcMechBase_DLL4221>();
            _util.Model.Owner = mainEnemy;
            _util.Model.ThisPassiveId = mainPassive.id;
            mainPassive.Util = _util;
        }

        private void ChangeMap()
        {
            if (!_util.Model.MechOptions.TryGetValue(_phase, out var mechOptions)) return;
            if (mechOptions.MusicOptions != null)
                CustomMapHandler.SetMapBgm(mechOptions.MusicOptions.MusicFileName, true,
                    mechOptions.MusicOptions.MapName);
            if (!mechOptions.HasCustomMap) return;
            CustomMapHandler.EnforceMap(mechOptions.MultiWaveMapOrderIndex);
            Singleton<StageController>.Instance.CheckMapChange();
            MapUtil.ActiveCreatureBattleCamFilterComponent(mechOptions.CreatureFilter);
        }
    }
}