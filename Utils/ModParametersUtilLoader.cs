using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using BigDLL4221.Enum;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using Mod;
using Debug = UnityEngine.Debug;

namespace BigDLL4221.Utils
{
    public static class ModParametersUtilLoader
    {
        private static readonly List<string> IgnoreDll = new List<string>
            { "0Harmony", "Mono.Cecil", "MonoMod.RuntimeDetour", "MonoMod.Utils", "1BigDLL4221", "1SMotion-Loader" };

        public static void LoadMods()
        {
            foreach (var modContentInfo in Singleton<ModContentManager>.Instance.GetAllMods().Where(modContentInfo =>
                         modContentInfo.activated &&
                         modContentInfo.invInfo.workshopInfo.uniqueId != "BigDLLUtilLoader21341" &&
                         Directory.Exists(modContentInfo.dirInfo.FullName + "/Assemblies/BigDllFolder")))
                try
                {
                    var modId = modContentInfo.invInfo.workshopInfo.uniqueId;
                    var path = modContentInfo.dirInfo.FullName + "/Assemblies";
                    var stopwatch = new Stopwatch();
                    Debug.Log($"BigDLL4221 : Start loading mod files {modId} at path {path}");
                    stopwatch.Start();
                    var directoryInfo = new DirectoryInfo(path);
                    var assemblies = (from fileInfo in directoryInfo.GetFiles()
                        where fileInfo.Extension.ToLower() == ".dll" && !IgnoreDll.Contains(fileInfo.FullName)
                        select Assembly.LoadFile(fileInfo.FullName)).ToList();
                    ModParameters.Path.Add(modId, path);
                    LoadKeypageParameters(path, modId, assemblies);
                    LoadPassiveParameters(path, modId, assemblies);
                    LoadStageOptions(path, modId, assemblies);
                    LoadCardParameters(path, modId);
                    LoadSpriteOptions(path, modId);
                    LoadCategoryOptions(path, modId);
                    LoadCredenzaOptions(path, modId);
                    LoadCustomSkinOptions(path, modId);
                    LoadSkinOptions(path, modId);
                    LoadStartUpRewardOptions(path, modId);
                    LoadSpriteOptions(path, modId);
                    try
                    {
                        ArtUtil.GetArtWorks(new DirectoryInfo(path + "/ArtWork"));
                        ArtUtil.GetSpeedDieArtWorks(new DirectoryInfo(path + "/CustomDiceArtWork"));
                        ArtUtil.GetCardArtWorks(new DirectoryInfo(path + "/CardArtWork"));
                    }
                    catch
                    {
                        // ignored
                    }

                    CardUtil.ChangeCardItem(ItemXmlDataList.instance, modId);
                    PassiveUtil.ChangePassiveItem(modId);
                    KeypageUtil.ChangeKeypageItem(BookXmlList.Instance, modId);
                    CardUtil.LoadEmotionAndEgoCards(modId, path + "/EmotionCards");
                    LocalizeUtil.AddGlobalLocalize(modId);
                    ArtUtil.MakeCustomBook(modId);
                    ArtUtil.PreLoadBufIcons();
                    LocalizeUtil.RemoveError();
                    ModParameters.Assemblies.AddRange(assemblies);
                    CardUtil.InitKeywordsList(assemblies);
                    ArtUtil.InitCustomEffects(assemblies);
                    CardUtil.LoadEmotionAbilities(assemblies);
                    CustomMapHandler.ModResources.CacheInit.InitCustomMapFilesPath(path);
                    LoadEmotionCardsExtraParameters(path, modId);
                    LoadEgoCardsExtraParameters(path, modId);
                    stopwatch.Stop();
                    Debug.Log($"BigDLL4221 : Loading mod files {modId} at path {path} finished in {stopwatch.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"Error while loading the mod {modContentInfo.invInfo.workshopInfo.uniqueId} - {ex.Message}");
                }
        }

        private static void LoadPassiveParameters(string path, string packageId, List<Assembly> assemblies)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/PassiveOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var passiveOptionsRoot =
                        (PassiveOptionsRoot)new XmlSerializer(typeof(PassiveOptionsRoot))
                            .Deserialize(stringReader);
                    if (passiveOptionsRoot.PassiveOptions.Any())
                        ModParameters.PassiveOptions.Add(packageId,
                            passiveOptionsRoot.PassiveOptions
                                .Select(passiveOption => passiveOption.ToPassiveOptions(assemblies))
                                .ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Passive Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadCardParameters(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/CardOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var cardOptionsRoot =
                        (CardOptionsRoot)new XmlSerializer(typeof(CardOptionsRoot))
                            .Deserialize(stringReader);
                    if (cardOptionsRoot.CardOption.Any())
                        ModParameters.CardOptions.Add(packageId,
                            cardOptionsRoot.CardOption.Select(cardOption => cardOption.ToCardOptions()).ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Card Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadKeypageParameters(string path, string packageId, List<Assembly> assemblies)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/KeypageOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var keypageOptionsRoot =
                        (KeypageOptionsRoot)new XmlSerializer(typeof(KeypageOptionsRoot))
                            .Deserialize(stringReader);
                    if (keypageOptionsRoot.KeypageOption.Any())
                        ModParameters.KeypageOptions.Add(packageId,
                            keypageOptionsRoot.KeypageOption
                                .Select(keypageOption => keypageOption.ToKeypageOptions(assemblies)).ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Keypage Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadEmotionCardsExtraParameters(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/EmotionCardsCode").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var emotionCardOptionsRoot =
                        (EmotionCardOptionsRoot)new XmlSerializer(typeof(EmotionCardOptionsRoot))
                            .Deserialize(stringReader);
                    if (!emotionCardOptionsRoot.EmotionCardOption.Any()) return;
                    foreach (var option in emotionCardOptionsRoot.EmotionCardOption)
                    {
                        foreach (var code in option.FloorCode)
                            CardUtil.SetFloorPullCodeCards(packageId, code,
                                TypeCardEnum.Emotion, option.CardId);
                        foreach (var code in option.Code)
                            CardUtil.SetPullCodeCards(packageId, code,
                                TypeCardEnum.Emotion, option.CardId);
                        CardUtil.SetEmotionCardColors(packageId, option.CardId,
                            new EmotionCardColorOptions(option.ColorOptions?.FrameColor.ConvertColor(),
                                option.ColorOptions?.TextColor.ConvertColor(),
                                option.ColorOptions?.FrameHSVColor.ConvertHsvColor()));
                    }
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Emotion Pages Options packageId : " + packageId + " Error : " +
                                   ex.Message);
            }
        }

        private static void LoadEgoCardsExtraParameters(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/EgoCardsCode").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var egoCardOptionsRoot =
                        (EgoCardOptionsRoot)new XmlSerializer(typeof(EgoCardOptionsRoot))
                            .Deserialize(stringReader);
                    if (!egoCardOptionsRoot.EgoCardOption.Any()) return;
                    foreach (var option in egoCardOptionsRoot.EgoCardOption)
                    {
                        foreach (var code in option.FloorCode)
                            CardUtil.SetFloorPullCodeCards(packageId, code,
                                TypeCardEnum.Emotion, option.CardId);
                        foreach (var code in option.Code)
                            CardUtil.SetPullCodeCards(packageId, code,
                                TypeCardEnum.Emotion, option.CardId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError(
                        "Error loading Ego Pages Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadSpriteOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/SpriteOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var spriteOptionsRoot =
                        (SpriteOptionsRoot)new XmlSerializer(typeof(SpriteOptionsRoot))
                            .Deserialize(stringReader);
                    if (spriteOptionsRoot.SpriteOption.Any())
                        ModParameters.SpriteOptions.Add(packageId,
                            spriteOptionsRoot.SpriteOption.Select(spriteOption => spriteOption.ToSpriteOptions())
                                .ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Sprite Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadCustomSkinOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/CustomBookSkinOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (CustomSkinOptionsRoot)new XmlSerializer(typeof(CustomSkinOptionsRoot))
                            .Deserialize(stringReader);
                    if (root.CustomSkinOption.Any())
                        ModParameters.CustomBookSkinsOptions.Add(packageId,
                            root.CustomSkinOption.Select(option => option.ToCustomBookSkinsOption()).ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Custom Book Skin Options packageId : " + packageId + " Error : " +
                                   ex.Message);
            }
        }

        private static void LoadStageOptions(string path, string packageId, List<Assembly> assemblies)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/StageOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (StageOptionsRoot)new XmlSerializer(typeof(StageOptionsRoot))
                            .Deserialize(stringReader);
                    if (root.StageOption.Any())
                        ModParameters.StageOptions.Add(packageId,
                            root.StageOption.Select(option => option.ToStageOptions(assemblies)).ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Stage Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadCategoryOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/CategoryOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (CategoryOptionsRoot)new XmlSerializer(typeof(CategoryOptionsRoot))
                            .Deserialize(stringReader);
                    if (root.CategoryOption.Any())
                        ModParameters.CategoryOptions.Add(packageId,
                            root.CategoryOption.Select(option => option.ToCategoryOptions()).ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Category Options packageId : " + packageId + " Error : " +
                                   ex.Message);
            }
        }

        private static void LoadCredenzaOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/CredenzaOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (CredenzaOptionsRoot)new XmlSerializer(typeof(CredenzaOptionsRoot))
                            .Deserialize(stringReader);
                    if (root.CredenzaOption.Any())
                        ModParameters.CredenzaOptions.Add(packageId,
                            root.CredenzaOption.FirstOrDefault().ToCredenzaOptions());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Credenza Options packageId : " + packageId + " Error : " +
                                   ex.Message);
            }
        }

        private static void LoadSkinOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/SkinOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (SkinOptionsRoot)new XmlSerializer(typeof(SkinOptionsRoot))
                            .Deserialize(stringReader);
                    if (!root.SkinOption.Any()) return;
                    foreach (var skinRoot in root.SkinOption)
                        ModParameters.SkinOptions.Add(skinRoot.SkinName, skinRoot.ToSkinOptions());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Skin Options packageId : " + packageId + " Error : " + ex.Message);
            }
        }

