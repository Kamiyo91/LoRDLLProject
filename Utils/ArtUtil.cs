using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BigDLL4221.Enum;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using LOR_BattleUnit_UI;
using Sound;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BigDLL4221.Utils
{
    public static class ArtUtil
    {
        public static void InitCustomEffects(List<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                assembly.GetTypes().ToList().FindAll(x => x.Name.StartsWith("DiceAttackEffect_"))
                    .ForEach(delegate(Type x)
                    {
                        ModParameters.CustomEffects[x.Name.Replace("DiceAttackEffect_", "")] = x;
                    });
        }

        public static void GetArtWorks(DirectoryInfo dir)
        {
            try
            {
                if (dir.GetDirectories().Length != 0)
                {
                    var directories = dir.GetDirectories();
                    foreach (var t in directories) GetArtWorks(t);
                }

                foreach (var fileInfo in dir.GetFiles())
                {
                    var texture2D = new Texture2D(2, 2);
                    texture2D.LoadImage(File.ReadAllBytes(fileInfo.FullName));
                    var value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
                        new Vector2(0f, 0f));
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                    ModParameters.ArtWorks[fileNameWithoutExtension] = value;
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void GetCardArtWorks(DirectoryInfo dir)
        {
            try
            {
                if (dir.GetDirectories().Length != 0)
                {
                    var directories = dir.GetDirectories();
                    foreach (var t in directories) GetArtWorks(t);
                }

                if (!dir.Exists || !dir.GetFiles().Any()) return;
                foreach (var fileInfo in dir.GetFiles())
                {
                    var texture2D = new Texture2D(2, 2);
                    texture2D.LoadImage(File.ReadAllBytes(fileInfo.FullName));
                    var value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
                        new Vector2(0f, 0f));
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                    ModParameters.CardArtWorks[fileNameWithoutExtension] = value;
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void GetSpeedDieArtWorks(DirectoryInfo dir)
        {
            try
            {
                if (dir.GetDirectories().Length != 0)
                {
                    var directories = dir.GetDirectories();
                    foreach (var t in directories) GetArtWorks(t);
                }

                if (!dir.Exists || !dir.GetFiles().Any()) return;
                foreach (var fileInfo in dir.GetFiles())
                {
                    var texture2D = new Texture2D(2, 2);
                    texture2D.LoadImage(File.ReadAllBytes(fileInfo.FullName));
                    var value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
                        new Vector2(0f, 0f));
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                    ModParameters.SpeedDieArtWorks[fileNameWithoutExtension] = value;
                }
            }
            catch
            {
                // ignored
            }
        }

        private static Sprite GetIcon(string customIconId, string baseIconId, string baseIcon)
        {
            return ModParameters.ArtWorks.TryGetValue(customIconId, out var customIcon)
                ? customIcon
                : UISpriteDataManager.instance.GetStoryIcon(string.IsNullOrEmpty(baseIconId)
                    ? baseIcon
                    : baseIconId).icon;
        }

        private static string CredenzaName(string nameId, string name, string packageId)
        {
            return ModParameters.LocalizedItems.TryGetValue(packageId, out var localizedItem)
                ? localizedItem.EffectTexts.TryGetValue(nameId, out var credenza)
                    ? credenza.Name
                    : !string.IsNullOrEmpty(name)
                        ? name
                        : packageId
                : !string.IsNullOrEmpty(name)
                    ? name
                    : packageId;
        }

        public static void OnSelectEpisodeSlot(UIBookStoryPanel instance,
            UIBookStoryEpisodeSlot slot, TextMeshProUGUI selectedEpisodeText, Image selectedEpisodeIcon,
            Image selectedEpisodeIconGlow)
        {
            if (slot == null) return;
            if (slot.books.Exists(x => x.id.IsBasic())) return;
            var book = slot.books.FirstOrDefault(x => ModParameters.PackageIds.Contains(x.id.packageId));
            if (book == null) return;
            if (ModParameters.CategoryOptions.TryGetValue(book.id.packageId, out var categoryOptions))
            {
                var categoryOption = categoryOptions.FirstOrDefault(x => x.CredenzaBooksId.Contains(book.id.id));
                if (categoryOption != null)
                {
                    selectedEpisodeText.text = CredenzaName(categoryOption.CategoryNameId, categoryOption.CategoryName,
                        book.id.packageId);
                    var iconCategory = GetIcon(categoryOption.CustomIconSpriteId, categoryOption.BaseIconSpriteId,
                        slot.books[0].BookIcon);
                    selectedEpisodeIcon.sprite = iconCategory;
                    selectedEpisodeIconGlow.sprite = iconCategory;
                    instance.UpdateBookSlots();
                    return;
                }
            }

            var credenzaOptionsTryGet =
                ModParameters.CredenzaOptions.TryGetValue(book.id.packageId, out var credenzaOptions);
            if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
            selectedEpisodeText.text = CredenzaName(credenzaOptions.CredenzaNameId, credenzaOptions.CredenzaName,
                book.id.packageId);
            var icon = GetIcon(credenzaOptions.CustomIconSpriteId, credenzaOptions.BaseIconSpriteId,
                slot.books[0].BookIcon);
            selectedEpisodeIcon.sprite = icon;
            selectedEpisodeIconGlow.sprite = icon;
            instance.UpdateBookSlots();
        }

        public static void SetBooksDataOriginal(UIInvenEquipPageListSlot instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            if (!ModParameters.PackageIds.Contains(storyKey.workshopId)) return;
            if (books.Count < 0) return;
            var credenzaOptionsTryGet =
                ModParameters.CredenzaOptions.TryGetValue(storyKey.workshopId, out var credenzaOptions);
            if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
            instance.img_IconGlow.enabled = true;
            instance.img_Icon.enabled = true;
            var icon = GetIcon(credenzaOptions.CustomIconSpriteId, credenzaOptions.BaseIconSpriteId,
                "Chapter" + storyKey.chapter);
            instance.img_IconGlow.sprite = icon;
            instance.img_Icon.sprite = icon;
            instance.txt_StoryName.text = CredenzaName(credenzaOptions.CredenzaNameId, credenzaOptions.CredenzaName,
                storyKey.workshopId);
            if (!StaticModsInfo.CustomColors || credenzaOptions.BookDataColor == null) return;
            if (credenzaOptions.BookDataColor.FrameColor.HasValue)
                SetBooksDataFrameColor(credenzaOptions.BookDataColor.FrameColor.Value, instance.img_EdgeFrame,
                    instance.img_LineFrame,
                    instance.img_IconGlow, instance.img_Icon);
            if (!credenzaOptions.BookDataColor.TextColor.HasValue) return;
            instance.txt_StoryName.color = credenzaOptions.BookDataColor.TextColor.Value;
            var component = instance.txt_StoryName.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = credenzaOptions.BookDataColor.TextColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void SetBooksDataOriginal(UISettingInvenEquipPageListSlot instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            if (!ModParameters.PackageIds.Contains(storyKey.workshopId)) return;
            if (books.Count < 0) return;
            var credenzaOptionsTryGet =
                ModParameters.CredenzaOptions.TryGetValue(storyKey.workshopId, out var credenzaOptions);
            if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
            instance.img_IconGlow.enabled = true;
            instance.img_Icon.enabled = true;
            var icon = GetIcon(credenzaOptions.CustomIconSpriteId, credenzaOptions.BaseIconSpriteId,
                "Chapter" + storyKey.chapter);
            instance.img_IconGlow.sprite = icon;
            instance.img_Icon.sprite = icon;
            instance.txt_StoryName.text = CredenzaName(credenzaOptions.CredenzaNameId, credenzaOptions.CredenzaName,
                storyKey.workshopId);
            if (!StaticModsInfo.CustomColors || credenzaOptions.BookDataColor == null) return;
            if (credenzaOptions.BookDataColor.FrameColor.HasValue)
                SetBooksDataFrameColor(credenzaOptions.BookDataColor.FrameColor.Value, instance.img_EdgeFrame,
                    instance.img_LineFrame,
                    instance.img_IconGlow, instance.img_Icon);
            if (!credenzaOptions.BookDataColor.TextColor.HasValue) return;
            instance.txt_StoryName.color = credenzaOptions.BookDataColor.TextColor.Value;
            var component = instance.txt_StoryName.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = credenzaOptions.BookDataColor.TextColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void SetBooksData(UISettingInvenEquipPageListSlot instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            var categoryOptions = ModParameters.CategoryOptions.SelectMany(x =>
                x.Value.Where(y => storyKey.workshopId == y.PackageId + y.AdditionalValue));
            var categoryOption =
                categoryOptions.FirstOrDefault(x => storyKey.workshopId == x.PackageId + x.AdditionalValue);
            if (categoryOption == null)
            {
                SetBooksDataOriginal(instance, books, storyKey);
                return;
            }

            if (books.Count < 0) return;
            instance.img_IconGlow.enabled = true;
            instance.img_Icon.enabled = true;
            var icon = GetIcon(categoryOption.CustomIconSpriteId, categoryOption.BaseIconSpriteId,
                "Chapter" + storyKey.chapter);
            instance.img_Icon.sprite = icon;
            instance.img_IconGlow.sprite = icon;
            instance.txt_StoryName.text = CredenzaName(categoryOption.CategoryNameId, categoryOption.CategoryName,
                categoryOption.PackageId);
            if (!StaticModsInfo.CustomColors || categoryOption.BookDataColor == null) return;
            if (categoryOption.BookDataColor.FrameColor.HasValue)
                SetBooksDataFrameColor(categoryOption.BookDataColor.FrameColor.Value, instance.img_EdgeFrame,
                    instance.img_LineFrame,
                    instance.img_IconGlow, instance.img_Icon);
            if (!categoryOption.BookDataColor.TextColor.HasValue) return;
            instance.txt_StoryName.color = categoryOption.BookDataColor.TextColor.Value;
            var component = instance.txt_StoryName.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = categoryOption.BookDataColor.TextColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void SetBooksData(UIInvenEquipPageListSlot instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            var categoryOptions = ModParameters.CategoryOptions.SelectMany(x =>
                x.Value.Where(y => storyKey.workshopId == y.PackageId + y.AdditionalValue));
            var categoryOption =
                categoryOptions.FirstOrDefault(x => storyKey.workshopId == x.PackageId + x.AdditionalValue);
            if (categoryOption == null)
            {
                SetBooksDataOriginal(instance, books, storyKey);
                return;
            }

            if (books.Count < 0) return;
            instance.img_IconGlow.enabled = true;
            instance.img_Icon.enabled = true;
            var icon = GetIcon(categoryOption.CustomIconSpriteId, categoryOption.BaseIconSpriteId,
                "Chapter" + storyKey.chapter);
            instance.img_Icon.sprite = icon;
            instance.img_IconGlow.sprite = icon;
            instance.txt_StoryName.text = CredenzaName(categoryOption.CategoryNameId, categoryOption.CategoryName,
                categoryOption.PackageId);
            if (!StaticModsInfo.CustomColors || categoryOption.BookDataColor == null) return;
            if (categoryOption.BookDataColor.FrameColor.HasValue)
                SetBooksDataFrameColor(categoryOption.BookDataColor.FrameColor.Value, instance.img_EdgeFrame,
                    instance.img_LineFrame,
                    instance.img_IconGlow, instance.img_Icon);
            if (!categoryOption.BookDataColor.TextColor.HasValue) return;
            instance.txt_StoryName.color = categoryOption.BookDataColor.TextColor.Value;
            var component = instance.txt_StoryName.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = categoryOption.BookDataColor.TextColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void ResetColorData(UIInvenEquipPageListSlot instance)
        {
            var defaultColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
            instance.img_Icon.color = defaultColor;
            if (instance.txt_StoryName == null) return;
            instance.txt_StoryName.color = defaultColor;
            var component = instance.txt_StoryName.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = defaultColor;
            component.enabled = false;
            component.enabled = true;
        }

        public static void ResetColorData(UISettingInvenEquipPageListSlot instance)
        {
            var defaultColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
            instance.img_Icon.color = defaultColor;
            if (instance.txt_StoryName == null) return;
            instance.txt_StoryName.color = defaultColor;
            var component = instance.txt_StoryName.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = defaultColor;
            component.enabled = false;
            component.enabled = true;
        }

        public static void UIInvitationSetColorPost(UIInvitationDropBookSlot instance, Color c)
        {
            if (instance.BookId == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted) ||
                c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.DropBookOptions.TryGetValue(instance.BookId.packageId,
                    out var dropBookOptions)) return;
            var dropBookOption = dropBookOptions.FirstOrDefault(x => x.DropBookId == instance.BookId.id);
            if (dropBookOption?.DropBookColorOptions == null) return;
            if (dropBookOption.DropBookColorOptions.FrameColor.HasValue)
                instance.bookNumBg.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
            if (!dropBookOption.DropBookColorOptions.NameColor.HasValue) return;
            instance.txt_bookNum.color = dropBookOption.DropBookColorOptions.NameColor.Value;
        }

        public static void EmotionPassiveCardUISetSpritesPost(EmotionPassiveCardUI instance,
            EmotionCardXmlExtension cardExtension)
        {
            instance._artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(cardExtension.LorId.packageId,
                    cardExtension.Artwork);
            if (!UtilExtensions.GetEmotionCardOptions(cardExtension.LorId.packageId, cardExtension.LorId.id,
                    out var cardOptions) || cardOptions.ColorOptions == null) return;
            instance.img_LeftTotalFrame.sprite = UISpriteDataManager.instance.AbnormalityFrame.ElementAtOrDefault(0);
            var orAddComponent = instance.img_LeftTotalFrame.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ChangeEmotionCardColor(cardOptions, ref orAddComponent);
            instance._rightBg.sprite = instance._positiveBgSprite.ElementAtOrDefault(1);
            var orAddComponent2 = instance._rightBg.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ChangeEmotionCardColor(cardOptions, ref orAddComponent2);
            instance._rightFrame.sprite = instance._positiveFrameSprite.ElementAtOrDefault(1);
            var orAddComponent3 = instance._rightFrame.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ChangeEmotionCardColor(cardOptions, ref orAddComponent3);
            instance._leftFrameTitleLineardodge.gameObject.SetActive(false);
            if (cardOptions.ColorOptions.TextColor.HasValue)
            {
                instance._flavorText.fontMaterial.SetColor("_UnderlayColor", cardOptions.ColorOptions.TextColor.Value);
                instance._abilityDesc.fontMaterial.SetColor("_UnderlayColor", cardOptions.ColorOptions.TextColor.Value);
            }

            if (!cardOptions.ColorOptions.FrameColor.HasValue) return;
            instance._hOverImg.color = cardOptions.ColorOptions.FrameColor.Value;
            var rootColor = cardOptions.ColorOptions.FrameColor.Value;
            rootColor.a = 0.25f;
            instance._rootImageBg.color = rootColor;
            var component = instance.txt_Level.GetComponent<TextMeshProMaterialSetter>();
            if (component == null) return;
            component.glowColor = cardOptions.ColorOptions.FrameColor.Value;
            component.underlayColor = cardOptions.ColorOptions.FrameColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void EmotionPassiveCardUISetSpritesPostNoColor(EmotionPassiveCardUI instance,
            EmotionCardXmlExtension cardExtension)
        {
            instance._artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(cardExtension.LorId.packageId,
                    cardExtension.Artwork);
        }

        public static void EmotionPassiveCardUISetSpritesPostNoColor(UIEmotionPassiveCardInven instance,
            EmotionCardXmlExtension cardExtension)
        {
            instance._artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(cardExtension.LorId.packageId,
                    cardExtension.Artwork);
        }

        public static void EmotionPassiveCardUISetSpritesPost(UIEmotionPassiveCardInven instance,
            EmotionCardXmlExtension cardExtension)
        {
            instance._artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(cardExtension.LorId.packageId,
                    cardExtension.Artwork);
            if (!UtilExtensions.GetEmotionCardOptions(cardExtension.LorId.packageId, cardExtension.LorId.id,
                    out var cardOptions) || cardOptions.ColorOptions == null) return;
            instance.img_LeftTotalFrame.sprite = UISpriteDataManager.instance.AbnormalityFrame.ElementAtOrDefault(0);
            var orAddComponent = instance.img_LeftTotalFrame.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ChangeEmotionCardColor(cardOptions, ref orAddComponent);
            instance._rightBg.sprite = instance._positiveBgSprite.ElementAtOrDefault(1);
            var orAddComponent2 = instance._rightBg.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ChangeEmotionCardColor(cardOptions, ref orAddComponent2);
            instance._rightFrame.sprite = instance._positiveFrameSprite.ElementAtOrDefault(1);
            var orAddComponent3 = instance._rightFrame.gameObject.GetOrAddComponent<_2dxFX_ColorChange>();
            ChangeEmotionCardColor(cardOptions, ref orAddComponent3);
            instance._leftFrameTitleLineardodge.gameObject.SetActive(false);
            if (cardOptions.ColorOptions.TextColor.HasValue)
            {
                instance._flavorText.fontMaterial.SetColor("_UnderlayColor", cardOptions.ColorOptions.TextColor.Value);
                instance._abilityDesc.fontMaterial.SetColor("_UnderlayColor", cardOptions.ColorOptions.TextColor.Value);
            }

            if (!cardOptions.ColorOptions.FrameColor.HasValue) return;
            instance._hOverImg.color = cardOptions.ColorOptions.FrameColor.Value;
            var rootColor = cardOptions.ColorOptions.FrameColor.Value;
            rootColor.a = 0.25f;
            instance._rootImageBg.color = rootColor;
            var component = instance.txt_Level.GetComponent<TextMeshProMaterialSetter>();
            if (component == null) return;
            component.glowColor = cardOptions.ColorOptions.FrameColor.Value;
            component.underlayColor = cardOptions.ColorOptions.FrameColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void UIInvitationSetColorPost(UIAddedFeedBookSlot instance, Color c)
        {
            if (instance.BookId == null || c == UIColorManager.Manager.GetUIColor(UIColor.Highlighted) ||
                c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.DropBookOptions.TryGetValue(instance.BookId.packageId,
                    out var dropBookOptions)) return;
            var dropBookOption = dropBookOptions.FirstOrDefault(x => x.DropBookId == instance.BookId.id);
            if (dropBookOption?.DropBookColorOptions == null) return;
            if (dropBookOption.DropBookColorOptions.FrameColor.HasValue)
                instance.bookNumBg.color = dropBookOption.DropBookColorOptions.FrameColor.Value;
            if (!dropBookOption.DropBookColorOptions.NameColor.HasValue) return;
            instance.txt_bookNum.color = dropBookOption.DropBookColorOptions.NameColor.Value;
        }

        public static void UIPassiveSuccessionEquipBookSlotSetRarityColorPost(UIPassiveEquipBookSlot instance, Color c)
        {
            if (instance.bookmodel == null || c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(instance.bookmodel.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == instance.bookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                instance.img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                instance.img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            instance.setter_txtbookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.setter_txtbookname.faceColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        public static void UIPassiveSuccessionEquipBookSlotSetRarityColorPost(UIPassiveSuccessionEquipBookSlot instance,
            Color c)
        {
            if (instance.bookmodel == null || c == UIColorManager.Manager.GetUIColor(UIColor.Disabled)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(instance.bookmodel.BookId.packageId, out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == instance.bookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                instance.img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                instance.img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            instance.setter_txtbookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.setter_txtbookname.faceColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        public static void UIPassiveSuccessionEquipBookSlotSetRarityColorPre(UIPassiveEquipBookSlot instance)
        {
            if (instance.bookmodel == null) return;
            instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
            instance.img_Frame.color = UIColorManager.Manager.GetUIColor(UIColor.Disabled);
            instance.img_IconGlow.color = UIColorManager.Manager.GetUIColor(UIColor.Disabled);
            instance.setter_txtbookname.underlayColor = UIColorManager.Manager.GetUIColor(UIColor.Disabled);
            instance.setter_txtbookname.faceColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        public static void UIPassiveSuccessionEquipBookSlotSetRarityColorPre(UIPassiveSuccessionEquipBookSlot instance)
        {
            if (instance.bookmodel == null) return;
            instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
            instance.img_Frame.color = UIColorManager.Manager.GetUIColor(UIColor.Disabled);
            instance.img_IconGlow.color = UIColorManager.Manager.GetUIColor(UIColor.Disabled);
            instance.setter_txtbookname.underlayColor = UIColorManager.Manager.GetUIColor(UIColor.Disabled);
            instance.setter_txtbookname.faceColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        public static void UIPassiveSuccessionPanelSetColorByRarityPre(UIPassiveSuccessionPreviewBookPanel instance)
        {
            if (instance._currentbookmodel == null) return;
            instance.txt_name.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        public static void UIPassiveSuccessionPanelSetColorByRarityPre(UIPassiveSuccessionCenterEquipBookSlot instance)
        {
            if (instance._currentbookmodel == null) return;
            instance.txt_name.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        public static void EmotionPassiveCardUISetSpritesPre(UIEmotionPassiveCardInven __instance)
        {
            __instance.img_LeftTotalFrame.gameObject.SafeDestroyComponent<_2dxFX_ColorChange>();
            __instance._leftFrameTitleLineardodge.gameObject.SetActive(true);
            __instance._rightFrame.gameObject.SafeDestroyComponent<_2dxFX_ColorChange>();
        }

        public static void EmotionPassiveCardUISetSpritesPre(EmotionPassiveCardUI __instance)
        {
            __instance.img_LeftTotalFrame.gameObject.SafeDestroyComponent<_2dxFX_ColorChange>();
            __instance._leftFrameTitleLineardodge.gameObject.SetActive(true);
            __instance._rightFrame.gameObject.SafeDestroyComponent<_2dxFX_ColorChange>();
        }

        public static void UIPassiveSuccessionPanelSetColorByRarityPost(UIPassiveSuccessionCenterEquipBookSlot instance)
        {
            if (instance._currentbookmodel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(instance._currentbookmodel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == instance._currentbookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                instance.img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                instance.img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            instance.setter_name.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.txt_name.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        public static void UIPassiveSuccessionPanelSetColorByRarityPost(UIPassiveSuccessionPreviewBookPanel instance)
        {
            if (instance._currentbookmodel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(instance._currentbookmodel.BookId.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == instance._currentbookmodel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
            {
                instance.img_Frame.color = keypageItem.KeypageColorOptions.FrameColor.Value;
                instance.img_IconGlow.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            }

            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            instance.setter_name.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.txt_name.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        public static void SetBooksDataFrameColor(Color c, Image img_EdgeFrame, Image img_LineFrame, Image img_IconGlow,
            Image img_Icon)
        {
            img_EdgeFrame.color = c;
            img_LineFrame.color = c;
            img_IconGlow.color = c;
            img_Icon.color = c;
        }

        public static void UIEquipPageSetDataPost(UIEquipPagePreviewPanel instance)
        {
            if (instance.bookDataModel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(instance.bookDataModel.BookId.packageId,
                    out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == instance.bookDataModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
                foreach (var t in instance.graphic_Frames)
                    t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            instance.setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.setter_bookname.enabled = false;
            instance.setter_bookname.enabled = true;
            instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        public static void UIEquipPageSetDataPost(UIEquipPageModelPreviewPanel instance)
        {
            if (instance.bookDataModel == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(instance.bookDataModel.BookId.packageId,
                    out var keypageOptions))
                return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == instance.bookDataModel.BookId.id);
            if (keypageItem?.KeypageColorOptions == null) return;
            if (keypageItem.KeypageColorOptions.FrameColor.HasValue)
                foreach (var t in instance.graphic_Frames)
                    t.color = keypageItem.KeypageColorOptions.FrameColor.Value;
            if (!keypageItem.KeypageColorOptions.NameColor.HasValue) return;
            instance.setter_bookname.underlayColor = keypageItem.KeypageColorOptions.NameColor.Value;
            instance.setter_bookname.enabled = false;
            instance.setter_bookname.enabled = true;
            instance.txt_BookName.color = keypageItem.KeypageColorOptions.NameColor.Value;
        }

        public static void UIEquipPageSetDataPre(UIEquipPagePreviewPanel instance)
        {
            if (instance.bookDataModel == null) return;
            instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        public static void UIEquipPageSetDataPre(UIEquipPageModelPreviewPanel instance)
        {
            if (instance.bookDataModel == null) return;
            instance.txt_BookName.color = UIColorManager.Manager.GetUIColor(UIColor.Default);
        }

        public static void UICardSetDataPre(UIDetailEgoCardSlot instance)
        {
            var frame = instance.img_Frames.FirstOrDefault(x => x.name.Contains("[Image]NormalFrame"));
            if (frame != null) frame.overrideSprite = null;
            var component = instance.img_Artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
            if (component != null) component.overrideSprite = null;
        }

        public static void UICardSetDataPre(UIOriginCardSlot instance)
        {
            var frame = instance.img_Frames.FirstOrDefault(x => x.name.Contains("[Image]NormalFrame"));
            if (frame != null) frame.overrideSprite = null;
            var component = instance.img_Artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
            if (component != null) component.overrideSprite = null;
        }

        public static void UICardSetDataPost(UIDetailEgoCardSlot instance, CardColorOptions cardColorOption)
        {
            if (cardColorOption.CardColor.HasValue)
            {
                foreach (var img in instance.img_Frames)
                    img.color = cardColorOption.CardColor.Value;
                foreach (var img in instance.img_linearDodge)
                    img.color = cardColorOption.CardColor.Value;
                instance.costNumbers.SetContentColor(cardColorOption.CardColor.Value);
                instance.colorFrame = cardColorOption.CardColor.Value;
                instance.colorLineardodge = cardColorOption.CardColor.Value;
                instance.img_RangeIcon.color = cardColorOption.CardColor.Value;
            }

            var frame = instance.img_Frames.FirstOrDefault(x => x.name.Contains("[Image]NormalFrame"));
            if (frame != null)
                if (!string.IsNullOrEmpty(cardColorOption.LeftFrame) &&
                    ModParameters.CardArtWorks.TryGetValue(cardColorOption.LeftFrame, out var leftFrameImg))
                {
                    frame.overrideSprite = leftFrameImg;
                    frame.overrideSprite.name = $"{cardColorOption.LeftFrame}_LFrame";
                    if (cardColorOption.ApplySideFrontColors && cardColorOption.CardColor.HasValue)
                        frame.color = cardColorOption.CardColor.Value;
                    else frame.color = Color.white;
                }

            var component = instance.img_Artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
            if (component != null)
                if (!string.IsNullOrEmpty(cardColorOption.FrontFrame) &&
                    ModParameters.CardArtWorks.TryGetValue(cardColorOption.FrontFrame, out var frontFrameImg))
                {
                    component.overrideSprite = frontFrameImg;
                    component.overrideSprite.name = $"{cardColorOption.FrontFrame}_FFrame";
                    if (cardColorOption.ApplyFrontColor && cardColorOption.CardColor.HasValue)
                        component.color = cardColorOption.CardColor.Value;
                }

            if (string.IsNullOrEmpty(cardColorOption.CustomIcon) ||
                !ModParameters.ArtWorks.TryGetValue(cardColorOption.CustomIcon, out var icon)) return;
            instance.img_RangeIcon.overrideSprite = icon;
        }

        public static void UICardSetDataPost(UIOriginCardSlot instance, CardColorOptions cardColorOption)
        {
            if (cardColorOption.CardColor.HasValue)
            {
                foreach (var img in instance.img_Frames)
                    img.color = cardColorOption.CardColor.Value;
                foreach (var img in instance.img_linearDodge)
                    img.color = cardColorOption.CardColor.Value;
                instance.costNumbers.SetContentColor(cardColorOption.CardColor.Value);
                instance.colorFrame = cardColorOption.CardColor.Value;
                instance.colorLineardodge = cardColorOption.CardColor.Value;
                instance.img_RangeIcon.color = cardColorOption.CardColor.Value;
            }

            var frame = instance.img_Frames.FirstOrDefault(x => x.name.Contains("[Image]NormalFrame"));
            if (frame != null)
                if (!string.IsNullOrEmpty(cardColorOption.LeftFrame) &&
                    ModParameters.CardArtWorks.TryGetValue(cardColorOption.LeftFrame, out var leftFrameImg))
                {
                    frame.overrideSprite = leftFrameImg;
                    frame.overrideSprite.name = $"{cardColorOption.LeftFrame}_LFrame";
                    if (cardColorOption.ApplySideFrontColors && cardColorOption.CardColor.HasValue)
                        frame.color = cardColorOption.CardColor.Value;
                    else frame.color = Color.white;
                }

            var component = instance.img_Artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
            if (component != null)
                if (!string.IsNullOrEmpty(cardColorOption.FrontFrame) &&
                    ModParameters.CardArtWorks.TryGetValue(cardColorOption.FrontFrame, out var frontFrameImg))
                {
                    component.overrideSprite = frontFrameImg;
                    component.overrideSprite.name = $"{cardColorOption.FrontFrame}_FFrame";
                    if (cardColorOption.ApplyFrontColor && cardColorOption.CardColor.HasValue)
                        component.color = cardColorOption.CardColor.Value;
                }

            if (string.IsNullOrEmpty(cardColorOption.CustomIcon) ||
                !ModParameters.ArtWorks.TryGetValue(cardColorOption.CustomIcon, out var icon)) return;
            instance.img_RangeIcon.overrideSprite = icon;
        }

        public static void SetMainData(List<BookModel> currentBookModelList, List<UIStoryKeyData> totalkeysdata,
            Dictionary<UIStoryKeyData, List<BookModel>> currentStoryBooksDic)
        {
            var categoryOptions =
                ModParameters.CategoryOptions.Where(x => currentBookModelList.Exists(y => y.BookId.packageId == x.Key));
            foreach (var categoryOption in categoryOptions)
            {
                var index = totalkeysdata.FindIndex(x => x.IsWorkshop && x.workshopId == categoryOption.Key);
                if (index == -1) continue;
                totalkeysdata.RemoveAt(index);
                var categoryKey =
                    currentStoryBooksDic.FirstOrDefault(x =>
                        x.Key.IsWorkshop && x.Key.workshopId == categoryOption.Key);
                if (categoryKey.Key != null) currentStoryBooksDic.Remove(categoryKey.Key);
                foreach (var category in categoryOption.Value)
                {
                    var actualKey = new UIStoryKeyData(category.Chapter,
                        categoryOption.Key + $"{category.AdditionalValue}");
                    if (totalkeysdata.Contains(actualKey) && !category.BaseGameCategory.HasValue)
                        totalkeysdata.Remove(actualKey);
                    var bookFound = false;
                    if (category.BaseGameCategory.HasValue)
                        actualKey = totalkeysdata.Find(x => x.StoryLine == category.BaseGameCategory.Value);
                    foreach (var book in category.CategoryBooksId.SelectMany(bookId =>
                                 currentBookModelList.Where(x =>
                                     x.BookId.packageId == categoryOption.Key && x.BookId.id == bookId)))
                    {
                        if (actualKey == null)
                        {
                            actualKey = new UIStoryKeyData(book.ClassInfo.Chapter, category.BaseGameCategory.Value);
                            totalkeysdata.Add(actualKey);
                        }

                        bookFound = true;
                        if (!currentStoryBooksDic.ContainsKey(actualKey))
                        {
                            var list = new List<BookModel> { book };
                            currentStoryBooksDic.Add(actualKey, list);
                        }
                        else
                        {
                            currentStoryBooksDic[actualKey].Add(book);
                        }
                    }

                    if (!bookFound || category.BaseGameCategory.HasValue) continue;
                    totalkeysdata.Insert(index, actualKey);
                    index++;
                }
            }
        }

        public static void SetEpisodeSlots(UIBookStoryChapterSlot instance, UIBookStoryPanel panel,
            List<UIBookStoryEpisodeSlot> episodeSlots)
        {
            foreach (var packageId in ModParameters.PackageIds)
            {
                var uibookStoryEpisodeSlot = episodeSlots.Find(x =>
                    x.books.Find(y => y.id.packageId == packageId) != null);
                if (uibookStoryEpisodeSlot == null) continue;
                if (ModParameters.CategoryOptions.TryGetValue(packageId, out var categoriesOptions))
                {
                    foreach (var categoryOption in categoriesOptions.Where(categoryOption =>
                                 categoryOption.Chapter == instance.chapter && categoryOption.BaseGameCategory == null))
                    {
                        uibookStoryEpisodeSlot = episodeSlots[episodeSlots.Count - 1];
                        switch (categoryOption.CredenzaType)
                        {
                            case CredenzaEnum.ModifiedCredenza:
                            {
                                var books = uibookStoryEpisodeSlot.books;
                                var panelBooks = panel.panel.GetChapterBooksData(instance.chapter).FindAll(x =>
                                    x.id.packageId == packageId && categoryOption.CredenzaBooksId.Contains(x.id.id));
                                var changed = false;
                                if (panelBooks.Any())
                                {
                                    changed = true;
                                    uibookStoryEpisodeSlot.Init(panelBooks, instance);
                                    uibookStoryEpisodeSlot.episodeText.text = CredenzaName(
                                        categoryOption.CategoryNameId, categoryOption.CategoryName, packageId);
                                    var icon = GetIcon(categoryOption.CustomIconSpriteId,
                                        categoryOption.BaseIconSpriteId,
                                        panelBooks[0].BookIcon);
                                    uibookStoryEpisodeSlot.episodeIconGlow.sprite = icon;
                                    uibookStoryEpisodeSlot.episodeIcon.sprite = icon;
                                }

                                var uibookStoryEpisodeSlot2 = episodeSlots[episodeSlots.Count - 1];
                                if (changed)
                                {
                                    instance.InstatiateAdditionalSlot();
                                    uibookStoryEpisodeSlot2 = episodeSlots[episodeSlots.Count - 1];
                                }

                                books.RemoveAll(x => x.id.packageId == packageId);
                                uibookStoryEpisodeSlot2.Init(instance.chapter, books, instance);
                                break;
                            }
                            case CredenzaEnum.NoCredenza:
                            {
                                var books = uibookStoryEpisodeSlot.books;
                                var uibookStoryEpisodeSlot2 = episodeSlots[episodeSlots.Count - 1];
                                books.RemoveAll(x => x.id.packageId == packageId);
                                uibookStoryEpisodeSlot2.Init(instance.chapter, books, instance);
                                break;
                            }
                        }
                    }
                }
                else if (ModParameters.CredenzaOptions.TryGetValue(packageId, out var credenzaOption))
                {
                    if (credenzaOption.Chapter != instance.chapter) continue;
                    switch (credenzaOption.CredenzaOption)
                    {
                        case CredenzaEnum.ModifiedCredenza:
                        {
                            var books = uibookStoryEpisodeSlot.books;
                            var panelBooks = panel.panel.GetChapterBooksData(instance.chapter).FindAll(x =>
                                x.id.packageId == packageId && credenzaOption.CredenzaBooksId.Contains(x.id.id));
                            var changed = false;
                            if (panelBooks.Any())
                            {
                                changed = true;
                                uibookStoryEpisodeSlot.Init(panelBooks, instance);
                                uibookStoryEpisodeSlot.episodeText.text = CredenzaName(credenzaOption.CredenzaNameId,
                                    credenzaOption.CredenzaName, packageId);
                                var icon = GetIcon(credenzaOption.CustomIconSpriteId, credenzaOption.BaseIconSpriteId,
                                    panelBooks[0].BookIcon);
                                uibookStoryEpisodeSlot.episodeIconGlow.sprite = icon;
                                uibookStoryEpisodeSlot.episodeIcon.sprite = icon;
                            }

                            var uibookStoryEpisodeSlot2 = episodeSlots[episodeSlots.Count - 1];
                            if (uibookStoryEpisodeSlot.Equals(uibookStoryEpisodeSlot2) && changed)
                            {
                                instance.InstatiateAdditionalSlot();
                                uibookStoryEpisodeSlot2 = episodeSlots[episodeSlots.Count - 1];
                            }

                            books.RemoveAll(x => x.id.packageId == packageId);
                            uibookStoryEpisodeSlot2.Init(instance.chapter, books, instance);
                            break;
                        }
                        case CredenzaEnum.NoCredenza:
                        {
                            var books = uibookStoryEpisodeSlot.books;
                            var uibookStoryEpisodeSlot2 = episodeSlots[episodeSlots.Count - 1];
                            books.RemoveAll(x => x.id.packageId == packageId);
                            uibookStoryEpisodeSlot2.Init(instance.chapter, books, instance);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetThumbSprite(LorId bookId, ref Sprite result)
        {
            if (!ModParameters.PackageIds.Contains(bookId.packageId)) return;
            if (!ModParameters.SpriteOptions.TryGetValue(bookId.packageId, out var sprites)) return;
            var sprite = sprites.FirstOrDefault(x => x.KeypageId == bookId.id);
            if (sprite == null) return;
            switch (sprite.SpriteOption)
            {
                case SpriteEnum.Base:
                    result = Resources.Load<Sprite>(sprite.SpritePK);
                    break;
                case SpriteEnum.Custom:
                    if (ModParameters.ArtWorks.TryGetValue(sprite.SpritePK, out var spriteArt)) result = spriteArt;
                    break;
            }
        }

        public static void PreLoadBufIcons()
        {
            foreach (var baseGameIcon in Resources.LoadAll<Sprite>("Sprites/BufIconSheet/")
                         .Where(x => !BattleUnitBuf._bufIconDictionary.ContainsKey(x.name)))
                BattleUnitBuf._bufIconDictionary.Add(baseGameIcon.name, baseGameIcon);
            foreach (var artWork in ModParameters.ArtWorks.Where(x =>
                         !x.Key.Contains("Glow") && !x.Key.Contains("Default") &&
                         !BattleUnitBuf._bufIconDictionary.ContainsKey(x.Key)))
                BattleUnitBuf._bufIconDictionary.Add(artWork.Key, artWork.Value);
        }

        public static IEnumerable<BattleDiceCardModel> ReloadEgoHandUI(BattleUnitCardsInHandUI instance,
            List<BattleDiceCardUI> cardList, BattleUnitModel unit, List<BattleDiceCardUI> activatedCardList,
            ref float xInt)
        {
            var list = unit.personalEgoDetail.GetHand();
            if (list.Count >= 9) xInt = 65f * 8f / list.Count;
            var num = 0;
            activatedCardList.Clear();
            while (num < list.Count)
            {
                cardList[num].gameObject.SetActive(true);
                cardList[num].SetCard(list[num], Array.Empty<BattleDiceCardUI.Option>());
                cardList[num].SetDefault();
                cardList[num].ResetSiblingIndex();
                activatedCardList.Add(cardList[num]);
                num++;
            }

            for (var i = 0; i < activatedCardList.Count; i++)
            {
                var navigation = default(Navigation);
                navigation.mode = Navigation.Mode.Explicit;
                if (i > 0)
                    navigation.selectOnLeft = activatedCardList[i - 1].selectable;
                else if (activatedCardList.Count >= 2)
                    navigation.selectOnLeft = activatedCardList[activatedCardList.Count - 1].selectable;
                else
                    navigation.selectOnLeft = null;
                if (i < activatedCardList.Count - 1)
                    navigation.selectOnRight = activatedCardList[i + 1].selectable;
                else if (activatedCardList.Count >= 2)
                    navigation.selectOnRight = activatedCardList[0].selectable;
                else
                    navigation.selectOnRight = null;
                activatedCardList[i].selectable.navigation = navigation;
                activatedCardList[i].selectable.parentSelectable = instance.selectablePanel;
            }

            return list;
        }

        public static void PrepareMultiDeckUI(GameObject multiDeckUI, List<string> labels, string packageId)
        {
            var uiButtons = multiDeckUI.GetComponentsInChildren<UICustomTabButton>(true);
            var num = 0;
            foreach (var uiButton in uiButtons)
            {
                if (num < labels.Count && !string.IsNullOrEmpty(labels[num]))
                    uiButton.TabName.text = ModParameters.LocalizedItems.TryGetValue(packageId, out var localizedItem)
                        ? localizedItem.EffectTexts.TryGetValue(labels[num], out var labelName)
                            ? labelName.Desc
                            : "Not Found"
                        : "Not Found";
                else
                    uiButton.gameObject.SetActive(false);
                num++;
            }
        }

        public static void RevertMultiDeckUI(GameObject multiDeckUI)
        {
            var uiButtons = multiDeckUI.GetComponentsInChildren<UICustomTabButton>(true);
            foreach (var uiButton in uiButtons)
                uiButton.gameObject.SetActive(true);
            var num = 0;
            foreach (var uiButton in uiButtons)
            {
                switch (num)
                {
                    case 0:
                        uiButton.TabName.text = TextDataModel.GetText("ui_slash_form");
                        break;
                    case 1:
                        uiButton.TabName.text = TextDataModel.GetText("ui_penetrate_form");
                        break;
                    case 2:
                        uiButton.TabName.text = TextDataModel.GetText("ui_hit_form");
                        break;
                    case 3:
                        uiButton.TabName.text = TextDataModel.GetText("ui_defense_form");
                        break;
                }

                num++;
            }
        }

        //public static void GetUnity(DirectoryInfo dir)
        //{
        //    if (dir.GetDirectories().Length != 0)
        //    {
        //        var directories = dir.GetDirectories();
        //        foreach (var t in directories)
        //            GetUnity(t);
        //    }

        //    foreach (var fileInfo in dir.GetFiles())
        //        AddAssets(Path.GetFileNameWithoutExtension(fileInfo.FullName), fileInfo.FullName);
        //}

        private static void AddAssets(string packageId)
        {
            ModParameters.AssetBundle.Add(packageId, new Assets(packageId));
        }

        public static void MakeEffect(BattleUnitModel unit, string path, float sizeFactor = 1f,
            BattleUnitModel target = null, float destroyTime = -1f)
        {
            try
            {
                SingletonBehavior<DiceEffectManager>.Instance.CreateCreatureEffect(path, sizeFactor, unit.view,
                    target?.view, destroyTime);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void ChangeColorToCombatPageList(Color c)
        {
            foreach (var text in Object.FindObjectsOfType<TextMeshProUGUI>()
                         .Where(x => x.GetComponentInParent<UICustomSelectable>() &&
                                     x.name.Contains("[Text]Feedbook_TextMesh")))
                text.color = c;
            foreach (var img in Object.FindObjectsOfType<Image>()
                         .Where(x => x.GetComponentInParent<UICustomSelectable>() &&
                                     x.name.Contains("[Image]buttonImage")))
                img.color = c;
            foreach (var img in Object.FindObjectsOfType<Image>()
                         .Where(x => x.GetComponentInParent<UICustomSelectable>() && x.name.Contains("[Image]Line")))
                img.color = c;
        }

        public static void BaseGameLoadPrefabEffect(BattleUnitModel unit, string prefabPath, string playSoundPath)
        {
            var gameObject = Util.LoadPrefab(prefabPath);
            if (gameObject != null)
                if (unit?.view != null)
                {
                    gameObject.transform.parent = unit.view.camRotationFollower;
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localScale = Vector3.one;
                    gameObject.transform.localRotation = Quaternion.identity;
                }

            SoundEffectPlayer.PlaySound(playSoundPath);
        }

        public static void IndexReleaseBreakEffect(BattleUnitModel unit)
        {
            var object2 = Resources.Load("Prefabs/Battle/SpecialEffect/IndexRelease_ActivateParticle");
            if (object2 != null)
            {
                var gameObject2 = Object.Instantiate(object2) as GameObject;
                if (gameObject2 != null)
                {
                    gameObject2.transform.parent = unit.view.charAppearance.transform;
                    gameObject2.transform.localPosition = Vector3.zero;
                    gameObject2.transform.localRotation = Quaternion.identity;
                    gameObject2.transform.localScale = Vector3.one;
                }
            }

            SingletonBehavior<SoundEffectManager>.Instance.PlayClip("Buf/Effect_Index_Unlock");
        }

        public static void MakeCustomBook(string packageId)
        {
            if (!ModParameters.CustomBookSkinsOptions.TryGetValue(packageId, out var customSkins)) return;
            var dictionary = Singleton<CustomizingResourceLoader>.Instance._skinData;
            foreach (var workshopSkinData in Singleton<CustomizingBookSkinLoader>.Instance
                         .GetWorkshopBookSkinData(packageId).Where(workshopSkinData =>
                             !workshopSkinData.dataName.Contains("x_proj")))
            {
                var customSkinOption = customSkins.FirstOrDefault(x => workshopSkinData.dataName.Contains(x.SkinName));
                if (customSkinOption == null) continue;
                var keypageName = string.Empty;
                if (customSkinOption.UseLocalization)
                {
                    var localization = ModParameters.LocalizedItems.TryGetValue(packageId, out var localizatedItem);
                    if (localization)
                        if (customSkinOption.KeypageId.HasValue)
                        {
                            var keypageLoc =
                                localizatedItem.Keypages.FirstOrDefault(x =>
                                    x.bookID == customSkinOption.KeypageId.Value);
                            if (keypageLoc != null) keypageName = keypageLoc.bookName;
                        }
                        else if (!string.IsNullOrEmpty(customSkinOption.KeypageName))
                        {
                            keypageName = customSkinOption.KeypageName;
                        }
                }

                dictionary.Add(workshopSkinData.dataName, new WorkshopSkinDataExtension
                {
                    dic = workshopSkinData.dic,
                    contentFolderIdx = workshopSkinData.dataName,
                    dataName = string.IsNullOrEmpty(keypageName) ? workshopSkinData.dataName : keypageName,
                    id = dictionary.Count,
                    PackageId = packageId,
                    RealKeypageId = customSkinOption.KeypageId
                });
            }
        }

        public static void ChangeEmotionCardColor(EmotionCardOptions cardOptions, ref _2dxFX_ColorChange component)
        {
            component._Color = cardOptions.ColorOptions.FrameColor ?? component._Color;
            component._HueShift = cardOptions.ColorOptions.FrameHSVColor?.H ?? component._HueShift;
            component._Saturation = cardOptions.ColorOptions.FrameHSVColor?.S ?? component._Saturation;
            component._ValueBrightness = cardOptions.ColorOptions.FrameHSVColor?.V ?? component._Saturation;
        }

        public static void ChangeEmotionCardColorTiphEgo(ref _2dxFX_ColorChange component)
        {
            component._Color = Color.yellow;
            component._HueShift = 90f;
        }

        public static void LocalizationCustomBook()
        {
            var dictionary = CustomizingResourceLoader.Instance._skinData;
            foreach (var packageId in ModParameters.PackageIds)
            {
                if (!ModParameters.CustomBookSkinsOptions.TryGetValue(packageId, out var customSkins)) continue;
                foreach (var workshopSkinData in dictionary.Where(x => customSkins.Exists(y => y.SkinName == x.Key))
                             .ToList())
                {
                    var customSkinOption = customSkins.FirstOrDefault(x => workshopSkinData.Key.Contains(x.SkinName));
                    if (customSkinOption?.KeypageId == null || !customSkinOption.UseLocalization) continue;
                    var localization = ModParameters.LocalizedItems.TryGetValue(packageId, out var localizatedItem);
                    if (!localization) continue;
                    var keypageLoc =
                        localizatedItem.Keypages.FirstOrDefault(x => x.bookID == customSkinOption.KeypageId.Value);
                    if (keypageLoc == null) continue;
                    workshopSkinData.Value.dataName = keypageLoc.bookName;
                    dictionary[workshopSkinData.Key] = workshopSkinData.Value;
                }
            }
        }

        public static void ChangeSpeedDiceColor(SpeedDiceUI instance, CustomDiceColorOptions colorOptions)
        {
            if (instance == null) return;
            if (!string.IsNullOrEmpty(colorOptions.IconId))
            {
                instance.img_normalFrame.sprite =
                    ModParameters.SpeedDieArtWorks.TryGetValue(colorOptions.IconId, out var sprite)
                        ? sprite
                        : instance.img_normalFrame.sprite;
                instance.img_lightFrame.sprite =
                    ModParameters.SpeedDieArtWorks.TryGetValue(colorOptions.IconId + "_Glow", out var spriteGlow)
                        ? spriteGlow
                        : instance.img_lightFrame.sprite;
                instance.img_highlightFrame.sprite =
                    ModParameters.SpeedDieArtWorks.TryGetValue(colorOptions.IconId + "_Hovered", out var spriteHover)
                        ? spriteHover
                        : instance.img_highlightFrame.sprite;
            }

            if (colorOptions.TextColor == null) return;
            instance._txtSpeedRange.color = colorOptions.TextColor.Value;
            instance._rouletteImg.color = colorOptions.TextColor.Value;
            instance._txtSpeedMax.color = colorOptions.TextColor.Value;
            instance.img_tensNum.color = colorOptions.TextColor.Value;
            instance.img_unitsNum.color = colorOptions.TextColor.Value;
            var rootColor = colorOptions.TextColor.Value;
            rootColor.a -= 0.6f;
            instance.img_breakedFrame.color = rootColor;
            instance.img_breakedLinearDodge.color = rootColor;
            instance.img_lockedFrame.color = rootColor;
            instance.img_lockedIcon.color = rootColor;
        }
    }
}