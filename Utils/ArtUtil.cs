using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BigDLL4221.Enum;
using BigDLL4221.Models;
using HarmonyLib;
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

        public static void GetCardArtWorks(DirectoryInfo dir)
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
                ModParameters.CardArtWorks[fileNameWithoutExtension] = value;
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

        public static void SetBooksDataOriginal(UIOriginEquipPageList instance,
            List<BookModel> books, UIStoryKeyData storyKey, Image img_EdgeFrame, Image img_LineFrame,
            Image img_IconGlow, Image img_Icon)
        {
            if (!ModParameters.PackageIds.Contains(storyKey.workshopId)) return;
            var image = (Image)instance.GetType().GetField("img_IconGlow", AccessTools.all).GetValue(instance);
            var image2 = (Image)instance.GetType().GetField("img_Icon", AccessTools.all).GetValue(instance);
            var textMeshProUGUI = (TextMeshProUGUI)instance.GetType().GetField("txt_StoryName", AccessTools.all)
                .GetValue(instance);
            if (books.Count < 0) return;
            var credenzaOptionsTryGet =
                ModParameters.CredenzaOptions.TryGetValue(storyKey.workshopId, out var credenzaOptions);
            if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
            image.enabled = true;
            image2.enabled = true;
            var icon = GetIcon(credenzaOptions.CustomIconSpriteId, credenzaOptions.BaseIconSpriteId,
                "Chapter" + storyKey.chapter);
            image2.sprite = icon;
            image.sprite = icon;
            textMeshProUGUI.text = CredenzaName(credenzaOptions.CredenzaNameId, credenzaOptions.CredenzaName,
                storyKey.workshopId);
            if (credenzaOptions.BookDataColor == null) return;
            if (credenzaOptions.BookDataColor.FrameColor.HasValue)
                SetBooksDataFrameColor(credenzaOptions.BookDataColor.FrameColor.Value, img_EdgeFrame, img_LineFrame,
                    img_IconGlow, img_Icon);
            if (!credenzaOptions.BookDataColor.TextColor.HasValue) return;
            textMeshProUGUI.color = credenzaOptions.BookDataColor.TextColor.Value;
            var component = textMeshProUGUI.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = credenzaOptions.BookDataColor.TextColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void SetBooksData(UIOriginEquipPageList instance,
            List<BookModel> books, UIStoryKeyData storyKey, Image img_EdgeFrame, Image img_LineFrame,
            Image img_IconGlow, Image img_Icon)
        {
            var categoryOptions = ModParameters.CategoryOptions.SelectMany(x =>
                x.Value.Where(y => storyKey.workshopId == y.PackageId + y.AdditionalValue));
            var categoryOption =
                categoryOptions.FirstOrDefault(x => storyKey.workshopId == x.PackageId + x.AdditionalValue);
            if (categoryOption == null)
            {
                SetBooksDataOriginal(instance, books, storyKey, img_EdgeFrame, img_LineFrame, img_IconGlow, img_Icon);
                return;
            }

            var image = (Image)instance.GetType().GetField("img_IconGlow", AccessTools.all).GetValue(instance);
            var image2 = (Image)instance.GetType().GetField("img_Icon", AccessTools.all).GetValue(instance);
            var textMeshProUGUI = (TextMeshProUGUI)instance.GetType().GetField("txt_StoryName", AccessTools.all)
                .GetValue(instance);
            if (books.Count < 0) return;
            image.enabled = true;
            image2.enabled = true;
            var icon = GetIcon(categoryOption.CustomIconSpriteId, categoryOption.BaseIconSpriteId,
                "Chapter" + storyKey.chapter);
            image2.sprite = icon;
            image.sprite = icon;
            textMeshProUGUI.text = CredenzaName(categoryOption.CategoryNameId, categoryOption.CategoryName,
                categoryOption.PackageId);
            if (categoryOption.BookDataColor == null) return;
            if (categoryOption.BookDataColor.FrameColor.HasValue)
                SetBooksDataFrameColor(categoryOption.BookDataColor.FrameColor.Value, img_EdgeFrame, img_LineFrame,
                    img_IconGlow, img_Icon);
            if (!categoryOption.BookDataColor.TextColor.HasValue) return;
            textMeshProUGUI.color = categoryOption.BookDataColor.TextColor.Value;
            var component = textMeshProUGUI.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = categoryOption.BookDataColor.TextColor.Value;
            component.enabled = false;
            component.enabled = true;
        }

        public static void ResetColorData(UIOriginEquipPageList instance, Image img_Icon)
        {
            var defaultColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
            img_Icon.color = defaultColor;
            var text = (TextMeshProUGUI)instance.GetType().GetField("txt_StoryName", AccessTools.all)
                ?.GetValue(instance);
            if (text == null) return;
            text.color = defaultColor;
            var component = text.gameObject.GetComponent<TextMeshProMaterialSetter>();
            component.underlayColor = defaultColor;
            component.enabled = false;
            component.enabled = true;
        }

        public static void SetBooksDataFrameColor(Color c, Image img_EdgeFrame, Image img_LineFrame, Image img_IconGlow,
            Image img_Icon)
        {
            img_EdgeFrame.color = c;
            img_LineFrame.color = c;
            img_IconGlow.color = c;
            img_Icon.color = c;
        }

        public static void SetMainData(List<BookModel> currentBookModelList, List<UIStoryKeyData> totalkeysdata,
            Dictionary<UIStoryKeyData, List<BookModel>> currentStoryBooksDic)
        {
            var categoryOptions =
                ModParameters.CategoryOptions.Where(x => currentBookModelList.Exists(y => y.BookId.packageId == x.Key));
            foreach (var categoryOption in categoryOptions)
            {
                var index = totalkeysdata.FindIndex(x => x.IsWorkshop && x.workshopId == categoryOption.Key);
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
                                    ((TextMeshProUGUI)uibookStoryEpisodeSlot.GetType()
                                            .GetField("episodeText", AccessTools.all)
                                            .GetValue(uibookStoryEpisodeSlot)).text =
                                        CredenzaName(categoryOption.CategoryNameId, categoryOption.CategoryName,
                                            packageId);
                                    var image = (Image)uibookStoryEpisodeSlot.GetType()
                                        .GetField("episodeIconGlow", AccessTools.all)
                                        .GetValue(uibookStoryEpisodeSlot);
                                    var image2 = (Image)uibookStoryEpisodeSlot.GetType()
                                        .GetField("episodeIcon", AccessTools.all)
                                        .GetValue(uibookStoryEpisodeSlot);
                                    var icon = GetIcon(categoryOption.CustomIconSpriteId,
                                        categoryOption.BaseIconSpriteId,
                                        panelBooks[0].BookIcon);
                                    image2.sprite = icon;
                                    image.sprite = icon;
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
                                ((TextMeshProUGUI)uibookStoryEpisodeSlot.GetType()
                                        .GetField("episodeText", AccessTools.all)
                                        .GetValue(uibookStoryEpisodeSlot)).text =
                                    CredenzaName(credenzaOption.CredenzaNameId, credenzaOption.CredenzaName,
                                        packageId);
                                var image = (Image)uibookStoryEpisodeSlot.GetType()
                                    .GetField("episodeIconGlow", AccessTools.all)
                                    .GetValue(uibookStoryEpisodeSlot);
                                var image2 = (Image)uibookStoryEpisodeSlot.GetType()
                                    .GetField("episodeIcon", AccessTools.all)
                                    .GetValue(uibookStoryEpisodeSlot);
                                var icon = GetIcon(credenzaOption.CustomIconSpriteId, credenzaOption.BaseIconSpriteId,
                                    panelBooks[0].BookIcon);
                                image2.sprite = icon;
                                image.sprite = icon;
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
    }
}