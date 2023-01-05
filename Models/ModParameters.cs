using System;
using System.Collections.Generic;
using System.Reflection;
using BigDLL4221.Enum;
using BigDLL4221.Extensions;
using BigDLL4221.Utils;
using LOR_DiceSystem;
using LOR_XML;
using UI;
using UnityEngine;

namespace BigDLL4221.Models
{
    public static class ModParameters
    {
        public static List<string> PackageIds = new List<string>();
        public static Dictionary<string, Sprite> ArtWorks = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> CardArtWorks = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> SpeedDieArtWorks = new Dictionary<string, Sprite>();
        public static Dictionary<string, string> Path = new Dictionary<string, string>();
        public static Dictionary<string, LocalizedItem> LocalizedItems = new Dictionary<string, LocalizedItem>();
        public static string Language = GlobalGameManager.Instance.CurrentOption.language;
        public static Dictionary<string, List<CardOptions>> CardOptions = new Dictionary<string, List<CardOptions>>();
        public static Dictionary<string, string> DefaultKeyword = new Dictionary<string, string>();
        public static Dictionary<string, Type> CustomEffects = new Dictionary<string, Type>();
        public static HarmonyLib.Harmony Harmony = new HarmonyLib.Harmony("LOR.BigDLL4221HarmonyPatch_MOD");

        public static Dictionary<string, List<DropBookOptions>> DropBookOptions =
            new Dictionary<string, List<DropBookOptions>>();

        public static Dictionary<string, List<PassiveOptions>> PassiveOptions =
            new Dictionary<string, List<PassiveOptions>>();

        public static Dictionary<string, CredenzaOptions> CredenzaOptions = new Dictionary<string, CredenzaOptions>();

        public static Dictionary<string, List<CategoryOptions>> CategoryOptions =
            new Dictionary<string, List<CategoryOptions>>();

        public static Dictionary<string, List<StageOptions>>
            StageOptions = new Dictionary<string, List<StageOptions>>();

        public static Dictionary<string, List<SpriteOptions>> SpriteOptions =
            new Dictionary<string, List<SpriteOptions>>();

        public static Dictionary<string, SkinOptions> SkinOptions = new Dictionary<string, SkinOptions>();

        public static Dictionary<string, List<KeypageOptions>> KeypageOptions =
            new Dictionary<string, List<KeypageOptions>>();

        public static List<RewardOptions> StartUpRewardOptions = new List<RewardOptions>();

        public static Dictionary<string, List<PassiveXmlInfo>> CreatePassives =
            new Dictionary<string, List<PassiveXmlInfo>>();

        public static Dictionary<string, List<EmotionCardOptions>> EmotionCards =
            new Dictionary<string, List<EmotionCardOptions>>();

        public static Dictionary<string, List<EmotionEgoOptions>> EmotionEgoCards =
            new Dictionary<string, List<EmotionEgoOptions>>();

        public static Dictionary<string, List<CustomBookSkinsOption>> CustomBookSkinsOptions =
            new Dictionary<string, List<CustomBookSkinsOption>>();

        public static Dictionary<string, List<ExtraOptions>> ExtraOptions =
            new Dictionary<string, List<ExtraOptions>>();

        public static List<Assembly> Assemblies = new List<Assembly>();

        //Unity
        public static Dictionary<string, Assets> AssetBundle = new Dictionary<string, Assets>();
    }

    public class CustomBookSkinsOption
    {
        public CustomBookSkinsOption(string skinName, int? keypageId = null, string keypageName = "",
            string characterName = "", int? characterNameId = null, bool useLocalization = true)
        {
            SkinName = skinName;
            KeypageName = keypageName;
            KeypageId = keypageId;
            CharacterName = characterName;
            CharacterNameId = characterNameId;
            UseLocalization = useLocalization;
        }

        public string SkinName { get; set; }
        public int? KeypageId { get; set; }
        public string KeypageName { get; set; }
        public string CharacterName { get; set; }
        public int? CharacterNameId { get; set; }
        public bool UseLocalization { get; set; }
    }

    public class StaticModsInfo
    {
        public static string EmotionCardPullCode = string.Empty;
        public static string EgoCardPullCode = string.Empty;
        public static Dictionary<string, Type> EmotionCardAbility = new Dictionary<string, Type>();

