using BigDLL4221.Extensions;
using BigDLL4221.Utils;
using HarmonyLib;
using UI;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class EmotionCardColorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
        [HarmonyPatch(typeof(UIEmotionPassiveCardInven), "SetSprites")]
        public static void EmotionPassiveCardUI_SetSprites(object __instance)
        {
            switch (__instance)
            {
                case EmotionPassiveCardUI instance:
                    if (!(instance.Card is EmotionCardXmlExtension cardExtension)) return;
                    ArtUtil.EmotionPassiveCardUISetSpritesPost(instance, cardExtension);
                    break;
                case UIEmotionPassiveCardInven instance:
                    if (!(instance.Card is EmotionCardXmlExtension cardExtension2)) return;
                    ArtUtil.EmotionPassiveCardUISetSpritesPost(instance, cardExtension2);
                    break;
            }
        }
    }
}