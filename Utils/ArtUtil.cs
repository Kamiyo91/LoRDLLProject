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

        private static Sprite GetIcon(CredenzaOptions credenzaOptions, string baseIcon)
        {
            return ModParameters.ArtWorks.TryGetValue(credenzaOptions.CustomIconSpriteId, out var customIcon)
                ? customIcon
                : UISpriteDataManager.instance.GetStoryIcon(string.IsNullOrEmpty(credenzaOptions.BaseIconSpriteId)
                    ? baseIcon
                    : credenzaOptions.BaseIconSpriteId).icon;
        }

        private static string CredenzaName(CredenzaOptions credenzaOptions, string packageId)
        {
            return ModParameters.LocalizedItems.TryGetValue(credenzaOptions.CredenzaNameId, out var localizedItem)
                ? localizedItem.EffectTexts.TryGetValue(packageId, out var credenza) ? credenza.Name
                : !string.IsNullOrEmpty(credenzaOptions.CredenzaName) ? credenzaOptions.CredenzaName
                : packageId
                : !string.IsNullOrEmpty(credenzaOptions.CredenzaName)
                    ? credenzaOptions.CredenzaName
                    : packageId;
        }

        public static void OnSelectEpisodeSlot(UIBookStoryPanel instance,
            UIBookStoryEpisodeSlot slot, TextMeshProUGUI selectedEpisodeText, Image selectedEpisodeIcon,
            Image selectedEpisodeIconGlow)
        {
            if (slot == null) return;
            var book = slot.books.FirstOrDefault(x => ModParameters.PackageIds.Contains(x.id.packageId));
            if (book == null) return;
            var credenzaOptionsTryGet =
                ModParameters.CredenzaOptions.TryGetValue(book.id.packageId, out var credenzaOptions);
            if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
            selectedEpisodeText.text = CredenzaName(credenzaOptions, book.id.packageId);
            var icon = GetIcon(credenzaOptions, slot.books[0].BookIcon);
            selectedEpisodeIcon.sprite = icon;
            selectedEpisodeIconGlow.sprite = icon;
            instance.UpdateBookSlots();
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
            var credenzaOptionsTryGet =
                ModParameters.CredenzaOptions.TryGetValue(storyKey.workshopId, out var credenzaOptions);
            if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
            image.enabled = true;
            image2.enabled = true;
            var icon = GetIcon(credenzaOptions, "Chapter" + storyKey.chapter);
            image2.sprite = icon;
            image.sprite = icon;
            textMeshProUGUI.text = CredenzaName(credenzaOptions, storyKey.workshopId);
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
                                var credenzaOptionsTryGet =
                                    ModParameters.CredenzaOptions.TryGetValue(packageId, out var credenzaOptions);
                                if (!credenzaOptionsTryGet) credenzaOptions = new CredenzaOptions();
                                uibookStoryEpisodeSlot.Init(panelBooks, instance);
                                ((TextMeshProUGUI)uibookStoryEpisodeSlot.GetType()
                                        .GetField("episodeText", AccessTools.all)
                                        .GetValue(uibookStoryEpisodeSlot)).text =
                                    CredenzaName(credenzaOptions, packageId);
                                var image = (Image)uibookStoryEpisodeSlot.GetType()
                                    .GetField("episodeIconGlow", AccessTools.all)
                                    .GetValue(uibookStoryEpisodeSlot);
                                var image2 = (Image)uibookStoryEpisodeSlot.GetType()
                                    .GetField("episodeIcon", AccessTools.all)
                                    .GetValue(uibookStoryEpisodeSlot);
                                var icon = GetIcon(credenzaOptions, panelBooks[0].BookIcon);
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
    }
}