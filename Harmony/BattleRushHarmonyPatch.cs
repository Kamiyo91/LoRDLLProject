using System;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.StageManagers;
using HarmonyLib;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class BattleRushHarmonyPatch
    {
        [HarmonyPatch(typeof(StageWaveModel), "Init")]
        [HarmonyPostfix]
        public static void StageWaveModel_Init(StageWaveModel __instance, StageModel stage)
        {
            StaticModsInfo.RushBattleManager = null;
            StaticModsInfo.StartWaveIndex = 0;
            if (StaticModsInfo.ChangedFormation.Item1)
            {
                __instance._formation =
                    new FormationModel(
                        Singleton<FormationXmlList>.Instance.GetData(StaticModsInfo.ChangedFormation.Item2));
                StaticModsInfo.ChangedFormation = new Tuple<bool, int>(false, 0);
            }

            if (StaticModsInfo.ChangingAct)
            {
                StaticModsInfo.ChangingAct = false;
                return;
            }

            var rushBattleOptions = BattleRushStaticInfo.RushBattleModels.FirstOrDefault(x =>
                x.Id == stage.ClassInfo.id.id && x.PackageId == stage.ClassInfo.id.packageId);
            if (rushBattleOptions == null) return;
            if (!rushBattleOptions.Waves.Any()) return;
            RushBattleModelSubRoot selectedWave;
            if (rushBattleOptions.IsRandom)
            {
                var index = RandomUtil.Range(0, rushBattleOptions.Waves.Count - 1);
                selectedWave = rushBattleOptions.Waves.ElementAtOrDefault(index);
                if (selectedWave == null) return;
                StaticModsInfo.StartWaveIndex = index;
            }
            else
            {
                selectedWave = rushBattleOptions.Waves.FirstOrDefault();
                if (selectedWave == null) return;
            }

            var stageName = string.Empty;
            if (!string.IsNullOrEmpty(selectedWave.StageManagerName))
                stageName = selectedWave.StageManagerName;
            StaticModsInfo.NextActManager = new Tuple<string, List<string>>(stageName, selectedWave.MapNames);
            __instance._availableUnitNumber = selectedWave.UnitAllowed;
            __instance._formation =
                new FormationModel(Singleton<FormationXmlList>.Instance.GetData(selectedWave.FormationId));
            __instance._unitList.Clear();
            foreach (var unitModel in selectedWave.UnitModels)
                __instance._unitList.Add(
                    UnitBattleDataModel.CreateUnitBattleDataByEnemyUnitId(stage,
                        new LorId(unitModel.PackageId, unitModel.Id)));
            __instance.team.Init(__instance._unitList, Faction.Enemy, stage.ClassInfo);
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.StartBattle))]
        [HarmonyPrefix]
        public static void StageController_StartBattle(StageController __instance)
        {
            var rushBattleOptions = BattleRushStaticInfo.RushBattleModels.FirstOrDefault(x =>
                x.Id == __instance._stageModel.ClassInfo.id.id &&
                x.PackageId == __instance._stageModel.ClassInfo.id.packageId);
            if (rushBattleOptions == null) return;
            var stage = __instance._stageModel.GetWave(__instance._currentWave);
            if (!string.IsNullOrEmpty(StaticModsInfo.NextActManager.Item1))
                stage._managerScript = StaticModsInfo.NextActManager.Item1;
            if (StaticModsInfo.NextActManager.Item2.Any())
            {
                __instance._stageModel.ClassInfo.mapInfo = new List<string>();
                foreach (var map in StaticModsInfo.NextActManager.Item2)
                    __instance._stageModel.ClassInfo.mapInfo.Add(map);
            }

            StaticModsInfo.NextActManager = new Tuple<string, List<string>>(string.Empty, new List<string>());
            StaticModsInfo.RushBattleManager = new EnemyTeamStageManager_RushBattle_DLL4221();
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.StartBattle))]
        [HarmonyPostfix]
        public static void StageController_StartBattle_Post()
        {
            StaticModsInfo.RushBattleManager?.OnWaveStart();
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.EndBattlePhase))]
        [HarmonyPostfix]
        public static void StageController_EndBattlePhase(StageController __instance)
        {
            var rushBattleOptions = BattleRushStaticInfo.RushBattleModels.FirstOrDefault(x =>
                x.Id == __instance._stageModel.ClassInfo.id.id &&
                x.PackageId == __instance._stageModel.ClassInfo.id.packageId);
            if (rushBattleOptions == null || StaticModsInfo.RushBattleManager == null) return;
            StaticModsInfo.RushBattleManager.OnEndBattle();
        }
    }
}