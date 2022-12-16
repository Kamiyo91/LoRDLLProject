using System.Linq;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Workshop;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class ColorPatch
    {
        [HarmonyPatch(typeof(BattleUnitInformationUI_PassiveList.BattleUnitInformationPassiveSlot), "SetData")]
        [HarmonyPostfix]
        public static void BattleUnitInformationPassiveSlot_SetData_Post(
            BattleUnitInformationUI_PassiveList.BattleUnitInformationPassiveSlot __instance, PassiveAbilityBase passive)
        {
            if (!ModParameters.PassiveOptions.TryGetValue(passive.id.packageId, out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x => x.PassiveId == passive.id.id);
            if (passiveItem?.PassiveColorOptions?.FillColor == null) return;
            if (__instance.img_Icon != null)
                __instance.img_Icon.color = passiveItem.PassiveColorOptions.FillColor.Value;
            if (__instance.img_IconGlow != null)
                __instance.img_IconGlow.color = passiveItem.PassiveColorOptions.FillColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianEquipInfoSlot), "SetData")]
        [HarmonyPostfix]
        private static void UILibrarianEquipInfoSlot_SetData_Post(UILibrarianEquipInfoSlot __instance,
            BookPassiveInfo passive)
        {
            if (!ModParameters.PassiveOptions.TryGetValue(passive.passive.id.packageId, out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x => x.PassiveId == passive.passive.id.id);
            if (passiveItem?.PassiveColorOptions == null) return;
            if (passiveItem.PassiveColorOptions.FillColor.HasValue)
                __instance.Frame.color = passiveItem.PassiveColorOptions.FillColor.Value;
            if (passiveItem.PassiveColorOptions.TextColor.HasValue)
                __instance.txt_cost.color = passiveItem.PassiveColorOptions.TextColor.Value;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionSlot), "SetColorByRarity")]
        [HarmonyPostfix]
        private static void UIPassiveColor_SetColorByRarity(UIPassiveSuccessionSlot __instance, Color c)
        {
            if (__instance.passivemodel == null || c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.PassiveOptions.TryGetValue(
                    __instance.passivemodel.reservedData.currentpassive.id.packageId,
                    out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x =>
                    x.PassiveId == __instance.passivemodel.reservedData.currentpassive.id.id);
            if (passiveItem?.PassiveColorOptions?.FillColor == null) return;
            foreach (var graphic in __instance.rarityGraphics)
                graphic.CrossFadeColor(passiveItem.PassiveColorOptions.FillColor.Value, 0f, true, true);
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionCenterPassiveSlot), "SetData")]
        [HarmonyPatch(typeof(UIPassiveSuccessionPreviewPassiveSlot), "SetData")]
        [HarmonyPostfix]
        public static void UIPassive_SetData_Post(object __instance, PassiveModel passive)
        {
            if (!ModParameters.PassiveOptions.TryGetValue(passive.originData.currentpassive.id.packageId,
                    out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x => x.PassiveId == passive.originData.currentpassive.id.id);
            if (passiveItem?.PassiveColorOptions?.FillColor == null) return;
            switch (__instance)
            {
                case UIPassiveSuccessionCenterPassiveSlot instance:
                    foreach (var graphic in instance.graphics_Rarity)
                        graphic.color = passiveItem.PassiveColorOptions.FillColor.Value;
                    break;
                case UIPassiveSuccessionPreviewPassiveSlot instance:
                    foreach (var graphic in instance.graphics_Rarity)
                        graphic.color = passiveItem.PassiveColorOptions.FillColor.Value;
                    break;
            }
        }

        [HarmonyPatch(typeof(UIDetailEgoCardSlot), "SetData")]
        [HarmonyPatch(typeof(UIOriginCardSlot), "SetData")]
        [HarmonyPrefix]
        public static void UICard_SetData_Pre(object __instance, DiceCardItemModel cardmodel)
        {
            if (cardmodel == null) return;
            switch (__instance)
            {
                case UIDetailEgoCardSlot instance:
                    ArtUtil.UICardSetDataPre(instance);
                    break;
                case UIOriginCardSlot instance:
                    ArtUtil.UICardSetDataPre(instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIDetailEgoCardSlot), "SetData")]
        [HarmonyPatch(typeof(UIOriginCardSlot), "SetData")]
        [HarmonyPostfix]
        public static void UICard_SetData_Post(object __instance, DiceCardItemModel cardmodel)
        {
            if (cardmodel == null) return;
            if (!ModParameters.CardOptions.TryGetValue(cardmodel.GetID().packageId, out var cardOptions)) return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == cardmodel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            switch (__instance)
            {
                case UIDetailEgoCardSlot instance:
                    ArtUtil.UICardSetDataPost(instance, cardItem.CardColorOptions);
                    break;
                case UIOriginCardSlot instance:
                    ArtUtil.UICardSetDataPost(instance, cardItem.CardColorOptions);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIDetailCardSlot), "SetData")]
        [HarmonyPrefix]
        public static void UIDetailCardSlot_SetData_Pre(UIDetailCardSlot __instance, DiceCardItemModel cardmodel)
        {
            if (cardmodel == null) return;
            var gameObject = __instance.ob_selfAbility.transform.parent.parent.parent.gameObject;
            var rightFrame = gameObject.GetComponentsInChildren<Image>()
                .FirstOrDefault(x => x.name.Contains("[Image]BgFrame"));
            if (rightFrame == null) return;
            rightFrame.overrideSprite = null;
        }

        [HarmonyPatch(typeof(UIDetailCardSlot), "SetData")]
        [HarmonyPostfix]
        public static void UIDetailCardSlot_SetData(UIDetailCardSlot __instance, DiceCardItemModel cardmodel)
        {
            if (cardmodel == null) return;
            var gameObject = __instance.ob_selfAbility.transform.parent.parent.parent.gameObject;
            var rightFrame = gameObject.GetComponentsInChildren<Image>()
                .FirstOrDefault(x => x.name.Contains("[Image]BgFrame"));
            if (rightFrame == null) return;
            if (!ModParameters.CardOptions.TryGetValue(cardmodel.GetID().packageId, out var cardOptions)) return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == cardmodel.GetID().id);
            if (cardItem?.CardColorOptions == null ||
                string.IsNullOrEmpty(cardItem.CardColorOptions.RightFrame)) return;
            if (!ModParameters.CardArtWorks.TryGetValue(cardItem.CardColorOptions.RightFrame, out var rightFrameImg))
                return;
            rightFrame.overrideSprite = rightFrameImg;
            rightFrame.overrideSprite.name = $"{cardItem.CardColorOptions.RightFrame}_RFrame";
            if (cardItem.CardColorOptions.ApplySideFrontColors && cardItem.CardColorOptions.CardColor.HasValue)
                rightFrame.color = cardItem.CardColorOptions.CardColor.Value;
            else rightFrame.color = Color.white;
        }

        // May Be Useful
        //[HarmonyPatch(typeof(UIOriginCardSlot), "SetRangeIconHsv")]
        //[HarmonyPrefix]
        //public static void UICard_SetRangeIconHsv_Pre(UIOriginCardSlot __instance, Vector3 hsvvalue,Image __instance.img_RangeIcon,
        //    RefineHsv __instance.hsv_rangeIcon)
        //{
        //    if (__instance.CardModel == null || __instance.hsv_rangeIcon == null) return;
        //    __instance.img_RangeIcon.color = Color.white;
        //    __instance.hsv_rangeIcon.enabled = true;
        //    __instance.hsv_rangeIcon._HueShift = hsvvalue.x;
        //    __instance.hsv_rangeIcon._Saturation = hsvvalue.y;
        //    __instance.hsv_rangeIcon._ValueBrightness = hsvvalue.z;
        //    __instance.hsv_rangeIcon.CallUpdate();
        //    __instance.hsv_rangeIcon.enabled = false;
        //    __instance.hsv_rangeIcon.enabled = true;
        //}
        [HarmonyPatch(typeof(UIOriginCardSlot), "SetRangeIconHsv")]
        [HarmonyPostfix]
        public static void UICard_SetRangeIconHsv_Post(UIOriginCardSlot __instance, Vector3 hsvvalue)
        {
            if (__instance.CardModel == null || hsvvalue == UIColorManager.Manager.CardRangeHsvValue[6]) return;
            if (!ModParameters.CardOptions.TryGetValue(__instance.CardModel.GetID().packageId, out var cardOptions))
                return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == __instance.CardModel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            if (__instance.hsv_rangeIcon == null) return;
            if (cardItem.CardColorOptions.CustomIconColor.HasValue)
                __instance.img_RangeIcon.color = cardItem.CardColorOptions.CustomIconColor.Value;
            if (!cardItem.CardColorOptions.UseHSVFilter)
            {
                __instance.hsv_rangeIcon.enabled = false;
                return;
            }

            if (cardItem.CardColorOptions.IconColor == null ||
                hsvvalue == UIColorManager.Manager.CardRangeHsvValue[6]) return;
            var hsvColor = HSVColors.White;
            __instance.hsv_rangeIcon._HueShift = hsvColor.H;
            __instance.hsv_rangeIcon._Saturation = hsvColor.S;
            __instance.hsv_rangeIcon._ValueBrightness = hsvColor.V;
            __instance.hsv_rangeIcon.CallUpdate();
            __instance.hsv_rangeIcon.enabled = false;
            __instance.hsv_rangeIcon.enabled = true;
        }

        [HarmonyPatch(typeof(BattleDiceCardUI), "SetRangeIconHsv")]
        [HarmonyPostfix]
        public static void BattleDiceCardUI_SetRangeIconHsv(BattleDiceCardUI __instance, Vector3 hsvvalue)
        {
            if (__instance.CardModel == null) return;
            if (!ModParameters.CardOptions.TryGetValue(__instance.CardModel.GetID().packageId, out var cardOptions))
                return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == __instance.CardModel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            if (cardItem.CardColorOptions.CustomIconColor.HasValue)
                __instance.img_icon.color = cardItem.CardColorOptions.CustomIconColor.Value;
            if (__instance.hsv_rangeIcon == null) return;
            if (!cardItem.CardColorOptions.UseHSVFilter)
            {
                __instance.hsv_rangeIcon.enabled = false;
                return;
            }

            if (cardItem.CardColorOptions.IconColor == null) return;
            var hsvColor = HSVColors.White;
            __instance.hsv_rangeIcon._HueShift = hsvColor.H;
            __instance.hsv_rangeIcon._Saturation = hsvColor.S;
            __instance.hsv_rangeIcon._ValueBrightness = hsvColor.V;
            __instance.hsv_rangeIcon.CallUpdate();
            __instance.hsv_rangeIcon.enabled = false;
            __instance.hsv_rangeIcon.enabled = true;
        }

        [HarmonyPatch(typeof(BattleDiceCardUI), "SetRangeIconHsv")]
        [HarmonyPrefix]
        public static void BattleDiceCardUI_SetRangeIconHsv_Pre(BattleDiceCardUI __instance, Vector3 hsvvalue)
        {
            if (__instance.CardModel == null || __instance.hsv_rangeIcon == null) return;
            __instance.img_icon.color = Color.white;
            __instance.hsv_rangeIcon.enabled = true;
            __instance.hsv_rangeIcon._HueShift = hsvvalue.x;
            __instance.hsv_rangeIcon._Saturation = hsvvalue.y;
            __instance.hsv_rangeIcon._ValueBrightness = hsvvalue.z;
            __instance.hsv_rangeIcon.CallUpdate();
        }

        [HarmonyPatch(typeof(BattleDiceCardUI), "SetCard")]
        [HarmonyPrefix]
        public static void UIBattleCard_SetCard_Pre(BattleDiceCardUI __instance, BattleDiceCardModel cardModel)
        {
            if (cardModel == null) return;
            var frame = __instance.img_Frames.ElementAtOrDefault(0);
            if (frame != null)
                frame.overrideSprite = null;
            var rightFrame = __instance.img_Frames.ElementAtOrDefault(4);
            if (rightFrame != null)
                rightFrame.overrideSprite = null;
            var component = __instance.img_artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
            if (component != null) component.overrideSprite = null;
        }

        [HarmonyPatch(typeof(BattleDiceCardUI), "SetCard")]
        [HarmonyPostfix]
        public static void UIBattleCard_SetCard_Post(BattleDiceCardUI __instance, BattleDiceCardModel cardModel)
        {
            if (!ModParameters.CardOptions.TryGetValue(cardModel.GetID().packageId, out var cardOptions)) return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == cardModel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            if (cardItem.CardColorOptions.CardColor.HasValue)
            {
                foreach (var img in __instance.img_Frames)
                    img.color = cardItem.CardColorOptions.CardColor.Value;
                foreach (var img in __instance.img_linearDodges)
                    img.color = cardItem.CardColorOptions.CardColor.Value;
                __instance.costNumbers.SetContentColor(cardItem.CardColorOptions.CardColor.Value);
                __instance.colorFrame = cardItem.CardColorOptions.CardColor.Value;
                __instance.colorLineardodge = cardItem.CardColorOptions.CardColor.Value;
                __instance.colorLineardodge_deactive = cardItem.CardColorOptions.CardColor.Value;
                __instance.img_icon.color = cardItem.CardColorOptions.CardColor.Value;
            }

            var frame = __instance.img_Frames.ElementAtOrDefault(0);
            if (frame != null && !string.IsNullOrEmpty(cardItem.CardColorOptions.LeftFrame) &&
                ModParameters.CardArtWorks.TryGetValue(cardItem.CardColorOptions.LeftFrame, out var leftFrameImg))
            {
                frame.overrideSprite = leftFrameImg;
                frame.overrideSprite.name = $"{cardItem.CardColorOptions.LeftFrame}_LFrame";
                if (cardItem.CardColorOptions.ApplySideFrontColors && cardItem.CardColorOptions.CardColor.HasValue)
                    frame.color = cardItem.CardColorOptions.CardColor.Value;
                else frame.color = Color.white;
            }

            var rightFrame = __instance.img_Frames.ElementAtOrDefault(4);
            if (rightFrame != null && !string.IsNullOrEmpty(cardItem.CardColorOptions.RightFrame) &&
                ModParameters.CardArtWorks.TryGetValue(cardItem.CardColorOptions.RightFrame, out var rightFrameImg))
            {
                rightFrame.overrideSprite = rightFrameImg;
                rightFrame.overrideSprite.name = $"{cardItem.CardColorOptions.RightFrame}_LFrame";
                if (cardItem.CardColorOptions.ApplySideFrontColors && cardItem.CardColorOptions.CardColor.HasValue)
                    rightFrame.color = cardItem.CardColorOptions.CardColor.Value;
                else rightFrame.color = Color.white;
            }

            var component = __instance.img_artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
            if (component != null)
                if (!string.IsNullOrEmpty(cardItem.CardColorOptions.FrontFrame) &&
                    ModParameters.CardArtWorks.TryGetValue(cardItem.CardColorOptions.FrontFrame, out var frontFrameImg))
                {
                    component.overrideSprite = frontFrameImg;
                    component.overrideSprite.name = $"{cardItem.CardColorOptions.FrontFrame}_FFrame";
                    if (cardItem.CardColorOptions.ApplyFrontColor && cardItem.CardColorOptions.CardColor.HasValue)
                        component.color = cardItem.CardColorOptions.CardColor.Value;
                }

            if (!string.IsNullOrEmpty(cardItem.CardColorOptions.CustomIcon) &&
                ModParameters.ArtWorks.TryGetValue(cardItem.CardColorOptions.CustomIcon, out var icon))
                __instance.img_icon.overrideSprite = icon;
        }

        [HarmonyPatch(typeof(UICharacterBookSlot), "SetHighlighted")]
        [HarmonyPrefix]
        public static void UICharacterBookSlot_SetHighlighted_Post_Pre(UICharacterBookSlot __instance)
        {
            if (__instance.BookModel == null) return;
            __instance.BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        [HarmonyPatch(typeof(UICharacterBookSlot), "SetHighlighted")]
        [HarmonyPostfix]
        public static void UICharacterBookSlot_SetHighlighted_Post(UICharacterBookSlot __instance, bool on)
        {
            if (__instance.BookModel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.BookModel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.BookModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (on) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                foreach (var graphic in __instance._targetGraphics.Where(x => x != null))
                    graphic.CrossFadeColor(keypageItem.KeypageColorOptions.FrameColor.Value, 0.1f, true, true);
                foreach (var graphic in __instance._defaultGraphics.Where(x => x != null))
                    graphic.CrossFadeColor(keypageItem.KeypageColorOptions.FrameColor.Value, 0.1f, true, true);
            }

            var component = __instance.BookName.GetComponent<TextMeshProMaterialSetter>();
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                if (component != null)
                {
                    component.independentSetting = true;
                    component.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                    component.enabled = false;
                    component.enabled = true;
                }
                else
                {
                    __instance.BookName.fontMaterial.SetColor("_UnderlayColor",
                        keypageItem.KeypageColorOptions.NameColor.Value);
                }

                __instance.BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            __instance.BookName.gameObject.SetActive(false);
            __instance.BookName.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(UIOriginEquipPageSlot), "SetGlowColor")]
        [HarmonyPrefix]
        public static void UIEquip_SetGlowColor_Pre(UIOriginEquipPageSlot __instance, Color gc)
        {
            if (__instance.BookDataModel == null ||
                gc == UIColorManager.Manager.GetUIColor(UIColor.Highlighted)) return;
            __instance.BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        [HarmonyPatch(typeof(UIOriginEquipPageSlot), "SetGlowColor")]
        [HarmonyPostfix]
        public static void UIEquip_SetGlowColor(UIOriginEquipPageSlot __instance, Color gc)
        {
            if (__instance.BookDataModel == null ||
                gc == UIColorManager.Manager.GetUIColor(UIColor.Highlighted)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.BookDataModel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.BookDataModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                __instance.setter_BookName.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                __instance.setter_BookName.InitMaterialProperty();
                __instance.BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            __instance.Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.FrameGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianEquipDeckPanel), "SetDefaultColorPanel")]
        [HarmonyPrefix]
        public static void UILibrarianEquipDeckPanel_SetDefaultColorPanel_Pre(UILibrarianEquipDeckPanel __instance)
        {
            if (__instance._unitdata == null) return;
            __instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        [HarmonyPatch(typeof(UILibrarianEquipDeckPanel), "SetDefaultColorPanel")]
        [HarmonyPostfix]
        public static void UILibrarianEquipDeckPanel_SetDefaultColorPanel(UILibrarianEquipDeckPanel __instance)
        {
            if (__instance._unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance._unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem =
                keypageOptions.FirstOrDefault(x => x.KeypageId == __instance._unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                __instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
                __instance.setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            __instance.setter_bookname.enabled = false;
            __instance.setter_bookname.enabled = true;
            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            __instance.img_BookIcon.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.img_BookIconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.Frames.SetColor(keypageItem.KeypageColorOptions.FrameColor.Value);
            __instance.img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.img_LineFrame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.DeckListPanel.img_DeckFrame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIEquipPageModelPreviewPanel), "SetData")]
        [HarmonyPatch(typeof(UIEquipPagePreviewPanel), "SetData")]
        [HarmonyPrefix]
        public static void UIEquipPage_SetData_Pre(object __instance)
        {
            switch (__instance)
            {
                case UIEquipPageModelPreviewPanel instance:
                    ArtUtil.UIEquipPageSetDataPre(instance);
                    break;
                case UIEquipPagePreviewPanel instance:
                    ArtUtil.UIEquipPageSetDataPre(instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIEquipPageModelPreviewPanel), "SetData")]
        [HarmonyPatch(typeof(UIEquipPagePreviewPanel), "SetData")]
        [HarmonyPostfix]
        public static void UIEquipPage_SetData(object __instance)
        {
            switch (__instance)
            {
                case UIEquipPageModelPreviewPanel instance:
                    ArtUtil.UIEquipPageSetDataPost(instance);
                    break;
                case UIEquipPagePreviewPanel instance:
                    ArtUtil.UIEquipPageSetDataPost(instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(UILibrarianInfoPanel), "SetFrameColor")]
        [HarmonyPostfix]
        public static void UILibrarianInfoPanel_SetFrameColor(UILibrarianInfoPanel __instance)
        {
            if (__instance._selectedUnit == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance._selectedUnit.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem =
                keypageOptions.FirstOrDefault(x => x.KeypageId == __instance._selectedUnit.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            __instance.Frames.SetColor(keypageItem.KeypageColorOptions.FrameColor.Value);
        }

        [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetUnitData")]
        [HarmonyPrefix]
        public static void UILibrarianEquipBookInfoPanel_SetUnitData_Pre(UILibrarianEquipBookInfoPanel __instance,
            UnitDataModel data)
        {
            if (data == null) return;
            __instance.bookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
            ArtUtil.ChangeColorToCombatPageList(UIColorManager.Manager.GetUIColor(UIColor.Default));
        }

        [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetUnitData")]
        [HarmonyPostfix]
        public static void UILibrarianEquipBookInfoPanel_SetUnitData(UILibrarianEquipBookInfoPanel __instance,
            UnitDataModel data)
        {
            if (data == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(data.bookItem.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == data.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            var component = __instance.bookName.GetComponent<TextMeshProMaterialSetter>();
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                __instance.bookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
                component.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            if (__instance.icon != null)
                __instance.icon.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            foreach (var graphic in __instance.targetGraphics)
                graphic.CrossFadeColor(keypageItem.KeypageColorOptions.FrameColor.Value, 0.1f, true, true);
            component.InitMaterialProperty();
            ArtUtil.ChangeColorToCombatPageList(keypageItem.KeypageColorOptions.FrameColor.Value);
        }

        [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetPassiveSlotColor")]
        [HarmonyPostfix]
        public static void UILibrarianEquipBookInfoPanel_SetPassiveSlotColor(UILibrarianEquipBookInfoPanel __instance,
            Color c)
        {
            if (__instance.unitData == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitData.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitData.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in __instance.graphic_passivesSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionPreviewBookPanel), "SetColorByRarity")]
        [HarmonyPatch(typeof(UIPassiveSuccessionCenterEquipBookSlot), "SetColorByRarity")]
        [HarmonyPrefix]
        public static void UIPassiveSuccessionPanel_SetColorByRarity_Pre(object __instance)
        {
            switch (__instance)
            {
                case UIPassiveSuccessionPreviewBookPanel instance:
                    ArtUtil.UIPassiveSuccessionPanelSetColorByRarityPre(instance);
                    break;
                case UIPassiveSuccessionCenterEquipBookSlot instance:
                    ArtUtil.UIPassiveSuccessionPanelSetColorByRarityPre(instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionPreviewBookPanel), "SetColorByRarity")]
        [HarmonyPatch(typeof(UIPassiveSuccessionCenterEquipBookSlot), "SetColorByRarity")]
        [HarmonyPostfix]
        public static void UIPassiveSuccessionPanel_SetColorByRarity(object __instance)
        {
            switch (__instance)
            {
                case UIPassiveSuccessionPreviewBookPanel instance:
                    ArtUtil.UIPassiveSuccessionPanelSetColorByRarityPost(instance);
                    break;
                case UIPassiveSuccessionCenterEquipBookSlot instance:
                    ArtUtil.UIPassiveSuccessionPanelSetColorByRarityPost(instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionEquipBookSlot), "SetRarityColor")]
        [HarmonyPatch(typeof(UIPassiveEquipBookSlot), "SetRarityColor")]
        [HarmonyPrefix]
        public static void UIPassiveSuccessionEquipBookSlot_SetRarityColor_Pre(object __instance)
        {
            switch (__instance)
            {
                case UIPassiveEquipBookSlot instance:
                    ArtUtil.UIPassiveSuccessionEquipBookSlotSetRarityColorPre(instance);
                    break;
                case UIPassiveSuccessionEquipBookSlot instance:
                    ArtUtil.UIPassiveSuccessionEquipBookSlotSetRarityColorPre(instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionEquipBookSlot), "SetRarityColor")]
        [HarmonyPatch(typeof(UIPassiveEquipBookSlot), "SetRarityColor")]
        [HarmonyPostfix]
        public static void UIPassiveSuccessionEquipBookSlot_SetRarityColor(object __instance, Color c)
        {
            switch (__instance)
            {
                case UIPassiveEquipBookSlot instance:
                    ArtUtil.UIPassiveSuccessionEquipBookSlotSetRarityColorPost(instance, c);
                    break;
                case UIPassiveSuccessionEquipBookSlot instance:
                    ArtUtil.UIPassiveSuccessionEquipBookSlotSetRarityColorPost(instance, c);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionBookSlot), "SetDefaultColor")]
        [HarmonyPrefix]
        public static void UIPassiveSuccessionBookSlot_SetDefaultColor_Pre(UIPassiveSuccessionBookSlot __instance)
        {
            if (__instance.currentbookmodel == null) return;
            __instance.txt_bookname.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionBookSlot), "SetDefaultColor")]
        [HarmonyPostfix]
        public static void UIPassiveSuccessionBookSlot_SetDefaultColor(UIPassiveSuccessionBookSlot __instance)
        {
            if (__instance.currentbookmodel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.currentbookmodel.BookId.packageId,
                    out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.currentbookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                __instance.txt_bookname.color = keypageItem.KeypageColorOptions.NameColor.Value;
                __instance.txt_bookname.enabled = false;
                __instance.txt_bookname.enabled = true;
                var component = __instance.txt_bookname.GetComponent<TextMeshProMaterialSetter>();
                if (component != null)
                    component.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                if (__instance.txt_bookname.isActiveAndEnabled)
                    __instance.txt_bookname.fontMaterial.SetColor("_UnderlayColor",
                        keypageItem.KeypageColorOptions.NameColor.Value);
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            __instance.equipSet.Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (__instance.equipSet.FrameGlow != null)
                __instance.equipSet.FrameGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (__instance.img_IconGlow != null)
                __instance.img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (__instance.img_levelFrame != null)
                __instance.img_levelFrame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (__instance.txt_booklevel != null)
                __instance.txt_booklevel.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetData")]
        [HarmonyPrefix]
        public static void UILibrarianInfoInCardPhase_SetData_Pre(UILibrarianInfoInCardPhase __instance)
        {
            if (__instance.unitdata == null) return;
            __instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetData")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_SetData(UILibrarianInfoInCardPhase __instance)
        {
            if (__instance.unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                if (UI.UIController.Instance.CurrentUIPhase != UIPhase.Main_ItemList)
                    foreach (var t in __instance.graphic_Frames)
                        t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                __instance.img_BookIconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            __instance.setter_bookname.Awake();
            __instance.setter_bookname.Start();
            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            __instance.setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            __instance.setter_bookname.enabled = false;
            __instance.setter_bookname.enabled = true;
            __instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetPassiveSlotColor")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_SetPassiveSlotColor(UILibrarianInfoInCardPhase __instance,
            Color c)
        {
            if (__instance.unitdata == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in __instance.graphic_passivesSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetBattlePageSlotColor")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_SetBattlePageSlotColor(UILibrarianInfoInCardPhase __instance,
            Color c)
        {
            if (__instance.unitdata == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in __instance.graphic_battlepageSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "OnPointerExitEquipPage")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_OnPointerExitEquipPage(UILibrarianInfoInCardPhase __instance)
        {
            if (__instance.unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var t in __instance.graphic_Frames) t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIGachaEquipSlot), "SetDefaultColor")]
        [HarmonyPrefix]
        public static void UIGachaEquipSlot_SetDefaultColor_Pre(UIGachaEquipSlot __instance)
        {
            if (__instance._book == null) return;
            __instance.BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        [HarmonyPatch(typeof(UIGachaEquipSlot), "SetDefaultColor")]
        [HarmonyPostfix]
        public static void UIGachaEquipSlot_SetDefaultColor(UIGachaEquipSlot __instance)
        {
            if (__instance._book == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance._book.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance._book.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                __instance.BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
                var component = __instance.BookName.GetComponent<TextMeshProMaterialSetter>();
                if (component != null)
                    component.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                if (__instance.BookName.isActiveAndEnabled)
                    __instance.BookName.fontMaterial.SetColor("_UnderlayColor",
                        keypageItem.KeypageColorOptions.NameColor.Value);
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            __instance.Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.FrameGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.Icon.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UICustomSelectable), "OnPointerExit")]
        [HarmonyPostfix]
        public static async void UICustomSelectable_OnPointerExit(UICustomSelectable __instance,
            PointerEventData eventData)
        {
            if (!__instance.gameObject.name.Contains("[Button]CustomSelectableGraphic")) return;
            if (UI.UIController.Instance.CurrentUIPhase != UIPhase.Librarian) return;
            if (!ModParameters.KeypageOptions.TryGetValue(
                    UI.UIController.Instance.CurrentUnit.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x =>
                x.KeypageId == UI.UIController.Instance.CurrentUnit.bookItem.BookId.id);
            var color = keypageItem?.KeypageColorOptions?.FrameColor ??
                        UIColorManager.Manager.GetUIColor(UIColor.Default);
            if (eventData.pointerCurrentRaycast.gameObject != null &&
                eventData.pointerCurrentRaycast.gameObject.name.Contains("[Xbox]SelectableTarget")) return;
            await GenericUtil.PutTaskDelay(30);
            ArtUtil.ChangeColorToCombatPageList(color);
        }

        [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetEquipPageSlotColor")]
        [HarmonyPrefix]
        public static void UIBattleSettingLibrarianInfoPanel_SetEquipPageSlotColor_Pre(
            UIBattleSettingLibrarianInfoPanel __instance)
        {
            if (__instance.unitdata == null) return;
            __instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
            foreach (var img in __instance.GetComponentsInChildren<Image>()
                         .Where(x => x.name.Contains("[Image]CenterFrame")))
                img.color = __instance.isSephirahPanel
                    ? UIColorManager.Manager.GetUIColor(UIColor.Default)
                    : UIColorManager.Manager.GetUIColor(UIColor.Disabled);
        }

        [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetEquipPageSlotColor")]
        [HarmonyPostfix]
        public static void UIBattleSettingLibrarianInfoPanel_SetEquipPageSlotColor(
            UIBattleSettingLibrarianInfoPanel __instance)
        {
            if (__instance.unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                __instance.setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                __instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            __instance.setter_bookname.InitMaterialProperty();
            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            __instance.img_BookIconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            foreach (var graphic in __instance.graphic_Frames.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            foreach (var img in __instance.GetComponentsInChildren<Image>()
                         .Where(x => x.name.Contains("[Image]CenterFrame")))
                img.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetBattlePageSlotColor")]
        [HarmonyPostfix]
        public static void UIBattleSettingLibrarianInfoPanel_SetBattlePageSlotColor_Post(
            UIBattleSettingLibrarianInfoPanel __instance, Color c)
        {
            if (__instance.unitdata == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in __instance.graphic_battlepageSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIBookSlot), "SetGlowColor")]
        [HarmonyPostfix]
        public static void UIBookSlot_SetGlowColor(UIBookSlot __instance, Color c)
        {
            if (__instance.BookId == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted) ||
                c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.DropBookOptions.TryGetValue(__instance.BookId.packageId,
                    out var dropBookOptions)) return;
            var dropBookOption = dropBookOptions.FirstOrDefault(x => x.DropBookId == __instance.BookId.id);
            if (dropBookOption?.DropBookColorOptions == null) return;
            if (dropBookOption.DropBookColorOptions.FrameColor.HasValue)
            {
                __instance.Frame.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                __instance.FrameGlow.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                __instance.Icon.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                if (__instance.IconGlow != null)
                    __instance.IconGlow.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
            }

            if (!dropBookOption.DropBookColorOptions.NameColor.HasValue) return;
            __instance.BookName.color = dropBookOption.DropBookColorOptions.NameColor.Value;
            var component = __instance.BookName.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = dropBookOption.DropBookColorOptions.NameColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        [HarmonyPatch(typeof(UIInvitationDropBookSlot), "SetColor")]
        [HarmonyPatch(typeof(UIAddedFeedBookSlot), "SetColor")]
        [HarmonyPostfix]
        public static void UIInvitation_SetColor(object __instance, Color c)
        {
            switch (__instance)
            {
                case UIAddedFeedBookSlot instance:
                    ArtUtil.UIInvitationSetColorPost(instance, c);
                    break;
                case UIInvitationDropBookSlot instance:
                    ArtUtil.UIInvitationSetColorPost(instance, c);
                    break;
            }
        }

        [HarmonyPatch(typeof(UIInvenFeedBookSlot), "SetColor")]
        [HarmonyPostfix]
        public static void UIInvenFeedBookSlot_SetColor(UIInvenFeedBookSlot __instance, Color c)
        {
            if (__instance.BookId == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted) ||
                c == UIColorManager.Manager.GetUIColor(UIColor.Disabled) ||
                __instance.bookNumRoot == null) return;
            if (!ModParameters.DropBookOptions.TryGetValue(__instance.BookId.packageId,
                    out var dropBookOptions)) return;
            var dropBookOption = dropBookOptions.FirstOrDefault(x => x.DropBookId == __instance.BookId.id);
            if (dropBookOption?.DropBookColorOptions == null) return;
            if (dropBookOption.DropBookColorOptions.FrameColor.HasValue)
            {
                __instance.bookNumBg.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                __instance.plusButton.specialColor = dropBookOption.DropBookColorOptions.FrameColor.Value;
                __instance.plusButton.SetDefault();
                __instance.minusButton.specialColor = dropBookOption.DropBookColorOptions.FrameColor.Value;
                __instance.minusButton.SetDefault();
            }

            if (!dropBookOption.DropBookColorOptions.NameColor.HasValue) return;
            __instance.txt_bookNum.color = dropBookOption.DropBookColorOptions.NameColor.Value;
        }

        [HarmonyPatch(typeof(UIInvitationBookSlot), "SetColor", typeof(Color))]
        [HarmonyPostfix]
        public static void UIInvitationBookSlot_SetColor(UIInvitationBookSlot __instance, Color c)
        {
            if (__instance.Appliedbookid == null) return;
            if (c == UIColorManager.Manager.GetUIColor(UIColor.Default))
            {
                if (!ModParameters.DropBookOptions.TryGetValue(__instance.Appliedbookid.packageId,
                        out var dropBookOptions)) return;
                var dropBookOption = dropBookOptions.FirstOrDefault(x => x.DropBookId == __instance.Appliedbookid.id);
                if (dropBookOption?.DropBookColorOptions == null) return;
                if (dropBookOption.DropBookColorOptions.FrameColor.HasValue)
                {
                    __instance.Frame.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                    __instance.FrameGlow.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                    __instance.Icon.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                    if (__instance.IconGlow != null)
                        __instance.IconGlow.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
                }

                if (!dropBookOption.DropBookColorOptions.NameColor.HasValue) return;
                __instance.BookName.color = dropBookOption.DropBookColorOptions.NameColor.Value;
                __instance.BookName.fontMaterial.SetColor("_UnderlayColor",
                    dropBookOption.DropBookColorOptions.NameColor.Value);
            }
            else
            {
                var recipe = __instance.rootPanel.GetBookRecipe();
                if (recipe == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted) ||
                    c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
                if (!ModParameters.StageOptions.TryGetValue(recipe.id.packageId, out var stageOptions)) return;
                var stageOption = stageOptions.FirstOrDefault(x => x.StageId == recipe.id.id);
                if (stageOption?.StageColorOptions == null) return;
                if (stageOption.StageColorOptions.FrameColor.HasValue)
                {
                    __instance.Frame.color = stageOption.StageColorOptions.FrameColor.Value;
                    __instance.FrameGlow.color = stageOption.StageColorOptions.FrameColor.Value;
                    __instance.Icon.color = stageOption.StageColorOptions.FrameColor.Value;
                    if (__instance.IconGlow != null)
                        __instance.IconGlow.color = stageOption.StageColorOptions.FrameColor.Value;
                }

                if (!stageOption.StageColorOptions.TextColor.HasValue) return;
                __instance.BookName.color = stageOption.StageColorOptions.TextColor.Value;
                __instance.BookName.fontMaterial.SetColor("_UnderlayColor",
                    stageOption.StageColorOptions.TextColor.Value);
            }
        }

        [HarmonyPatch(typeof(UIInvitationRightMainPanel), "SetColorAllFrames")]
        [HarmonyPostfix]
        public static void UIInvitationRightMainPanel_SetColorAllFrames(UIInvitationRightMainPanel __instance, Color c)
        {
            var recipe = __instance.GetBookRecipe();
            if (recipe == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted) ||
                c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.StageOptions.TryGetValue(recipe.id.packageId, out var stageOptions)) return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == recipe.id.id);
            if (stageOption?.StageColorOptions == null) return;
            if (stageOption.StageColorOptions.FrameColor.HasValue)
            {
                foreach (var t in __instance.AllFrames)
                    t.CrossFadeColor(stageOption.StageColorOptions.FrameColor.Value, 0f, false, false);
                var color = stageOption.StageColorOptions.FrameColor.Value;
                color.a = 0.5f;
                __instance.ButtonFrameHighlight.GetComponent<Image>().color = color;
                if (__instance.button_SendButton.interactable)
                    __instance.button_SendButton.SetColor(stageOption.StageColorOptions.FrameColor.Value);
            }

            if (!stageOption.StageColorOptions.TextColor.HasValue) return;
            foreach (var textMeshProMaterialSetter in __instance.setter_changetxts)
            {
                if (textMeshProMaterialSetter.isActiveAndEnabled)
                    textMeshProMaterialSetter.underlayColor = stageOption.StageColorOptions.TextColor.Value;
                textMeshProMaterialSetter.enabled = false;
                textMeshProMaterialSetter.enabled = true;
            }
        }

        [HarmonyPatch(typeof(UIInvitationRightMainPanel), "OnPointerExit_SendButton")]
        [HarmonyPostfix]
        public static void UIInvitationRightMainPanel_OnPointerExit_SendButton(UIInvitationRightMainPanel __instance)
        {
            var recipe = __instance.GetBookRecipe();
            if (recipe == null || __instance.currentColor == UIColorManager.Manager.GetUIColor(UIColor.Default) ||
                __instance.currentColor == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.StageOptions.TryGetValue(recipe.id.packageId, out var stageOptions)) return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == recipe.id.id);
            if (stageOption?.StageColorOptions?.FrameColor == null) return;
            if (__instance.button_SendButton.interactable)
                __instance.button_SendButton.SetColor(stageOption.StageColorOptions.FrameColor.Value);
        }

        [HarmonyPatch(typeof(UIEquipPageCustomizeSlot), "SetData", typeof(WorkshopSkinData))]
        [HarmonyPostfix]
        public static void UIEquipPageCustomizeSlot_SetData(object __instance, object data)
        {
            if (!(data is WorkshopSkinDataExtension workshopData) ||
                !(__instance is UIOriginEquipPageSlot instance)) return;
            if (!workshopData.RealKeypageId.HasValue) return;
            instance._bookDataModel =
                new BookModel(Singleton<BookXmlList>.Instance.GetData(new LorId(workshopData.PackageId,
                    workshopData.RealKeypageId.Value)));
            var sprite = UISpriteDataManager.instance.GetStoryIcon(instance._bookDataModel.ClassInfo.BookIcon);
            instance.Icon.sprite = sprite.icon;
            instance.IconGlow.sprite = sprite.iconGlow;
            if (!ModParameters.KeypageOptions.TryGetValue(workshopData.PackageId, out var keypageOptions)) return;
            var keypageOption = keypageOptions.FirstOrDefault(x => x.KeypageId == workshopData.RealKeypageId);
            if (keypageOption?.KeypageColorOptions?.FrameColor == null) return;
            instance.SetGlowColor(keypageOption.KeypageColorOptions.FrameColor.Value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
        [HarmonyPatch(typeof(UIEmotionPassiveCardInven), "SetSprites")]
        public static void EmotionPassiveCardUI_SetSprites_Pre(object __instance)
        {
            switch (__instance)
            {
                case EmotionPassiveCardUI instance:
                    ArtUtil.EmotionPassiveCardUISetSpritesPre(instance);
                    break;
                case UIEmotionPassiveCardInven instance:
                    ArtUtil.EmotionPassiveCardUISetSpritesPre(instance);
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
            if (!UtilExtensions.GetEmotionCardOptions(cardExtension.LorId.packageId, cardExtension.LorId.id,
                    out var cardOptions) || cardOptions.ColorOptions == null) return;
            if (cardOptions.ColorOptions.FrameColor.HasValue)
                __instance.frame.color = cardOptions.ColorOptions.FrameColor.Value;
            if (!cardOptions.ColorOptions.TextColor.HasValue) return;
            __instance.cardName.color = cardOptions.ColorOptions.TextColor.Value;
            __instance.cardName.GetComponent<TextMeshProMaterialSetter>().underlayColor =
                cardOptions.ColorOptions.TextColor.Value;
            __instance.cardName.gameObject.SetActive(false);
            __instance.cardName.gameObject.SetActive(true);
            __instance.cardLevel.color = cardOptions.ColorOptions.TextColor.Value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleDialogUI), "TurnOnAbnormalityDlg")]
        public static void BattleDialogUI_TurnOnAbnormalityDlg(BattleDialogUI __instance, object card)
        {
            if (!(card is EmotionCardXmlExtension cardExtension) ||
                !UtilExtensions.GetEmotionCardOptions(cardExtension.LorId.packageId, cardExtension.LorId.id,
                    out var cardOptions) || cardOptions.ColorOptions?.TextColor == null) return;
            __instance._txtAbnormalityDlg.fontMaterial.SetColor("_GlowColor", cardOptions.ColorOptions.TextColor.Value);
            __instance._txtAbnormalityDlg.color = cardOptions.ColorOptions.TextColor.Value;
            __instance._txtAbnormalityDlg.GetComponent<AbnormalityDlgEffect>().Init();
        }
    }
}