        public static Dictionary<SephirahType, SavedFloorOptions> EgoAndEmotionCardChanged =
            new Dictionary<SephirahType, SavedFloorOptions>();

        public static LorId OnPlayEmotionCardUsedBy = null;
        public static bool OnPlayCardEmotion = false;
        public static bool DaatFloorFound = false;
        public static bool BaseModFound = false;
        public static bool SpeedDiceColorModFound = false;
        public static bool TiphEgoModFound = false;
        public static bool ModsLoaded = false;
        public static FieldInfo MatchInfoEmotionSelection = null;
        public static int RandomWaveStart = 0;

        //Not Used
        public static AssemblyManager.TypeDictionary<BattleUnitBuf> BuffDict =
            new AssemblyManager.TypeDictionary<BattleUnitBuf>();
    }

    public class SavedFloorOptions
    {
        public SavedFloorOptions(bool isActive = false, CustomFloorOptions floorOptions = null)
        {
            IsActive = isActive;
            FloorOptions = floorOptions;
        }

        public bool IsActive { get; set; }
        public CustomFloorOptions FloorOptions { get; set; }
    }

    public class EmotionEgoOptions
    {
        public EmotionEgoOptions(EmotionEgoCardXmlExtension cardXml, List<string> code = null,
            List<string> floorCode = null,
            string packageId = null)
        {
            CardXml = cardXml;
            Code = code ?? new List<string>();
            FloorCode = floorCode ?? new List<string>();
            PackageId = packageId;
        }

        public EmotionEgoCardXmlExtension CardXml { get; set; }
        public List<string> Code { get; set; }
        public List<string> FloorCode { get; set; }
        public string PackageId { get; set; }
    }

    public class EmotionCardOptions
    {
        public EmotionCardOptions(EmotionCardXmlExtension cardXml, List<string> code = null,
            List<string> floorCode = null,
            List<LorId> usableByBookIds = null, EmotionCardColorOptions colorOptions = null)
        {
            CardXml = cardXml;
            Code = code ?? new List<string>();
            FloorCode = floorCode ?? new List<string>();
            UsableByBookIds = usableByBookIds ?? new List<LorId>();
            ColorOptions = colorOptions;
        }

        public EmotionCardXmlExtension CardXml { get; set; }
        public List<LorId> UsableByBookIds { get; set; }
        public List<string> Code { get; set; }
        public List<string> FloorCode { get; set; }
        public EmotionCardColorOptions ColorOptions { get; set; }
    }

    public class EmotionCardColorOptions
    {
        public EmotionCardColorOptions(Color? frameColor = null, Color? textColor = null, HSVColor frameHSVColor = null)
        {
            FrameColor = frameColor;
            TextColor = textColor;
            FrameHSVColor = frameHSVColor;
        }

        public HSVColor FrameHSVColor { get; set; }
        public Color? FrameColor { get; set; }
        public Color? TextColor { get; set; }
    }

    public static class UIOptions
    {
        public static bool ChangedMultiView;
    }

    public class MultiDeckOptions
    {
        public MultiDeckOptions(List<string> labelIds)
        {
            LabelIds = labelIds;
        }

        public List<string> LabelIds { get; set; }
    }

    public class CustomDiceColorOptions
    {
        public CustomDiceColorOptions(string iconId = "", Color? textColor = null)
        {
            IconId = iconId;
            TextColor = textColor;
        }

        public string IconId { get; set; }
        public Color? TextColor { get; set; }
    }

