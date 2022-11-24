﻿using BigDLL4221.Utils;

namespace BigDLL4221.Harmony
{
    public class BigDLL4221StaticHarmonyInit : ModInitializer
    {
        public override void OnInitializeMod()
        {
            GenericUtil.OtherModCheck();
            CardUtil.FillDictionary();
            new HarmonyLib.Harmony("LOR.BigDLL4221HarmonyPatch_MOD").PatchAll();
        }
    }
}