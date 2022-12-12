using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using LOR_XML;
using Mod;
using UnityEngine;

namespace BigDLL4221.Utils
{
    public static class LocalizeUtil
    {
        public static void AddGlobalLocalize(string packageId = "")
        {
            foreach (var item in ModParameters.Path.Where(x =>
                         string.IsNullOrEmpty(packageId) || x.Key.Equals(packageId)))
            {
                var localizedItem = new LocalizedItem();
                var error = false;
                FileInfo file;
                try
                {
                    var dictionary = Singleton<BattleEffectTextsXmlList>.Instance._dictionary;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/EffectTexts")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var battleEffectTextRoot =
                                (BattleEffectTextRoot)new XmlSerializer(typeof(BattleEffectTextRoot))
                                    .Deserialize(stringReader);
                            foreach (var battleEffectText in battleEffectTextRoot.effectTextList)
                            {
                                dictionary.Remove(battleEffectText.ID);
                                dictionary?.Add(battleEffectText.ID, battleEffectText);
                                localizedItem.EffectTexts.Add(battleEffectText.ID, new EffectText
                                {
                                    Name = battleEffectText.Name,
                                    Desc = battleEffectText.Desc
                                });
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Effect Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    var dictionary2 = Singleton<AbnormalityCardDescXmlList>.Instance._dictionary;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/AbnormalityCards")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader2 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            foreach (var abnormalityCard in ((AbnormalityCardsRoot)new XmlSerializer(
                                         typeof(AbnormalityCardsRoot)).Deserialize(stringReader2)).sephirahList
                                     .SelectMany(sephirah => sephirah.list))
                            {
                                dictionary2[abnormalityCard.id] = abnormalityCard;
                                localizedItem.AbnormalityCards.Add(abnormalityCard);
                            }
                        }
                }
                catch (Exception ex2)
                {
                    if (error)
                        Debug.LogError(string.Concat("Error loading Abnormality Text packageId : ", packageId,
                            " Language : ", ModParameters.Language, " Error : ", ex2.Message));
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/BattlesCards")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader2 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var battleCardDescRoot =
                                (BattleCardDescRoot)new XmlSerializer(typeof(BattleCardDescRoot)).Deserialize(
                                    stringReader2);
                            using (var enumerator =
                                   ItemXmlDataList.instance.GetAllWorkshopData()[item.Key].GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var card = enumerator.Current;
                                    card.workshopName = battleCardDescRoot.cardDescList
                                        .Find(x => x.cardID == card.id.id)
                                        .cardName;
                                    localizedItem.CardNames.Add(card.id.id, card.workshopName);
                                }
                            }

                            using (var enumerator2 = ItemXmlDataList.instance.GetCardList()
                                       .FindAll(x => x.id.packageId == item.Key).GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    var card = enumerator2.Current;
                                    card.workshopName = battleCardDescRoot.cardDescList
                                        .Find(x => x.cardID == card.id.id)
                                        .cardName;
                                    ItemXmlDataList.instance.GetCardItem(card.id).workshopName = card.workshopName;
                                }
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Battle Cards Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/BattleDialog")
                        .GetFiles().FirstOrDefault();
                    var dialogDictionary = BattleDialogXmlList.Instance._dictionary;
                    error = true;
                    if (file != null)
                        using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var battleDialogList =
                                ((BattleDialogRoot)new XmlSerializer(typeof(BattleDialogRoot))
                                    .Deserialize(stringReader)).characterList;
                            foreach (var dialog in battleDialogList)
                            {
                                dialog.workshopId = item.Key;
                                dialog.bookId = int.Parse(dialog.characterID);
                            }

                            if (!dialogDictionary.ContainsKey("Workshop")) continue;
                            dialogDictionary["Workshop"].characterList
                                .RemoveAll(x => x.workshopId.Equals(item.Key));
                            if (dialogDictionary.ContainsKey("Workshop"))
                            {
                                dialogDictionary["Workshop"].characterList.AddRange(battleDialogList);
                            }
                            else
                            {
                                var battleDialogRoot = new BattleDialogRoot
                                {
                                    groupName = "Workshop",
                                    characterList = battleDialogList
                                };
                                dialogDictionary.Add("Workshop", battleDialogRoot);
                            }