    public class KeypageOptions
    {
        public KeypageOptions(int keypageId, bool editable = true, string editErrorMessageId = "",
            SephirahType sephirahType = SephirahType.None, bool everyoneCanEquip = false,
            bool onlySephirahCanEquip = false, string bookIconId = "",
            bool isDeckFixed = false, bool isMultiDeck = false, bool? canNotEquip = null,
            EquipRangeType? rangeType = null, MultiDeckOptions multiDeckOptions = null,
            BookCustomOptions bookCustomOptions = null, bool bannedEgoFloorCards = false,
            bool isBaseGameKeypage = false, KeypageColorOptions keypageColorOptions = null,
            bool bannedEmotionCards = false, bool targetableBySpecialCards = true,
            CustomFloorOptions customFloorOptions = null, List<int> forceAggroSpeedDie = null,
            bool forceAggroLastDie = false, bool redirectOnlyWithSlowerSpeed = false,
            CustomDiceColorOptions customDiceColorOptions = null)
        {
            KeypageId = keypageId;
            Editable = editable;
            EditErrorMessageId = editErrorMessageId;
            SephirahType = sephirahType;
            EveryoneCanEquip = everyoneCanEquip;
            OnlySephirahCanEquip = onlySephirahCanEquip;
            BookIconId = bookIconId;
            IsDeckFixed = isDeckFixed;
            IsMultiDeck = isMultiDeck;
            CanNotEquip = canNotEquip;
            EquipRangeType = rangeType;
            MultiDeckOptions = multiDeckOptions ?? new MultiDeckOptions(new List<string>());
            BookCustomOptions = bookCustomOptions;
            BannedEgoFloorCards = bannedEgoFloorCards;
            IsBaseGameKeypage = isBaseGameKeypage;
            KeypageColorOptions = keypageColorOptions;
            BannedEmotionCards = bannedEmotionCards;
            TargetableBySpecialCards = targetableBySpecialCards;
            CustomFloorOptions = customFloorOptions;
            ForceAggroSpeedDie = forceAggroSpeedDie ?? new List<int>();
            ForceAggroLastDie = forceAggroLastDie;
            RedirectOnlyWithSlowerSpeed = redirectOnlyWithSlowerSpeed;
            CustomDiceColorOptions = customDiceColorOptions;
        }

        public int KeypageId { get; set; }
        public bool Editable { get; set; }
        public List<int> ForceAggroSpeedDie { get; set; }
        public bool ForceAggroLastDie { get; set; }
        public bool RedirectOnlyWithSlowerSpeed { get; set; }
        public string EditErrorMessageId { get; set; }
        public SephirahType SephirahType { get; set; }
        public bool EveryoneCanEquip { get; set; }
        public bool OnlySephirahCanEquip { get; set; }
        public string BookIconId { get; set; }
        public bool IsDeckFixed { get; set; }
        public bool IsMultiDeck { get; set; }
        public bool? CanNotEquip { get; set; }
        public EquipRangeType? EquipRangeType { get; set; }
        public MultiDeckOptions MultiDeckOptions { get; set; }
        public BookCustomOptions BookCustomOptions { get; set; }
        public bool BannedEgoFloorCards { get; set; }
        public bool BannedEmotionCards { get; set; }
        public bool IsBaseGameKeypage { get; set; }
        public bool TargetableBySpecialCards { get; set; }
        public KeypageColorOptions KeypageColorOptions { get; set; }
        public CustomFloorOptions CustomFloorOptions { get; set; }
        public CustomDiceColorOptions CustomDiceColorOptions { get; set; }
    }

    public class CustomFloorOptions
    {
        public CustomFloorOptions(string packageId = "", string floorCode = "", string iconId = "",
            string floorName = "", string floorNameId = "", MapModel customFloorMap = null,
            CustomDiceColorOptions customDiceColorOptions = null)
        {
            PackageId = packageId;
            FloorCode = floorCode;
            IconId = iconId;
            CustomFloorMap = customFloorMap;
            FloorName = floorName;
            FloorNameId = floorNameId;
            CustomDiceColorOptions = customDiceColorOptions;
        }

        public string PackageId { get; set; }
        public string FloorCode { get; set; }

        public string IconId { get; set; }
        public string FloorNameId { get; set; }
        public string FloorName { get; set; }
        public List<EmotionCardXmlInfo> OriginalEmotionCards { get; set; }
        public List<EmotionEgoXmlInfo> OriginalEgoCards { get; set; }
        public MapModel CustomFloorMap { get; set; }
        public CustomDiceColorOptions CustomDiceColorOptions { get; set; }
    }

    public class KeypageColorOptions
    {
        public KeypageColorOptions(Color? frameColor = null, Color? nameColor = null)
        {
            FrameColor = frameColor;
            NameColor = nameColor;
        }

        public Color? FrameColor { get; set; }
        public Color? NameColor { get; set; }
    }

