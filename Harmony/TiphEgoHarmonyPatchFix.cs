using BigDLL4221.Extensions;
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
        public static void EmotionPassiveCardUI_SetSprites(object __instance, MentalState state)
        {
            if (state != (MentalState)1000000) return;
            switch (__instance)
            {
                case EmotionPassiveCardUI instance:
                    if (!(instance.Card is EmotionCardXmlExtension card1)) return;
                    ArtUtil.SetSpritesEmotionTiphEgo(instance, card1);
                    break;
                case UIEmotionPassiveCardInven instance:
                    if (!(instance.Card is EmotionCardXmlExtension card2)) return;
                    ArtUtil.SetSpritesEmotionTiphEgo(instance, card2);
                    break;
            }
        }
    }
}