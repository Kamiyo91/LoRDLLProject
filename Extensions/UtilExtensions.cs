using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BigDLL4221.Buffs;
using BigDLL4221.Models;
using BigDLL4221.Passives;
using LOR_DiceSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigDLL4221.Extensions
{
    public static class UtilExtensions
    {
        public static BattleUnitBuf_BaseBufChanged_DLL4221 AddBuff<T>(this BattleUnitModel owner, int stack)
            where T : BattleUnitBuf_BaseBufChanged_DLL4221, new()
        {
            var buff = owner.GetActiveBuff<T>();
            if (buff == null)
            {
                buff = new T();
                owner.bufListDetail.AddBuf(buff);
                buff.stack = 0;
            }

            buff.OnAddBuf(stack);
            return buff;
        }

        public static void ChangeSameCardsCost(this BattleUnitModel owner, BattlePlayingCardDataInUnitModel card,
            int value)
        {
            foreach (var battleDiceCardModel in owner.allyCardDetail.GetAllDeck()
                         .FindAll(x => x != card.card && x.GetID() == card.card.GetID()))
            {
                battleDiceCardModel.GetBufList();
                battleDiceCardModel.AddCost(value);
            }
        }

        public static void ChangeAllCardCostByCardId(this BattleUnitModel owner, LorId cardId, int value)
        {
            foreach (var battleDiceCardModel in owner.allyCardDetail.GetAllDeck()
                         .FindAll(x => x.GetID() == cardId))
            {
                battleDiceCardModel.GetBufList();
                battleDiceCardModel.AddCost(value);
            }
        }

        public static T GetActivePassive<T>(this BattleUnitModel owner) where T : PassiveAbilityBase
        {
            return (T)owner.passiveDetail.PassiveList.FirstOrDefault(x => x is T && !x.destroyed);
        }

        public static T GetActiveBuff<T>(this BattleUnitModel owner) where T : BattleUnitBuf
        {
            return (T)owner.bufListDetail.GetActivatedBufList().FirstOrDefault(x => x is T && !x.IsDestroyed());
        }

        public static bool HasPassive(this BattleUnitModel owner, Type passiveType, out PassiveAbilityBase passive)
        {
            passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.GetType() == passiveType && !x.destroyed);
            return passive != null;
        }

        public static void RemovePassive(this BattleUnitModel owner, LorId passiveId)
        {
            var passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.id == passiveId);
            owner.passiveDetail.PassiveList.Remove(passive);
        }

        public static bool IsSupportCharCheck(this BattleUnitModel owner)
        {
            return owner.passiveDetail.PassiveList.Exists(x => x is PassiveAbility_SupportChar_DLL4221);
        }

        public static void DestroyPassive(this BattleUnitModel owner, LorId passiveId)
        {
            var passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.id == passiveId && !x.destroyed);
            if (passive != null) passive.destroyed = true;
        }

        public static bool HasBuff(this BattleUnitModel owner, Type buffType, out BattleUnitBuf buf)
        {
            buf = owner.bufListDetail.GetActivatedBufList()
                .FirstOrDefault(x => x.GetType() == buffType && !x.IsDestroyed());
            return buf != null;
        }

        public static bool HasPassivePlayerMech(this BattleUnitModel owner,
            out PassiveAbility_PlayerMechBase_DLL4221 passive)
        {
            passive = null;
            if (owner.passiveDetail.PassiveList.Find(x => x is PassiveAbility_PlayerMechBase_DLL4221 && !x.destroyed) is
                PassiveAbility_PlayerMechBase_DLL4221 outPassive)
                passive = outPassive;
            return passive != null;
        }

        public static bool HasPassiveNpcMech(this BattleUnitModel owner,
            out PassiveAbility_NpcMechBase_DLL4221 passive)
        {
            passive = null;
            if (owner.passiveDetail.PassiveList.Find(x => x is PassiveAbility_NpcMechBase_DLL4221 && !x.destroyed) is
                PassiveAbility_NpcMechBase_DLL4221 outPassive)
                passive = outPassive;
            return passive != null;
        }

        public static void SetPassiveCombatLog(this BattleUnitModel owner, PassiveAbilityBase passive)
        {
            var battleCardResultLog = owner.battleCardResultLog;
            battleCardResultLog?.SetPassiveAbility(passive);
        }

        //Not Working
        //public static void SetBuffCombatLog(this BattleUnitModel owner, BattleUnitBuf buf)
        //{
        //    var battleCardResultLog = owner.battleCardResultLog;
        //    battleCardResultLog?.SetNewBufs(buf);
        //}

        public static bool GetActivatedCustomEmotionCard(this BattleUnitModel owner, string packageId, int id,
            out EmotionCardXmlExtension xmlInfo)
        {
            var cards = owner.emotionDetail.GetSelectedCardList().Where(x => x.XmlInfo is EmotionCardXmlExtension)
                .Select(x => x.XmlInfo as EmotionCardXmlExtension);
            xmlInfo = cards.FirstOrDefault(x => x != null && x.LorId == new LorId(packageId, id));
            return xmlInfo != null;
        }

        public static bool GetEmotionCard(string packageId, int id, out EmotionCardXmlExtension xmlInfo)
        {
            if (!ModParameters.EmotionCards.TryGetValue(packageId, out var cards))
            {
                xmlInfo = null;
                return false;
            }

            xmlInfo = cards.Where(x => x.CardXml.LorId == new LorId(packageId, id)).Select(x => x.CardXml)
                .FirstOrDefault();
            return xmlInfo != null;
        }

        public static bool GetEmotionCardOptions(string packageId, int id, out EmotionCardOptions cardOptions)
        {
            if (!ModParameters.EmotionCards.TryGetValue(packageId, out var cards))
            {
                cardOptions = null;
                return false;
            }

            cardOptions = cards.FirstOrDefault(x => x.CardXml.LorId == new LorId(packageId, id));
            return cardOptions != null;
        }

        public static bool GetFloorEgoCard(string packageId, int id, out EmotionEgoCardXmlExtension xmlInfo)
        {
            if (!ModParameters.EmotionEgoCards.TryGetValue(packageId, out var cards))
            {
                xmlInfo = null;
                return false;
            }

            xmlInfo = cards.Where(x => x.CardXml.CardId == new LorId(packageId, id)).Select(x => x.CardXml)
                .FirstOrDefault();
            return xmlInfo != null;
        }

        public static void SetEmotionCombatLog(this BattleUnitModel owner, BattleEmotionCardModel emotionCard)
        {
            owner.battleCardResultLog.SetEmotionAbility(true, emotionCard, emotionCard.XmlInfo.id);
        }

        public static void SetDieAbility(this BattleUnitModel owner, DiceCardAbilityBase ability)
        {
            var battleCardResultLog = owner.battleCardResultLog;
            battleCardResultLog?.SetDiceBehaviourAbility(true, ability.behavior, ability.card.card);
        }

        public static bool ForceEgoPlayer(this BattleUnitModel owner, int egoPhase)
        {
            if (!(owner.passiveDetail.PassiveList.Find(x => x is PassiveAbility_PlayerMechBase_DLL4221 && !x.destroyed)
                    is
                    PassiveAbility_PlayerMechBase_DLL4221 passive)) return false;
            passive.ForcedEgo(egoPhase);
            return true;
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : MonoBehaviour
        {
            if (self == null) return default;
            var t = self.GetComponent<T>();
            if (t == null)
                t = self.AddComponent<T>();
            return t;
        }

        public static void SafeDestroyComponent<T>(this GameObject self) where T : MonoBehaviour
        {
            if (self == null) return;
            var component = self.GetComponent<T>();
            if (component != null)
                Object.DestroyImmediate(component);
        }

        public static ExtraOptions ToExtraOptions(this ExtraOptionRoot extraOption, List<Assembly> assemblies)
        {
            int? id = null;
            if (extraOption.Id != 0) id = extraOption.Id;
            return new ExtraOptions(id, TrasformBuffNameInType(extraOption.Buff, assemblies),
                extraOption.OptionType, extraOption.ExtraBools.ToDictionaryExtraValue(),
                extraOption.ExtraInts.ToDictionaryExtraValue(), extraOption.ExtraLorIds.ToDictionaryExtraValue(),
                extraOption.ExtraUnitModelOptions.ToDictionaryExtraValue(assemblies),
                extraOption.ExtraColorOptions.ToDictionaryExtraValue(),
                extraOption.ExtraBuffOptions.ToDictionaryExtraValue(assemblies),
                extraOption.ExtraString.ToDictionaryExtraValue());
        }

        public static void AddDefaultKeyword(this DefaultKeywordRoot defaultKeyword)
        {
            ModParameters.DefaultKeyword.Add(defaultKeyword.DefaultKeywordOption.PackageId,
                defaultKeyword.DefaultKeywordOption.Keyword);
        }

        public static Type TrasformBuffNameInType(string name, List<Assembly> assemblies)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(x => x.Name.Equals($"BattleUnitBuf_{name}"));
        }

        public static Dictionary<string, List<Type>> ToDictionaryExtraValue(this List<ExtraBuffsRoot> extraValues,
            List<Assembly> assemblies)
        {
            return extraValues.Where(x => !string.IsNullOrEmpty(x.Condition))
                .ToDictionary(item => item.Condition,
                    item => item.Buff.Select(x => TrasformBuffNameInType(x, assemblies)).ToList());
        }

        public static Dictionary<string, BaseColorOptions> ToDictionaryExtraValue(
            this List<ExtraColorOptionsRoot> extraValues)
        {
            return extraValues.Where(x => !string.IsNullOrEmpty(x.Condition))
                .ToDictionary(value => value.Condition,
                    value => new BaseColorOptions(value.Color.FrameColor?.ConvertColor(),
                        value.Color.TextColor?.ConvertColor()));
        }

        public static Dictionary<string, bool> ToDictionaryExtraValue(this List<ExtraBool> extraValues)
        {
            return extraValues.Where(x => !string.IsNullOrEmpty(x.Condition))
                .ToDictionary(value => value.Condition, value => value.BoolValue);
        }

        public static Dictionary<string, string> ToDictionaryExtraValue(this List<ExtraString> extraValues)
        {
            return extraValues.Where(x => !string.IsNullOrEmpty(x.Condition))
                .ToDictionary(value => value.Condition, value => value.StringValue);
        }

        public static Dictionary<string, List<int>> ToDictionaryExtraValue(this List<ExtraOptionInts> extraValues)
        {
            return extraValues.Where(x => !string.IsNullOrEmpty(x.Condition))
                .ToDictionary(value => value.Condition, value => value.IntValue);
        }

        public static Dictionary<string, List<LorId>> ToDictionaryExtraValue(this List<ExtraLorIdRoot> extraValues)
        {
            return extraValues.Where(x => !string.IsNullOrEmpty(x.Condition))
                .ToDictionary(value => value.Condition, value => value.LorId.ToListLorId());
        }

        public static Dictionary<string, List<UnitModel>> ToDictionaryExtraValue(
            this List<ExtraUnitModelRoot> extraValues, List<Assembly> assemblies)
        {
            var dic = new Dictionary<string, List<UnitModel>>();
            foreach (var item in extraValues.Where(x => !string.IsNullOrEmpty(x.Condition)))
            {
                var list = (from customUnit in item.UnitModel
                    let buffList =
                        (from buffName in customUnit.AdditionalBuffs
                            select GetTypeByName($"BattleUnitBuf_{buffName}", assemblies)
                            into buff
                            where buff != null
                            select (BattleUnitBuf)Activator.CreateInstance(buff)).ToList()
                    select new UnitModel(customUnit.Id, customUnit.PackageId, customUnit.UnitNameId, customUnit.Name,
                        customUnit.LockedEmotion, customUnit.MaxEmotionLevel, customUnit.AutoPlay,
                        customUnit.SummonedOnPlay, customUnit.CustomPos, customUnit.SkinName, customUnit.IsMainEnemy,
                        customUnit.AdditionalPassiveIds.ToListLorId(), customUnit.ForcedEgoOnStart, buffList)).ToList();
                dic.Add(item.Condition, list);
            }

            return dic;
        }

        public static StageOptions ToStageOptions(this StageOptionRoot stageOption, List<Assembly> assemblies)
        {
            return new StageOptions(stageOption.StageId, stageOption.BannedEmotionLevel,
                stageOption.StageRewardOptions.ToRewardOptions(),
                stageOption.PreBattleOptions.ToPreBattleOptions(assemblies),
                new StageColorOptions(stageOption.StageColorOptions?.FrameColor.ConvertColor(),
                    stageOption.StageColorOptions?.TextColor.ConvertColor()),
                stageOption.StageRequirements.ToStageRequirements(),
                stageOption.CustomDiceColorOptions.ToCustomDiceColorOptions());
        }

        public static StageRequirements ToStageRequirements(this StageRequirementRoot stageRequirementRoot)
        {
            if (stageRequirementRoot == null) return null;
            return new StageRequirements(stageRequirementRoot.RequiredLibraryLevel,
                stageRequirementRoot.RequiredStageIds.ToListLorId());
        }

        public static PreBattleOptions ToPreBattleOptions(this PreBattleOptionRoot preBattleOption,
            List<Assembly> assemblies)
        {
            if (preBattleOption == null) return null;
            return new PreBattleOptions(preBattleOption.CustomUnits.ToCustomUnitDictionary(assemblies),
                preBattleOption.SetToggles, preBattleOption.FillWithBaseUnits, preBattleOption.BattleType,
                preBattleOption.SephirahUnits.ToSephirahUnitDictionary(), preBattleOption.OnlySephirah,
                preBattleOption.SephirahLocked, preBattleOption.UnlockedSephirah);
        }

        public static Dictionary<SephirahType, List<UnitModel>> ToCustomUnitDictionary(
            this CustomUnitsRoot customUnitsOption, List<Assembly> assemblies)
        {
            if (customUnitsOption == null) return null;
            var list = (from customUnit in customUnitsOption.CustomUnit
                let buffList =
                    (from buffName in customUnit.AdditionalBuffs
                        select GetTypeByName($"BattleUnitBuf_{buffName}", assemblies)
                        into buff
                        where buff != null
                        select (BattleUnitBuf)Activator.CreateInstance(buff)).ToList()
                select new UnitModel(customUnit.Id, customUnit.PackageId, customUnit.UnitNameId, customUnit.Name,
                    customUnit.LockedEmotion, customUnit.MaxEmotionLevel, customUnit.AutoPlay,
                    customUnit.SummonedOnPlay, customUnit.CustomPos, customUnit.SkinName, customUnit.IsMainEnemy,
                    customUnit.AdditionalPassiveIds.ToListLorId(), customUnit.ForcedEgoOnStart, buffList)).ToList();
            return new Dictionary<SephirahType, List<UnitModel>> { { customUnitsOption.Floor, list } };
        }

        public static Dictionary<SephirahType, List<SephirahType>> ToSephirahUnitDictionary(
            this SephiorahUnitsRoot sephirahUnitsRoot)
        {
            if (sephirahUnitsRoot == null) return null;
            return new Dictionary<SephirahType, List<SephirahType>>
                { { sephirahUnitsRoot.Floor, sephirahUnitsRoot.SephirahUnit } };
            ;
        }

        public static CredenzaOptions ToCredenzaOptions(this CredenzaOptionRoot credenzaOption)
        {
            return new CredenzaOptions(credenzaOption.CredenzaOption, credenzaOption.CredenzaBooksId,
                credenzaOption.CustomIconSpriteId, credenzaOption.BaseIconSpriteId, credenzaOption.CredenzaNameId,
                credenzaOption.CredenzaName, new CredenzaColorOptions(
                    credenzaOption.BookDataColor?.FrameColor.ConvertColor(),
                    credenzaOption.BookDataColor?.TextColor.ConvertColor()), credenzaOption.Chapter);
        }

        public static RewardOptions ToRewardOptions(this RewardOptionRoot rewardOption)
        {
            if (rewardOption == null) return null;
            return new RewardOptions(rewardOption.Books.ToItemDictionary(), rewardOption.Cards.ToItemDictionary(),
                rewardOption.Keypages.ToListLorId(), rewardOption.MessageId, rewardOption.SingleTimeReward);
        }

        public static Dictionary<LorId, int> ToItemDictionary(this List<ItemQuantityRoot> itemOption)
        {
            return itemOption.ToDictionary(item => new LorId(item.LorId.PackageId, item.LorId.Id),
                item => item.Quantity);
        }

        public static CategoryOptions ToCategoryOptions(this CategoryOptionRoot categoryOption)
        {
            return new CategoryOptions(categoryOption.PackageId, categoryOption.AdditionalValue,
                categoryOption.CategoryBooksId, categoryOption.CustomIconSpriteId, categoryOption.BaseIconSpriteId,
                categoryOption.CategoryNameId, categoryOption.CategoryName, categoryOption.Chapter,
                new CategoryColorOptions(categoryOption.BookDataColor?.FrameColor.ConvertColor(),
                    categoryOption.BookDataColor?.TextColor.ConvertColor()), categoryOption.BaseGameCategory,
                categoryOption.CredenzaBooksId, categoryOption.CredenzaType);
        }

        public static CustomBookSkinsOption ToCustomBookSkinsOption(this CustomSkinOptionRoot customSkinOption)
        {
            return new CustomBookSkinsOption(customSkinOption.SkinName, customSkinOption.KeypageId,
                customSkinOption.KeypageName, customSkinOption.CharacterName, customSkinOption.CharacterNameId,
                customSkinOption.UseLocalization);
        }

        public static DropBookOptions ToDropBookOptions(this DropBookOptionRoot dropBookOption)
        {
            return new DropBookOptions(dropBookOption.DropBookId,
                new DropBookColorOptions(dropBookOption.DropBookColorOptions?.FrameColor.ConvertColor(),
                    dropBookOption.DropBookColorOptions?.TextColor.ConvertColor()));
        }

        public static SkinOptions ToSkinOptions(this SkinOptionRoot skinOption)
        {
            return new SkinOptions(skinOption.PackageId, skinOption.CustomHeight,
                skinOption.MotionSounds.ToMotionSounds());
        }

        public static Dictionary<MotionDetail, MotionSound> ToMotionSounds(
            this List<MotionSoundOptionRoot> motionSounds)
        {
            return motionSounds.Where(motion => motion.MotionSound != null).ToDictionary(motion => motion.Motion,
                motion => new MotionSound(motion.MotionSound.FileNameWin, motion.MotionSound.FileNameLose,
                    motion.MotionSound.IsBaseSoundWin, motion.MotionSound.IsBaseSoundLose));
        }

        public static SpriteOptions ToSpriteOptions(this SpriteOptionRoot spriteOption)
        {
            return new SpriteOptions(spriteOption.SpriteOption, spriteOption.KeypageId, spriteOption.SpritePK);
        }

        public static CardOptions ToCardOptions(this CardOptionRoot cardOption)
        {
            return new CardOptions(cardOption.CardId, cardOption.Option, cardOption.Keywords,
                cardOption.BookId.ToListLorId(), cardOption.OnlyAllyTargetCard, cardOption.OneSideOnlyCard,
                cardOption.IsBaseGameCard, cardOption.CardColorOptions.ToCardColorOptions());
        }

        public static PassiveOptions ToPassiveOptions(this PassiveOptionRoot passiveOption, List<Assembly> assemblies)
        {
            return new PassiveOptions(passiveOption.PassiveId, passiveOption.Transferable, passiveOption.InnerTypeId,
                passiveOption.ForceAggroOptions.ToForcedAggroOptions(assemblies), passiveOption.IsMultiDeck,
                passiveOption.CannotBeUsedWithPassives.ToListLorId(),
                passiveOption.CanBeUsedWithPassivesAll.ToListLorId(),
                passiveOption.CanBeUsedWithPassivesOne.ToListLorId(),
                passiveOption.ChainReleasePassives.ToListLorId(), passiveOption.GainCoins,
                passiveOption.BannedEmotionCardSelection,
                passiveOption.BannedEgoFloorCards, passiveOption.IgnoreClashPassive,
                passiveOption.MultiDeckOptions.ToMultiDeckOptions(),
                passiveOption.IsBaseGamePassive, passiveOption.PassiveScriptId,
                new PassiveColorOptions(passiveOption.PassiveColorOptions?.TextColor.ConvertColor(),
                    passiveOption.PassiveColorOptions?.FrameColor.ConvertColor()));
        }

        public static CardColorOptions ToCardColorOptions(this CardColorOptionRoot cardColorOption)
        {
            if (cardColorOption == null) return null;
            return new CardColorOptions(cardColorOption.CardColor.ConvertColor(), cardColorOption.CustomIcon,
                cardColorOption.CustomIconColor.ConvertColor(), cardColorOption.IconColor.ConvertHsvColor(),
                cardColorOption.UseHSVFilter, cardColorOption.LeftFrame,
                cardColorOption.RightFrame, cardColorOption.FrontFrame, cardColorOption.ApplyFrontColor,
                cardColorOption.ApplySideFrontColors);
        }

        public static KeypageOptions ToKeypageOptions(this KeypageOptionRoot keypageOption, List<Assembly> assemblies)
        {
            return new KeypageOptions(keypageOption.KeypageId, keypageOption.Editable, keypageOption.EditErrorMessageId,
                keypageOption.SephirahType, keypageOption.EveryoneCanEquip, keypageOption.OnlySephirahCanEquip,
                keypageOption.BookIconId, keypageOption.IsDeckFixed, keypageOption.IsMultiDeck,
                keypageOption.CanNotEquip, keypageOption.EquipRangeType,
                keypageOption.MultiDeckOptions.ToMultiDeckOptions(),
                keypageOption.BookCustomOptions.ToBookCustomOptions(),
                keypageOption.BannedEgoFloorCards, keypageOption.IsBaseGameKeypage,
                new KeypageColorOptions(keypageOption.KeypageColorOptions?.FrameColor.ConvertColor(),
                    keypageOption.KeypageColorOptions?.TextColor.ConvertColor()), keypageOption.BannedEmotionCards,
                keypageOption.TargetableBySpecialCards,
                keypageOption.CustomFloorOptions.ToCustomFloorOptions(assemblies));
        }

        public static CustomFloorOptions ToCustomFloorOptions(this CustomFloorOptionRoot customFloorOptions,
            List<Assembly> assemblies)
        {
            if (customFloorOptions == null) return null;
            return new CustomFloorOptions(customFloorOptions.PackageId, customFloorOptions.FloorCode,
                customFloorOptions.IconId, customFloorOptions.FloorName, customFloorOptions.FloorNameId,
                customFloorOptions.CustomFloorMap.ToMapModel(assemblies),
                customFloorOptions.CustomDiceColorOptions.ToCustomDiceColorOptions());
        }

        public static MapModel ToMapModel(this MapModelRoot mapModel, List<Assembly> assemblies)
        {
            if (mapModel == null) return null;
            return new MapModel(GetTypeByName(mapModel.Component, assemblies), mapModel.Stage, mapModel.IsPlayer,
                mapModel.OneTurnEgo, mapModel.Bgx, mapModel.Bgy, mapModel.Fx, mapModel.Fy, mapModel.UnderX,
                mapModel.UnderY, mapModel.InitBgm,
                mapModel.OriginalMapStageIds.ToListLorId());
        }

        public static CustomDiceColorOptions ToCustomDiceColorOptions(
            this CustomDiceColorOptionRoot customDieColorOption)
        {
            if (customDieColorOption == null) return null;
            return new CustomDiceColorOptions(customDieColorOption.IconId,
                customDieColorOption.TextColor.ConvertColor());
        }

        public static ForceAggroOptions ToForcedAggroOptions(this ForceAggroOptionsRoot forceAggroOptions,
            List<Assembly> assemblies)
        {
            if (forceAggroOptions == null) return null;
            var targetBuffs = (from buffName in forceAggroOptions.ForceAggroByTargetBuffs
                select GetTypeByName($"BattleUnitBuf_{buffName}", assemblies)
                into buff
                where buff != null
                select (BattleUnitBuf)Activator.CreateInstance(buff)).ToList();
            var targetedBuffs = (from buffName in forceAggroOptions.ForceAggroByTargetedBuffs
                select GetTypeByName($"BattleUnitBuf_{buffName}", assemblies)
                into buff
                where buff != null
                select (BattleUnitBuf)Activator.CreateInstance(buff)).ToList();
            return new ForceAggroOptions(forceAggroOptions.ForceAggro,
                forceAggroOptions.ForceAggroByTargetedPassive.ToListLorId(),
                targetedBuffs, forceAggroOptions.ForceAggroByTargetPassive.ToListLorId(),
                targetBuffs, forceAggroOptions.ForceAggroLastDie,
                forceAggroOptions.ForceAggroSpeedDie);
        }

        public static BookCustomOptions ToBookCustomOptions(this BookCustomOptionRoot bookCustomOption)
        {
            if (bookCustomOption == null) return null;
            LorId lorId = null;
            if (bookCustomOption.CustomDialogId != null)
                lorId = new LorId(bookCustomOption.CustomDialogId.PackageId, bookCustomOption.CustomDialog.bookId);
            return new BookCustomOptions(bookCustomOption.Name, bookCustomOption.NameTextId,
                bookCustomOption.CustomFaceData, bookCustomOption.OriginalSkin, bookCustomOption.EgoSkin,
                lorId, bookCustomOption.CustomDialog, bookCustomOption.OriginalSkinIsBaseGame,
                bookCustomOption.XiaoTaotieAction);
        }

        public static MultiDeckOptions ToMultiDeckOptions(this MultiDeckOptionsRoot multiDeckOptions)
        {
            return multiDeckOptions == null || !multiDeckOptions.LabelIds.Any()
                ? null
                : new MultiDeckOptions(multiDeckOptions.LabelIds ?? new List<string>());
        }

        public static List<LorId> ToListLorId(this List<LorIdRoot> list)
        {
            return list.Select(id => new LorId(id.PackageId, id.Id)).ToList();
        }

        public static Color? ConvertColor(this ColorRoot color)
        {
            if (color == null) return null;
            return new Color(color.R / 255, color.G / 255, color.B / 255, color.A / 255);
        }

        public static HSVColor ConvertHsvColor(this HsvColorRoot color)
        {
            return color == null ? null : new HSVColor(color.H, color.S, color.V);
        }

        public static Type GetTypeByName(string className, List<Assembly> assemblies)
        {
            return assemblies.SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.Name == className);
        }
    }
}