    //int in dictionary in this class mean *Quantity*
    public class RewardOptions
    {
        public RewardOptions(Dictionary<LorId, int> books = null, Dictionary<LorId, int> cards = null,
            List<LorId> keypages = null, string messageId = "", bool singleTimeReward = false)
        {
            Books = books ?? new Dictionary<LorId, int>();
            Cards = cards ?? new Dictionary<LorId, int>();
            Keypages = keypages ?? new List<LorId>();
            MessageId = messageId;
            SingleTimeReward = singleTimeReward;
        }

        public Dictionary<LorId, int> Books { get; set; }
        public Dictionary<LorId, int> Cards { get; set; }
        public List<LorId> Keypages { get; set; }
        public string MessageId { get; set; }
        public bool SingleTimeReward { get; set; }
    }

    public class SkinOptions
    {
        public SkinOptions(string packageId = "", int customHeight = 0,
            Dictionary<MotionDetail, MotionSound> motionSounds = null)
        {
            PackageId = packageId;
            CustomHeight = customHeight;
            MotionSounds = motionSounds ?? new Dictionary<MotionDetail, MotionSound>();
        }

        public string PackageId { get; set; }
        public int CustomHeight { get; set; }
        public Dictionary<MotionDetail, MotionSound> MotionSounds { get; set; }
    }

    public class BookCustomOptions
    {
        public BookCustomOptions(string name = "", int nameTextId = 0, bool customFaceData = true,
            string originalSkin = "", List<string> egoSkin = null, LorId customDialogId = null,
            BattleDialogCharacter customDialog = null, bool originalSkinIsBaseGame = false,
            ActionDetail xiaoTaotieAction = ActionDetail.NONE)
        {
            NameTextId = nameTextId;
            CustomFaceData = customFaceData;
            OriginalSkin = originalSkin;
            EgoSkin = egoSkin ?? new List<string>();
            Name = name;
            CustomDialogId = customDialogId;
            CustomDialog = customDialog;
            OriginalSkinIsBaseGame = originalSkinIsBaseGame;
            XiaoTaotieAction = xiaoTaotieAction;
        }

        public int NameTextId { get; set; }
        public string Name { get; set; }
        public bool CustomFaceData { get; set; }
        public string OriginalSkin { get; set; }
        public List<string> EgoSkin { get; set; }
        public LorId CustomDialogId { get; set; }
        public BattleDialogCharacter CustomDialog { get; set; }
        public bool OriginalSkinIsBaseGame { get; set; }
        public ActionDetail XiaoTaotieAction { get; set; }
    }

    public class CardOptions
    {
        public CardOptions(int cardId, CardOption option = CardOption.Basic, List<string> keywords = null,
            List<LorId> bookId = null, bool onlyAllyTargetCard = false, bool oneSideOnlyCard = false,
            bool isBaseGameCard = false, CardColorOptions cardColorOptions = null)
        {
            CardId = cardId;
            Option = option;
            Keywords = keywords ?? new List<string>();
            BookId = bookId ?? new List<LorId>();
            OnlyAllyTargetCard = onlyAllyTargetCard;
            OneSideOnlyCard = oneSideOnlyCard;
            IsBaseGameCard = isBaseGameCard;
            CardColorOptions = cardColorOptions;
        }

        public int CardId { get; set; }
        public CardOption Option { get; set; }
        public List<string> Keywords { get; set; }
        public List<LorId> BookId { get; set; }
        public bool OnlyAllyTargetCard { get; set; }
        public bool OneSideOnlyCard { get; set; }
        public bool IsBaseGameCard { get; set; }
        public CardColorOptions CardColorOptions { get; set; }
    }

    public class CardColorOptions
    {
        public CardColorOptions(Color? cardColor = null, string customIcon = "",
            Color? customIconColor = null, HSVColor iconColor = null, bool useHSVFilter = true, string leftFrame = "",
            string rightFrame = "", string frontFrame = "", bool applyFrontColor = false,
            bool applySideFrontColors = false)
        {
            CardColor = cardColor;
            CustomIcon = customIcon;
            CustomIconColor = customIconColor;
            IconColor = iconColor;
            UseHSVFilter = useHSVFilter;
            LeftFrame = leftFrame;
            RightFrame = rightFrame;
            ApplyFrontColor = applyFrontColor;
            FrontFrame = frontFrame;
            ApplySideFrontColors = applySideFrontColors;
        }

