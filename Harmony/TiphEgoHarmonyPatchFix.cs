using BigDLL4221.Utils;
using HarmonyLib;
using UI;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class TiphEgoHarmonyPatchFix
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
        [HarmonyPatch(typeof(UIEmotionPassiveCardInven), "SetSprites")]
        public static void EmotionPassiveCardUI_SetSprites(object __instance, object ___card, MentalState state)
        {
            if (state != (MentalState)1000000) return;
            var card = ___card as EmotionCardXmlInfo;
            switch (__instance)
            {
                case EmotionPassiveCardUI instance:
                    ArtUtil.SetSpritesEmotionTiphEgo(instance, card);
                    break;
                case UIEmotionPassiveCardInven instance:
                    ArtUtil.SetSpritesEmotionTiphEgo(instance, card);
                    break;
            }
        }
    }
}