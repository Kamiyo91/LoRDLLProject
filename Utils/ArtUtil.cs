using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BigDLL4221.Enum;
using BigDLL4221.Models;
using HarmonyLib;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace BigDLL4221.Utils
{
    public static class ArtUtil
    {
        public static void InitCustomEffects(List<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                assembly.GetTypes().ToList().FindAll(x => x.Name.StartsWith("DiceAttackEffect_"))
                    .ForEach(delegate (Type x)
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

        public static void SetBooksData(UIOriginEquipPageList instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            if (!ModParameters.PackageIds.Contains(storyKey.workshopId)) return;
            var image = (Image)instance.GetType().GetField("img_IconGlow", AccessTools.all).GetValue(instance);
            var image2 = (Image)instance.GetType().GetField("img_Icon", AccessTools.all).GetValue(instance);
            var textMeshProUGUI = (TextMeshProUGUI)instance.GetType().GetField("txt_StoryName", AccessTools.all)
                .GetValue(instance);
            if (books.Count < 0) return;
            image.enabled = true;
            image2.enabled = true;
            var customIconTryGet = ModParameters.ArtWorks.TryGetValue(storyKey.workshopId, out var customIcon);
            var storyIcon = UISpriteDataManager.instance.GetStoryIcon("Chapter" + storyKey.chapter).icon;
            image2.sprite = customIconTryGet ? customIcon : storyIcon;
            image.sprite = customIconTryGet ? customIcon : storyIcon;
            var tryGetLocalizeItem =
                ModParameters.LocalizedItems.TryGetValue(storyKey.workshopId, out var localizedItem);
            textMeshProUGUI.text = tryGetLocalizeItem
                ? localizedItem.EffectTexts.TryGetValue(storyKey.workshopId, out var chapterName)
                    ? chapterName.Name
                    : storyKey.workshopId
                : storyKey.workshopId;
        }

        public static void SetEpisodeSlots(UIBookStoryChapterSlot instance, UIBookStoryPanel panel,
            List<UIBookStoryEpisodeSlot> episodeSlots)
        {
            if (instance.chapter != 7) return;
            foreach (var packageId in ModParameters.PackageIds)
            {
                var uibookStoryEpisodeSlot = episodeSlots.Find(x =>
                    x.books.Find(y => y.id.packageId == packageId) != null);
                if (uibookStoryEpisodeSlot == null) continue;
                if (ModParameters.CredenzaOptions.TryGetValue(packageId, out var credenzaOption))
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
                                        ModParameters.LocalizedItems.TryGetValue(packageId, out var localizedItem)
                                            ? localizedItem.EffectTexts.TryGetValue(packageId, out var credenza)
                                                ? credenza.Name
                                                : packageId
                                            : packageId;
                                    var image = (Image)uibookStoryEpisodeSlot.GetType()
                                        .GetField("episodeIconGlow", AccessTools.all)
                                        .GetValue(uibookStoryEpisodeSlot);
                                    var image2 = (Image)uibookStoryEpisodeSlot.GetType()
                                        .GetField("episodeIcon", AccessTools.all)
                                        .GetValue(uibookStoryEpisodeSlot);
                                    var customIconTryGet = ModParameters.ArtWorks.TryGetValue(packageId, out var customIcon);
                                    var storyIcon = UISpriteDataManager.instance.GetStoryIcon(panelBooks[0].BookIcon).icon;
                                    image2.sprite = customIconTryGet ? customIcon : storyIcon;
                                    image.sprite = customIconTryGet ? customIcon : storyIcon;
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

        public static void GetUnity(DirectoryInfo dir)
        {
            if (dir.GetDirectories().Length != 0)
            {
                var directories = dir.GetDirectories();
                foreach (var t in directories)
                    GetUnity(t);
            }

            foreach (var fileInfo in dir.GetFiles())
                AddAssets(Path.GetFileNameWithoutExtension(fileInfo.FullName), fileInfo.FullName);
        }

        private static void AddAssets(string name, string path)
        {
            var value = AssetBundle.LoadFromFile(path);
            ModParameters.AssetBundle.Add(name, value);
        }
    }
}