        public Color? CardColor { get; set; }
        public string CustomIcon { get; set; }
        public Color? CustomIconColor { get; set; }
        public HSVColor IconColor { get; set; }
        public bool UseHSVFilter { get; set; }
        public string LeftFrame { get; set; }
        public string RightFrame { get; set; }
        public string FrontFrame { get; set; }
        public bool ApplyFrontColor { get; set; }
        public bool ApplySideFrontColors { get; set; }
    }

    public class StageOptions
    {
        public StageOptions(int stageId, bool bannedEmotionLevel = false, RewardOptions stageRewardOptions = null,
            PreBattleOptions preBattleOptions = null, StageColorOptions stageColorOptions = null,
            StageRequirements stageRequirements = null, CustomDiceColorOptions customDiceColorOptions = null)
        {
            StageId = stageId;
            BannedEmotionLevel = bannedEmotionLevel;
            StageRewardOptions = stageRewardOptions;
            PreBattleOptions = preBattleOptions;
            StageColorOptions = stageColorOptions;
            StageRequirements = stageRequirements;
            CustomDiceColorOptions = customDiceColorOptions;
        }

        public int StageId { get; set; }
        public bool BannedEmotionLevel { get; set; }
        public RewardOptions StageRewardOptions { get; set; }
        public PreBattleOptions PreBattleOptions { get; set; }
        public StageColorOptions StageColorOptions { get; set; }
        public StageRequirements StageRequirements { get; set; }
        public CustomDiceColorOptions CustomDiceColorOptions { get; set; }
    }

    public class StageRequirements
    {
        public StageRequirements(int? requiredLibraryLevel = null, List<LorId> requiredStageIds = null)
        {
            RequiredLibraryLevel = requiredLibraryLevel;
            RequiredStageIds = requiredStageIds ?? new List<LorId>();
        }

        public int? RequiredLibraryLevel { get; set; }
        public List<LorId> RequiredStageIds { get; set; }
    }

    public class StageColorOptions
    {
        public StageColorOptions(Color? frameColor = null, Color? textColor = null)
        {
            FrameColor = frameColor;
            TextColor = textColor;
        }

        public Color? FrameColor { get; set; }
        public Color? TextColor { get; set; }
    }

    public class BaseColorOptions
    {
        public BaseColorOptions(Color? frameColor = null, Color? textColor = null)
        {
            FrameColor = frameColor;
            TextColor = textColor;
        }

        public Color? FrameColor { get; set; }
        public Color? TextColor { get; set; }
    }

    public class PreBattleOptions
    {
        public PreBattleOptions(Dictionary<SephirahType, List<UnitModel>> customUnits = null,
            bool setToggles = false, bool fillWithBaseUnits = false,
            PreBattleType battleType = PreBattleType.CustomUnits,
            Dictionary<SephirahType, List<SephirahType>> sephirahUnits = null,
            bool onlySephirah = false, bool sephirahLocked = false, List<SephirahType> unlockedSephirah = null)
        {
            CustomUnits = customUnits ?? new Dictionary<SephirahType, List<UnitModel>>();
            SetToggles = setToggles;
            BattleType = battleType;
            SephirahUnits = sephirahUnits ?? new Dictionary<SephirahType, List<SephirahType>>();
            OnlySephirah = onlySephirah;
            SephirahLocked = sephirahLocked;
            UnlockedSephirah = unlockedSephirah ?? new List<SephirahType>();
            FillWithBaseUnits = fillWithBaseUnits;
        }

        public PreBattleType BattleType { get; set; }
        public Dictionary<SephirahType, List<UnitModel>> CustomUnits { get; set; }
        public bool SetToggles { get; set; }
        public Dictionary<SephirahType, List<SephirahType>> SephirahUnits { get; set; }
        public bool FillWithBaseUnits { get; set; }
        public bool OnlySephirah { get; set; }
        public bool SephirahLocked { get; set; }
        public List<SephirahType> UnlockedSephirah { get; set; }
    }

