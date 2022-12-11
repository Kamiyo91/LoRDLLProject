using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BigDLL4221.Models;
using HarmonyLib;
using Mod;

namespace BigDLL4221.Utils
{
    public static class GenericUtil
    {
        public static async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        public static void OtherModCheck()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Any(x => x.GetName().Name == "Daat Floor MOD")) StaticModsInfo.DaatFloorFound = true;
            if (assemblies.Any(
                    x => x.GetName().Name == "BaseMod" && x.GetType("SummonLiberation.Harmony_Patch") != null))
                StaticModsInfo.BaseModFound = true;
            var tiphAssembly = assemblies.FirstOrDefault(x => x.GetName().Name == "Luca1125_EgoTiphereth");
            if (tiphAssembly == null) return;
            LucasTiphEgoModInfo.TiphEgoModFound = true;
            LucasTiphEgoModInfo.TiphEgoPath =
                Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(tiphAssembly.CodeBase).Path));
        }

        public static void PutUtilInTheFirstSlot()
        {
            var modContentInfoList = (List<ModContentInfo>)Singleton<ModContentManager>.Instance.GetType()
                .GetField("_allMods", AccessTools.all)?.GetValue(Singleton<ModContentManager>.Instance);
            var modContentInfo =
                modContentInfoList?.FirstOrDefault(x => x.invInfo.workshopInfo.uniqueId == "BigDLLUtilLoader21341");
            if (modContentInfo == null || modContentInfoList[0] == modContentInfo) return;
            modContentInfoList.Remove(modContentInfo);
            modContentInfoList.Insert(0, modContentInfo);
        }
    }
}