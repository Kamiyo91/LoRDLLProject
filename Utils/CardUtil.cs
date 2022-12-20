using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using BigDLL4221.Enum;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using HarmonyLib;
using LOR_DiceSystem;
using UnityEngine;

namespace BigDLL4221.Utils
{
    public static class CardUtil
    {
        public static void ChangeCardItem(ItemXmlDataList instance, string packageId)
        {
            try
            {
                var dictionary = instance._cardInfoTable;
                var list = instance._cardInfoList;
                if (dictionary == null) return;
                if (!ModParameters.CardOptions.TryGetValue(packageId, out var cardOptions)) return;
                foreach (var item in dictionary.Where(x => x.Key.packageId == packageId).ToList())
                {
                    var cardOption = cardOptions.FirstOrDefault(x => x.CardId == item.Key.id);
                    if (cardOption != null)
                        SetCustomCardOption(cardOption.Option, cardOption.Keywords, item.Key, ref dictionary, ref list);
                    else SetCustomCardOption(CardOption.Basic, new List<string>(), item.Key, ref dictionary, ref list);
                }

                if (!cardOptions.Any(x => x.IsBaseGameCard)) return;
                foreach (var item in dictionary.Where(x =>
                             string.IsNullOrEmpty(x.Key.packageId) &&
                             cardOptions.Exists(y => y.CardId == x.Key.id && y.IsBaseGameCard)).ToList())
                {
                    var cardOption = cardOptions.FirstOrDefault(x => x.CardId == item.Key.id && x.IsBaseGameCard);
                    if (cardOption != null)
                        SetCustomCardOption(cardOption.Option, cardOption.Keywords, item.Key, ref dictionary, ref list);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("There was an error while changing the Cards values " + ex.Message + " ModId : " +
                               packageId);
            }
        }

        private static void SetCustomCardOption(CardOption option, IEnumerable<string> keywords, LorId id,
            ref Dictionary<LorId, DiceCardXmlInfo> cardDictionary, ref List<DiceCardXmlInfo> cardXmlList)
        {
            if (ModParameters.DefaultKeyword.TryGetValue(id.packageId, out var defaultKeyword))
                keywords = keywords.Prepend(defaultKeyword);
            else if (option == CardOption.Basic) return;
            var cardOptions = option == CardOption.Basic ? new List<CardOption>() : new List<CardOption> { option };
            var diceCardXmlInfo2 = CardOptionChange(cardDictionary[id], cardOptions, keywords);
            cardDictionary[id] = diceCardXmlInfo2;
            cardXmlList.Add(diceCardXmlInfo2);
        }

        private static DiceCardXmlInfo CardOptionChange(DiceCardXmlInfo cardXml, List<CardOption> option,
            IEnumerable<string> keywords, string skinName = "", string mapName = "", int skinHeight = 0)
        {
            var enumerable = keywords.ToList();
            if (enumerable.Any())
            {
                cardXml.Keywords.AddRange(enumerable.Where(x => !cardXml.Keywords.Contains(x)));
                cardXml.Keywords = cardXml.Keywords.OrderBy(x =>
                {
                    var index = x.IndexOf("ModPage", StringComparison.InvariantCultureIgnoreCase);
                    return index < 0 ? 9999 : index;
                }).ToList();
            }

            return new DiceCardXmlInfo(cardXml.id)
            {
                workshopID = cardXml.workshopID,
                workshopName = cardXml.workshopName,
                Artwork = cardXml.Artwork,
                Chapter = cardXml.Chapter,
                category = cardXml.category,
                DiceBehaviourList = cardXml.DiceBehaviourList,
                _textId = cardXml._textId,
                optionList = option.Any() ? option : cardXml.optionList,
                Priority = cardXml.Priority,
                Rarity = cardXml.Rarity,
                Script = cardXml.Script,
                ScriptDesc = cardXml.ScriptDesc,
                Spec = cardXml.Spec,
                SpecialEffect = cardXml.SpecialEffect,
                SkinChange = string.IsNullOrEmpty(skinName) ? cardXml.SkinChange : skinName,
                SkinChangeType = cardXml.SkinChangeType,
                SkinHeight = skinHeight != 0 ? skinHeight : cardXml.SkinHeight,
                MapChange = string.IsNullOrEmpty(mapName) ? cardXml.MapChange : mapName,
                PriorityScript = cardXml.PriorityScript,
                Keywords = cardXml.Keywords
            };
        }

        public static void InitKeywordsList(List<Assembly> assemblies)
        {
            var dictionary = BattleCardAbilityDescXmlList.Instance._dictionaryKeywordCache;
            foreach (var assembly in assemblies)
            {
                assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(DiceCardSelfAbilityBase))
                                               && x.Name.StartsWith("DiceCardSelfAbility_"))
                    .Do(x => dictionary[x.Name.Replace("DiceCardSelfAbility_", "")] =
                        new List<string>(((DiceCardSelfAbilityBase)Activator.CreateInstance(x)).Keywords));
                assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(DiceCardAbilityBase))
                                               && x.Name.StartsWith("DiceCardAbility_"))
                    .Do(x => dictionary[x.Name.Replace("DiceCardAbility_", "")] =
                        new List<string>(((DiceCardAbilityBase)Activator.CreateInstance(x)).Keywords));
            }
        }

        public static List<EmotionCardXmlInfo> CustomCreateSelectableList(int emotionLevel, string pullCode = "")
        {
            var emotionLevelPull = emotionLevel <= 2 ? 1 : emotionLevel <= 4 ? 2 : 3;
            var code = string.IsNullOrEmpty(pullCode) ? StaticModsInfo.EmotionCardPullCode : pullCode;
            var dataCardList = ModParameters.EmotionCards.SelectMany(x => x.Value).Where(x =>
                x.FloorCode.Contains(code) && x.CardXml.EmotionLevel == emotionLevelPull &&
                !x.CardXml.Locked).Select(x => x.CardXml as EmotionCardXmlInfo).ToList();
            if (!dataCardList.Any()) return dataCardList;
            var instance = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            var selectedList = instance._selectedList;
            if (selectedList != null && selectedList.Any())
                foreach (var item in selectedList)
                    dataCardList.Remove(item);
            var center = CalcuateSelectionCoins(instance, emotionLevel);
            dataCardList.Sort((x, y) => Mathf.Abs(x.EmotionRate - center) - Mathf.Abs(y.EmotionRate - center));
            var list = new List<EmotionCardXmlInfo>();
            while (dataCardList.Count > 0 && list.Count < 3)
            {
                var er = Mathf.Abs(dataCardList[0].EmotionRate - center);
                var list2 = dataCardList.FindAll(x => Mathf.Abs(x.EmotionRate - center) == er);
                if (list2.Count + list.Count <= 3)
                {
                    list.AddRange(list2);
                    using (var enumerator2 = list2.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            var item2 = enumerator2.Current;
                            dataCardList.Remove(item2);
                        }

                        continue;
                    }
                }

                var i = 0;
                while (i < 3 - list.Count && list2.Count != 0)
                {
                    var item3 = RandomUtil.SelectOne(list2);
                    list2.Remove(item3);
                    dataCardList.Remove(item3);
                    list.Add(item3);
                    i++;
                }
            }

            return list;
        }

        public static int CalcuateSelectionCoins(StageLibraryFloorModel instance, int emotionLevel)
        {
            var num = 0;
            var num2 = 0;
            var unitList = instance._unitList;
            if (unitList == null || !unitList.Any()) return 0;
            foreach (var unitBattleDataModel in
                     unitList.Where(unitBattleDataModel => unitBattleDataModel.IsAddedBattle))
            {
                num += unitBattleDataModel.emotionDetail.totalPositiveCoins.Count;
                num2 += unitBattleDataModel.emotionDetail.totalNegativeCoins.Count;
            }

            var num4 = num + num2 > 0 ? (num - num2) / (float)(num + num2) : 0.5f;
            var num5 = num4 / ((11f - emotionLevel) / 10f);
            var center = Mathf.Abs(num5) < 0.1 ? 0 : Mathf.Abs(num5) < 0.3 ? num5 > 0f ? 1 : -1 : num5 > 0f ? 2 : -2;
            return center;
        }

        public static List<EmotionEgoXmlInfo> CustomCreateSelectableEgoList(string pullCode = "")
        {
            var code = string.IsNullOrEmpty(pullCode) ? StaticModsInfo.EgoCardPullCode : pullCode;
            var dataEgoCardList = ModParameters.EmotionEgoCards.SelectMany(x => x.Value)
                .Where(x => x.FloorCode.Contains(code)).Select(x => x.CardXml).ToList();
            var sephirah = Singleton<StageController>.Instance.GetCurrentStageFloorModel().Sephirah;
            var egoCardList = new List<EmotionEgoXmlInfo>();
            if (!Singleton<SpecialCardListModel>.Instance._cardSelectedDataByFloor.TryGetValue(sephirah,
                    out var cardList)) return egoCardList;
            if (cardList.Any())
                using (var enumerator = cardList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var cardModel = enumerator.Current;
                        dataEgoCardList.RemoveAll(x => cardModel != null && x.CardId == cardModel.GetID());
                    }
                }

            egoCardList.AddRange(MathUtil.Combination(3, dataEgoCardList.Count)
                .Select(index => dataEgoCardList[index]));
            return egoCardList;
        }

        public static EmotionCardXmlExtension CardXmlConverter(string packageId, EmotionCardXmlInfo cardXml)
        {
            var newXml = new EmotionCardXmlExtension
            {
                LorId = new LorId(packageId, cardXml.id),
                Sephirah = cardXml.Sephirah,
                TargetType = cardXml.TargetType,
                Script = cardXml.Script,
                EmotionLevel = cardXml.EmotionLevel,
                EmotionRate = cardXml.EmotionRate,
                Level = cardXml.Level,
                Locked = cardXml.Locked,
                Name = cardXml.Name,
                _artwork = cardXml._artwork,
                State = cardXml.State,
                id = cardXml.id
            };
            return newXml;
        }

        public static void LoadEmotionAndEgoCards(string packageId, string path, List<Assembly> assemblies)
        {
            var error = false;
            try
            {
                var file = new DirectoryInfo(path).GetFiles().FirstOrDefault();
                error = true;
                var changedCardList = new List<EmotionCardOptions>();
                if (file != null)
                {
                    var list = EmotionCardXmlList.Instance._list;
                    using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                    {
                        using (var enumerator =
                               ((EmotionCardXmlRoot)new XmlSerializer(typeof(EmotionCardXmlRoot)).Deserialize(
                                   stringReader)).emotionCardXmlList.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var a = enumerator.Current;
                                if (a == null) continue;
                                var card = CardXmlConverter(packageId, a);
                                card.LorId = new LorId(packageId, card.id);
                                card.id = -1;
                                changedCardList.Add(new EmotionCardOptions(card));
                                list.Add(card);
                            }
                        }
                    }

                    if (changedCardList.Any())
                    {
                        if (ModParameters.EmotionCards.ContainsKey(packageId))
                            ModParameters.EmotionCards.Remove(packageId);
                        ModParameters.EmotionCards.Add(packageId, changedCardList);
                    }
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Emotion Card packageId : " + packageId + " Error : " + ex.Message);
            }

            var changedEgoCardList = new List<EmotionEgoOptions>();
            var cardList = ItemXmlDataList.instance._cardInfoList;
            var egoList = EmotionEgoXmlList.Instance._list;
            foreach (var emotionEgoXmlInfo in cardList.FindAll(x =>
                         x.id.packageId == packageId && x.optionList.Contains(CardOption.EGO)).Select(diceCardXmlInfo =>
                         new EmotionEgoCardXmlExtension
                         {
                             id = diceCardXmlInfo.id.id,
                             _CardId = diceCardXmlInfo.id.id,
                             Sephirah = SephirahType.ETC,
                             isLock = false,
                             PackageId = packageId
                         }))
            {
                egoList?.Add(emotionEgoXmlInfo);
                changedEgoCardList.Add(new EmotionEgoOptions(emotionEgoXmlInfo, packageId: packageId));
            }

            if (changedEgoCardList.Any())
            {
                if (ModParameters.EmotionEgoCards.ContainsKey(packageId))
                    ModParameters.EmotionEgoCards.Remove(packageId);
                ModParameters.EmotionEgoCards.Add(packageId, changedEgoCardList);
            }

            foreach (var type in from assembly in assemblies
                     from type in assembly.GetTypes()
                     where type.Name.Contains("EmotionCardAbility_")
                     select type)
            {
                if (StaticModsInfo.EmotionCardAbility.ContainsKey(type.Name))
                {
                    Debug.LogError(
                        $"Emotion Script ability with this name already Exist, being overwritten by Mod Id {packageId}.");
                    StaticModsInfo.EmotionCardAbility.Remove(type.Name);
                }

                StaticModsInfo.EmotionCardAbility.Add(type.Name, type);
            }
        }

        public static void LoadEmotionAbilities(List<Assembly> assemblies)
        {
            foreach (var type in from assembly in assemblies
                     from type in assembly.GetTypes()
                     where type.Name.Contains("EmotionCardAbility_")
                     select type)
            {
                if (StaticModsInfo.EmotionCardAbility.ContainsKey(type.Name))
                {
                    Debug.LogError("Emotion Script ability with this name already Exist, being overwritten.");
                    StaticModsInfo.EmotionCardAbility.Remove(type.Name);
                }

                StaticModsInfo.EmotionCardAbility.Add(type.Name, type);
            }
        }

        public static void LoadEmotionAndEgoCards(string packageId, string path)
        {
            var error = false;
            try
            {
                var file = new DirectoryInfo(path).GetFiles().FirstOrDefault();
                error = true;
                var changedCardList = new List<EmotionCardOptions>();
                if (file != null)
                {
                    var list = EmotionCardXmlList.Instance._list;
                    using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                    {
                        using (var enumerator =
                               ((EmotionCardXmlRoot)new XmlSerializer(typeof(EmotionCardXmlRoot)).Deserialize(
                                   stringReader)).emotionCardXmlList.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var a = enumerator.Current;
                                if (a == null) continue;
                                var card = CardXmlConverter(packageId, a);
                                card.LorId = new LorId(packageId, card.id);
                                card.id = -1;
                                changedCardList.Add(new EmotionCardOptions(card));
                                list.Add(card);
                            }
                        }
                    }

                    if (changedCardList.Any())
                    {
                        if (ModParameters.EmotionCards.ContainsKey(packageId))
                            ModParameters.EmotionCards.Remove(packageId);
                        ModParameters.EmotionCards.Add(packageId, changedCardList);
                    }
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Emotion Card packageId : " + packageId + " Error : " + ex.Message);
            }

            var changedEgoCardList = new List<EmotionEgoOptions>();
            var cardList = ItemXmlDataList.instance._cardInfoList;
            var egoList = EmotionEgoXmlList.Instance._list;
            foreach (var emotionEgoXmlInfo in cardList.FindAll(x =>
                         x.id.packageId == packageId && x.optionList.Contains(CardOption.EGO)).Select(diceCardXmlInfo =>
                         new EmotionEgoCardXmlExtension
                         {
                             id = diceCardXmlInfo.id.id,
                             _CardId = diceCardXmlInfo.id.id,
                             Sephirah = SephirahType.ETC,
                             isLock = false,
                             PackageId = packageId
                         }))
            {
                egoList?.Add(emotionEgoXmlInfo);
                changedEgoCardList.Add(new EmotionEgoOptions(emotionEgoXmlInfo, packageId: packageId));
            }

            if (!changedEgoCardList.Any()) return;
            if (ModParameters.EmotionEgoCards.ContainsKey(packageId))
                ModParameters.EmotionEgoCards.Remove(packageId);
            ModParameters.EmotionEgoCards.Add(packageId, changedEgoCardList);
        }

        public static void SaveCardsBeforeChange(SephirahType sephirah)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(sephirah, out var savedOptions)) return;
            var listEmotionXmlCards = EmotionCardXmlList.Instance._list;
            if (listEmotionXmlCards != null)
            {
                var listEmotionCards = (from emotionCardXmlInfo in listEmotionXmlCards
                    where emotionCardXmlInfo.Sephirah == sephirah
                    select new EmotionCardXmlInfo
                    {
                        Name = emotionCardXmlInfo.Name,
                        _artwork = emotionCardXmlInfo._artwork,
                        State = emotionCardXmlInfo.State,
                        Sephirah = sephirah,
                        EmotionLevel = emotionCardXmlInfo.EmotionLevel,
                        TargetType = emotionCardXmlInfo.TargetType,
                        Script = emotionCardXmlInfo.Script,
                        Level = emotionCardXmlInfo.Level,
                        EmotionRate = emotionCardXmlInfo.EmotionRate,
                        Locked = emotionCardXmlInfo.Locked
                    }).ToList();
                savedOptions.FloorOptions.OriginalEmotionCards = listEmotionCards;
            }

            var listEmotionEgoXmlCards = EmotionEgoXmlList.Instance._list;
            if (listEmotionEgoXmlCards == null) return;
            var listFloorEgoCards = (from emotionEgoXmlInfo in listEmotionEgoXmlCards
                where emotionEgoXmlInfo.Sephirah == sephirah
                select new EmotionEgoXmlInfo
                {
                    _CardId = emotionEgoXmlInfo._CardId,
                    id = emotionEgoXmlInfo.id,
                    Sephirah = sephirah,
                    isLock = emotionEgoXmlInfo.isLock
                }).ToList();
            savedOptions.FloorOptions.OriginalEgoCards = listFloorEgoCards;
        }

        public static void ChangeAbnoAndEgo(string packageId, SephirahType sephirah, CustomFloorOptions floorOptions)
        {
            if (ModParameters.EmotionCards.TryGetValue(packageId, out var cards))
            {
                var customEmotionCardList = cards
                    .Where(x => x.FloorCode.Contains(floorOptions.FloorCode))
                    .Select(x => x.CardXml).ToList();
                if (customEmotionCardList.Any())
                {
                    var listEmotionXmlCards = EmotionCardXmlList.Instance._list;
                    if (listEmotionXmlCards != null)
                    {
                        foreach (var item in listEmotionXmlCards.Where(x => x.Sephirah == sephirah))
                        {
                            item.Name = $"{item.Name}RevertCardBigDLL4221{sephirah}";
                            item.Sephirah = SephirahType.ETC;
                        }

                        foreach (var card in listEmotionXmlCards.Where(x => customEmotionCardList.Contains(x)))
                            card.Sephirah = sephirah;
                    }
                }
            }

            if (!ModParameters.EmotionEgoCards.TryGetValue(packageId, out var egoCards)) return;
            var customEmotionEgoCardList = egoCards
                .Where(x => x.FloorCode.Contains(floorOptions.FloorCode)).Select(x => x.CardXml).ToList();
            var listEmotionEgoXmlCards = EmotionEgoXmlList.Instance._list;
            if (listEmotionEgoXmlCards == null) return;
            foreach (var item in listEmotionEgoXmlCards.Where(x => x.Sephirah == sephirah))
                item.Sephirah = SephirahType.ETC;
            foreach (var card in listEmotionEgoXmlCards.Where(x => customEmotionEgoCardList.Contains(x)))
                card.Sephirah = sephirah;
        }

        public static void RevertAbnoAndEgo(SephirahType sephirah)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(sephirah, out var savedOptions)) return;
            var emotionCardList = EmotionCardXmlList.Instance._list;
            if (emotionCardList != null)
            {
                foreach (var card in emotionCardList.Where(x => x.Sephirah == sephirah))
                    card.Sephirah = SephirahType.ETC;
                foreach (var card in emotionCardList.Where(x => x.Name.Contains($"RevertCardBigDLL4221{sephirah}")))
                {
                    card.Name = card.Name.Replace($"RevertCardBigDLL4221{sephirah}", "");
                    card.Sephirah = sephirah;
                }
            }

            var floorEgoCards = savedOptions.FloorOptions.OriginalEgoCards;
            var emotionEgoCardList = EmotionEgoXmlList.Instance._list;
            if (emotionEgoCardList == null) return;
            foreach (var card in emotionEgoCardList.Where(x => x.Sephirah == sephirah))
                card.Sephirah = SephirahType.ETC;
            foreach (var card in emotionEgoCardList.Where(x =>
                         x.Sephirah == SephirahType.ETC && floorEgoCards.Exists(y => y.id == x.id)))
                card.Sephirah = sephirah;
        }

        public static void SetPullCodeCards(string packageId, string pullCode, TypeCardEnum cardType, List<int> cardIds)
        {
            var emotionCardFound = ModParameters.EmotionCards.TryGetValue(packageId, out var cards);
            var emotionEgoFound = ModParameters.EmotionEgoCards.TryGetValue(packageId, out var egoCards);
            switch (cardType)
            {
                case TypeCardEnum.Emotion:
                    if (!emotionCardFound) return;
                    foreach (var card in cardIds
                                 .Select(cardId =>
                                     cards.FirstOrDefault(x => x.CardXml.LorId == new LorId(packageId, cardId)))
                                 .Where(card => card != null))
                    {
                        cards.Remove(card);
                        card.Code.Add(pullCode);
                        cards.Add(card);
                    }

                    return;
                case TypeCardEnum.Ego:
                    if (!emotionEgoFound) return;
                    foreach (var card in cardIds
                                 .Select(cardId =>
                                     egoCards.FirstOrDefault(x => x.CardXml.CardId == new LorId(packageId, cardId)))
                                 .Where(card => card != null))
                    {
                        egoCards.Remove(card);
                        card.Code.Add(pullCode);
                        egoCards.Add(card);
                    }

                    return;
                default:
                    return;
            }
        }

        public static void SetFloorPullCodeCards(string packageId, string pullCode, TypeCardEnum cardType,
            List<int> cardIds)
        {
            var emotionCardFound = ModParameters.EmotionCards.TryGetValue(packageId, out var cards);
            var emotionEgoFound = ModParameters.EmotionEgoCards.TryGetValue(packageId, out var egoCards);
            switch (cardType)
            {
                case TypeCardEnum.Emotion:
                    if (!emotionCardFound) return;
                    foreach (var card in cardIds
                                 .Select(cardId =>
                                     cards.FirstOrDefault(x => x.CardXml.LorId == new LorId(packageId, cardId)))
                                 .Where(card => card != null))
                    {
                        cards.Remove(card);
                        card.FloorCode.Add(pullCode);
                        cards.Add(card);
                    }

                    return;
                case TypeCardEnum.Ego:
                    if (!emotionEgoFound) return;
                    foreach (var card in cardIds
                                 .Select(cardId =>
                                     egoCards.FirstOrDefault(x => x.CardXml.CardId == new LorId(packageId, cardId)))
                                 .Where(card => card != null))
                    {
                        egoCards.Remove(card);
                        card.FloorCode.Add(pullCode);
                        egoCards.Add(card);
                    }

                    return;
                default:
                    return;
            }
        }

        public static void SetEmotionCardColors(string packageId, List<int> cardIds,
            EmotionCardColorOptions cardColorOptions)
        {
            if (!ModParameters.EmotionCards.TryGetValue(packageId, out var cards)) return;
            foreach (var card in cardIds
                         .Select(cardId =>
                             cards.FirstOrDefault(x => x.CardXml.LorId == new LorId(packageId, cardId)))
                         .Where(card => card != null))
            {
                cards.Remove(card);
                card.ColorOptions = cardColorOptions;
                cards.Add(card);
            }
        }

        public static void SetEmotionCardOnlyForBookIds(string packageId, List<int> cardIds, List<LorId> bookIds)
        {
            if (!ModParameters.EmotionCards.TryGetValue(packageId, out var cards)) return;
            foreach (var card in cardIds
                         .Select(cardId => cards.FirstOrDefault(x => x.CardXml.LorId == new LorId(packageId, cardId)))
                         .Where(card => card != null))
            {
                cards.Remove(card);
                card.UsableByBookIds.AddRange(bookIds);
                cards.Add(card);
            }
        }

        public static void FillDictionary()
        {
            var sephirahTypeList = new List<SephirahType>
            {
                SephirahType.Keter, SephirahType.Hokma, SephirahType.Binah,
                SephirahType.Chesed, SephirahType.Gebura, SephirahType.Tiphereth,
                SephirahType.Netzach, SephirahType.Hod, SephirahType.Yesod, SephirahType.Malkuth
            };
            foreach (var sephirah in sephirahTypeList)
                StaticModsInfo.EgoAndEmotionCardChanged.Add(sephirah, new SavedFloorOptions());
        }

        public static EmotionCardXmlInfo GetEmotionCard(string packageId, int cardId)
        {
            return !ModParameters.EmotionCards.TryGetValue(packageId, out var cards)
                ? null
                : cards.Where(x => x.CardXml.LorId.id == cardId).Select(x => x.CardXml).FirstOrDefault();
        }

        public static void ChangeCardColor(LorId cardId, CardColorOptions colorOptions)
        {
            if (!ModParameters.CardOptions.TryGetValue(cardId.packageId, out var cardOptions)) return;
            var cardOption = cardOptions.FirstOrDefault(x => x.CardId == cardId.id);
            if (cardOption == null) return;
            cardOptions.Remove(cardOption);
            cardOption.CardColorOptions = colorOptions;
            cardOptions.Add(cardOption);
        }
    }
}