    public class PassiveOptions
    {
        public PassiveOptions(int passiveId, bool transferable = true, int innerTypeId = 0,
            ForceAggroOptions forceAggroOptions = null,
            bool isMultiDeck = false, List<LorId> cannotBeUsedWithPassives = null,
            List<LorId> canBeUsedWithPassivesAll = null, List<LorId> canBeUsedWithPassivesOne = null,
            List<LorId> chainReleasePassives = null, bool gainCoins = true,
            bool bannedEmotionCardSelection = false,
            bool bannedEgoFloorCards = false, bool ignoreClashPassive = false, MultiDeckOptions multiDeckOptions = null,
            bool isBaseGamePassive = false, string passiveScriptId = "", PassiveColorOptions passiveColorOptions = null)
        {
            PassiveId = passiveId;
            Transferable = transferable;
            InnerTypeId = innerTypeId;
            ForceAggroOptions = forceAggroOptions;
            IsMultiDeck = isMultiDeck;
            CannotBeUsedWithPassives = cannotBeUsedWithPassives ?? new List<LorId>();
            CanBeUsedWithPassivesAll = canBeUsedWithPassivesAll ?? new List<LorId>();
            CanBeUsedWithPassivesOne = canBeUsedWithPassivesOne ?? new List<LorId>();
            ChainReleasePassives = chainReleasePassives ?? new List<LorId>();
            GainCoins = gainCoins;
            BannedEmotionCardSelection = bannedEmotionCardSelection;
            BannedEgoFloorCards = bannedEgoFloorCards;
            IgnoreClashPassive = ignoreClashPassive;
            MultiDeckOptions = multiDeckOptions ?? new MultiDeckOptions(new List<string>());
            IsBaseGamePassive = isBaseGamePassive;
            PassiveScriptId = passiveScriptId;
            PassiveColorOptions = passiveColorOptions;
        }

        public int PassiveId { get; set; }
        public bool Transferable { get; set; }
        public int InnerTypeId { get; set; }
        public ForceAggroOptions ForceAggroOptions { get; set; }
        public bool IsMultiDeck { get; set; }
        public List<LorId> CannotBeUsedWithPassives { get; set; }
        public List<LorId> CanBeUsedWithPassivesAll { get; set; }
        public List<LorId> CanBeUsedWithPassivesOne { get; set; }
        public List<LorId> ChainReleasePassives { get; set; }
        public bool GainCoins { get; set; }
        public bool BannedEmotionCardSelection { get; set; }
        public bool BannedEgoFloorCards { get; set; }
        public bool IgnoreClashPassive { get; set; }
        public MultiDeckOptions MultiDeckOptions { get; set; }
        public bool IsBaseGamePassive { get; set; }
        public string PassiveScriptId { get; set; }
        public PassiveColorOptions PassiveColorOptions { get; set; }
    }

    public class ForceAggroOptions
    {
        public ForceAggroOptions(bool forceAggro = false, List<LorId> forceAggroByTargetedPassive = null,
            List<BattleUnitBuf> forceAggroByTargetedBuffs = null, List<LorId> forceAggroByTargetPassive = null,
            List<BattleUnitBuf> forceAggroByTargetBuffs = null, bool forceAggroLastDie = false,
            List<int> forceAggroSpeedDie = null)
        {
            ForceAggro = forceAggro;
            ForceAggroByTargetedPassive = forceAggroByTargetedPassive ?? new List<LorId>();
            ForceAggroByTargetedBuffs = forceAggroByTargetedBuffs ?? new List<BattleUnitBuf>();
            ForceAggroByTargetPassive = forceAggroByTargetPassive ?? new List<LorId>();
            ForceAggroByTargetBuffs = forceAggroByTargetBuffs ?? new List<BattleUnitBuf>();
            ForceAggroLastDie = forceAggroLastDie;
            ForceAggroSpeedDie = forceAggroSpeedDie ?? new List<int>();
        }

        public bool ForceAggro { get; set; }
        public List<LorId> ForceAggroByTargetPassive { get; set; }
        public List<BattleUnitBuf> ForceAggroByTargetBuffs { get; set; }
        public List<LorId> ForceAggroByTargetedPassive { get; set; }
        public List<BattleUnitBuf> ForceAggroByTargetedBuffs { get; set; }
        public bool ForceAggroLastDie { get; set; }
        public List<int> ForceAggroSpeedDie { get; set; }
    }

