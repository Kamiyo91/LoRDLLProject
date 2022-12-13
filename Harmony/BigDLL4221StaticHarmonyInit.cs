using System;
using System.IO;
using System.Reflection;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using UnityEngine;

namespace BigDLL4221.Harmony
{
    public class BigDLL4221StaticHarmonyInit : ModInitializer
    {
        public override void OnInitializeMod()
        {
            Debug.Log($"BigDLL4221: Using Version {Assembly.GetExecutingAssembly().GetName().Version}");
            GenericUtil.PutUtilInTheFirstSlot();
            GenericUtil.OtherModCheck();
            CardUtil.FillDictionary();
            ModParameters.Harmony.CreateClassProcessor(typeof(MainHarmonyPatch)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(ColorPatch)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(EmotionSelectionUnitPatch)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(BlockUiRepeat)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(UpdateEmotionCoinPatch)).Patch();
            if (!StaticModsInfo.BaseModFound)
                ModParameters.Harmony.CreateClassProcessor(typeof(UnitLimitPatch)).Patch();
            if (!StaticModsInfo.SpeedDiceColorModFound)
            {
                try
                {
                    ArtUtil.GetSpeedDieArtWorks(new DirectoryInfo(
                        Path.GetDirectoryName(
                            Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path)) +
                        "/CustomDiceArtWork"));
                }
                catch (Exception)
                {
                    // ignored
                }

                ModParameters.Harmony.CreateClassProcessor(typeof(SpeedDiceColorPatch)).Patch();
            }
            else
            {
                ModParameters.Harmony.CreateClassProcessor(typeof(SpeedDiceColorPatchWithPattyMod)).Patch();
            }

            if (!LucasTiphEgoModInfo.TiphEgoModFound) return;
            ModParameters.Harmony.CreateClassProcessor(typeof(TiphEgoHarmonyPatchFix)).Patch();
        }
    }
}