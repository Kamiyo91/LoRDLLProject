using System;
using System.Collections.Generic;
using System.Linq;
using Battle.DiceAttackEffect;
using BigDLL4221.Buffs;
using BigDLL4221.Enum;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using LOR_DiceSystem;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Workshop;
using Object = UnityEngine.Object;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class MainHarmoyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBookStoryChapterSlot), "SetEpisodeSlots")]
        public static void UIBookStoryChapterSlot_SetEpisodeSlots(UIBookStoryChapterSlot __instance,
            UIBookStoryPanel ___panel, List<UIBookStoryEpisodeSlot> ___EpisodeSlots)
        {
            ArtUtil.SetEpisodeSlots(__instance, ___panel, ___EpisodeSlots);
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
            UIBookStoryEpisodeSlot slot, TextMeshProUGUI ___selectedEpisodeText, Image ___selectedEpisodeIcon,
            Image ___selectedEpisodeIconGlow)
        {
            ArtUtil.OnSelectEpisodeSlot(__instance, slot, ___selectedEpisodeText, ___selectedEpisodeIcon,
                ___selectedEpisodeIconGlow);
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
        public static void BookModel_SetXmlInfo(BookModel __instance, ref List<DiceCardXmlInfo> ____onlyCards)
        {
            if (!ModParameters.PackageIds.Contains(__instance.BookId.packageId)) return;
            foreach (var cardOption in ModParameters.CardOptions)
                ____onlyCards.AddRange(cardOption.Value
                    .Where(x => x.Option == CardOption.OnlyPage && x.BookId.Contains(__instance.BookId))
                    .Select(card =>
                        ItemXmlDataList.instance.GetCardItem(new LorId(cardOption.Key, card.CardId))));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitDataModel), "IsLockUnit")]
        public static void UnitDataModel_IsLockUnit(UnitDataModel __instance, ref bool __result,
            SephirahType ____ownerSephirah)
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
                __result = !__instance.isSephirah && ____ownerSephirah != SephirahType.None;
                return;
            }

            if (stageOption.PreBattleOptions.SephirahLocked)
                __result = __instance.isSephirah && (!stageOption.PreBattleOptions.UnlockedSephirah.Any() ||
                                                     stageOption.PreBattleOptions.UnlockedSephirah.Contains(
                                                         ____ownerSephirah));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StageLibraryFloorModel), "InitUnitList")]
        public static void StageLibraryFloorModel_InitUnitList(StageLibraryFloorModel __instance,
            List<UnitBattleDataModel> ____unitList, StageModel stage, LibraryFloorModel floor)
        {
            if (!ModParameters.PackageIds.Contains(stage.ClassInfo.id.packageId)) return;
            if (!ModParameters.StageOptions.TryGetValue(stage.ClassInfo.id.packageId, out var stageOptions)) return;
            var stageOption = stageOptions.FirstOrDefault(x => x.StageId == stage.ClassInfo.id.id);
            if (stageOption?.PreBattleOptions == null ||
                (!stageOption.PreBattleOptions.CustomUnits.ContainsKey(__instance.Sephirah) &&
                 !stageOption.PreBattleOptions.SephirahUnits.ContainsKey(__instance.Sephirah))) return;
            ____unitList.Clear();
            switch (stageOption.PreBattleOptions.BattleType)
            {
                case PreBattleType.CustomUnits:
                    UnitUtil.AddCustomUnits(__instance, stage, ____unitList, stageOption.PreBattleOptions,
                        stage.ClassInfo.id.packageId);
                    break;
                case PreBattleType.SephirahUnits:
                    UnitUtil.AddSephirahUnits(__instance, stage, ____unitList, stageOption.PreBattleOptions);
                    break;
                case PreBattleType.HybridUnits:
                    UnitUtil.AddSephirahUnits(__instance, stage, ____unitList, stageOption.PreBattleOptions);
                    UnitUtil.AddCustomUnits(__instance, stage, ____unitList, stageOption.PreBattleOptions,
                        stage.ClassInfo.id.packageId);
                    break;
            }

            if (!stageOption.PreBattleOptions.FillWithBaseUnits || ____unitList.Count >= 5) return;
            foreach (var unitDataModel in floor.GetUnitDataList().Where(x => !x.isSephirah))
                if (____unitList.Count < 5)
                    ____unitList.Add(UnitUtil.InitUnitDefault(stage, unitDataModel));
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
                    StaticModsInfo.EgoAndEmotionCardChanged[__instance.OwnerSephirah] = new SavedFloorOptions();
                    CardUtil.RevertAbnoAndEgo(__instance.OwnerSephirah);
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
            if (bookOptions?.BookCustomOptions == null) return;
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
                    CardUtil.ChangeAbnoAndEgo(__instance.OwnerSephirah, bookOptions.CustomFloorOptions);
                }

            if (UnitUtil.CheckSkinUnitData(__instance)) return;
            __instance.customizeData.SetCustomData(bookOptions.BookCustomOptions.CustomFaceData);
            var locTryGet = ModParameters.LocalizedItems.TryGetValue(__state.BookId.packageId, out var localizedItem);
            __instance.SetTempName(!locTryGet ||
                                   !localizedItem.EnemyNames.TryGetValue(bookOptions.BookCustomOptions.NameTextId,
                                       out var name)
                ? bookOptions.BookCustomOptions.Name
                : name);
            if (((bookOptions.EveryoneCanEquip && (__instance.OwnerSephirah == SephirahType.Keter ||
                                                   __instance.OwnerSephirah == SephirahType.Binah)) ||
                 (bookOptions.SephirahType == SephirahType.Keter && __instance.OwnerSephirah == SephirahType.Keter) ||
                 (bookOptions.SephirahType == SephirahType.Binah && __instance.OwnerSephirah == SephirahType.Binah)) &&
                __instance.isSephirah)
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
        [HarmonyPatch(typeof(BattleUnitEmotionDetail), "CanForcelyAggro")]
        public static void BattleUnitEmotionDetail_CanForcelyAggro(BattleUnitModel ____self, ref bool __result)
        {
            __result = (____self != null && ____self.passiveDetail.PassiveList
                           .Exists(x =>
                               ModParameters.PassiveOptions.Any(y =>
                                   y.Key == x.id.packageId &&
                                   y.Value.Any(z => z.PassiveId == x.id.id && z.ForceAggro)))) ||
                       __result;
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
            if ((string.IsNullOrEmpty(__instance.workshopSkin) && __instance.bookItem == __instance.CustomBookItem) ||
                !ModParameters.PackageIds.Contains(__instance.bookItem.ClassInfo.id.packageId)) return;
            if (!ModParameters.KeypageOptions.TryGetValue(__instance.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.bookItem.ClassInfo.id.id);
            if (keypageItem?.BookCustomOptions == null) return;
            __instance.ResetTempName();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UICustomizePopup), "OnClickSave")]
        public static void UICustomizePopup_OnClickSave(UICustomizePopup __instance)
        {
            if (!ModParameters.PackageIds.Contains(__instance.SelectedUnit.bookItem.ClassInfo.id.packageId) ||
                !ModParameters.KeypageOptions.TryGetValue(__instance.SelectedUnit.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var tempName =
                (string)__instance.SelectedUnit.GetType().GetField("_tempName", AccessTools.all)
                    ?.GetValue(__instance.SelectedUnit);
            __instance.SelectedUnit.ResetTempName();
            if (__instance.SelectedUnit.bookItem == __instance.SelectedUnit.CustomBookItem &&
                string.IsNullOrEmpty(__instance.SelectedUnit.workshopSkin))
            {
                __instance.previewData.Name = __instance.SelectedUnit.name;
                var keypageItem =
                    keypageOptions.FirstOrDefault(x => x.KeypageId == __instance.SelectedUnit.bookItem.ClassInfo.id.id);
                if (keypageItem?.BookCustomOptions == null) return;
                var locTryGet =
                    ModParameters.LocalizedItems.TryGetValue(__instance.SelectedUnit.bookItem.ClassInfo.id.packageId,
                        out var localizedItem);
                __instance.SelectedUnit.SetTempName(!locTryGet ||
                                                    !localizedItem.EnemyNames.TryGetValue(
                                                        keypageItem.BookCustomOptions.NameTextId, out var name)
                    ? keypageItem.BookCustomOptions.Name
                    : name);
            }
            else
            {
                if (string.IsNullOrEmpty(tempName) || __instance.previewData.Name == tempName)
                    __instance.previewData.Name = __instance.SelectedUnit.name;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextDataModel), "InitTextData")]
        public static void TextDataModel_InitTextData(string currentLanguage)
        {
            ModParameters.Language = currentLanguage;
            LocalizeUtil.AddGlobalLocalize();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISettingInvenEquipPageListSlot), "SetBooksData")]
        [HarmonyPatch(typeof(UIInvenEquipPageListSlot), "SetBooksData")]
        public static void General_SetBooksData(object __instance,
            List<BookModel> books, UIStoryKeyData storyKey)
        {
            var uiOrigin = __instance as UIOriginEquipPageList;
            ArtUtil.SetBooksData(uiOrigin, books, storyKey);
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
                    iconGlow = ModParameters.ArtWorks.FirstOrDefault(x => x.Key.Equals($"{artWork.Key}Glow")).Value ??
                               artWork.Value
                });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DropBookInventoryModel), "LoadFromSaveData")]
        public static void DropBookInventoryModel_LoadFromSaveData(DropBookInventoryModel __instance)
        {
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
        public static void BonusRewardWithPopup(LorId stageId)
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
        public static void BattleUnitCardsInHandUI_UpdateCardList(BattleUnitCardsInHandUI __instance,
            List<BattleDiceCardUI> ____activatedCardList, ref float ____xInterval)
        {
            if (__instance.CurrentHandState != BattleUnitCardsInHandUI.HandState.EgoCard) return;
            var unit = __instance.SelectedModel ?? __instance.HOveredModel;
            if (!unit.passiveDetail.PassiveList.Exists(y => ModParameters.PassiveOptions.Any(z =>
                    (z.Key == y.id.packageId && z.Value.Any(v => v.PassiveId == y.id.id && v.BannedEgoFloorCards)) ||
                    z.Value.Any(v =>
                        v.PassiveId == y.id.id && v.IsBaseGamePassive && v.BannedEgoFloorCards))) &&
                !ModParameters.KeypageOptions.Any(x =>
                    (x.Key == unit.Book.BookId.packageId &&
                     x.Value.Any(y => y.KeypageId == unit.Book.BookId.id && y.BannedEgoFloorCards))
                    || x.Value.Any(y =>
                        y.IsBaseGameKeypage && y.KeypageId == unit.Book.BookId.id && y.BannedEgoFloorCards))) return;
            var list = ArtUtil.ReloadEgoHandUI(__instance, __instance.GetCardUIList(), unit, ____activatedCardList,
                ref ____xInterval).ToList();
            __instance.SetSelectedCardUI(null);
            for (var i = list.Count; i < __instance.GetCardUIList().Count; i++)
                __instance.GetCardUIList()[i].gameObject.SetActive(false);
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
            var slot =
                typeof(UICharacterListPanel).GetField("CharacterList", AccessTools.all)?.GetValue(__instance) as
                    UICharacterList;
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
        public static void General_SetOperatingPanel(object __instance,
            UICustomGraphicObject ___button_Equip, TextMeshProUGUI ___txt_equipButton, BookModel ____bookDataModel)
        {
            var uiOrigin = __instance as UIOriginEquipPageSlot;
            SephiraUtil.SetOperationPanel(uiOrigin, ___button_Equip, ___txt_equipButton, ____bookDataModel);
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
        public static void UIEquipDeckCardList_SetDeckLayout(UIEquipDeckCardList __instance,
            GameObject ___multiDeckLayout)
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
                ArtUtil.PrepareMultiDeckUI(___multiDeckLayout, labels,
                    packageId);
            }
            else if (UIOptions.ChangedMultiView)
            {
                UIOptions.ChangedMultiView = false;
                ArtUtil.RevertMultiDeckUI(___multiDeckLayout);
                __instance.GetType().GetMethod("SetDeckLayout", AccessTools.all)
                    ?.Invoke(__instance, Array.Empty<object>());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPassiveSuccessionPopup), "Close")]
        public static void UIPassiveSuccessionPopup_Close(UIPassiveSuccessionPopup __instance)
        {
            if (__instance.CurrentBookModel == null) return;
            try
            {
                SingletonBehavior<UIEquipDeckCardList>.Instance.GetType()
                    .GetMethod("SetDeckLayout", AccessTools.all)
                    ?.Invoke(SingletonBehavior<UIEquipDeckCardList>.Instance, Array.Empty<object>());
            }
            catch (Exception)
            {
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
        public static bool StageController_StartParrying_Pre(BattlePlayingCardDataInUnitModel cardA,
            BattlePlayingCardDataInUnitModel cardB, ref StageController.StagePhase ____phase)
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
                    ____phase = StageController.StagePhase.ExecuteOneSideAction;
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
        public static void BattleDiceCardBuf_GetBufIcon(BattleDiceCardBuf __instance, ref bool ____iconInit,
            ref Sprite ____bufIcon)
        {
            if (____iconInit) return;
            var bufIconKey = (string)__instance.GetType().GetProperty("keywordIconId", AccessTools.all)
                ?.GetValue(__instance);
            if (string.IsNullOrEmpty(bufIconKey)) return;
            if (!ModParameters.ArtWorks.TryGetValue(bufIconKey, out var bufIconCustom)) return;
            ____iconInit = true;
            ____bufIcon = bufIconCustom;
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
            var soundInfo = (CharacterSound)__result.GetType()
                .GetField("_soundInfo", AccessTools.all)?.GetValue(__result);
            var motionSounds = (List<CharacterSound.Sound>)soundInfo.GetType()
                .GetField("_motionSounds", AccessTools.all)?.GetValue(soundInfo);
            var dic = (Dictionary<MotionDetail, CharacterSound.Sound>)soundInfo.GetType()
                .GetField("_dic", AccessTools.all)?.GetValue(soundInfo);
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
            if (typeof(BattleUnitView).GetField("_skinInfo", AccessTools.all)?.GetValue(__instance) is
                BattleUnitView.SkinInfo skinInfo)
            {
                skinInfo.state = BattleUnitView.SkinState.Changed;
                skinInfo.skinName = charName;
            }

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
            var soundInfo = (CharacterSound)__instance.charAppearance.GetType()
                .GetField("_soundInfo", AccessTools.all)?.GetValue(__instance.charAppearance);
            var motionSounds = (List<CharacterSound.Sound>)soundInfo.GetType()
                .GetField("_motionSounds", AccessTools.all)?.GetValue(soundInfo);
            var dic = (Dictionary<MotionDetail, CharacterSound.Sound>)soundInfo.GetType()
                .GetField("_dic", AccessTools.all)?.GetValue(soundInfo);
            UnitUtil.PrepareSounds(motionSounds, dic, skin.MotionSounds);
            __instance.charAppearance.ChangeMotion(currentMotionDetail);
            __instance.charAppearance.ChangeLayer("Character");
            __instance.charAppearance.SetLibrarianOnlySprites(__instance.model.faction);
            if (skin.CustomHeight == 0) return;
            __instance.ChangeHeight(skin.CustomHeight);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FarAreaEffect_Xiao_Taotie), "LateInit")]
        public static void FarAreaEffect_Xiao_Taotie_LateInit(BattleUnitModel ____self)
        {
            if (!ModParameters.KeypageOptions.TryGetValue(____self.UnitData.unitData.bookItem.ClassInfo.id.packageId,
                    out var keypageOptions)) return;
            var keypageItem =
                keypageOptions.FirstOrDefault(x => x.KeypageId == ____self.UnitData.unitData.bookItem.ClassInfo.id.id);
            if (keypageItem?.BookCustomOptions == null ||
                keypageItem.BookCustomOptions.XiaoTaotieAction == ActionDetail.NONE) return;
            ____self.view.charAppearance.ChangeMotion(keypageItem.BookCustomOptions.XiaoTaotieAction);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
        [HarmonyPatch(typeof(UIEmotionPassiveCardInven), "SetSprites")]
        public static void EmotionPassiveCardUI_SetSprites(EmotionCardXmlInfo ____card, ref Image ____artwork)
        {
            var artworkId = ModParameters.EmotionCards.Where(x => x.Value.CardXml.Artwork.Equals(____card.Artwork))
                .Select(x => x.Value.CardXml.Artwork).FirstOrDefault();
            if (string.IsNullOrEmpty(artworkId)) return;
            if (!ModParameters.CardArtWorks.TryGetValue(____card.Artwork, out var sprite)) return;
            ____artwork.sprite = sprite;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StageLibraryFloorModel), "StartPickEmotionCard")]
        private static bool StageLibraryFloorModel_StartPickEmotionCard(StageLibraryFloorModel __instance)
        {
            if (!string.IsNullOrEmpty(StaticModsInfo.EmotionCardPullCode))
            {
                var emotionList = CardUtil.CustomCreateSelectableList(__instance.team.emotionLevel);
                StaticModsInfo.EmotionCardPullCode = string.Empty;
                if (emotionList.Count <= 0) return true;
                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.Init(0, emotionList);
                return false;
            }

            if (string.IsNullOrEmpty(StaticModsInfo.EgoCardPullCode)) return true;
            var egoList = CardUtil.CustomCreateSelectableEgoList();
            StaticModsInfo.EgoCardPullCode = string.Empty;
            if (egoList.Count <= 0) return true;
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.InitEgo(0, egoList);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAbnormalityCardPreviewSlot), "Init")]
        public static void UIAbnormalityCardPreviewSlot_Init(EmotionCardXmlInfo card, ref Image ___artwork)
        {
            var artworkId = ModParameters.EmotionCards.Where(x => x.Value.CardXml.Artwork.Equals(card.Artwork))
                .Select(x => x.Value.CardXml.Artwork).FirstOrDefault();
            if (string.IsNullOrEmpty(artworkId)) return;
            if (!ModParameters.CardArtWorks.TryGetValue(card.Artwork, out var sprite)) return;
            ___artwork.sprite = sprite;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIFloorTitlePanel), "SetData")]
        public static void UIFloorTitlePanel_SetData(SephirahType sep, ref Image ___img_floorTitle,
            TextMeshProUGUI ___txt_titlename)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(sep, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            ___img_floorTitle.sprite = icon;
            var name = ModParameters.LocalizedItems.TryGetValue(savedOptions.FloorOptions.PackageId,
                out var localizedItem)
                ? localizedItem.EffectTexts.TryGetValue(savedOptions.FloorOptions.FloorNameId,
                    out var floorLocalization) ? floorLocalization.Name
                : !string.IsNullOrEmpty(savedOptions.FloorOptions.FloorName) ? savedOptions.FloorOptions.FloorName
                : ""
                : !string.IsNullOrEmpty(savedOptions.FloorOptions.FloorName)
                    ? savedOptions.FloorOptions.FloorName
                    : "";
            if (!string.IsNullOrEmpty(name)) ___txt_titlename.text = name;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISephirahButton), "SetButtonState")]
        public static void UISephirahButton_SetButtonState(UISephirahButton.ButtonState value,
            ref Image ___img_Icon, SephirahType ___sephirahType)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(___sephirahType, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            ___img_Icon.sprite = icon;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UISephirahSelectionButton), "InitAndActivate")]
        public static void UISephirahSelectionButton_InitAndActivate(ref Image ___sephirahImage,
            SephirahType _sephirahType)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(_sephirahType, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            ___sephirahImage.sprite = icon;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleEmotionBarTeamSlotUI), "InitPlayerTeamIcon")]
        public static void BattleEmotionBarTeamSlotUI_InitPlayerTeamIcon(ref Image ___img_Icon,
            EmotionBattleTeamModel team)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(team.sep, out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (string.IsNullOrEmpty(savedOptions.FloorOptions.IconId) ||
                !ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            ___img_Icon.sprite = icon;
        }

        [HarmonyPatch(typeof(EmotionEgoXmlInfo), "CardId", MethodType.Getter)]
        [HarmonyPostfix]
        private static void EmotionEgoXmlInfo_get_CardId(EmotionEgoXmlInfo __instance, ref LorId __result,
            int ____CardId)
        {
            if (ModParameters.EmotionEgoCards.TryGetValue(__instance.id, out var egoCardOptions))
                __result = new LorId(egoCardOptions.PackageId, __instance.id);
        }

        [HarmonyPatch(typeof(EmotionEgoCardUI), "Init")]
        [HarmonyPostfix]
        private static void EmotionEgoCardUI_Init(EmotionEgoCardUI __instance, EmotionEgoXmlInfo card,
            ref EmotionEgoXmlInfo ____card, TextMeshProUGUI ____cardName)
        {
            if (!ModParameters.EmotionEgoCards.TryGetValue(card.id, out var egoCardOptions)) return;
            __instance.gameObject.SetActive(false);
            var cardItem = ItemXmlDataList.instance.GetCardItem(new LorId(egoCardOptions.PackageId, card.id));
            ____cardName.text = cardItem.Name;
            __instance.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(EmotionEgoXmlList), "GetData", typeof(LorId), typeof(SephirahType))]
        [HarmonyPostfix]
        private static void EmotionEgoXmlList_GetData(LorId id, ref EmotionEgoXmlInfo __result,
            List<EmotionEgoXmlInfo> ____list)
        {
            var cardListExist = ModParameters.EmotionEgoCards.Any(x => x.Value.PackageId.Equals(id.packageId));
            if (cardListExist) __result = ____list.Find(x => x.CardId.id == id.id);
        }

        [HarmonyPatch(typeof(UIEgoCardPreviewSlot), "Init")]
        [HarmonyPostfix]
        private static void UIEgoCardPreviewSlot_Init(DiceCardItemModel cardModel, TextMeshProUGUI ___cardName,
            TextMeshProUGUI ___cardCost, Image ___artwork)
        {
            if (cardModel?.ClassInfo == null) return;
            if (!ModParameters.EmotionEgoCards.Any(x =>
                    x.Value.PackageId == cardModel.GetID().packageId &&
                    x.Value.CardXml.id == cardModel.GetID().id)) return;
            ___cardName.text = cardModel.ClassInfo.Name;
            ___cardCost.text = cardModel.GetSpec().Cost.ToString();
            ___artwork.sprite =
                Singleton<CustomizingCardArtworkLoader>.Instance.GetSpecificArtworkSprite(
                    cardModel.ClassInfo.workshopID, cardModel.GetArtworkSrc());
        }

        [HarmonyPatch(typeof(BattleEmotionCardModel), MethodType.Constructor, typeof(EmotionCardXmlInfo),
            typeof(BattleUnitModel))]
        [HarmonyPostfix]
        private static void BattleEmotionCardModel_ctor_Post(BattleEmotionCardModel __instance,
            EmotionCardXmlInfo xmlInfo, ref List<EmotionCardAbilityBase> ____abilityList)
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
                    ____abilityList.RemoveAll(x =>
                        x.GetType().Name.Substring("EmotionCardAbility_".Length).Trim() == text);
                    ____abilityList.Add(ability);
                }
            }
        }

        [HarmonyPatch(typeof(UISephirahFloor), "Init")]
        [HarmonyPostfix]
        private static void UISephirahFloor_Init_Post(UISephirahFloor __instance)
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
        private static void UIGetAbnormalityPanel_SetData_Post(Image ___FloorIconImage, Image ___ego_FloorIconImage,
            Image ___NeedSelectAb_FloorIconImage)
        {
            if (!StaticModsInfo.EgoAndEmotionCardChanged.TryGetValue(Singleton<StageController>.Instance.CurrentFloor,
                    out var savedOptions)) return;
            if (!savedOptions.IsActive) return;
            if (!ModParameters.ArtWorks.TryGetValue(savedOptions.FloorOptions.IconId, out var icon)) return;
            ___FloorIconImage.sprite = icon;
            ___ego_FloorIconImage.sprite = icon;
            ___NeedSelectAb_FloorIconImage.sprite = icon;
        }
    }
}