    public class PassiveColorOptions
    {
        public PassiveColorOptions(Color? textColor = null, Color? fillColor = null)
        {
            TextColor = textColor;
            FillColor = fillColor;
        }

        public Color? TextColor { get; set; }
        public Color? FillColor { get; set; }
    }

    public class CredenzaOptions
    {
        public CredenzaOptions(CredenzaEnum credenzaOption = CredenzaEnum.NoCredenza, List<int> credenzaBooksId = null,
            string customIconSpriteId = "", string baseIconSpriteId = "", string credenzaNameId = "",
            string credenzaName = "", CredenzaColorOptions bookDataColor = null, int chapter = 7)
        {
            CredenzaOption = credenzaOption;
            CredenzaBooksId = credenzaBooksId ?? new List<int>();
            CustomIconSpriteId = customIconSpriteId;
            BaseIconSpriteId = baseIconSpriteId;
            CredenzaNameId = credenzaNameId;
            CredenzaName = credenzaName;
            BookDataColor = bookDataColor;
            Chapter = chapter;
        }

        public int Chapter { get; set; }
        public CredenzaEnum CredenzaOption { get; set; }
        public List<int> CredenzaBooksId { get; set; }
        public string CustomIconSpriteId { get; set; }
        public string BaseIconSpriteId { get; set; }
        public string CredenzaNameId { get; set; }
        public string CredenzaName { get; set; }
        public CredenzaColorOptions BookDataColor { get; set; }
    }

    public class CredenzaColorOptions
    {
        public CredenzaColorOptions(Color? frameColor = null, Color? textColor = null)
        {
            FrameColor = frameColor;
            TextColor = textColor;
        }

        public Color? FrameColor { get; set; }
        public Color? TextColor { get; set; }
    }

    public class CategoryOptions
    {
        public CategoryOptions(string packageId = "", string additionalValue = "", List<int> categoryBooksId = null,
            string customIconSpriteId = "", string baseIconSpriteId = "", string categoryNameId = "",
            string categoryName = "", int chapter = 7, CategoryColorOptions bookDataColor = null,
            UIStoryLine? baseGameCategory = null, List<int> credenzaBooksId = null,
            CredenzaEnum credenzaType = CredenzaEnum.ModifiedCredenza)
        {
            CategoryBooksId = categoryBooksId ?? new List<int>();
            CustomIconSpriteId = customIconSpriteId;
            BaseIconSpriteId = baseIconSpriteId;
            CategoryNameId = categoryNameId;
            CategoryName = categoryName;
            Chapter = chapter;
            AdditionalValue = additionalValue;
            PackageId = packageId;
            BookDataColor = bookDataColor;
            BaseGameCategory = baseGameCategory;
            CredenzaType = credenzaType;
            CredenzaBooksId = credenzaBooksId ?? new List<int>();
        }

        public CredenzaEnum CredenzaType { get; set; }
        public string PackageId { get; set; }
        public string AdditionalValue { get; set; }
        public int Chapter { get; set; }
        public List<int> CategoryBooksId { get; set; }
        public List<int> CredenzaBooksId { get; set; }
        public string CustomIconSpriteId { get; set; }
        public string BaseIconSpriteId { get; set; }
        public string CategoryNameId { get; set; }
        public string CategoryName { get; set; }
        public CategoryColorOptions BookDataColor { get; set; }
        public UIStoryLine? BaseGameCategory { get; set; }
    }

    public class CategoryColorOptions
    {
        public CategoryColorOptions(Color? frameColor = null, Color? textColor = null)
        {
            FrameColor = frameColor;
            TextColor = textColor;
        }

        public Color? FrameColor { get; set; }
        public Color? TextColor { get; set; }
    }

    public class SpriteOptions
    {
        public SpriteOptions(SpriteEnum spriteOption = SpriteEnum.Custom, int keypageId = 0, string spritePK = "")
        {
            SpriteOption = spriteOption;
            KeypageId = keypageId;
            SpritePK = spritePK;
        }

        public SpriteEnum SpriteOption { get; set; }
        public int KeypageId { get; set; }
        public string SpritePK { get; set; }
    }

