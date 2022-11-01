using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    }
}