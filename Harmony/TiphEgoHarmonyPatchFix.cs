using System.Linq;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class TiphEgoHarmonyPatchFix
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
        [HarmonyPatch(typeof(UIEmotionPassiveCardInven), "SetSprites")]
        public static void EmotionPassiveCardUI_SetSprites(object ____card, ref Image ____artwork,
            TextMeshProUGUI ____flavorText,
            TextMeshProUGUI ____abilityDesc, TextMeshProUGUI ___txt_Level, Image ___img_LeftTotalFrame,
            Image ____leftFrameTitleLineardodge, Image ____rightFrame,
            Image ____rightBg, Sprite[] ____positiveBgSprite, Image ____hOverImg, Image ____rootImageBg,
            Sprite[] ____positiveFrameSprite, MentalState state)
        {
            if (state != (MentalState)1000000) return;
            var card = ____card as EmotionCardXmlInfo;
            ____artwork.sprite = LucasTiphEgoModInfo.TiphEgoArtWorks.TryGetValue(card.Artwork, out var sprite)
                ? sprite
                : null;
            ___img_LeftTotalFrame.sprite = UISpriteDataManager.instance.AbnormalityFrame.ElementAtOrDefault(0);
            var orAddComponent = ___img_LeftTotalFrame.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ArtUtil.ChangeEmotionCardColorTiphEgo(ref orAddComponent);
            ____rightBg.sprite = ____positiveBgSprite.ElementAtOrDefault(1);
            var orAddComponent2 = ____rightBg.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ArtUtil.ChangeEmotionCardColorTiphEgo(ref orAddComponent2);
            ____rightFrame.sprite = ____positiveFrameSprite.ElementAtOrDefault(1);
            var orAddComponent3 = ____rightFrame.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ArtUtil.ChangeEmotionCardColorTiphEgo(ref orAddComponent3);
            ____leftFrameTitleLineardodge.gameObject.SetActive(false);
            ____flavorText.fontMaterial.SetColor("_UnderlayColor", Color.yellow);
            ____abilityDesc.fontMaterial.SetColor("_UnderlayColor", Color.yellow);
            ____hOverImg.color = Color.yellow;
            var rootColor = Color.yellow;
            rootColor.a = 0.25f;
            ____rootImageBg.color = rootColor;
            var component = ___txt_Level.GetComponent<TextMeshProMaterialSetter>();
            if (component == null) return;
            component.glowColor = Color.yellow;
            component.underlayColor = Color.yellow;
            component.enabled = false;
            component.enabled = true;
        }
    }
}