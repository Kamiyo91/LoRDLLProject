using System;
using System.Collections.Generic;
using BigDLL4221.Enum;
using LOR_DiceSystem;
using LOR_XML;
using UnityEngine;

namespace BigDLL4221.Models
{
    public static class ModParameters
    {
        public static List<string> PackageIds = new List<string>();
        public static Dictionary<string, Sprite> ArtWorks = new Dictionary<string, Sprite>();
        public static Dictionary<string, string> Path = new Dictionary<string, string>();
        public static Dictionary<string, LocalizedItem> LocalizedItems = new Dictionary<string, LocalizedItem>();
        public static string Language = GlobalGameManager.Instance.CurrentOption.language;
        public static Dictionary<string, List<CardOptions>> CardOptions = new Dictionary<string, List<CardOptions>>();
        public static Dictionary<string, string> DefaultKeyword = new Dictionary<string, string>();
        public static Dictionary<string, Type> CustomEffects = new Dictionary<string, Type>();

        public static Dictionary<string, List<DropBookOptions>> DropBookOptions =
            new Dictionary<string, List<DropBookOptions>>();

        public static Dictionary<string, List<PassiveOptions>> PassiveOptions =
            new Dictionary<string, List<PassiveOptions>>();

        public static Dictionary<string, CredenzaOptions> CredenzaOptions = new Dictionary<string, CredenzaOptions>();

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

        //Unity
        public static Dictionary<string, AssetBundle> AssetBundle = new Dictionary<string, AssetBundle>();
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

    public class KeypageOptions
    {
        public KeypageOptions(int keypageId, bool editable = true, string editErrorMessageId = "",
            SephirahType sephirahType = SephirahType.None, bool everyoneCanEquip = false,
            bool onlySephirahCanEquip = false, string bookIconId = "",
            bool isDeckFixed = false, bool isMultiDeck = false, bool? canNotEquip = null,
            EquipRangeType? rangeType = null, MultiDeckOptions multiDeckOptions = null,
            BookCustomOptions bookCustomOptions = null, bool bannedEgoFloorCards = false,
            bool isBaseGameKeypage = false, KeypageColorOptions keypageColorOptions = null)
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
        }

        public int KeypageId { get; set; }
        public bool Editable { get; set; }
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
        public bool IsBaseGameKeypage { get; set; }
        public KeypageColorOptions KeypageColorOptions { get; set; }
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
            Color? customIconColor = null, HSVColor iconColor = null, bool useHSVFilter = true)
        {
            CardColor = cardColor;
            CustomIcon = customIcon;
            CustomIconColor = customIconColor;
            IconColor = iconColor;
            UseHSVFilter = useHSVFilter;
        }

        public Color? CardColor { get; set; }
        public string CustomIcon { get; set; }
        public Color? CustomIconColor { get; set; }
        public HSVColor IconColor { get; set; }
        public bool UseHSVFilter { get; set; }
    }

    public class StageOptions
    {
        public StageOptions(int stageId, bool bannedEmotionLevel = false, RewardOptions stageRewardOptions = null,
            PreBattleOptions preBattleOptions = null, StageColorOptions stageColorOptions = null)
        {
            StageId = stageId;
            BannedEmotionLevel = bannedEmotionLevel;
            StageRewardOptions = stageRewardOptions;
            PreBattleOptions = preBattleOptions;
            StageColorOptions = stageColorOptions;
        }

        public int StageId { get; set; }
        public bool BannedEmotionLevel { get; set; }
        public RewardOptions StageRewardOptions { get; set; }
        public PreBattleOptions PreBattleOptions { get; set; }
        public StageColorOptions StageColorOptions { get; set; }
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

    public class PreBattleOptions
    {
        public PreBattleOptions(List<SephirahType> sephirah = null, List<UnitModel> unitModels = null,
            bool setToggles = false, bool fillWithBaseUnits = false,
            PreBattleType battleType = PreBattleType.CustomUnits, List<SephirahType> sephirahUnits = null,
            bool onlySephirah = false, bool sephirahLocked = false, List<SephirahType> unlockedSephirah = null)
        {
            Sephirah = sephirah ?? new List<SephirahType>();
            UnitModels = unitModels ?? new List<UnitModel>();
            SetToggles = setToggles;
            BattleType = battleType;
            SephirahUnits = sephirahUnits ?? new List<SephirahType>();
            OnlySephirah = onlySephirah;
            SephirahLocked = sephirahLocked;
            UnlockedSephirah = unlockedSephirah ?? new List<SephirahType>();
            FillWithBaseUnits = fillWithBaseUnits;
        }

        public PreBattleType BattleType { get; set; }
        public List<SephirahType> Sephirah { get; set; }
        public List<UnitModel> UnitModels { get; set; }
        public bool SetToggles { get; set; }
        public List<SephirahType> SephirahUnits { get; set; }
        public bool FillWithBaseUnits { get; set; }
        public bool OnlySephirah { get; set; }
        public bool SephirahLocked { get; set; }
        public List<SephirahType> UnlockedSephirah { get; set; }
    }

    public class PassiveOptions
    {
        public PassiveOptions(int passiveId, bool transferable = true, int innerTypeId = 0, bool forceAggro = false,
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
            ForceAggro = forceAggro;
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
        public bool ForceAggro { get; set; }
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
            string credenzaName = "")
        {
            CredenzaOption = credenzaOption;
            CredenzaBooksId = credenzaBooksId ?? new List<int>();
            CustomIconSpriteId = customIconSpriteId;
            BaseIconSpriteId = baseIconSpriteId;
            CredenzaNameId = credenzaNameId;
            CredenzaName = credenzaName;
        }

        public CredenzaEnum CredenzaOption { get; set; }
        public List<int> CredenzaBooksId { get; set; }
        public string CustomIconSpriteId { get; set; }
        public string BaseIconSpriteId { get; set; }
        public string CredenzaNameId { get; set; }
        public string CredenzaName { get; set; }
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
}