using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battle.DiceAttackEffect;
using BigDLL4221.Buffs;
using BigDLL4221.Enum;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using LOR_DiceSystem;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Workshop;
using Object = UnityEngine.Object;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class MainHarmonyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBookStoryChapterSlot), "SetEpisodeSlots")]
        public static void UIBookStoryChapterSlot_SetEpisodeSlots(UIBookStoryChapterSlot __instance)
        {
            ArtUtil.SetEpisodeSlots(__instance, __instance.panel, __instance.EpisodeSlots);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStoryArchivesPanel), "GetEpisodeBooksData")]
        public static void UIStoryArchivesPanel_GetEpisodeBooksData(UIStoryLine ep, ref List<BookXmlInfo> __result)
        {
            var categoryOptions = ModParameters.CategoryOptions.Where(x =>
                x.Value.Any(y => y.BaseGameCategory != null && y.BaseGameCategory == ep));
            __result.AddRange(from mainItem in categoryOptions
                from bookId in mainItem.Value.Where(x => x.BaseGameCategory != null && x.BaseGameCategory == ep)
                    .SelectMany(x => x.CredenzaBooksId)
                select Singleton<BookXmlList>.Instance.GetData(new LorId(mainItem.Key, bookId)));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BookModel), "GetThumbSprite")]
        [HarmonyPatch(typeof(BookXmlInfo), "GetThumbSprite")]
        public static void General_GetThumbSprite(object __instance, ref Sprite __result)
        {
            switch (__instance)
            {
                case BookXmlInfo bookInfo:
                    ArtUtil.GetThumbSprite(bookInfo.id, ref __result);
                    break;
                case BookModel bookModel:
                    ArtUtil.GetThumbSprite(bookModel.BookId, ref __result);
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBookStoryPanel), "OnSelectEpisodeSlot")]
        public static void UIBookStoryPanel_OnSelectEpisodeSlot(UIBookStoryPanel __instance,
            UIBookStoryEpisodeSlot slot)
        {
            ArtUtil.OnSelectEpisodeSlot(__instance, slot, __instance.selectedEpisodeText,
                __instance.selectedEpisodeIcon,
                __instance.selectedEpisodeIconGlow);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBattleSettingPanel), "SetToggles")]
        public static void UIBattleSettingPanel_SetToggles(UIBattleSettingPanel __instance)
        {
            if (!ModParameters.PackageIds.Contains(Singleton<StageController>.Instance.GetStageModel().ClassInfo.id
                    .packageId)) return;
            if (!ModParameters.StageOptions.TryGetValue(Singleton<StageController>.Instance.GetStageModel()
                    .ClassInfo
                    .id.packageId, out var stageOptions)) return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == Singleton<StageController>.Instance
                .GetStageModel()
                .ClassInfo
                .id.id);
            if (stageOption?.PreBattleOptions == null || stageOption.PreBattleOptions.SetToggles) return;
            foreach (var currentAvailbleUnitslot in __instance.currentAvailbleUnitslots)
            {
                currentAvailbleUnitslot.SetToggle(false);
                currentAvailbleUnitslot.SetYesToggleState();
            }

            __instance.SetAvailibleText();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BookModel), "SetXmlInfo")]
        public static void BookModel_SetXmlInfo(BookModel __instance)
        {
            if (!ModParameters.PackageIds.Contains(__instance.BookId.packageId)) return;
            foreach (var cardOption in ModParameters.CardOptions)
                __instance._onlyCards.AddRange(cardOption.Value
                    .Where(x => x.Option == CardOption.OnlyPage && x.BookId.Contains(__instance.BookId))
                    .Select(card =>
                        ItemXmlDataList.instance.GetCardItem(new LorId(cardOption.Key, card.CardId))));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitDataModel), "IsLockUnit")]
        public static void UnitDataModel_IsLockUnit(UnitDataModel __instance, ref bool __result)
        {
            if (UI.UIController.Instance.CurrentUIPhase != UIPhase.BattleSetting) return;
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            if (stageModel == null || !ModParameters.PackageIds.Contains(stageModel.ClassInfo.id.packageId)) return;
            if (!ModParameters.StageOptions.TryGetValue(stageModel.ClassInfo.id.packageId, out var stageOptions))
                return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == stageModel.ClassInfo.id.id);
            if (stageOption?.PreBattleOptions == null) return;
            if (stageOption.PreBattleOptions.OnlySephirah)
            {
                __result = !__instance.isSephirah && __instance._ownerSephirah != SephirahType.None;
                return;
            }

            if (stageOption.PreBattleOptions.SephirahLocked)
                __result = __instance.isSephirah && (!stageOption.PreBattleOptions.UnlockedSephirah.Any() ||
                                                     stageOption.PreBattleOptions.UnlockedSephirah.Contains(
                                                         __instance._ownerSephirah));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StageLibraryFloorModel), "InitUnitList")]
        public static void StageLibraryFloorModel_InitUnitList(StageLibraryFloorModel __instance, StageModel stage,
            LibraryFloorModel floor)
        {
            if (!ModParameters.PackageIds.Contains(stage.ClassInfo.id.packageId)) return;
            if (!ModParameters.StageOptions.TryGetValue(stage.ClassInfo.id.packageId, out var stageOptions)) return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == stage.ClassInfo.id.id);
            if (stageOption?.PreBattleOptions == null ||
                (!stageOption.PreBattleOptions.CustomUnits.ContainsKey(__instance.Sephirah) &&
                 !stageOption.PreBattleOptions.SephirahUnits.ContainsKey(__instance.Sephirah))) return;
            __instance._unitList.Clear();
            switch (stageOption.PreBattleOptions.BattleType)
            {
                case PreBattleType.CustomUnits:
                    UnitUtil.AddCustomUnits(__instance, stage, __instance._unitList, stageOption.PreBattleOptions,
                        stage.ClassInfo.id.packageId);
                    break;
                case PreBattleType.SephirahUnits:
                    UnitUtil.AddSephirahUnits(__instance, stage, __instance._unitList, stageOption.PreBattleOptions);
                    break;
                case PreBattleType.HybridUnits:
                    UnitUtil.AddSephirahUnits(__instance, stage, __instance._unitList, stageOption.PreBattleOptions);
                    UnitUtil.AddCustomUnits(__instance, stage, __instance._unitList, stageOption.PreBattleOptions,
                        stage.ClassInfo.id.packageId);
                    break;
            }

            if (!stageOption.PreBattleOptions.FillWithBaseUnits || __instance._unitList.Count >= 5) return;
            foreach (var unitDataModel in floor.GetUnitDataList().Where(x => !x.isSephirah))
                if (__instance._unitList.Count < 5)
                    __instance._unitList.Add(UnitUtil.InitUnitDefault(stage, unitDataModel));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnitDataModel), "EquipBook")]
        public static void UnitDataModel_EquipBookPrefix(UnitDataModel __instance, BookModel newBook, bool force,
            ref BookModel __state)
        {
            if (force) return;
            __state = newBook;
            if (__instance.isSephirah && StaticModsInfo.EgoAndEmotionCardChanged.ContainsKey(__instance.OwnerSephirah))
                if (StaticModsInfo.EgoAndEmotionCardChanged[__instance.OwnerSephirah].IsActive)
                {
                    CardUtil.RevertAbnoAndEgo(__instance.OwnerSephirah);
                    StaticModsInfo.EgoAndEmotionCardChanged[__instance.OwnerSephirah] = new SavedFloorOptions();
                }

            if (!ModParameters.PackageIds.Contains(__instance.bookItem.ClassInfo.id.packageId)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var bookOptions = keypageOptions.FirstOrDefault(x =>
                x.KeypageId == __instance.bookItem.ClassInfo.id.id && x.BookCustomOptions != null);
            if (bookOptions == null) return;
            __instance.ResetTempName();
            __instance.customizeData.SetCustomData(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitDataModel), "EquipBook")]
        public static void UnitDataModel_EquipBookPostfix(UnitDataModel __instance, bool force, BookModel __state)
        {
            if (force) return;
            if (__state == null || !ModParameters.PackageIds.Contains(__state.ClassInfo.id.packageId)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__state.ClassInfo.id.packageId, out var keypageOptions))
                return;
            var bookOptions = keypageOptions.FirstOrDefault(x => x.KeypageId == __state.ClassInfo.id.id);
            if (bookOptions == null) return;
            if (bookOptions.BookCustomOptions != null)
            {
                if (!bookOptions.Editable) __instance.EquipCustomCoreBook(null);
                if (bookOptions.BookCustomOptions.EgoSkin.Contains(__state.GetCharacterName()) ||
                    __state.ClassInfo.CharacterSkin.Any(x => bookOptions.BookCustomOptions.EgoSkin.Contains(x)))
                    if (bookOptions.BookCustomOptions.OriginalSkinIsBaseGame)
                        __state.SetCharacterName(bookOptions.BookCustomOptions.OriginalSkin);
                    else
                        __state.ClassInfo.CharacterSkin = new List<string>
                        {
                            bookOptions.BookCustomOptions.OriginalSkin
                        };
                if (__instance.isSephirah && bookOptions.CustomFloorOptions != null)
                    if (StaticModsInfo.EgoAndEmotionCardChanged.ContainsKey(__instance.OwnerSephirah))
                    {
                        StaticModsInfo.EgoAndEmotionCardChanged[__instance.OwnerSephirah] =
                            new SavedFloorOptions(true, bookOptions.CustomFloorOptions);
                        CardUtil.SaveCardsBeforeChange(__instance.OwnerSephirah);
                        CardUtil.ChangeAbnoAndEgo(__state.BookId.packageId, __instance.OwnerSephirah,
                            bookOptions.CustomFloorOptions);
                    }

                if (!UnitUtil.CheckSkinUnitData(__instance))
                {
                    __instance.customizeData.SetCustomData(bookOptions.BookCustomOptions.CustomFaceData);
                    var locTryGet =
                        ModParameters.LocalizedItems.TryGetValue(__state.BookId.packageId, out var localizedItem);
                    __instance.SetTempName(!locTryGet ||
                                           !localizedItem.EnemyNames.TryGetValue(
                                               bookOptions.BookCustomOptions.NameTextId,
                                               out var name)
                        ? bookOptions.BookCustomOptions.Name
                        : name);
                }
            }

            if (((bookOptions.EveryoneCanEquip && (__instance.OwnerSephirah == SephirahType.Keter ||
                                                   __instance.OwnerSephirah == SephirahType.Binah)) ||
                 (bookOptions.SephirahType == SephirahType.Keter && __instance.OwnerSephirah == SephirahType.Keter) ||
                 (bookOptions.SephirahType == SephirahType.Binah && __instance.OwnerSephirah == SephirahType.Binah)) &&
                __instance.isSephirah)
                __instance.EquipBook(__state, false, true);
        }

        [HarmonyPatch(typeof(UnitDataModel), "ResetForBlackSilence")]
        [HarmonyPrefix]
        private static void UnitDataModel_ResetForBlackSilence_Pre(UnitDataModel __instance, ref BookModel __state)
        {
            __state = __instance.bookItem;
        }

        [HarmonyPatch(typeof(UnitDataModel), "ResetForBlackSilence")]
        [HarmonyPostfix]
        private static void UnitDataModel_ResetForBlackSilence_Post(UnitDataModel __instance, BookModel __state)
        {
            if (__state == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__state.ClassInfo.id.packageId, out var keypageOptions))
                return;
            var keypageOption = keypageOptions.FirstOrDefault(x => x.KeypageId == __state.ClassInfo.id.id);
            if (keypageOption == null) return;
            if (__instance.isSephirah && __instance.OwnerSephirah == SephirahType.Keter &&
                !LibraryModel.Instance.IsBlackSilenceLockedInLibrary() && (keypageOption.EveryoneCanEquip ||
                                                                           (keypageOption.SephirahType ==
                                                                            SephirahType.Keter &&
                                                                            keypageOption.OnlySephirahCanEquip)))
                __instance.EquipBook(__state, false, true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BookModel), "CanSuccessionPassive")]
        public static void BookModel_CanSuccessionPassive(BookModel __instance, PassiveModel targetpassive,
            ref GivePassiveState haspassiveState, ref bool __result)
        {
            if (ModParameters.PassiveOptions.TryGetValue(targetpassive.originData.currentpassive.id.packageId,
                    out var passiveOptions))
            {
                var passiveItem = passiveOptions.FirstOrDefault(x =>
                    x.PassiveId == targetpassive.originData.currentpassive.id.id && !x.IsBaseGamePassive);
                if (passiveItem == null) return;
                var unitPassiveList = __instance.GetPassiveModelList();
                if (__instance.GetPassiveModelList().Exists(x =>
                        passiveItem.CannotBeUsedWithPassives.Contains(x.reservedData.currentpassive.id)))
                {
                    haspassiveState = GivePassiveState.Lock;
                    __result = false;
                    return;
                }

                if (!passiveItem.CanBeUsedWithPassivesAll.All(passiveId =>
                        unitPassiveList.Exists(x => x.reservedData.currentpassive.id == passiveId)))
                {
                    haspassiveState = GivePassiveState.Lock;
                    __result = false;
                    return;
                }

                if (passiveItem.CanBeUsedWithPassivesOne.Any() && !__instance.GetPassiveModelList().Exists(x =>
                        passiveItem.CanBeUsedWithPassivesOne.Contains(x.reservedData.currentpassive.id)))
                {
                    haspassiveState = GivePassiveState.Lock;
                    __result = false;
                    return;
                }

                if (!passiveItem.IsMultiDeck ||
                    (!__instance.ClassInfo.categoryList.Contains(BookCategory.DeckFixed) &&
                     !__instance.ClassInfo.optionList.Contains(BookOption.MultiDeck) &&
                     !__instance.IsMultiDeck())) return;
                haspassiveState = GivePassiveState.Lock;
                __result = false;
            }
            else
            {
                var cannotBeUsedList = (from passiveOption in ModParameters.PassiveOptions
                    from passiveItem in passiveOption.Value.Where(x =>
                        x.CannotBeUsedWithPassives.Contains(targetpassive.originData.currentpassive.id))
                    select new LorId(passiveOption.Key, passiveItem.PassiveId)).ToList();
                if (!cannotBeUsedList.Any()) return;
                if (!__instance.GetPassiveModelList()
                        .Exists(x => cannotBeUsedList.Contains(x.reservedData.currentpassive.id))) return;
                haspassiveState = GivePassiveState.Lock;
                __result = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BookModel), "IsMultiDeck")]
        public static void BookModel_IsMultiDeck(BookModel __instance, ref bool __result)
        {
            try
            {
                __result = __instance.GetPassiveInfoList()
                               .Exists(x => ModParameters.PassiveOptions.Any(y =>
                                   y.Key == x.passive.id.packageId && y.Value.Any(z =>
                                       z.PassiveId == x.passive.id.id && z.IsMultiDeck))) ||
                           __result;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UILibrarianEquipDeckPanel), "IsMultiDeck")]
        public static void UILibrarianEquipDeckPanel_IsMultiDeck(UILibrarianEquipDeckPanel __instance,
            ref bool __result)
        {
            __result = (__instance.Unitdata != null && __instance.Unitdata.bookItem.GetPassiveInfoList()
                .Exists(x => ModParameters.PassiveOptions.Any(y =>
                    y.Key == x.passive.id.packageId &&
                    y.Value.Any(z => z.PassiveId == x.passive.id.id && z.IsMultiDeck)))) || __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUnitModel), "CanChangeAttackTarget")]
        public static void BattleUnitModel_CanChangeAttackTarget(BattleUnitModel __instance, BattleUnitModel target,
            int myIndex, ref bool __result)
        {
            if (__instance == null) return;
            var slottedCard = __instance.cardSlotDetail.cardAry[myIndex];
            var cardAbility = slottedCard?.card.CreateDiceCardSelfAbilityScript();
            if (cardAbility != null && !cardAbility.IsTargetChangable(target)) __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUnitModel), "CanChangeAttackTarget")]
        public static void BattleUnitModel_CanChangeAttackTarget(BattleUnitModel __instance, BattleUnitModel target,
            int myIndex, int targetIndex, ref bool __result)
        {
            if (target == null) return;
            if (!target.AllowTargetChanging(__instance, targetIndex) || __instance.DirectAttack() ||
                !target.IsTauntable() || (target.faction == Faction.Enemy &&
                                          Singleton<StageController>.Instance.IsBlockEnemyAggroChange()))
                return;
            var isLastDie = myIndex == __instance.speedDiceResult.Count - 1;
            if (ModParameters.KeypageOptions.TryGetValue(__instance.Book.BookId.packageId, out var keypageOptions))
            {
                var keypageOption = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.Book.BookId.id);
                if (keypageOption != null)
                {
                    if (keypageOption.ForceAggroSpeedDie.Contains(myIndex) ||
                        (keypageOption.ForceAggroLastDie && isLastDie))
                    {
                        __result = true;
                        return;
                    }

                    if (keypageOption.RedirectOnlyWithSlowerSpeed)
                    {
                        var speed = __instance.GetSpeed(myIndex);
                        var speed2 = target.GetSpeed(targetIndex);
                        __result = speed < speed2;
                    }
                }
            }

            var checkCard = false;
            BattleUnitModel targetedUnit = null;
            if (targetIndex < target.cardSlotDetail.cardAry.Count)
                if (target.cardSlotDetail.cardAry[targetIndex] != null)
                {
                    checkCard = true;
                    targetedUnit = target.cardSlotDetail.cardAry[targetIndex].target;
                }

            var passives = ModParameters.PassiveOptions.SelectMany(x => x.Value.Select(y => Tuple.Create(x.Key, y)))
                .ToList();
            foreach (var passive in __instance.passiveDetail.PassiveList.Select(unitPassive =>
                         passives.FirstOrDefault(x =>
                             x.Item1 == unitPassive.id.packageId && x.Item2.PassiveId == unitPassive.id.id &&
                             x.Item2.ForceAggroOptions != null)))
            {
                if (passive == null) continue;
                if (passive.Item2.ForceAggroOptions.ForceAggroSpeedDie.Contains(myIndex) ||
                    (passive.Item2.ForceAggroOptions.ForceAggroLastDie && isLastDie))
                {
                    __result = true;
                    return;
                }

                if (passive.Item2.ForceAggroOptions.ForceAggro)
                {
                    __result = true;
                    return;
                }

                if (passive.Item2.ForceAggroOptions.ForceAggroByTargetPassive.Any() &&
                    target.passiveDetail.PassiveList.Any(x =>
                        passive.Item2.ForceAggroOptions.ForceAggroByTargetPassive.Any(y => x.id == y)))
                {
                    __result = true;
                    return;
                }

                if (passive.Item2.ForceAggroOptions.ForceAggroByTargetBuffs.Any() && target.bufListDetail
                        .GetActivatedBufList().Any(x =>
                            passive.Item2.ForceAggroOptions.ForceAggroByTargetBuffs.Any(y =>
                                x.GetType() == y.GetType())))
                {
                    __result = true;
                    return;
                }

                if (!checkCard) continue;
                if (passive.Item2.ForceAggroOptions.ForceAggroByTargetedPassive.Any() &&
                    targetedUnit.passiveDetail.PassiveList.Any(x =>
                        passive.Item2.ForceAggroOptions.ForceAggroByTargetedPassive.Any(y => x.id == y)))
                {
                    __result = true;
                    return;
                }

                if (!passive.Item2.ForceAggroOptions.ForceAggroByTargetedBuffs.Any()) continue;
                if (!targetedUnit.bufListDetail.GetActivatedBufList().Any(x =>
                        passive.Item2.ForceAggroOptions.ForceAggroByTargetedBuffs.Any(y =>
                            x.GetType() == y.GetType()))) continue;
                __result = true;
                return;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BookModel), "ReleasePassive")]
        public static void BookModel_ReleasePassive(BookModel __instance, PassiveModel passive)
        {
            try
            {
                var currentPassive = passive.originData.currentpassive.id != new LorId(9999999)
                    ? passive.originData.currentpassive
                    : passive.reservedData.currentpassive;
                if (!ModParameters.PassiveOptions.TryGetValue(currentPassive.id.packageId, out var passiveOptions))
                    return;
                var passiveItem = passiveOptions.FirstOrDefault(x => x.PassiveId == currentPassive.id.id);
                if (passiveItem == null) return;
                var passivesToRelease = __instance.GetPassiveModelList().Where(x =>
                    passiveItem.ChainReleasePassives.Contains(x.reservedData.currentpassive.id));
                foreach (var passiveToRelease in passivesToRelease)
                    __instance.ReleasePassive(passiveToRelease);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BookModel), "UnEquipGivePassiveBook")]
        public static void BookModel_UnEquipGivePassiveBook(BookModel __instance, BookModel unequipbook)
        {
            try
            {
                var passiveOptions = ModParameters.PassiveOptions.Where(x => unequipbook.GetPassiveModelList().Exists(
                    y =>
                        x.Key == y.originData.currentpassive.id.packageId ||
                        x.Key == y.reservedData.currentpassive.id.packageId));
                var passiveToRelease = (from passiveOption in passiveOptions
                    from passiveId in passiveOption.Value.SelectMany(x => x.ChainReleasePassives)
                    from passiveModel in __instance.GetPassiveModelList().Where(x =>
                        x.originData.currentpassive.id == passiveId || x.reservedData.currentpassive.id == passiveId)
                    select passiveModel).ToList();
                if (!passiveToRelease.Any()) return;
                foreach (var passiveRelease in passiveToRelease.Distinct())
                    __instance.ReleasePassive(passiveRelease);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PassiveModel), "ReleaseSuccesionGivePassive")]
        public static void PassiveModel_ReleaseSuccesionGivePassive(PassiveModel __instance)
        {
            try
            {
                var currentPassive = __instance.originData.currentpassive.id != new LorId(9999999)
                    ? __instance.originData
                    : __instance.reservedData;
                if (!ModParameters.PassiveOptions.TryGetValue(currentPassive.currentpassive.id.packageId,
                        out var passiveOptions))
                    return;
                var passiveItem =
                    passiveOptions.FirstOrDefault(x => x.PassiveId == currentPassive.currentpassive.id.id);
                if (passiveItem == null) return;
                var book = Singleton<BookInventoryModel>.Instance.GetBookByInstanceId(currentPassive.givePassiveBookId);
                var passiveModels = book != null
                    ? book.GetPassiveModelList().Where(x =>
                        passiveItem.ChainReleasePassives.Contains(x.originData.currentpassive.id))
                    : Singleton<BookInventoryModel>.Instance.GetBlackSilenceBook().GetPassiveModelList().Where(
                        x => passiveItem.ChainReleasePassives.Contains(x.originData.currentpassive.id));
                foreach (var passiveModel in passiveModels)
                    passiveModel.ReleaseSuccesionReceivePassive(true);
            }
            catch (Exception)
            {
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitDataModel), "LoadFromSaveData")]
        public static void UnitDataModel_LoadFromSaveData(UnitDataModel __instance)
        {
            if (!string.IsNullOrEmpty(__instance.workshopSkin))
            {
                var skins = ModParameters.CustomBookSkinsOptions.SelectMany(x =>
                    x.Value.Select(y => Tuple.Create(x.Key, y)));
                var skin = skins.FirstOrDefault(x => x.Item2.SkinName == __instance.workshopSkin);
                if (skin != null && (skin.Item2.CharacterNameId.HasValue ||
                                     !string.IsNullOrEmpty(skin.Item2.CharacterName)))
                {
                    if (!ModParameters.LocalizedItems.TryGetValue(skin.Item1, out var localizatedItem)) return;
                    var name = string.Empty;
                    if (skin.Item2.CharacterNameId.HasValue)
                        __instance.SetTempName(localizatedItem != null &&
                                               !localizatedItem.EnemyNames.TryGetValue(skin.Item2.CharacterNameId ?? -1,
                                                   out name)
                            ? skin.Item2.CharacterName
                            : name);
                    return;
                }
            }

            if ((string.IsNullOrEmpty(__instance.workshopSkin) && __instance.bookItem == __instance.CustomBookItem) ||
                !ModParameters.PackageIds.Contains(__instance.bookItem.ClassInfo.id.packageId)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.bookItem.ClassInfo.id.id);
            if (keypageItem?.BookCustomOptions == null) return;
            __instance.ResetTempName();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UICustomizePopup), "Open")]
        public static void UICustomizePopup_Open(UICustomizePopup __instance)
        {
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.SelectedUnit.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var keypageItem =
                keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.SelectedUnit.bookItem.ClassInfo.id.id);
            if (keypageItem?.BookCustomOptions == null) return;
            __instance.SelectedUnit.customizeData.SetCustomData(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UICustomizePopup), "OnClickExit")]
        public static void UICustomizePopup_OnClickExit(UICustomizePopup __instance)
        {
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.SelectedUnit.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var keypageItem =
                keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.SelectedUnit.bookItem.ClassInfo.id.id);
            if (keypageItem?.BookCustomOptions == null) return;
            if (__instance.SelectedUnit.bookItem == __instance.SelectedUnit.CustomBookItem &&
                string.IsNullOrEmpty(__instance.SelectedUnit.workshopSkin))
                __instance.SelectedUnit.customizeData.SetCustomData(keypageItem.BookCustomOptions.CustomFaceData);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UICustomizePopup), "OnClickSave")]
        public static void UICustomizePopup_OnClickSave(UICustomizePopup __instance)
        {
            var tempName = __instance.SelectedUnit._tempName;
            __instance.SelectedUnit.ResetTempName();
            var keypageOptionsTry =
                ModParameters.KeypageOptions.TryGetValue(__instance.SelectedUnit.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions);
            if (__instance.SelectedUnit.bookItem == __instance.SelectedUnit.CustomBookItem &&
                string.IsNullOrEmpty(__instance.SelectedUnit.workshopSkin) && keypageOptionsTry)
            {
                __instance.previewData.Name = __instance.SelectedUnit.name;
                var keypageItem =
                    keypageOptions.FirstOrDefault(x =>
                        x.KeypageId == __instance.SelectedUnit.bookItem.ClassInfo.id.id);
                if (keypageItem?.BookCustomOptions == null) return;
                __instance.SelectedUnit.customizeData.SetCustomData(keypageItem.BookCustomOptions.CustomFaceData);
                var locTryGet =
                    ModParameters.LocalizedItems.TryGetValue(
                        __instance.SelectedUnit.bookItem.ClassInfo.id.packageId,
                        out var localizedItem);
                __instance.SelectedUnit.SetTempName(!locTryGet ||
                                                    !localizedItem.EnemyNames.TryGetValue(
                                                        keypageItem.BookCustomOptions.NameTextId, out var name)
                    ? keypageItem.BookCustomOptions.Name
                    : name);
            }
            else
            {
                __instance.SelectedUnit.customizeData.SetCustomData(true);
                var skins = ModParameters.CustomBookSkinsOptions.SelectMany(x =>
                    x.Value.Select(y => Tuple.Create(x.Key, y)));
                var skin = skins.FirstOrDefault(x => x.Item2.SkinName == __instance.SelectedUnit.workshopSkin);
                if (skin == null)
                {
                    if (string.IsNullOrEmpty(tempName))
                        __instance.SelectedUnit.SetCustomName(__instance.previewData.Name);
                    else if (__instance.previewData.Name == tempName)
                        __instance.previewData.Name = __instance.SelectedUnit.name;
                    return;
                }

                __instance.previewData.Name = __instance.SelectedUnit.name;
                if (!ModParameters.LocalizedItems.TryGetValue(skin.Item1, out var localizatedItem)) return;
                var name = string.Empty;
                if (!skin.Item2.CharacterNameId.HasValue && string.IsNullOrEmpty(skin.Item2.CharacterName)) return;
                __instance.SelectedUnit.SetTempName(
                    localizatedItem != null &&
                    !localizatedItem.EnemyNames.TryGetValue(skin.Item2.CharacterNameId ?? -1, out name)
                        ? skin.Item2.CharacterName
                        : name);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextDataModel), "InitTextData")]
        public static void TextDataModel_InitTextData(string currentLanguage)
        {
            ModParameters.Language = currentLanguage;
            LocalizeUtil.AddGlobalLocalize();
            ArtUtil.LocalizationCustomBook();
            CardUtil.InitKeywordsList(ModParameters.Assemblies);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UISettingInvenEquipPageListSlot), "SetBooksData")]
        [HarmonyPatch(typeof(UIInvenEquipPageListSlot), "SetBooksData")]
        public static void General_SetBooksData_Pre(object __instance)
        {
            switch (__instance)
            {
                case UISettingInvenEquipPageListSlot instance:
                    ArtUtil.ResetColorData(instance);
                    break;
                case UIInvenEquipPageListSlot instance:
                    ArtUtil.ResetColorData(instance);
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISettingInvenEquipPageListSlot), "SetBooksData")]
        [HarmonyPatch(typeof(UIInvenEquipPageListSlot), "SetBooksData")]
        public static void General_SetBooksData(object __instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            switch (__instance)
            {
                case UISettingInvenEquipPageListSlot instance:
                    ArtUtil.SetBooksData(instance, books, storyKey);
                    break;
                case UIInvenEquipPageListSlot instance:
                    ArtUtil.SetBooksData(instance, books, storyKey);
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UISettingEquipPageScrollList), "CalculateSlotsHeight")]
        [HarmonyPatch(typeof(UIEquipPageScrollList), "CalculateSlotsHeight")]
        public static void General_CalculateSlotsHeight(object __instance)
        {
            switch (__instance)
            {
                case UISettingEquipPageScrollList instance:
                    ArtUtil.SetMainData(instance.currentBookModelList, instance.totalkeysdata,
                        instance.currentStoryBooksDic);
                    break;
                case UIEquipPageScrollList instance:
                    ArtUtil.SetMainData(instance.currentBookModelList, instance.totalkeysdata,
                        instance.currentStoryBooksDic);
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UISpriteDataManager), "Init")]
        public static void UISpriteDataManager_Init(UISpriteDataManager __instance)
        {
            foreach (var artWork in ModParameters.ArtWorks.Where(x =>
                         !x.Key.Contains("Glow") && !__instance._storyicons.Exists(y => y.type.Equals(x.Key))))
                __instance._storyicons.Add(new UIIconManager.IconSet
                {
                    type = artWork.Key,
                    icon = artWork.Value,
                    iconGlow = ModParameters.ArtWorks.TryGetValue($"{artWork.Key}Glow", out var artWorkGlow)
                        ? artWorkGlow
                        : artWork.Value
                });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DropBookInventoryModel), "LoadFromSaveData")]
        public static void DropBookInventoryModel_LoadFromSaveData(DropBookInventoryModel __instance)
        {
            if (LucasTiphEgoModInfo.TiphEgoModFound && !LucasTiphEgoModInfo.TiphEgoPatchChanged)
            {
                LucasTiphEgoModInfo.TiphEgoPatchChanged = true;
                try
                {
                    ArtUtil.GetArtWorksTiphEgo(new DirectoryInfo(LucasTiphEgoModInfo.TiphEgoPath + "/ArtWork"));
                    var method = typeof(EmotionPassiveCardUI).GetMethod("SetSprites", AccessTools.all);
                    ModParameters.Harmony.Unpatch(method, HarmonyPatchType.Postfix, LucasTiphEgoModInfo.TiphEgoModId);
                    method = typeof(UIEmotionPassiveCardInven).GetMethod("SetSprites", AccessTools.all);
                    ModParameters.Harmony.Unpatch(method, HarmonyPatchType.Postfix, LucasTiphEgoModInfo.TiphEgoModId);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            foreach (var book in ModParameters.StartUpRewardOptions.SelectMany(x => x.Books))
            {
                var bookCount = __instance.GetBookCount(book.Key);
                if (bookCount < 99) __instance.AddBook(book.Key, 99 - bookCount);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryModel), "LoadFromSaveData")]
        public static void InventoryModel_LoadFromSaveData(InventoryModel __instance)
        {
            foreach (var cardItem in from cardItem in ModParameters.StartUpRewardOptions.SelectMany(x => x.Cards)
                     let cardCount = __instance.GetCardCount(cardItem.Key)
                     where cardCount < cardItem.Value
                     select cardItem)
                __instance.AddCard(cardItem.Key, cardItem.Value);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StageController), "BonusRewardWithPopup")]
        public static void StageController_BonusRewardWithPopup(LorId stageId)
        {
            if (!ModParameters.PackageIds.Contains(stageId.packageId)) return;
            if (!ModParameters.StageOptions.TryGetValue(stageId.packageId, out var stageOptions)) return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == stageId.id);
            if (stageOption?.StageRewardOptions == null) return;
            var message = false;
            foreach (var book in stageOption.StageRewardOptions.Books)
            {
                if (!message) message = true;
                Singleton<DropBookInventoryModel>.Instance.AddBook(book.Key,
                    book.Value);
            }

            foreach (var keypageId in stageOption.StageRewardOptions.Keypages.Where(keypageId =>
                         !Singleton<BookInventoryModel>.Instance.GetBookListAll().Exists(x =>
                             x.GetBookClassInfoId() == keypageId)))
            {
                if (!message) message = true;
                Singleton<BookInventoryModel>.Instance.CreateBook(keypageId);
            }

            foreach (var card in stageOption.StageRewardOptions.Cards.Where(x =>
                         !stageOption.StageRewardOptions.SingleTimeReward ||
                         Singleton<InventoryModel>.Instance.GetCardCount(x.Key) < 1))
            {
                if (!message) message = true;
                Singleton<InventoryModel>.Instance.AddCard(card.Key, card.Value);
            }

            if (message)
                UIAlarmPopup.instance.SetAlarmText(!string.IsNullOrEmpty(stageOption.StageRewardOptions.MessageId)
                    ? ModParameters.LocalizedItems.TryGetValue(stageId.packageId, out var localizedItem)
                        ? localizedItem.EffectTexts.TryGetValue(stageOption.StageRewardOptions.MessageId, out var text)
                            ? text.Desc
                            : "Reward Added"
                        : "Reward Added"
                    : "Reward Added");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUnitCardsInHandUI), "UpdateCardList")]
        public static void BattleUnitCardsInHandUI_UpdateCardList(BattleUnitCardsInHandUI __instance)
        {
            if (__instance.CurrentHandState != BattleUnitCardsInHandUI.HandState.EgoCard) return;
            try
            {
                var unit = __instance.SelectedModel ?? __instance.HOveredModel;
                if (!unit.passiveDetail.PassiveList.Exists(y => ModParameters.PassiveOptions.Any(z =>
                        (z.Key == y.id.packageId &&
                         z.Value.Any(v => v.PassiveId == y.id.id && v.BannedEgoFloorCards)) ||
                        z.Value.Any(v =>
                            v.PassiveId == y.id.id && v.IsBaseGamePassive && v.BannedEgoFloorCards))) &&
                    !ModParameters.KeypageOptions.Any(x =>
                        (x.Key == unit.Book.BookId.packageId &&
                         x.Value.Any(y => y.KeypageId == unit.Book.BookId.id && y.BannedEgoFloorCards))
                        || x.Value.Any(y =>
                            y.IsBaseGameKeypage && y.KeypageId == unit.Book.BookId.id && y.BannedEgoFloorCards)))
                    return;
                var list = ArtUtil.ReloadEgoHandUI(__instance, __instance.GetCardUIList(), unit,
                    __instance._activatedCardList,
                    ref __instance._xInterval).ToList();
                __instance.SetSelectedCardUI(null);
                for (var i = list.Count; i < __instance.GetCardUIList().Count; i++)
                    __instance.GetCardUIList()[i].gameObject.SetActive(false);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UICharacterListPanel), "RefreshBattleUnitDataModel")]
        public static void RefreshBattleUnitDataModel(UICharacterListPanel __instance,
            UnitDataModel data)
        {
            if (Singleton<StageController>.Instance.GetStageModel() == null ||
                !ModParameters.PackageIds.Contains(Singleton<StageController>.Instance.GetStageModel().ClassInfo.id
                    .packageId) ||
                !ModParameters.StageOptions.TryGetValue(Singleton<StageController>.Instance.GetStageModel()
                    .ClassInfo.id.packageId, out var stageOptions)) return;
            var unitOptions = stageOptions.FirstOrDefault(x => x.StageId == Singleton<StageController>.Instance
                .GetStageModel()
                .ClassInfo.id.id);
            if (unitOptions?.PreBattleOptions == null) return;
            var slot = __instance.CharacterList;
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            if (!unitOptions.PreBattleOptions.SephirahUnits.TryGetValue(
                    Singleton<StageController>.Instance.CurrentFloor, out var sephirahUnitTypes)) return;
            var list = UnitUtil.UnitsToRecover(stageModel, data, sephirahUnitTypes);
            foreach (var unit in list)
            {
                unit.Refreshhp();
                var uicharacterSlot = slot?.slotList.Find(x => x.unitBattleData == unit);
                if (uicharacterSlot == null || uicharacterSlot.unitBattleData == null) continue;
                uicharacterSlot.ReloadHpBattleSettingSlot();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIInvenEquipPageSlot), "SetOperatingPanel")]
        [HarmonyPatch(typeof(UIInvenLeftEquipPageSlot), "SetOperatingPanel")]
        [HarmonyPatch(typeof(UISettingInvenEquipPageSlot), "SetOperatingPanel")]
        [HarmonyPatch(typeof(UISettingInvenEquipPageLeftSlot), "SetOperatingPanel")]
        public static void General_SetOperatingPanel(object __instance)
        {
            switch (__instance)
            {
                case UIInvenEquipPageSlot instance:
                    SephiraUtil.SetOperationPanel(instance, instance.button_Equip, instance.txt_equipButton,
                        instance._bookDataModel);
                    break;
                case UIInvenLeftEquipPageSlot instance:
                    SephiraUtil.SetOperationPanel(instance, instance.button_Equip, instance.txt_equipButton,
                        instance._bookDataModel);
                    break;
                case UISettingInvenEquipPageSlot instance:
                    SephiraUtil.SetOperationPanel(instance, instance.button_Equip, instance.txt_equipButton,
                        instance._bookDataModel);
                    break;
                case UISettingInvenEquipPageLeftSlot instance:
                    SephiraUtil.SetOperationPanel(instance, instance.button_Equip, instance.txt_equipButton,
                        instance._bookDataModel);
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UILibrarianAppearanceInfoPanel), "OnClickCustomizeButton")]
        public static bool UILibrarianAppearanceInfoPanel_OnClickCustomizeButton(
            UILibrarianAppearanceInfoPanel __instance)
        {
            if (!ModParameters.PackageIds.Contains(__instance.unitData.bookItem.BookId.packageId) ||
                !ModParameters.KeypageOptions.TryGetValue(__instance.unitData.bookItem.BookId.packageId,
                    out var keypageOptions))
                return true;
            var keypageOption =
                keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.unitData.bookItem.BookId.id);
            if (keypageOption == null || keypageOption.Editable) return true;
            UIAlarmPopup.instance.SetAlarmText(!string.IsNullOrEmpty(keypageOption.EditErrorMessageId)
                ? ModParameters.LocalizedItems.TryGetValue(__instance.unitData.bookItem.BookId.packageId,
                    out var localizedItem)
                    ? localizedItem.EffectTexts.TryGetValue(keypageOption.EditErrorMessageId, out var text)
                        ? text.Desc
                        : "Can't edit this keypage"
                    : "Can't edit this keypage"
                : "Can't edit this keypage");
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIEquipDeckCardList), "SetDeckLayout")]
        public static void UIEquipDeckCardList_SetDeckLayout(UIEquipDeckCardList __instance)
        {
            if (ModParameters.KeypageOptions.Any(x =>
                    x.Key == __instance.currentunit.bookItem.BookId.packageId && x.Value.Any(y =>
                        y.KeypageId == __instance.currentunit.bookItem.BookId.id && y.IsMultiDeck)) || __instance
                    .currentunit.bookItem.GetPassiveInfoList().Exists(x =>
                        ModParameters.PassiveOptions.Any(y =>
                            y.Key == x.passive.id.packageId &&
                            y.Value.Any(z => z.PassiveId == x.passive.id.id && z.IsMultiDeck))))
            {
                var labels = new List<string>();
                var packageId = string.Empty;
                var keypageTryGet =
                    ModParameters.KeypageOptions.TryGetValue(__instance.currentunit.bookItem.BookId.packageId,
                        out var keypageOptions);
                if (keypageTryGet)
                {
                    var keypageOption = keypageOptions.FirstOrDefault(x =>
                        x.KeypageId == __instance.currentunit.bookItem.BookId.id && x.IsMultiDeck);
                    if (keypageOption != null)
                    {
                        labels = keypageOption.MultiDeckOptions.LabelIds;
                        packageId = __instance.currentunit.bookItem.BookId.packageId;
                    }
                }

                if (!labels.Any())
                {
                    PassiveOptions item = null;
                    foreach (var passiveOptions in ModParameters.PassiveOptions)
                    foreach (var passiveOption in passiveOptions.Value.Where(passiveOption =>
                                 __instance.currentunit.bookItem.GetPassiveInfoList().Exists(x =>
                                     x.passive.id.packageId == passiveOptions.Key &&
                                     x.passive.id.id == passiveOption.PassiveId && passiveOption.IsMultiDeck)))
                    {
                        item = passiveOption;
                        packageId = passiveOptions.Key;
                    }

                    if (item == null) return;
                    labels = item.MultiDeckOptions.LabelIds;
                }

                UIOptions.ChangedMultiView = true;
                if (__instance.currentunit.bookItem.GetCurrentDeckIndex() > 1)
                    __instance.currentunit.ReEquipDeck();
                ArtUtil.PrepareMultiDeckUI(__instance.multiDeckLayout, labels,
                    packageId);
            }
            else if (UIOptions.ChangedMultiView)
            {
                UIOptions.ChangedMultiView = false;
                ArtUtil.RevertMultiDeckUI(__instance.multiDeckLayout);
                __instance.SetDeckLayout();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPassiveSuccessionPopup), "Close")]
        public static void UIPassiveSuccessionPopup_Close(UIPassiveSuccessionPopup __instance)
        {
            if (__instance.CurrentBookModel == null) return;
            try
            {
                SingletonBehavior<UIEquipDeckCardList>.Instance.SetDeckLayout();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BookInventoryModel), "LoadFromSaveData")]
        public static void BookInventoryModel_LoadFromSaveData(BookInventoryModel __instance)
        {
            foreach (var keypageId in ModParameters.StartUpRewardOptions.SelectMany(x => x.Keypages).Where(keypageId =>
                         !Singleton<BookInventoryModel>.Instance.GetBookListAll().Exists(x =>
                             x.GetBookClassInfoId() == keypageId)))
                __instance.CreateBook(keypageId);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UISpriteDataManager), "GetStoryIcon")]
        public static void UISpriteDataManager_GetStoryIcon(ref string story)
        {
            if (story.Contains("Binah_Se21341"))
                story = "Chapter1";
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StageController), "StartParrying")]
        public static bool StageController_StartParrying_Pre(StageController __instance,
            BattlePlayingCardDataInUnitModel cardA,
            BattlePlayingCardDataInUnitModel cardB)
        {
            try
            {
                if (ModParameters.PassiveOptions.Any(
                        x => cardA.owner.passiveDetail.PassiveList.Any(y =>
                            x.Key == y.id.packageId &&
                            x.Value.Any(z => z.PassiveId == y.id.id && z.IgnoreClashPassive))) ||
                    ModParameters.CardOptions.Any(x =>
                        x.Key == cardA.card.GetID().packageId &&
                        x.Value.Any(y => y.CardId == cardA.card.GetID().id && y.OneSideOnlyCard)))
                {
                    __instance._phase = StageController.StagePhase.ExecuteOneSideAction;
                    cardA.owner.turnState = BattleUnitTurnState.DOING_ACTION;
                    cardA.target.turnState = BattleUnitTurnState.DOING_ACTION;
                    cardB.owner.currentDiceAction = null;
                    Singleton<BattleOneSidePlayManager>.Instance.StartOneSidePlay(cardA);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DiceEffectManager), "CreateBehaviourEffect")]
        public static void DiceEffectManager_CreateBehaviourEffect(ref DiceAttackEffect __result, string resource,
            float scaleFactor, BattleUnitView self, BattleUnitView target, float time)
        {
            if (string.IsNullOrEmpty(resource) || __result != null ||
                !ModParameters.CustomEffects.ContainsKey(resource)) return;
            var componentType = ModParameters.CustomEffects[resource];
            var diceAttackEffect = new GameObject(resource).AddComponent(componentType) as DiceAttackEffect;
            if (diceAttackEffect == null) return;
            diceAttackEffect.Initialize(self, target, time);
            diceAttackEffect.SetScale(scaleFactor);
            __result = diceAttackEffect;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleObjectManager), "GetTargetByCardForPlayer")]
        public static void BattleObjectManager_GetTargetByCardForPlayer(BattleUnitModel actor, BattleDiceCardModel card,
            ref BattleUnitModel __result, bool teamkill = false)
        {
            if (!ModParameters.CardOptions.TryGetValue(card.GetID().packageId, out var cardOptions)) return;
            var cardOption = cardOptions.FirstOrDefault(x => x.CardId == card.GetID().id);
            if (cardOption == null || !cardOption.OnlyAllyTargetCard) return;
            var factions = new List<Faction> { Faction.Player, Faction.Enemy };
            var units = BattleObjectManager.instance.GetAliveList(!actor.IsControlable() && teamkill
                ? RandomUtil.SelectOne(factions)
                : actor.faction);
            if (units == null) return;
            units.RemoveAll(x => x == actor);
            if (units.Any())
                __result = actor.targetSetter.SelectTargetUnit(units);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BattleDiceCardBuf), "GetBufIcon")]
        public static void BattleDiceCardBuf_GetBufIcon(BattleDiceCardBuf __instance)
        {
            if (__instance._iconInit) return;
            var bufIconKey = (string)__instance.GetType().GetProperty("keywordIconId", AccessTools.all)
                ?.GetValue(__instance);
            if (string.IsNullOrEmpty(bufIconKey)) return;
            if (!ModParameters.ArtWorks.TryGetValue(bufIconKey, out var bufIconCustom)) return;
            __instance._iconInit = true;
            __instance._bufIcon = bufIconCustom;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUnitBuf), "bufActivatedNameWithStack", MethodType.Getter)]
        public static void BattleUnitBuf_SetBuffNameWithStack(object __instance, ref string __result)
        {
            if (!(__instance is BattleUnitBuf_BaseBufChanged_DLL4221 buf)) return;
            if (string.IsNullOrEmpty(buf.BufName)) return;
            if (string.IsNullOrEmpty(__result))
            {
                __result = buf.BufName;
                return;
            }

            var resultWithoutSpace = __result.Replace(" ", "");
            if (!double.TryParse(resultWithoutSpace, out _)) return;
            __result = buf.BufName + " " + buf.stack;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SdCharacterUtil), "CreateSkin")]
        public static void SdCharacterUtil_CreateSkin(ref CharacterAppearance __result, UnitDataModel unit,
            Faction faction, Transform characterRoot)
        {
            bool skinTryGet;
            SkinOptions skin;
            string stringKey;
            if (!string.IsNullOrEmpty(unit.workshopSkin))
            {
                stringKey = unit.workshopSkin;
                skinTryGet = ModParameters.SkinOptions.TryGetValue(unit.workshopSkin, out skin);
            }
            else if (unit.bookItem != unit.CustomBookItem)
            {
                stringKey = unit.CustomBookItem.ClassInfo.GetCharacterSkin();
                skinTryGet =
                    ModParameters.SkinOptions.TryGetValue(unit.CustomBookItem.ClassInfo.GetCharacterSkin(), out skin);
            }
            else
            {
                stringKey = unit.bookItem.ClassInfo.GetCharacterSkin();
                skinTryGet =
                    ModParameters.SkinOptions.TryGetValue(unit.bookItem.ClassInfo.GetCharacterSkin(), out skin);
            }

            if (!skinTryGet) return;
            var customizeData = unit.customizeData;
            var giftInventory = unit.giftInventory;
            Object.Destroy(__result.gameObject);
            var gameObject = Object.Instantiate(
                Singleton<AssetBundleManagerRemake>.Instance.LoadCharacterPrefab(stringKey, "",
                    out var resourceName), characterRoot);
            var workshopBookSkinData =
                Singleton<CustomizingBookSkinLoader>.Instance.GetWorkshopBookSkinData(
                    skin.PackageId, stringKey);
            gameObject.GetComponent<WorkshopSkinDataSetter>().SetData(workshopBookSkinData);
            __result = gameObject.GetComponent<CharacterAppearance>();
            __result.Initialize(resourceName);
            var soundInfo = __result._soundInfo;
            var motionSounds = soundInfo._motionSounds;
            var dic = soundInfo._dic;
            UnitUtil.PrepareSounds(motionSounds, dic, skin.MotionSounds);
            __result.InitCustomData(customizeData, unit.defaultBook.GetBookClassInfoId());
            __result.InitGiftDataAll(giftInventory.GetEquippedList());
            __result.ChangeMotion(ActionDetail.Standing);
            __result.ChangeLayer("Character");
            __result.SetLibrarianOnlySprites(faction);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUnitView), "ChangeSkin")]
        public static void BattleUnitView_ChangeSkin(BattleUnitView __instance, string charName)
        {
            if (!ModParameters.SkinOptions.TryGetValue(charName, out var skin)) return;
            var skinInfo = __instance._skinInfo;
            skinInfo.state = BattleUnitView.SkinState.Changed;
            skinInfo.skinName = charName;
            var currentMotionDetail = __instance.charAppearance.GetCurrentMotionDetail();
            __instance.DestroySkin();
            var gameObject =
                Object.Instantiate(
                    Singleton<AssetBundleManagerRemake>.Instance.LoadCharacterPrefab(charName, "",
                        out var resourceName), __instance.model.view.characterRotationCenter);
            var workshopBookSkinData =
                Singleton<CustomizingBookSkinLoader>.Instance.GetWorkshopBookSkinData(
                    skin.PackageId, charName);
            gameObject.GetComponent<WorkshopSkinDataSetter>().SetData(workshopBookSkinData);
            __instance.charAppearance = gameObject.GetComponent<CharacterAppearance>();
            __instance.charAppearance.Initialize(resourceName);
            var soundInfo = __instance.charAppearance._soundInfo;
            var motionSounds = soundInfo._motionSounds;
            var dic = soundInfo._dic;
            UnitUtil.PrepareSounds(motionSounds, dic, skin.MotionSounds);
            __instance.charAppearance.ChangeMotion(currentMotionDetail);
            __instance.charAppearance.ChangeLayer("Character");
            __instance.charAppearance.SetLibrarianOnlySprites(__instance.model.faction);
            if (skin.CustomHeight == 0) return;
            __instance.ChangeHeight(skin.CustomHeight);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FarAreaEffect_Xiao_Taotie), "LateInit")]
        public static void FarAreaEffect_Xiao_Taotie_LateInit(FarAreaEffect_Xiao_Taotie __instance)
        {
            if (!ModParameters.KeypageOptions.TryGetValue(
                    __instance._self.UnitData.unitData.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var keypageItem =
                keypageOptions.FirstOrDefault(x =>
                    x.KeypageId == __instance._self.UnitData.unitData.bookItem.ClassInfo.id.id);
            if (keypageItem?.BookCustomOptions == null ||
                keypageItem.BookCustomOptions.XiaoTaotieAction == ActionDetail.NONE) return;
            __instance._self.view.charAppearance.ChangeMotion(keypageItem.BookCustomOptions.XiaoTaotieAction);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIFloorTitlePanel), "SetData")]
        public static void UIFloorTitlePanel_SetData(UIFloorTitlePanel __instance, SephirahType sep)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(sep, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            __instance.img_floorTitle.sprite = icon;
            var name = ModParameters.LocalizedItems.TryGetValue(savedOptions.FloorOptions.PackageId,
                out var localizedItem)
                ? localizedItem.EffectTexts.TryGetValue(savedOptions.FloorOptions.FloorNameId,
                    out var floorLocalization) ? floorLocalization.Name
                : !string.IsNullOrEmpty(savedOptions.FloorOptions.FloorName) ? savedOptions.FloorOptions.FloorName
                : ""
                : !string.IsNullOrEmpty(savedOptions.FloorOptions.FloorName)
                    ? savedOptions.FloorOptions.FloorName
                    : "";
            if (!string.IsNullOrEmpty(name)) __instance.txt_titlename.text = name;
            __instance.txt_titlename.rectTransform.sizeDelta = new Vector2(__instance.txt_titlename.preferredWidth,
                __instance.txt_titlename.rectTransform.sizeDelta.y);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISephirahButton), "SetButtonState")]
        public static void UISephirahButton_SetButtonState(UISephirahButton __instance,
            UISephirahButton.ButtonState value)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(__instance.sephirahType, out var savedOptions))
                return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            __instance.img_Icon.sprite = icon;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISephirahSelectionButton), "InitAndActivate")]
        public static void UISephirahSelectionButton_InitAndActivate(UISephirahSelectionButton __instance,
            SephirahType _sephirahType)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(_sephirahType, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            __instance.sephirahImage.sprite = icon;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleEmotionBarTeamSlotUI), "InitPlayerTeamIcon")]
        public static void BattleEmotionBarTeamSlotUI_InitPlayerTeamIcon(BattleEmotionBarTeamSlotUI __instance,
            EmotionBattleTeamModel team)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(team.sep, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            __instance.img_Icon.sprite = icon;
        }

        [HarmonyPatch(typeof(EmotionEgoXmlInfo), "CardId", MethodType.Getter)]
        [HarmonyPostfix]
        public static void EmotionEgoXmlInfo_get_CardId(object __instance, ref LorId __result)
        {
            if (!(__instance is EmotionEgoCardXmlExtension card)) return;
            __result = new LorId(card.PackageId, card.id);
        }

        [HarmonyPatch(typeof(UIEgoCardPreviewSlot), "Init")]
        [HarmonyPostfix]
        public static void UIEgoCardPreviewSlot_Init(UIEgoCardPreviewSlot __instance, DiceCardItemModel cardModel)
        {
            if (cardModel?.ClassInfo == null) return;
            var emotionEgoCards = ModParameters.EmotionEgoCards.SelectMany(x => x.Value);
            if (!emotionEgoCards.Any(x =>
                    x.PackageId == cardModel.GetID().packageId &&
                    x.CardXml.id == cardModel.GetID().id)) return;
            __instance.cardName.text = cardModel.ClassInfo.Name;
            __instance.cardCost.text = cardModel.GetSpec().Cost.ToString();
            __instance.artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(
                    cardModel.ClassInfo.workshopID, cardModel.GetArtworkSrc());
        }

        [HarmonyPatch(typeof(BattleEmotionCardModel), MethodType.Constructor, typeof(EmotionCardXmlInfo),
            typeof(BattleUnitModel))]
        [HarmonyPostfix]
        public static void BattleEmotionCardModel_ctor_Post(BattleEmotionCardModel __instance,
            EmotionCardXmlInfo xmlInfo)
        {
            using (var enumerator = xmlInfo.Script.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var text = enumerator.Current;
                    if (string.IsNullOrEmpty(text)) continue;
                    if (!StaticModsInfo.EmotionCardAbility.TryGetValue("EmotionCardAbility_" + text.Trim(),
                            out var abilityType)) continue;
                    var ability = (EmotionCardAbilityBase)Activator.CreateInstance(abilityType);
                    ability.SetEmotionCard(__instance);
                    __instance._abilityList.RemoveAll(x =>
                        x.GetType().Name.Substring("EmotionCardAbility_".Length).Trim() == text);
                    __instance._abilityList.Add(ability);
                }
            }
        }

        [HarmonyPatch(typeof(UISephirahFloor), "Init")]
        [HarmonyPostfix]
        public static void UISephirahFloor_Init_Post(UISephirahFloor __instance)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(__instance.sephirah, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (!ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            if (__instance.sephirah != SephirahType.Keter) __instance.imgLockIcon.sprite = icon;
            else if (StaticModsInfo.DaatFloorFound)
                __instance.transform.GetChild(0).GetChild(3).gameObject.GetComponent<Image>().sprite = icon;
            else
                __instance.transform.GetChild(1).GetChild(3).gameObject.GetComponent<Image>().sprite = icon;
        }

        [HarmonyPatch(typeof(LevelUpUI), "InitBase")]
        [HarmonyPostfix]
        public static void LevelUpUI_InitBase_Post(LevelUpUI __instance)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(Singleton<StageController>.Instance.CurrentFloor,
                    out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (!ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            __instance.FloorIconImage.sprite = icon;
            __instance.ego_FloorIconImage.sprite = icon;
            __instance.NeedSelectAb_FloorIconImage.sprite = icon;
        }

        [HarmonyPatch(typeof(BattleSceneRoot), "LoadFloor")]
        [HarmonyPostfix]
        public static void BattleSceneRoot_LoadFloor(BattleSceneRoot __instance, SephirahType sephirah)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(sephirah, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (savedOptions.FloorOptions.CustomFloorMap == null) return;
            foreach (var map in __instance.mapList.Where(x => x.sephirahType == sephirah))
            {
                map.gameObject.SetActive(false);
                Object.Destroy(map.gameObject);
            }

            __instance.mapList.RemoveAll(x => x.sephirahType == sephirah);
            __instance.mapList.Add(MapUtil.InitSephirahMap(savedOptions.FloorOptions.CustomFloorMap, sephirah));
        }

        [HarmonyPatch(typeof(BattleSceneRoot), "ChangeToSephirahMap")]
        [HarmonyPostfix]
        public static void BattleSceneRoot_ChangeToSephirahMap(BattleSceneRoot __instance, SephirahType sephirah,
            bool playEffect)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(sephirah, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (savedOptions.FloorOptions.CustomFloorMap == null) return;
            var x2 = __instance.mapList.FirstOrDefault(x =>
                x.name.Contains(savedOptions.FloorOptions.CustomFloorMap.Stage));
            if (x2 == null)
            {
                foreach (var map in __instance.mapList.Where(x => x.sephirahType == sephirah))
                {
                    map.gameObject.SetActive(false);
                    Object.Destroy(map.gameObject);
                }

                __instance.mapList.RemoveAll(x => x.sephirahType == sephirah);
                x2 = MapUtil.InitSephirahMap(savedOptions.FloorOptions.CustomFloorMap, sephirah);
                __instance.mapList.Add(x2);
            }

            if (x2 == __instance.currentMapObject)
                return;
            if (playEffect)
                __instance._mapChangeFilter.StartMapChangingEffect(Direction.RIGHT);
            if (__instance.currentMapObject.isCreature)
                Object.Destroy(__instance.currentMapObject.gameObject);
            else if (__instance.currentMapObject != null)
                if (__instance.currentMapObject.isEgo)
                    Object.Destroy(__instance.currentMapObject.gameObject);
                else
                    __instance.currentMapObject.EnableMap(false);
            __instance.currentMapObject = x2;
            if (!__instance.currentMapObject.IsMapInitialized)
                __instance.currentMapObject.InitializeMap();
            __instance.currentMapObject.EnableMap(true);
            __instance.currentMapObject.PlayMapChangedSound();
            SingletonBehavior<BattleCamManager>.Instance.SetVignetteColorBgCam(
                __instance.currentMapObject.sephirahColor);
            foreach (var battleUnitModel in BattleObjectManager.instance.GetList())
                battleUnitModel.view.ChangeScale(__instance.currentMapObject.mapSize);
        }

        [HarmonyPatch(typeof(StageLibraryFloorModel), "OnPickPassiveCard")]
        [HarmonyPrefix]
        public static void StageLibraryFloorModel_OnPickPassiveCard_Pre()
        {
            StaticModsInfo.OnPlayEmotionCardUsedBy = null;
        }

        [HarmonyPatch(typeof(StageLibraryFloorModel), "OnPickPassiveCard")]
        [HarmonyPostfix]
        public static void StageLibraryFloorModel_OnPickPassiveCard_Post(StageLibraryFloorModel __instance,
            EmotionCardXmlInfo card)
        {
            if (!StaticModsInfo.OnPlayCardEmotion) return;
            StaticModsInfo.OnPlayCardEmotion = false;
            __instance.team.currentSelectEmotionLevel--;
        }

        [HarmonyPatch(typeof(StageLibraryFloorModel), "OnPickEgoCard")]
        [HarmonyPostfix]
        public static void StageLibraryFloorModel_OnPickEgoCard_Post(EmotionEgoXmlInfo egoCard)
        {
            if (!StaticModsInfo.OnPlayCardEmotion) return;
            StaticModsInfo.OnPlayCardEmotion = false;
        }

        [HarmonyPatch(typeof(StageClassInfo), "currentState", MethodType.Getter)]
        [HarmonyPostfix]
        public static void StageClassInfo_OnClickTargetUnit(StageClassInfo __instance, ref StoryState __result)
        {
            if (!ModParameters.StageOptions.TryGetValue(__instance.id.packageId, out var stageOptions)) return;
            var stage = stageOptions.FirstOrDefault(x => x.StageId == __instance.id.id);
            if (stage?.StageRequirements == null) return;
            if (UnitUtil.IsLocked(stage.StageRequirements)) __result = StoryState.Close;
        }

        [HarmonyPatch(typeof(LevelUpUI), "Init")]
        [HarmonyPrefix]
        public static void LevelUpUI_Init(LevelUpUI __instance, ref int count)
        {
            if (count >= __instance._emotionLevels.Length)
                count = __instance._emotionLevels.Length - 1;
        }

        [HarmonyPatch(typeof(LevelUpUI), "InitEgo")]
        [HarmonyPrefix]
        public static void LevelUpUI_InitEgo(LevelUpUI __instance, ref int count)
        {
            if (count >= __instance._emotionLevels.Length)
                count = __instance._emotionLevels.Length - 1;
        }
    }
}