                            localizedItem.BattleDialogCharacterList = battleDialogList;
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Battle Dialogs Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/CharactersName")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader3 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var charactersNameRoot =
                                (CharactersNameRoot)new XmlSerializer(typeof(CharactersNameRoot)).Deserialize(
                                    stringReader3);
                            using (var enumerator3 =
                                   Singleton<EnemyUnitClassInfoList>.Instance.GetAllWorkshopData()[item.Key]
                                       .GetEnumerator())
                            {
                                while (enumerator3.MoveNext())
                                {
                                    var enemy = enumerator3.Current;
                                    enemy.name = charactersNameRoot.nameList.Find(x => x.ID == enemy.id.id).name;
                                    Singleton<EnemyUnitClassInfoList>.Instance.GetData(enemy.id).name = enemy.name;
                                    localizedItem.EnemyNames.Add(enemy.id.id, enemy.name);
                                }
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Characters Name Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/Books").GetFiles()
                        .FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader4 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var bookDescRoot =
                                (BookDescRoot)new XmlSerializer(typeof(BookDescRoot)).Deserialize(stringReader4);
                            using (var enumerator4 =
                                   Singleton<BookXmlList>.Instance.GetAllWorkshopData()[item.Key]
                                       .GetEnumerator())
                            {
                                while (enumerator4.MoveNext())
                                {
                                    var bookXml = enumerator4.Current;
                                    bookXml.InnerName = bookDescRoot.bookDescList.Find(x => x.bookID == bookXml.id.id)
                                        .bookName;
                                }
                            }

                            using (var enumerator5 = Singleton<BookXmlList>.Instance.GetList()
                                       .FindAll(x => x.id.packageId == item.Value).GetEnumerator())
                            {
                                while (enumerator5.MoveNext())
                                {
                                    var bookXml = enumerator5.Current;
                                    bookXml.InnerName = bookDescRoot.bookDescList.Find(x => x.bookID == bookXml.id.id)
                                        .bookName;
                                    Singleton<BookXmlList>.Instance.GetData(bookXml.id).InnerName = bookXml.InnerName;
                                }
                            }

                            BookDescXmlList.Instance._dictionaryWorkshop[item.Key] = bookDescRoot.bookDescList;
                            localizedItem.Keypages = bookDescRoot.bookDescList;
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Books Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/DropBooks")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader5 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var charactersNameRoot2 =
                                (CharactersNameRoot)new XmlSerializer(typeof(CharactersNameRoot)).Deserialize(
                                    stringReader5);
                            using (var enumerator6 =
                                   Singleton<DropBookXmlList>.Instance.GetAllWorkshopData()[item.Key]
                                       .GetEnumerator())
                            {
                                while (enumerator6.MoveNext())
                                {
                                    var dropBook = enumerator6.Current;
                                    dropBook.workshopName =
                                        charactersNameRoot2.nameList.Find(x => x.ID == dropBook.id.id).name;
                                    localizedItem.DropBookNames.Add(dropBook.id.id, dropBook.workshopName);
                                }
                            }

                            using (var enumerator7 = Singleton<DropBookXmlList>.Instance.GetList()
                                       .FindAll(x => x.id.packageId == item.Key).GetEnumerator())
                            {
                                while (enumerator7.MoveNext())
                                {
                                    var dropBook = enumerator7.Current;
                                    dropBook.workshopName =
                                        charactersNameRoot2.nameList.Find(x => x.ID == dropBook.id.id).name;
                                    Singleton<DropBookXmlList>.Instance.GetData(dropBook.id).workshopName =
                                        dropBook.workshopName;
                                }
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Drop Books packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/StageName")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader6 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var charactersNameRoot3 =
                                (CharactersNameRoot)new XmlSerializer(typeof(CharactersNameRoot)).Deserialize(
                                    stringReader6);
                            using (var enumerator8 =
                                   Singleton<StageClassInfoList>.Instance.GetAllWorkshopData()[item.Key]
                                       .GetEnumerator())
                            {
                                while (enumerator8.MoveNext())
                                {
                                    var stage = enumerator8.Current;
                                    stage.stageName = charactersNameRoot3.nameList.Find(x => x.ID == stage.id.id).name;
                                    localizedItem.StageNames.Add(stage.id.id, stage.stageName);
                                }
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Stage Names Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language + "/PassiveDesc")
                        .GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader7 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            var passiveDescRoot =
                                (PassiveDescRoot)new XmlSerializer(typeof(PassiveDescRoot)).Deserialize(stringReader7);
                            using (var enumerator9 = Singleton<PassiveXmlList>.Instance.GetDataAll()
                                       .FindAll(x => x.id.packageId == item.Key).GetEnumerator())
                            {
                                while (enumerator9.MoveNext())
                                {
                                    var passive = enumerator9.Current;
                                    passive.name = passiveDescRoot.descList.Find(x => x.ID == passive.id.id).name;
                                    passive.desc = passiveDescRoot.descList.Find(x => x.ID == passive.id.id).desc;
                                    localizedItem.PassiveTexts.Add(passive.id.id, new EffectText
                                    {
                                        Name = passive.name,
                                        Desc = passive.desc
                                    });
                                }
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Passive Desc Texts packageId : " + item.Key + " Language : " +
                                       ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    var cardAbilityDictionary = Singleton<BattleCardAbilityDescXmlList>.Instance._dictionary;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language +
                                             "/BattleCardAbilities").GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader8 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            foreach (var battleCardAbilityDesc in
                                     ((BattleCardAbilityDescRoot)new XmlSerializer(typeof(BattleCardAbilityDescRoot))
                                         .Deserialize(stringReader8)).cardDescList)
                            {
                                cardAbilityDictionary.Remove(battleCardAbilityDesc.id);
                                cardAbilityDictionary.Add(battleCardAbilityDesc.id, battleCardAbilityDesc);
                                localizedItem.BattleCardAbilitiesText.Add(battleCardAbilityDesc.id,
                                    battleCardAbilityDesc.desc);
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Battle Card Abilities Texts packageId : " + item.Key +
                                       " Language : " + ModParameters.Language + " Error : " + ex.Message);
                }

                try
                {
                    error = false;
                    var etcDictionary = TextDataModel.textDic;
                    file = new DirectoryInfo(item.Value + "/Localize/" + ModParameters.Language +
                                             "/Etc").GetFiles().FirstOrDefault();
                    error = true;
                    if (file != null)
                        using (var stringReader8 = new StringReader(File.ReadAllText(file.FullName)))
                        {
                            foreach (var etcText in
                                     ((EtcRoot)new XmlSerializer(typeof(EtcRoot))
                                         .Deserialize(stringReader8)).Text)
                            {
                                etcDictionary.Remove(etcText.ID);
                                etcDictionary.Add(etcText.ID, etcText.Desc);
                                localizedItem.Etc.Add(etcText.ID, etcText.Desc);
                            }
                        }
                }
                catch (Exception ex)
                {
                    if (error)
                        Debug.LogError("Error loading Etc Texts packageId : " + item.Key +
                                       " Language : " + ModParameters.Language + " Error : " + ex.Message);
                }

                ModParameters.LocalizedItems.Remove(item.Key);
                ModParameters.LocalizedItems.Add(item.Key, localizedItem);
            }
        }

        public static void RemoveError()
        {
            Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => new List<string>
            {
                "0Harmony",
                "Mono.Cecil",
                "MonoMod.RuntimeDetour",
                "MonoMod.Utils",
                "0KamiyoStaticHarmony",
                "KamiyoStaticBLL",
                "KamiyoStaticUtil"
            }.Exists(y => x.Contains("The same assembly name already exists. : " + y)));
            Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => new List<string>
            {
                "DLL4221"
            }.Exists(x.Contains));
        }
    }
}