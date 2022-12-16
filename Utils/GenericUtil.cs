using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BigDLL4221.Models;
using Mod;

namespace BigDLL4221.Utils
{
    public static class GenericUtil
    {
        private static readonly List<string> LoadingOrder = new List<string>
            { "UnityExplorer", "HarmonyLoadOrderFix", "BaseMod" };

        public static async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        public static void OtherModCheck()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Any(x => x.GetName().Name == "Daat Floor MOD")) StaticModsInfo.DaatFloorFound = true;
            if (assemblies.Any(x => x.GetName().Name == "Patty_SpeedDiceColor_MOD"))
                StaticModsInfo.SpeedDiceColorModFound = true;
            if (assemblies.Any(
                    x => x.GetName().Name == "BaseMod" && x.GetType("SummonLiberation.Harmony_Patch") != null))
                StaticModsInfo.BaseModFound = true;
            var tiphAssembly = assemblies.FirstOrDefault(x => x.GetName().Name == "Luca1125_EgoTiphereth");
            if (tiphAssembly == null) return;
            StaticModsInfo.TiphEgoModFound = true;
        }

        public static void PutUtilInTheFirstSlot()
        {
            var modContentInfoList = Singleton<ModContentManager>.Instance._allMods;
            var modContentInfo =
                modContentInfoList?.FirstOrDefault(x => x.invInfo.workshopInfo.uniqueId == "BigDLLUtilLoader21341");
            if (modContentInfo == null) return;
            var index = modContentInfoList.FindIndex(x => !LoadingOrder.Contains(x.invInfo.workshopInfo.uniqueId));
            if (index == -1 || modContentInfoList[index] == modContentInfo) return;
            modContentInfoList.Remove(modContentInfo);
            modContentInfoList.Insert(index, modContentInfo);
        }

        //public static void OnLoadingScreen(Scene scene, LoadSceneMode _)
        //{
        //}
    }
}