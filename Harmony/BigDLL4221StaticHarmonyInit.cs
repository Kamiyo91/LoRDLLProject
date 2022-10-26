namespace BigDLL4221.Harmony
{
    public class BigDLL4221StaticHarmonyInit : ModInitializer
    {
        public override void OnInitializeMod()
        {
            new HarmonyLib.Harmony("LOR.BigDLL4221HarmonyPatch_MOD").PatchAll();
        }
    }
}