    public class LocalizedItem
    {
        public Dictionary<int, string> CardNames { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> DropBookNames { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> StageNames { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> EnemyNames { get; set; } = new Dictionary<int, string>();

        public Dictionary<string, List<string>> BattleCardAbilitiesText { get; set; } =
            new Dictionary<string, List<string>>();

        public Dictionary<string, EffectText> EffectTexts { get; set; } = new Dictionary<string, EffectText>();
        public Dictionary<int, EffectText> PassiveTexts { get; set; } = new Dictionary<int, EffectText>();
        public List<BattleDialogCharacter> BattleDialogCharacterList { get; set; } = new List<BattleDialogCharacter>();
        public List<BookDesc> Keypages { get; set; } = new List<BookDesc>();
        public List<AbnormalityCard> AbnormalityCards { get; set; } = new List<AbnormalityCard>();
        public Dictionary<string, string> Etc { get; set; } = new Dictionary<string, string>();
    }

    public class DropBookOptions
    {
        public DropBookOptions(int dropBookId = 0, DropBookColorOptions dropBookColorOptions = null)
        {
            DropBookColorOptions = dropBookColorOptions;
            DropBookId = dropBookId;
        }

        public int DropBookId { get; set; }
        public DropBookColorOptions DropBookColorOptions { get; set; }
    }

    public class DropBookColorOptions
    {
        public DropBookColorOptions(Color? frameColor = null, Color? nameColor = null)
        {
            FrameColor = frameColor;
            NameColor = nameColor;
        }

        public Color? FrameColor { get; set; }
        public Color? NameColor { get; set; }
    }

    public class EffectText
    {
        public string Name { get; set; }
        public string Desc { get; set; }
    }

    public class MotionSound
    {
        public string FileNameLose;
        public string FileNameWin;
        public bool IsBaseSoundLose;
        public bool IsBaseSoundWin;

        public MotionSound(string fileNameWin, string fileNameLose = "", bool isBaseSoundWin = false,
            bool isBaseSoundLose = false)
        {
            FileNameWin = fileNameWin;
            FileNameLose = fileNameLose;
            IsBaseSoundWin = isBaseSoundWin;
            IsBaseSoundLose = isBaseSoundLose;
        }
    }

    public class ExtraOptions
    {
        public ExtraOptions(int? id = null, Type buff = null, ParameterTypeEnum optionType = ParameterTypeEnum.Passive,
            Dictionary<string, bool> bools = null, Dictionary<string, List<int>> ints = null,
            Dictionary<string, List<LorId>> lorIds = null, Dictionary<string, List<UnitModel>> unitModels = null,
            Dictionary<string, BaseColorOptions> colors = null,
            Dictionary<string, List<Type>> buffs = null, Dictionary<string, string> strings = null)
        {
            Id = id;
            Buff = buff;
            OptionType = optionType;
            Bools = bools ?? new Dictionary<string, bool>();
            Ints = ints ?? new Dictionary<string, List<int>>();
            LorIds = lorIds ?? new Dictionary<string, List<LorId>>();
            UnitModels = unitModels ?? new Dictionary<string, List<UnitModel>>();
            Colors = colors ?? new Dictionary<string, BaseColorOptions>();
            Buffs = buffs ?? new Dictionary<string, List<Type>>();
            Strings = strings ?? new Dictionary<string, string>();
        }

        public int? Id { get; set; }
        public Type Buff { get; set; }

        public ParameterTypeEnum OptionType { get; set; }

        public Dictionary<string, string> Strings { get; set; }
        public Dictionary<string, bool> Bools { get; set; }
        public Dictionary<string, List<int>> Ints { get; set; }
        public Dictionary<string, List<LorId>> LorIds { get; set; }
        public Dictionary<string, List<UnitModel>> UnitModels { get; set; }
        public Dictionary<string, BaseColorOptions> Colors { get; set; }
        public Dictionary<string, List<Type>> Buffs { get; set; }
    }

    public static class Condition
    {
        public static string ForceAggro = "ForceAggro";
        public static string MultiUsePassive = "PassiveCanBeUsedMoreTimes";
        public static string IgnoreClashOnlyForAlly = "IgnoreClashAlly";
        public static string RandomWave = "RandomWave";
        public static string HidePreview = "HidePreview";
        public static string ManagerScriptName = "ManagerScript";
        public static string UsableUnits = "UsableUnits";
        public static string FormationId = "FormationId";
    }
}