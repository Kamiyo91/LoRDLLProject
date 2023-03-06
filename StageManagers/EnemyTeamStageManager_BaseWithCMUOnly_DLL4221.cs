using System.Collections.Generic;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using CustomMapUtility;

namespace BigDLL4221.StageManagers
{
    public class EnemyTeamStageManager_BaseWithCMUOnly_DLL4221 : EnemyTeamStageManager
    {
        private CustomMapHandler _cmh;
        private bool _creatureFilter;
        private List<MapModel> _mapModels;
        private int _mapPhase;

        public void SetParameters(CustomMapHandler cmh, List<MapModel> mapModels = null, int mapPhase = 0)
        {
            _mapModels = mapModels ?? new List<MapModel>();
            _cmh = cmh;
            _mapPhase = mapPhase;
        }

        public override void OnWaveStart()
        {
            MapUtil.PrepareEnemyMaps(_cmh, _mapModels);
            Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData("BaseCMUOnlyDLL4221", out _mapPhase);
            if (_mapPhase == -1) return;
            _cmh.EnforceMap(_mapPhase);
            Singleton<StageController>.Instance.CheckMapChange();
        }

        public override void OnRoundStart()
        {
            if (_mapPhase == -1) return;
            _cmh.EnforceMap(_mapPhase);
        }

        public override void OnRoundStart_After()
        {
            MapUtil.ActiveCreatureBattleCamFilterComponent(_creatureFilter);
        }

        public void ChangeFilterStatus(bool value)
        {
            _creatureFilter = value;
        }

        public void ChangePhase(int value)
        {
            _mapPhase = value;
        }

        public void ChangeMapPhase(int value)
        {
            _mapPhase = value;
            ChangeMap();
        }

        public void ChangeMusic(string musicFileName, string mapName)
        {
            _cmh.SetMapBgm(musicFileName, true, mapName);
        }

        public override void OnEndBattle()
        {
            Singleton<StageController>.Instance.GetStageModel()
                .SetStageStorgeData("BaseCMUOnlyDLL4221", _mapPhase);
        }

        private void ChangeMap()
        {
            _cmh.EnforceMap(_mapPhase);
            MapUtil.MapChangedValue(false);
            Singleton<StageController>.Instance.CheckMapChange();
            MapUtil.ActiveCreatureBattleCamFilterComponent(_creatureFilter);
        }
    }
}