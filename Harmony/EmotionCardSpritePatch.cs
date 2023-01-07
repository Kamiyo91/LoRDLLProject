using BigDLL4221.Extensions;
using BigDLL4221.Utils;
using HarmonyLib;
using UI;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class EmotionCardSpritePatch
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
                    ArtUtil.EmotionPassiveCardUISetSpritesPostNoColor(instance, cardExtension);
                    break;
                case UIEmotionPassiveCardInven instance:
                    if (!(instance.Card is EmotionCardXmlExtension cardExtension2)) return;
                    ArtUtil.EmotionPassiveCardUISetSpritesPostNoColor(instance, cardExtension2);
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAbnormalityCardPreviewSlot), "Init")]
        public static void UIAbnormalityCardPreviewSlot_Init(UIAbnormalityCardPreviewSlot __instance, object card)
        {
            if (!(card is EmotionCardXmlExtension cardExtension)) return;
            __instance.artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(cardExtension.LorId.packageId,
                    cardExtension.Artwork);
        }
    }
}