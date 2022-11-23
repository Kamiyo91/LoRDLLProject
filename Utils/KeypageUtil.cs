using System;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Models;
using HarmonyLib;
using UnityEngine;

namespace BigDLL4221.Utils
{
    public static class KeypageUtil
    {
        public static void ChangeKeypageItem(BookXmlList instance, string packageId)
        {
            try
            {
                var dictionary = (Dictionary<LorId, BookXmlInfo>)instance.GetType()
                    .GetField("_dictionary", AccessTools.all)
                    ?.GetValue(instance);
                var workshopDictionary = (Dictionary<string, List<BookXmlInfo>>)instance.GetType()
                    .GetField("_workshopBookDict", AccessTools.all)
                    ?.GetValue(instance);
                var list = (List<BookXmlInfo>)instance.GetType()
                    .GetField("_list", AccessTools.all)
                    ?.GetValue(instance);
                if (dictionary == null) return;
                if (!ModParameters.KeypageOptions.TryGetValue(packageId, out var keypageOptions)) return;
                var changedKeypageList = (from item in dictionary.Where(x => x.Key.packageId == packageId).ToList()
                    let keypageOption = keypageOptions.FirstOrDefault(x => x.KeypageId == item.Key.id)
                    where keypageOption != null
                    select SetCustomKeypageOptions(item.Key, keypageOption, ref dictionary)).ToList();
                foreach (var changedKeypage in changedKeypageList)
                {
                    var index = workshopDictionary[packageId].FindIndex(x => x.id == changedKeypage.id);
                    if (index == -1) continue;
                    workshopDictionary[packageId][index] = changedKeypage;
                    if (dictionary.ContainsKey(changedKeypage.id)) dictionary[changedKeypage.id] = changedKeypage;
                    if (list.Contains(changedKeypage)) list.Remove(changedKeypage);
                    list.Add(changedKeypage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("There was an error while changing the Keypages values " + ex.Message + " ModId : " +
                               packageId);
            }
        }

        private static BookXmlInfo SetCustomKeypageOptions(LorId id, KeypageOptions options,
            ref Dictionary<LorId, BookXmlInfo> bookDictionary)
        {
            return KeypageOptionChange(bookDictionary[id], options);
        }

        private static BookXmlInfo KeypageOptionChange(BookXmlInfo bookXml, KeypageOptions options)
        {
            return new BookXmlInfo
            {
                _id = bookXml._id,
                workshopID = bookXml.workshopID,
                InnerName = bookXml.InnerName,
                TextId = bookXml.TextId,
                _bookIcon = string.IsNullOrEmpty(options.BookIconId) ? bookXml.BookIcon : options.BookIconId,
                categoryList = options.IsDeckFixed
                    ? new List<BookCategory> { BookCategory.DeckFixed }
                    : bookXml.categoryList,
                optionList = options.IsMultiDeck ? new List<BookOption> { BookOption.MultiDeck } : bookXml.optionList,
                EquipEffect = bookXml.EquipEffect,
                Rarity = bookXml.Rarity,
                CharacterSkin = bookXml.CharacterSkin,
                skinType = bookXml.skinType,
                gender = bookXml.gender,
                Chapter = bookXml.Chapter,
                episode = bookXml.episode,
                RangeType = options.EquipRangeType ?? bookXml.RangeType,
                canNotEquip = options.CanNotEquip ?? bookXml.canNotEquip,
                RandomFace = bookXml.RandomFace,
                speedDiceNumber = bookXml.speedDiceNumber,
                SuccessionPossibleNumber = bookXml.SuccessionPossibleNumber,
                motionSoundList = bookXml.motionSoundList
            };
        }
    }
}