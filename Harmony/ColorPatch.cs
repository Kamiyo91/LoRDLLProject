﻿using System;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        private static void UILibrarianEquipInfoSlot_SetData_Post(BookPassiveInfo passive, Image ___Frame,
            TextMeshProUGUI ___txt_cost)
        {
            if (!ModParameters.PassiveOptions.TryGetValue(passive.passive.id.packageId, out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x => x.PassiveId == passive.passive.id.id);
            if (passiveItem?.PassiveColorOptions == null) return;
            if (passiveItem.PassiveColorOptions.FillColor.HasValue)
                ___Frame.color = passiveItem.PassiveColorOptions.FillColor.Value;
            if (passiveItem.PassiveColorOptions.TextColor.HasValue)
                ___txt_cost.color = passiveItem.PassiveColorOptions.TextColor.Value;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionSlot), "SetColorByRarity")]
        [HarmonyPostfix]
        private static void UIPassiveColor_SetColorByRarity(PassiveModel ___passivemodel,
            List<Graphic> ___rarityGraphics, Color c)
        {
            if (___passivemodel == null || c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.PassiveOptions.TryGetValue(___passivemodel.reservedData.currentpassive.id.packageId,
                    out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x => x.PassiveId == ___passivemodel.reservedData.currentpassive.id.id);
            if (passiveItem?.PassiveColorOptions?.FillColor == null) return;
            foreach (var graphic in ___rarityGraphics)
                graphic.CrossFadeColor(passiveItem.PassiveColorOptions.FillColor.Value, 0f, true, true);
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionCenterPassiveSlot), "SetData")]
        [HarmonyPatch(typeof(UIPassiveSuccessionPreviewPassiveSlot), "SetData")]
        [HarmonyPostfix]
        public static void UIPassive_SetData_Post(PassiveModel passive, List<Graphic> ___graphics_Rarity)
        {
            if (!ModParameters.PassiveOptions.TryGetValue(passive.originData.currentpassive.id.packageId,
                    out var passiveOptions)) return;
            var passiveItem =
                passiveOptions.FirstOrDefault(x => x.PassiveId == passive.originData.currentpassive.id.id);
            if (passiveItem?.PassiveColorOptions?.FillColor == null) return;
            foreach (var graphic in ___graphics_Rarity)
                graphic.color = passiveItem.PassiveColorOptions.FillColor.Value;
        }

        [HarmonyPatch(typeof(UIDetailEgoCardSlot), "SetData")]
        [HarmonyPatch(typeof(UIOriginCardSlot), "SetData")]
        [HarmonyPostfix]
        public static void UICard_SetData_Post(DiceCardItemModel cardmodel, Image[] ___img_Frames,
            Image[] ___img_linearDodge, NumbersData ___costNumbers, Image ___img_RangeIcon, ref Color ___colorFrame,
            ref Color ___colorLineardodge)
        {
            if (cardmodel == null) return;
            if (!ModParameters.CardOptions.TryGetValue(cardmodel.GetID().packageId, out var cardOptions)) return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == cardmodel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            if (cardItem.CardColorOptions.CardColor.HasValue)
            {
                foreach (var img in ___img_Frames)
                    img.color = cardItem.CardColorOptions.CardColor.Value;
                foreach (var img in ___img_linearDodge)
                    img.color = cardItem.CardColorOptions.CardColor.Value;
                ___costNumbers.SetContentColor(cardItem.CardColorOptions.CardColor.Value);
                ___colorFrame = cardItem.CardColorOptions.CardColor.Value;
                ___colorLineardodge = cardItem.CardColorOptions.CardColor.Value;
                ___img_RangeIcon.color = cardItem.CardColorOptions.CardColor.Value;
            }

            if (string.IsNullOrEmpty(cardItem.CardColorOptions.CustomIcon) ||
                !ModParameters.ArtWorks.TryGetValue(cardItem.CardColorOptions.CustomIcon, out var icon)) return;
            ___img_RangeIcon.overrideSprite = icon;
        }

        [HarmonyPatch(typeof(UIOriginCardSlot), "SetRangeIconHsv")]
        [HarmonyPostfix]
        public static void UICard_SetRangeIconHsv_Post(UIOriginCardSlot __instance, Vector3 hsvvalue,
            RefineHsv ___hsv_rangeIcon, Image ___img_RangeIcon)
        {
            if (__instance.CardModel == null || hsvvalue == UIColorManager.Manager.CardRangeHsvValue[6]) return;
            if (!ModParameters.CardOptions.TryGetValue(__instance.CardModel.GetID().packageId, out var cardOptions))
                return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == __instance.CardModel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            if (___hsv_rangeIcon == null) return;
            if (cardItem.CardColorOptions.CustomIconColor.HasValue)
                ___img_RangeIcon.color = cardItem.CardColorOptions.CustomIconColor.Value;
            if (!cardItem.CardColorOptions.UseHSVFilter)
            {
                ___hsv_rangeIcon.enabled = false;
                return;
            }

            if (cardItem.CardColorOptions.IconColor == null ||
                hsvvalue == UIColorManager.Manager.CardRangeHsvValue[6]) return;
            var hsvColor = HSVColors.White;
            ___hsv_rangeIcon._HueShift = hsvColor.H;
            ___hsv_rangeIcon._Saturation = hsvColor.S;
            ___hsv_rangeIcon._ValueBrightness = hsvColor.V;
            ___hsv_rangeIcon.CallUpdate();
            ___hsv_rangeIcon.enabled = false;
            ___hsv_rangeIcon.enabled = true;
        }

        [HarmonyPatch(typeof(BattleDiceCardUI), "SetCard")]
        [HarmonyPostfix]
        public static void UIBattleCard_SetCard_Post(BattleDiceCardModel cardModel, Image[] ___img_Frames,
            Image[] ___img_linearDodges, NumbersData ___costNumbers, Image ___img_icon, RefineHsv ___hsv_rangeIcon,
            ref Color ___colorFrame, ref Color ___colorLineardodge, ref Color ___colorLineardodge_deactive)
        {
            if (cardModel == null) return;
            if (!ModParameters.CardOptions.TryGetValue(cardModel.GetID().packageId, out var cardOptions)) return;
            var cardItem = cardOptions.FirstOrDefault(x => x.CardId == cardModel.GetID().id);
            if (cardItem?.CardColorOptions == null) return;
            if (cardItem.CardColorOptions.CardColor.HasValue)
            {
                foreach (var img in ___img_Frames)
                    img.color = cardItem.CardColorOptions.CardColor.Value;
                foreach (var img in ___img_linearDodges)
                    img.color = cardItem.CardColorOptions.CardColor.Value;
                ___costNumbers.SetContentColor(cardItem.CardColorOptions.CardColor.Value);
                ___colorFrame = cardItem.CardColorOptions.CardColor.Value;
                ___colorLineardodge = cardItem.CardColorOptions.CardColor.Value;
                ___colorLineardodge_deactive = cardItem.CardColorOptions.CardColor.Value;
                ___img_icon.color = cardItem.CardColorOptions.CardColor.Value;
            }

            if (!string.IsNullOrEmpty(cardItem.CardColorOptions.CustomIcon) &&
                ModParameters.ArtWorks.TryGetValue(cardItem.CardColorOptions.CustomIcon, out var icon))
                ___img_icon.overrideSprite = icon;
            if (cardItem.CardColorOptions.CustomIconColor.HasValue)
                ___img_icon.color = cardItem.CardColorOptions.CustomIconColor.Value;
            if (___hsv_rangeIcon == null) return;
            if (!cardItem.CardColorOptions.UseHSVFilter)
            {
                ___hsv_rangeIcon.enabled = false;
                return;
            }

            if (cardItem.CardColorOptions.IconColor == null) return;
            var hsvColor = HSVColors.White;
            ___hsv_rangeIcon._HueShift = hsvColor.H;
            ___hsv_rangeIcon._Saturation = hsvColor.S;
            ___hsv_rangeIcon._ValueBrightness = hsvColor.V;
            ___hsv_rangeIcon.CallUpdate();
            ___hsv_rangeIcon.enabled = false;
            ___hsv_rangeIcon.enabled = true;
        }

        [HarmonyPatch(typeof(UICharacterBookSlot), "SetHighlighted")]
        [HarmonyPrefix]
        public static void UICharacterBookSlot_SetHighlighted_Post_Pre(UICharacterBookSlot __instance,
            TextMeshProUGUI ___BookName)
        {
            if (__instance.BookModel == null) return;
            ___BookName.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UICharacterBookSlot), "SetHighlighted")]
        [HarmonyPostfix]
        public static void UICharacterBookSlot_SetHighlighted_Post(UICharacterBookSlot __instance, bool on,
            TextMeshProUGUI ___BookName, List<Graphic> ____defaultGraphics, List<Graphic> ____targetGraphics)
        {
            if (__instance.BookModel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.BookModel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.BookModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (on) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                foreach (var graphic in ____targetGraphics.Where(x => x != null))
                    graphic.CrossFadeColor(keypageItem.KeypageColorOptions.FrameColor.Value, 0.1f, true, true);
                foreach (var graphic in ____defaultGraphics.Where(x => x != null))
                    graphic.CrossFadeColor(keypageItem.KeypageColorOptions.FrameColor.Value, 0.1f, true, true);
            }

            var component = ___BookName.GetComponent<TextMeshProMaterialSetter>();
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
                    ___BookName.fontMaterial.SetColor("_UnderlayColor",
                        keypageItem.KeypageColorOptions.NameColor.Value);
                }

                ___BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            ___BookName.gameObject.SetActive(false);
            ___BookName.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(UIOriginEquipPageSlot), "SetGlowColor")]
        [HarmonyPrefix]
        public static void UIEquip_SetGlowColor_Pre(UIOriginEquipPageSlot __instance, Color gc,
            TextMeshProUGUI ___BookName)
        {
            if (__instance.BookDataModel == null || gc == LoRColorUtil.HighlightColor) return;
            ___BookName.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UIOriginEquipPageSlot), "SetGlowColor")]
        [HarmonyPostfix]
        public static void UIEquip_SetGlowColor(UIOriginEquipPageSlot __instance, Color gc, Image ___Frame,
            Image ___IconGlow, Image ___FrameGlow, TextMeshProMaterialSetter ___setter_BookName,
            TextMeshProUGUI ___BookName)
        {
            if (__instance.BookDataModel == null || gc == LoRColorUtil.HighlightColor) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.BookDataModel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.BookDataModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                ___setter_BookName.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                ___setter_BookName.InitMaterialProperty();
                ___BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            ___Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            ___IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            ___FrameGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianEquipDeckPanel), "SetDefaultColorPanel")]
        [HarmonyPrefix]
        public static void UILibrarianEquipDeckPanel_SetDefaultColorPanel_Pre(UnitDataModel ____unitdata,
            TextMeshProUGUI ___txt_BookName)
        {
            if (____unitdata == null) return;
            ___txt_BookName.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UILibrarianEquipDeckPanel), "SetDefaultColorPanel")]
        [HarmonyPostfix]
        public static void UILibrarianEquipDeckPanel_SetDefaultColorPanel(UILibrarianEquipDeckPanel __instance,
            UnitDataModel ____unitdata, Image ___img_BookIcon, TextMeshProUGUI ___txt_BookName,
            Image ___img_BookIconGlow, TextMeshProMaterialSetter ___setter_bookname, Image ___img_LineFrame,
            Image ___img_Frame, GraphicBundle ___Frames)
        {
            if (____unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(____unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ____unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                ___txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
                ___setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            ___setter_bookname.enabled = false;
            ___setter_bookname.enabled = true;
            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            ___img_BookIcon.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            ___img_BookIconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            ___Frames.SetColor(keypageItem.KeypageColorOptions.FrameColor.Value);
            ___img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            ___img_LineFrame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            __instance.DeckListPanel.img_DeckFrame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIEquipPageModelPreviewPanel), "SetData")]
        [HarmonyPatch(typeof(UIEquipPagePreviewPanel), "SetData")]
        [HarmonyPrefix]
        public static void UIEquipPage_SetData_Pre(BookModel ___bookDataModel, TextMeshProUGUI ___txt_BookName)
        {
            if (___bookDataModel == null) return;
            ___txt_BookName.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UIEquipPageModelPreviewPanel), "SetData")]
        [HarmonyPatch(typeof(UIEquipPagePreviewPanel), "SetData")]
        [HarmonyPostfix]
        public static void UIEquipPage_SetData(BookModel ___bookDataModel, TextMeshProMaterialSetter ___setter_bookname,
            Graphic[] ___graphic_Frames, TextMeshProUGUI ___txt_BookName)
        {
            if (___bookDataModel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___bookDataModel.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___bookDataModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
                foreach (var t in ___graphic_Frames)
                    t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            ___setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            ___setter_bookname.enabled = false;
            ___setter_bookname.enabled = true;
            ___txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoPanel), "SetFrameColor")]
        [HarmonyPostfix]
        public static void UILibrarianInfoPanel_SetFrameColor(UnitDataModel ____selectedUnit, GraphicBundle ___Frames)
        {
            if (____selectedUnit == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(____selectedUnit.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ____selectedUnit.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            ___Frames.SetColor(keypageItem.KeypageColorOptions.FrameColor.Value);
        }

        [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetUnitData")]
        [HarmonyPrefix]
        public static void UILibrarianEquipBookInfoPanel_SetUnitData_Pre(UnitDataModel data,
            TextMeshProUGUI ___bookName)
        {
            if (data == null) return;
            ___bookName.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetUnitData")]
        [HarmonyPostfix]
        public static void UILibrarianEquipBookInfoPanel_SetUnitData(UnitDataModel data,
            List<Graphic> ___targetGraphics, TextMeshProUGUI ___bookName, Image ___icon)
        {
            if (data == null) return;
            ArtUtil.ChangeColorToCombatPageList(LoRColorUtil.DefaultColor);
            if (!ModParameters.KeypageOptions.TryGetValue(data.bookItem.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == data.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            var component = ___bookName.GetComponent<TextMeshProMaterialSetter>();
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                ___bookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
                component.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            if (___icon != null)
                ___icon.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            foreach (var graphic in ___targetGraphics)
                graphic.CrossFadeColor(keypageItem.KeypageColorOptions.FrameColor.Value, 0.1f, true, true);
            component.InitMaterialProperty();
            ArtUtil.ChangeColorToCombatPageList(keypageItem.KeypageColorOptions.FrameColor.Value);
        }

        [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetPassiveSlotColor")]
        [HarmonyPostfix]
        public static void UILibrarianEquipBookInfoPanel_SetPassiveSlotColor(Color c, UnitDataModel ___unitData,
            Graphic[] ___graphic_passivesSlot)
        {
            if (___unitData == null || c == LoRColorUtil.HighlightColor) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitData.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitData.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in ___graphic_passivesSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionPreviewBookPanel), "SetColorByRarity")]
        [HarmonyPatch(typeof(UIPassiveSuccessionCenterEquipBookSlot), "SetColorByRarity")]
        [HarmonyPrefix]
        public static void UIPassiveSuccessionPanel_SetColorByRarity_Pre(BookModel ____currentbookmodel,
            TextMeshProUGUI ___txt_name)
        {
            if (____currentbookmodel == null) return;
            ___txt_name.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionPreviewBookPanel), "SetColorByRarity")]
        [HarmonyPatch(typeof(UIPassiveSuccessionCenterEquipBookSlot), "SetColorByRarity")]
        [HarmonyPostfix]
        public static void UIPassiveSuccessionPanel_SetColorByRarity(BookModel ____currentbookmodel, Image ___img_Frame,
            Image ___img_IconGlow, TextMeshProMaterialSetter ___setter_name, TextMeshProUGUI ___txt_name)
        {
            if (____currentbookmodel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(____currentbookmodel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ____currentbookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                ___img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                ___img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            ___setter_name.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            ___txt_name.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionEquipBookSlot), "SetRarityColor")]
        [HarmonyPatch(typeof(UIPassiveEquipBookSlot), "SetRarityColor")]
        [HarmonyPrefix]
        public static void UIPassiveSuccessionEquipBookSlot_SetRarityColor_Pre(BookModel ___bookmodel,
            Image ___img_Frame, Image ___img_IconGlow, TextMeshProMaterialSetter ___setter_txtbookname,
            TextMeshProUGUI ___txt_BookName)
        {
            if (___bookmodel == null) return;
            ___txt_BookName.color = LoRColorUtil.DefaultColor;
            ___img_Frame.color = LoRColorUtil.DisabledColor;
            ___img_IconGlow.color = LoRColorUtil.DisabledColor;
            ___setter_txtbookname.underlayColor = LoRColorUtil.DisabledColor;
            ___setter_txtbookname.faceColor = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionEquipBookSlot), "SetRarityColor")]
        [HarmonyPatch(typeof(UIPassiveEquipBookSlot), "SetRarityColor")]
        [HarmonyPostfix]
        public static void UIPassiveSuccessionEquipBookSlot_SetRarityColor(BookModel ___bookmodel, Color c,
            Image ___img_Frame, Image ___img_IconGlow, TextMeshProMaterialSetter ___setter_txtbookname,
            TextMeshProUGUI ___txt_BookName)
        {
            if (___bookmodel == null || c == LoRColorUtil.DisabledColor) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___bookmodel.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___bookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                ___img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                ___img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            ___setter_txtbookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            ___setter_txtbookname.faceColor = keypageItem.KeypageColorOptions.NameColor.Value;
            ___txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionBookSlot), "SetDefaultColor")]
        [HarmonyPrefix]
        public static void UIPassiveSuccessionBookSlot_SetDefaultColor_Pre(BookModel ___currentbookmodel,
            TextMeshProUGUI ___txt_bookname)
        {
            if (___currentbookmodel == null) return;
            ___txt_bookname.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UIPassiveSuccessionBookSlot), "SetDefaultColor")]
        [HarmonyPostfix]
        public static void UIPassiveSuccessionBookSlot_SetDefaultColor(BookModel ___currentbookmodel,
            UIPassiveSuccessionBookSlot.SlotImageSet ___equipSet, Image ___img_IconGlow, Image ___img_levelFrame,
            TextMeshProUGUI ___txt_bookname, TextMeshProUGUI ___txt_booklevel)
        {
            if (___currentbookmodel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___currentbookmodel.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___currentbookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                ___txt_bookname.color = keypageItem.KeypageColorOptions.NameColor.Value;
                ___txt_bookname.enabled = false;
                ___txt_bookname.enabled = true;
                var component = ___txt_bookname.GetComponent<TextMeshProMaterialSetter>();
                if (component != null)
                    component.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                if (___txt_bookname.isActiveAndEnabled)
                    ___txt_bookname.fontMaterial.SetColor("_UnderlayColor",
                        keypageItem.KeypageColorOptions.NameColor.Value);
            }

            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            ___equipSet.Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (___equipSet.FrameGlow != null)
                ___equipSet.FrameGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (___img_IconGlow != null)
                ___img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (___img_levelFrame != null)
                ___img_levelFrame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (___txt_booklevel != null)
                ___txt_booklevel.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetData")]
        [HarmonyPrefix]
        public static void UILibrarianInfoInCardPhase_SetData_Pre(UnitDataModel ___unitdata,
            TextMeshProUGUI ___txt_BookName)
        {
            if (___unitdata == null) return;
            ___txt_BookName.color = LoRColorUtil.DefaultColor;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetData")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_SetData(UnitDataModel ___unitdata, Graphic[] ___graphic_Frames,
            Image ___img_BookIconGlow, TextMeshProMaterialSetter ___setter_bookname, TextMeshProUGUI ___txt_BookName)
        {
            if (___unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                if (UI.UIController.Instance.CurrentUIPhase != UIPhase.Main_ItemList)
                    foreach (var t in ___graphic_Frames)
                        t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                ___img_BookIconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            ___setter_bookname.GetType().GetMethod("Awake", AccessTools.all)
                ?.Invoke(___setter_bookname, Array.Empty<object>());
            ___setter_bookname.GetType().GetMethod("Start", AccessTools.all)
                ?.Invoke(___setter_bookname, Array.Empty<object>());
            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            ___setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            ___setter_bookname.enabled = false;
            ___setter_bookname.enabled = true;
            ___txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetPassiveSlotColor")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_SetPassiveSlotColor(Color c, UnitDataModel ___unitdata,
            Graphic[] ___graphic_passivesSlot)
        {
            if (___unitdata == null || c == LoRColorUtil.HighlightColor) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in ___graphic_passivesSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetBattlePageSlotColor")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_SetBattlePageSlotColor(Color c, UnitDataModel ___unitdata,
            Graphic[] ___graphic_battlepageSlot)
        {
            if (___unitdata == null || c == LoRColorUtil.HighlightColor) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in ___graphic_battlepageSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "OnPointerExitEquipPage")]
        [HarmonyPostfix]
        public static void UILibrarianInfoInCardPhase_OnPointerExitEquipPage(UnitDataModel ___unitdata,
            Graphic[] ___graphic_Frames)
        {
            if (___unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var t in ___graphic_Frames) t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIGachaEquipSlot), "SetDefaultColor")]
        [HarmonyPrefix]
        public static void UIGachaEquipSlot_SetDefaultColor_Pre(UIGachaEquipSlot __instance)
        {
            if (__instance._book == null) return;
            __instance.BookName.color = LoRColorUtil.DefaultColor;
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
                        LoRColorUtil.DefaultColor;
            if (eventData.pointerCurrentRaycast.gameObject != null &&
                eventData.pointerCurrentRaycast.gameObject.name.Contains("[Xbox]SelectableTarget")) return;
            await GenericUtil.PutTaskDelay(30);
            ArtUtil.ChangeColorToCombatPageList(color);
        }

        [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetEquipPageSlotColor")]
        [HarmonyPrefix]
        public static void UIBattleSettingLibrarianInfoPanel_SetEquipPageSlotColor_Pre(
            UIBattleSettingLibrarianInfoPanel __instance, UnitDataModel ___unitdata,
            TextMeshProUGUI ___txt_BookName, bool ___isSephirahPanel)
        {
            if (___unitdata == null) return;
            ___txt_BookName.color = LoRColorUtil.DefaultColor;
            foreach (var img in __instance.GetComponentsInChildren<Image>()
                         .Where(x => x.name.Contains("[Image]CenterFrame")))
                img.color = ___isSephirahPanel ? LoRColorUtil.DefaultColor : LoRColorUtil.DisabledColor;
        }

        [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetEquipPageSlotColor")]
        [HarmonyPostfix]
        public static void UIBattleSettingLibrarianInfoPanel_SetEquipPageSlotColor(
            UIBattleSettingLibrarianInfoPanel __instance, UnitDataModel ___unitdata,
            TextMeshProMaterialSetter ___setter_bookname, Image ___img_BookIconGlow, Graphic[] ___graphic_Frames,
            TextMeshProUGUI ___txt_BookName, bool ___isSephirahPanel)
        {
            if (___unitdata == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.NameColor.HasValue)
            {
                ___setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
                ___txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
            }

            ___setter_bookname.InitMaterialProperty();
            if (!keypageItem.KeypageColorOptions.FrameColor.HasValue) return;
            ___img_BookIconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            foreach (var graphic in ___graphic_Frames.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            foreach (var img in __instance.GetComponentsInChildren<Image>()
                         .Where(x => x.name.Contains("[Image]CenterFrame")))
                img.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }

        [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetBattlePageSlotColor")]
        [HarmonyPostfix]
        public static void UIBattleSettingLibrarianInfoPanel_SetBattlePageSlotColor_Post(Color c,
            UnitDataModel ___unitdata, Graphic[] ___graphic_battlepageSlot)
        {
            if (___unitdata == null || c == LoRColorUtil.HighlightColor) return;
            if (!ModParameters.KeypageOptions.TryGetValue(___unitdata.bookItem.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == ___unitdata.bookItem.BookId.id);
            if (keypageItem?.KeypageColorOptions?.FrameColor == null) return;
            foreach (var graphic in ___graphic_battlepageSlot.Where(x => x != null))
                graphic.color = keypageItem.KeypageColorOptions.FrameColor.Value;
        }
    }
}