using System;
using System.IO;
using System.Reflection;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BigDLL4221.Harmony
{
    public class BigDLL4221StaticHarmonyInit : ModInitializer
    {
        public override void OnInitializeMod()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Debug.Log($"BigDLL4221: Using Version {assembly.GetName().Version}");
            ModParametersUtilLoader.LoadDllUtilOptions(Path.Combine(Application.dataPath, "Mods"));
            GenericUtil.PutUtilInTheFirstSlot();
            GenericUtil.OtherModCheck();
            CardUtil.FillDictionary();
            ModParameters.Harmony.CreateClassProcessor(typeof(MainHarmonyPatch)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(EmotionSelectionUnitPatch)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(BlockUiRepeat)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(UpdateEmotionCoinPatch)).Patch();
            ModParameters.Harmony.CreateClassProcessor(typeof(HotfixTranspilers)).Patch();
            if (!StaticModsInfo.BaseModFound)
                ModParameters.Harmony.CreateClassProcessor(typeof(UnitLimitPatch)).Patch();
            if (StaticModsInfo.CustomSpeedDice)
            {
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
            }

            SceneManager.sceneLoaded += GenericUtil.OnLoadingScreen;
            if (!StaticModsInfo.CustomColors)
            {
                ModParameters.Harmony.CreateClassProcessor(typeof(EmotionCardSpritePatch)).Patch();
                return;
            }

            ModParameters.Harmony.CreateClassProcessor(typeof(ColorPatch)).Patch();
            if (StaticModsInfo.TiphEgoModFound)
                ModParameters.Harmony.CreateClassProcessor(typeof(EmotionCardColorPatchWithTiphEgo)).Patch();
            else ModParameters.Harmony.CreateClassProcessor(typeof(EmotionCardColorPatch)).Patch();
        }
    }
}