        private static void LoadStartUpRewardOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/RewardOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (RewardOptionsRoot)new XmlSerializer(typeof(RewardOptionsRoot))
                            .Deserialize(stringReader);
                    if (!root.RewardOption.Any()) return;
                    ModParameters.StartUpRewardOptions.Add(root.RewardOption.FirstOrDefault().ToRewardOptions());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Start Up Reward Options packageId : " + packageId + " Error : " +
                                   ex.Message);
            }
        }
        private static void LoadDropBookOptions(string path, string packageId)
        {
            var error = false;
            FileInfo file;
            try
            {
                file = new DirectoryInfo(path + "/BigDllFolder/DropBookOptions").GetFiles().FirstOrDefault();
                error = true;
                if (file == null) return;
                using (var stringReader = new StringReader(File.ReadAllText(file.FullName)))
                {
                    var root =
                        (DropBookOptionsRoot)new XmlSerializer(typeof(DropBookOptionsRoot))
                            .Deserialize(stringReader);
                    if (root.DropBookOption.Any())
                        ModParameters.DropBookOptions.Add(packageId,
                            root.DropBookOption.Select(option => option.ToDropBookOptions()).ToList());
                }
            }
            catch (Exception ex)
            {
                if (error)
                    Debug.LogError("Error loading Category Options packageId : " + packageId + " Error : " +
                                   ex.Message);
            }
        }
    }
}