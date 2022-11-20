using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using BigDLL4221.Enum;
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
                var dictionary = (Dictionary<LorId, DiceCardXmlInfo>)instance.GetType()
                    .GetField("_cardInfoTable", AccessTools.all)
                    ?.GetValue(instance);
                var list = (List<DiceCardXmlInfo>)instance.GetType()
                    .GetField("_cardInfoList", AccessTools.all)
                    ?.GetValue(instance);
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
            foreach (var assembly in assemblies)
                if (typeof(BattleCardAbilityDescXmlList).GetField("_dictionaryKeywordCache", AccessTools.all)
                        ?.GetValue(BattleCardAbilityDescXmlList.Instance) is Dictionary<string, List<string>>
                    dictionary)
                    assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(DiceCardSelfAbilityBase))
                                                   && x.Name.StartsWith("DiceCardSelfAbility_"))
                        .Do(x => dictionary[x.Name.Replace("DiceCardSelfAbility_", "")] =
                            new List<string>(((DiceCardSelfAbilityBase)Activator.CreateInstance(x)).Keywords));
        }

        public static List<EmotionCardXmlInfo> CustomCreateSelectableList(int emotionLevel)
        {
            var emotionLevelPull = emotionLevel <= 2 ? 1 : emotionLevel <= 4 ? 2 : 3;
            var code = StaticModsInfo.EmotionCardPullCode;
            var dataCardList = ModParameters.EmotionCards
                .Where(x => x.Value.Code.Contains(code) && x.Value.CardXml.EmotionLevel == emotionLevelPull &&
                            !x.Value.CardXml.Locked).Select(x => x.Value.CardXml).ToList();
            if (!dataCardList.Any()) return dataCardList;
            var instance = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            var selectedList = (List<EmotionCardXmlInfo>)instance.GetType().GetField("_selectedList", AccessTools.all)
                ?.GetValue(instance);
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
            var unitList =
                (List<UnitBattleDataModel>)instance.GetType().GetField("_unitList", AccessTools.all)
                    ?.GetValue(instance);
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

        public static List<EmotionEgoXmlInfo> CustomCreateSelectableEgoList()
        {
            var code = StaticModsInfo.EgoCardPullCode;
            var dataEgoCardList = ModParameters.EmotionEgoCards.Where(x => x.Value.Code.Contains(code))
                .Select(x => x.Value.CardXml).ToList();
            var sephirah = Singleton<StageController>.Instance.GetCurrentStageFloorModel().Sephirah;
            var egoCardList = new List<EmotionEgoXmlInfo>();
            if (!(Singleton<SpecialCardListModel>.Instance.GetType()
                        .GetField("_cardSelectedDataByFloor", AccessTools.all)
                        ?.GetValue(Singleton<SpecialCardListModel>.Instance) is
                    Dictionary<SephirahType, List<BattleDiceCardModel>> dictionary) ||
                !dictionary.TryGetValue(sephirah, out var cardList)) return egoCardList;
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

        public static void LoadEmotionAndEgoCards(string packageId, string path, List<Assembly> assemblies)
        {
            var error = false;
            try
            {
                var file = new DirectoryInfo(path).GetFiles().FirstOrDefault();
                error = true;
                if (file != null)
                {
                    var list = (List<EmotionCardXmlInfo>)typeof(EmotionCardXmlList).GetField("_list", AccessTools.all)
                        .GetValue(Singleton<EmotionCardXmlList>.Instance);
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
                                list.RemoveAll(x => x.id == a.id);
                                if (ModParameters.EmotionCards.TryGetValue(a.id, out var oldCard))
                                {
                                    Debug.LogError(
                                        $"Emotion Card with this Id already Exist, being overwritten my Mod Id {packageId}.Card script before being overwritten {oldCard.CardXml.Script.FirstOrDefault()} - Card script overwritten with {a.Script.FirstOrDefault()}");
                                    ModParameters.EmotionCards.Remove(a.id);
                                }

                                ModParameters.EmotionCards.Add(a.id, new EmotionCardOptions(a));
                                list.Add(a);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Emotion Card packageId : " + packageId + " Error : " + ex.Message);
            }

            var cardList = (List<DiceCardXmlInfo>)ItemXmlDataList.instance.GetType()
                .GetField("_cardInfoList", AccessTools.all)
                ?.GetValue(ItemXmlDataList.instance);
            var egoList = typeof(EmotionEgoXmlList).GetField("_list", AccessTools.all)
                .GetValue(Singleton<EmotionEgoXmlList>.Instance) as List<EmotionEgoXmlInfo>;
            foreach (var diceCardXmlInfo in cardList.FindAll(x =>
                         x.id.packageId == packageId && x.optionList.Contains(CardOption.EGO)))
            {
                var emotionEgoXmlInfo = new EmotionEgoXmlInfo
                {
                    id = diceCardXmlInfo.id.id,
                    _CardId = diceCardXmlInfo.id.id,
                    Sephirah = SephirahType.ETC,
                    isLock = false
                };
                egoList?.Add(emotionEgoXmlInfo);
                if (ModParameters.EmotionEgoCards.ContainsKey(diceCardXmlInfo.id.id))
                {
                    Debug.LogError(
                        $"Emotion Ego Card with this Id already Exist, being overwritten by Mod Id {packageId}.");
                    ModParameters.EmotionCards.Remove(diceCardXmlInfo.id.id);
                }

                ModParameters.EmotionEgoCards.Add(diceCardXmlInfo.id.id,
                    new EmotionEgoOptions(emotionEgoXmlInfo, packageId: packageId));
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

        public static void GetOringinAbnoAndEgo()
        {
            var sephirahTypeList = new List<SephirahType>
            {
                SephirahType.Keter, SephirahType.Hokma, SephirahType.Binah,
                SephirahType.Chesed, SephirahType.Gebura, SephirahType.Tiphereth,
                SephirahType.Netzach, SephirahType.Hod, SephirahType.Yesod, SephirahType.Malkuth
            };
            foreach (var sephirah in sephirahTypeList)
            {
                var listEmotionXmlCards = (List<EmotionCardXmlInfo>)typeof(EmotionCardXmlList)
                    .GetField("_list", AccessTools.all)?.GetValue(Singleton<EmotionCardXmlList>.Instance);
                if (listEmotionXmlCards != null)
                {
                    var listEmotionCards = (from emotionCardXmlInfo in listEmotionXmlCards
                        where emotionCardXmlInfo.id >= 1 && emotionCardXmlInfo.id <= 15 &&
                              emotionCardXmlInfo.Sephirah == sephirah
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
                    ModParameters.OriginalEmotionCards.Add(sephirah, listEmotionCards);
                }

                var listEmotionEgoXmlCards = (List<EmotionEgoXmlInfo>)typeof(EmotionEgoXmlList)
                    .GetField("_list", AccessTools.all)?.GetValue(Singleton<EmotionEgoXmlList>.Instance);
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
                ModParameters.OriginalEgoFloorCards.Add(sephirah, listFloorEgoCards);
            }
        }

        public static void ChangeAbnoAndEgo(SephirahType sephirah, CustomFloorOptions floorOptions)
        {
            var customEmotionCardList = ModParameters.EmotionCards
                .Where(x => x.Value.FloorCode.Contains(floorOptions.FloorCode))
                .Select(x => x.Value.CardXml).ToList();
            if (customEmotionCardList.Any())
            {
                var listEmotionXmlCards = (List<EmotionCardXmlInfo>)typeof(EmotionCardXmlList)
                    .GetField("_list", AccessTools.all)?.GetValue(Singleton<EmotionCardXmlList>.Instance);
                if (listEmotionXmlCards != null)
                    foreach (var item in listEmotionXmlCards.Where(x => x.Sephirah == sephirah).ToList()
                                 .Select((card, i) => (i, card)))
                        if (item.i >= customEmotionCardList.Count)
                        {
                            if (!floorOptions.LockOriginalEmotionSlots) continue;
                            item.card.Name = $"RevertCardBigDLL4221{sephirah}";
                            item.card.Sephirah = SephirahType.None;
                        }
                        else
                        {
                            item.card.Name = customEmotionCardList[item.i].Name;
                            item.card._artwork = customEmotionCardList[item.i]._artwork;
                            item.card.State = customEmotionCardList[item.i].State;
                            item.card.Sephirah = sephirah;
                            item.card.EmotionLevel = customEmotionCardList[item.i].EmotionLevel;
                            item.card.TargetType = customEmotionCardList[item.i].TargetType;
                            item.card.Script = customEmotionCardList[item.i].Script;
                            item.card.Level = customEmotionCardList[item.i].Level;
                            item.card.EmotionRate = customEmotionCardList[item.i].EmotionRate;
                            item.card.Locked = customEmotionCardList[item.i].Locked;
                        }
            }

            var customEmotionEgoCardList = ModParameters.EmotionEgoCards
                .Where(x => x.Value.FloorCode.Contains(floorOptions.FloorCode)).Select(x => x.Value.CardXml).ToList();
            var listEmotionEgoXmlCards = (List<EmotionEgoXmlInfo>)typeof(EmotionEgoXmlList)
                .GetField("_list", AccessTools.all)?.GetValue(Singleton<EmotionEgoXmlList>.Instance);
            if (listEmotionEgoXmlCards == null) return;
            foreach (var item in listEmotionEgoXmlCards.Where(x => x.Sephirah == sephirah)
                         .Select((card, i) => (i, card)))
                if (item.i >= customEmotionEgoCardList.Count)
                {
                    if (!floorOptions.LockOriginalEgoSlots) continue;
                    item.card.isLock = true;
                }
                else
                {
                    item.card.id = customEmotionEgoCardList[item.i].id;
                    item.card._CardId = customEmotionEgoCardList[item.i]._CardId;
                    item.card.isLock = customEmotionEgoCardList[item.i].isLock;
                }
        }

        public static void RevertAbnoAndEgo(SephirahType sephirah)
        {
            if (ModParameters.OriginalEmotionCards.TryGetValue(sephirah, out var emotionCards))
            {
                var floorEmotionCards = emotionCards.Where(x => x.Sephirah == sephirah).ToList();
                var emotionCardList = (List<EmotionCardXmlInfo>)typeof(EmotionCardXmlList)
                    .GetField("_list", AccessTools.all)?.GetValue(Singleton<EmotionCardXmlList>.Instance);
                if (emotionCardList != null)
                    foreach (var emotionCardXmlInfo in emotionCardList.Where(x =>
                                 x.Sephirah == sephirah || x.Name.Contains($"RevertCardBigDLL4221{sephirah}")))
                    {
                        emotionCardXmlInfo.Name = floorEmotionCards[emotionCardXmlInfo.id - 1].Name;
                        emotionCardXmlInfo._artwork = floorEmotionCards[emotionCardXmlInfo.id - 1]._artwork;
                        emotionCardXmlInfo.State = floorEmotionCards[emotionCardXmlInfo.id - 1].State;
                        emotionCardXmlInfo.Sephirah = sephirah;
                        emotionCardXmlInfo.EmotionLevel = floorEmotionCards[emotionCardXmlInfo.id - 1].EmotionLevel;
                        emotionCardXmlInfo.TargetType = floorEmotionCards[emotionCardXmlInfo.id - 1].TargetType;
                        emotionCardXmlInfo.Script = floorEmotionCards[emotionCardXmlInfo.id - 1].Script;
                        emotionCardXmlInfo.Level = floorEmotionCards[emotionCardXmlInfo.id - 1].Level;
                        emotionCardXmlInfo.EmotionRate = floorEmotionCards[emotionCardXmlInfo.id - 1].EmotionRate;
                        emotionCardXmlInfo.Locked = floorEmotionCards[emotionCardXmlInfo.id - 1].Locked;
                    }
            }

            if (!ModParameters.OriginalEgoFloorCards.TryGetValue(sephirah, out var egoCards)) return;
            var floorEgoCards = egoCards.Where(x => x.Sephirah == sephirah);
            var emotionEgoCardList = (List<EmotionEgoXmlInfo>)typeof(EmotionEgoXmlList)
                .GetField("_list", AccessTools.all)?.GetValue(Singleton<EmotionEgoXmlList>.Instance);
            if (emotionEgoCardList == null) return;
            var emotionEgoFloorCardList = emotionEgoCardList.Where(x => x.Sephirah == sephirah);
            foreach (var (x, y) in emotionEgoFloorCardList.Zip(floorEgoCards, Tuple.Create))
            {
                x.id = y.id;
                x._CardId = y._CardId;
                x.isLock = y.isLock;
            }
        }

        public static void SetPullCodeCards(string pullCode, TypeCardEnum cardType, List<int> cardIds)
        {
            switch (cardType)
            {
                case TypeCardEnum.Emotion:
                    foreach (var cardId in cardIds)
                    {
                        if (!ModParameters.EmotionCards.TryGetValue(cardId, out var card)) continue;
                        card.Code.Add(pullCode);
                        ModParameters.EmotionCards[cardId] = card;
                    }

                    return;
                case TypeCardEnum.Ego:
                    foreach (var cardId in cardIds)
                    {
                        if (!ModParameters.EmotionEgoCards.TryGetValue(cardId, out var card)) continue;
                        card.Code.Add(pullCode);
                        ModParameters.EmotionEgoCards[cardId] = card;
                    }

                    return;
                default:
                    return;
            }
        }

        public static void SetFloorPullCodeCards(string pullCode, TypeCardEnum cardType, List<int> cardIds)
        {
            switch (cardType)
            {
                case TypeCardEnum.Emotion:
                    foreach (var cardId in cardIds)
                    {
                        if (!ModParameters.EmotionCards.TryGetValue(cardId, out var card)) continue;
                        card.FloorCode.Add(pullCode);
                        ModParameters.EmotionCards[cardId] = card;
                    }

                    return;
                case TypeCardEnum.Ego:
                    foreach (var cardId in cardIds)
                    {
                        if (!ModParameters.EmotionEgoCards.TryGetValue(cardId, out var card)) continue;
                        card.FloorCode.Add(pullCode);
                        ModParameters.EmotionEgoCards[cardId] = card;
                    }

                    return;
                default:
                    return;
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
    }
}