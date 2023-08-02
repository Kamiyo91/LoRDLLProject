using System.Collections.Generic;
using System.Xml.Serialization;
using BigDLL4221.Enum;
using LOR_DiceSystem;
using LOR_XML;
using UI;

namespace BigDLL4221.Extensions
{
    public class PassiveOptionsRoot
    {
        [XmlElement("PassiveOption")] public List<PassiveOptionRoot> PassiveOptions;
    }

    public class DefaultKeywordRoot
    {
        [XmlElement("DefaultKeyword")] public DefaultKeywordOption DefaultKeywordOption;
    }

    public class ExtraOptionsRoot
    {
        [XmlElement("ExtraOption")] public List<ExtraOptionRoot> ExtraOption;
    }

    public class DllUtilOptionsRoot
    {
        [XmlElement("CustomColors")] public bool CustomColors = true;
        [XmlElement("CustomSpeedDice")] public bool CustomSpeedDice = true;
    }

    public class ExtraOptionRoot
    {
        [XmlAttribute("Buff")] public string Buff = "";
        [XmlElement("ExtraBool")] public List<ExtraBool> ExtraBools;

        [XmlElement("ExtraLorId")] public List<ExtraLorIdRoot> ExtraLorIds;

        [XmlElement("ExtraModelOption")]
        public List<ExtraUnitModelRoot> ExtraUnitModelOptions = new List<ExtraUnitModelRoot>();

        [XmlElement("ExtraColorOption")]
        public List<ExtraColorOptionsRoot> ExtraColorOptions = new List<ExtraColorOptionsRoot>();

        [XmlElement("ExtraBuffOption")] public List<ExtraBuffsRoot> ExtraBuffOptions = new List<ExtraBuffsRoot>();


        [XmlElement("ExtraInt")] public List<ExtraOptionInts> ExtraInts;


        [XmlElement("ExtraString")] public List<ExtraString> ExtraString;


        [XmlAttribute("Id")] public int Id;
        [XmlAttribute("OptionType")] public ParameterTypeEnum OptionType;
    }

    public class ExtraOptionBase
    {
        [XmlAttribute("Condition")] public string Condition = "";
    }

    public class ExtraBool : ExtraOptionBase
    {
        [XmlAttribute("Value")] public bool BoolValue;
    }

    public class ExtraString : ExtraOptionBase
    {
        [XmlAttribute("Value")] public string StringValue;
    }

    public class ExtraUnitModelRoot : ExtraOptionBase
    {
        [XmlElement("ExtraUnitModel")] public List<UnitModelRoot> UnitModel = new List<UnitModelRoot>();
    }

    public class ExtraBuffsRoot : ExtraOptionBase
    {
        [XmlElement("Buff")] public List<string> Buff = new List<string>();
    }

    public class ExtraColorOptionsRoot : ExtraOptionBase
    {
        [XmlElement("Color")] public ColorOptionsRoot Color;
    }

    public class ExtraOptionInts : ExtraOptionBase
    {
        [XmlElement("IntValue")] public List<int> IntValue = new List<int>();
    }

    public class ExtraLorIdRoot : ExtraOptionBase
    {
        [XmlElement("LorId")] public List<LorIdRoot> LorId = new List<LorIdRoot>();
    }

    public class DefaultKeywordOption
    {
        [XmlAttribute("Keyword")] public string Keyword;
        [XmlAttribute("PackageId")] public string PackageId;
    }

    public class CardOptionsRoot
    {
        [XmlElement("CardOption")] public List<CardOptionRoot> CardOption;
    }

    public class DropBookOptionsRoot
    {
        [XmlElement("DropBookOption")] public List<DropBookOptionRoot> DropBookOption;
    }

    public class CredenzaOptionsRoot
    {
        [XmlElement("CredenzaOption")] public List<CredenzaOptionRoot> CredenzaOption;
    }

    public class CategoryOptionsRoot
    {
        [XmlElement("CategoryOption")] public List<CategoryOptionRoot> CategoryOption;
    }

    public class StageOptionsRoot
    {
        [XmlElement("StageOption")] public List<StageOptionRoot> StageOption;
    }

    public class SpriteOptionsRoot
    {
        [XmlElement("SpriteOption")] public List<SpriteOptionRoot> SpriteOption;
    }

    public class SkinOptionsRoot
    {
        [XmlElement("SkinOption")] public List<SkinOptionRoot> SkinOption;
    }

    public class KeypageOptionsRoot
    {
        [XmlElement("KeypageOption")] public List<KeypageOptionRoot> KeypageOption;
    }

    public class RewardOptionsRoot
    {
        [XmlElement("RewardOptions")] public List<RewardOptionRoot> RewardOption;
    }

    public class CustomSkinOptionsRoot
    {
        [XmlElement("CustomSkinOption")] public List<CustomSkinOptionRoot> CustomSkinOption;
    }

    public class EmotionCardOptionsRoot
    {
        [XmlElement("EmotionCardOption")] public List<EmotionCardOptionRoot> EmotionCardOption;
    }

    public class EgoCardOptionsRoot
    {
        [XmlElement("EgoCardOption")] public List<EgoCardOptionRoot> EgoCardOption;
    }

    public class ColorRoot
    {
        [XmlAttribute("A")] public float A;
        [XmlAttribute("B")] public float B;
        [XmlAttribute("G")] public float G;
        [XmlAttribute("R")] public float R;
    }

    public class HsvColorRoot
    {
        [XmlAttribute("H")] public float H;
        [XmlAttribute("S")] public float S;
        [XmlAttribute("V")] public float V;
    }

    public class ColorOptionsRoot
    {
        [XmlElement("TextColor")] public ColorRoot TextColor;
        [XmlElement("FrameColor")] public ColorRoot FrameColor;
    }

    public class MultiDeckOptionsRoot
    {
        [XmlElement("LabelId")] public List<string> LabelIds = new List<string>();
    }

    public class LorIdRoot
    {
        [XmlAttribute("Id")] public int Id;
        [XmlAttribute("PackageId")] public string PackageId = "";
    }

    public class ForceAggroOptionsRoot
    {
        [XmlElement("ForceAggro")] public bool ForceAggro;

        [XmlElement("ForceAggroLastDie")] public bool ForceAggroLastDie;
        [XmlElement("ForceAggroSpeedDie")] public List<int> ForceAggroSpeedDie;

        [XmlElement("ForceAggroByTargetBuffs")]
        public List<string> ForceAggroByTargetBuffs;

        [XmlElement("ForceAggroByTargetedBuffs")]
        public List<string> ForceAggroByTargetedBuffs;

        [XmlElement("ForceAggroByTargetPassive")]
        public List<LorIdRoot> ForceAggroByTargetPassive;

        [XmlElement("ForceAggroByTargetedPassive")]
        public List<LorIdRoot> ForceAggroByTargetedPassive;
    }

    public class CustomSkinOptionRoot
    {
        [XmlElement("KeypageId")] public int? KeypageId;
        [XmlElement("KeypageName")] public string KeypageName = "";
        [XmlElement("CharacterName")] public string CharacterName = "";
        [XmlElement("CharacterNameId")] public int? CharacterNameId;


        [XmlElement("SkinName")] public string SkinName = "";


        [XmlElement("UseLocalization")] public bool UseLocalization = true;
    }

    public class PassiveOptionRoot
    {
        [XmlElement("ChainReleasePassives")] public List<LorIdRoot> ChainReleasePassives = new List<LorIdRoot>();
        [XmlElement("GainCoins")] public bool GainCoins = true;

        [XmlElement("BannedEmotionCardSelection")]
        public bool BannedEmotionCardSelection;

        [XmlElement("BannedEgoFloorCards")] public bool BannedEgoFloorCards;


        [XmlElement("ForceAggroOptions")] public ForceAggroOptionsRoot ForceAggroOptions;
        [XmlElement("IsMultiDeck")] public bool IsMultiDeck;

        [XmlElement("CannotBeUsedWithPassives")]
        public List<LorIdRoot> CannotBeUsedWithPassives = new List<LorIdRoot>();

        [XmlElement("CanBeUsedWithPassivesAll")]
        public List<LorIdRoot> CanBeUsedWithPassivesAll = new List<LorIdRoot>();

        [XmlElement("CanBeUsedWithPassivesOne")]
        public List<LorIdRoot> CanBeUsedWithPassivesOne = new List<LorIdRoot>();


        [XmlElement("Transferable")] public bool Transferable = true;
        [XmlElement("InnerTypeId")] public int InnerTypeId = -1;
        [XmlElement("IgnoreClashPassive")] public bool IgnoreClashPassive;

        [XmlElement("MultiDeckOptions")] public MultiDeckOptionsRoot MultiDeckOptions;
        [XmlElement("IsBaseGamePassive")] public bool IsBaseGamePassive;


        [XmlElement("PassiveScriptId")] public string PassiveScriptId = "";
        [XmlElement("PassiveColorOptions")] public ColorOptionsRoot PassiveColorOptions;
        [XmlAttribute("Id")] public int PassiveId;
    }

    public class DropBookOptionRoot
    {
        [XmlElement("DropBookColorOptions")] public ColorOptionsRoot DropBookColorOptions;

        [XmlAttribute("Id")] public int DropBookId;
    }

    public class SpriteOptionRoot
    {
        [XmlAttribute("Id")] public int KeypageId;
        [XmlElement("SpriteOption")] public SpriteEnum SpriteOption = SpriteEnum.Custom;
        [XmlElement("SpritePK")] public string SpritePK = "";
    }

    public class MotionSoundRoot
    {
        [XmlElement("FileNameLose")] public string FileNameLose = "";
        [XmlElement("FileNameWin")] public string FileNameWin = "";
        [XmlElement("IsBaseSoundLose")] public bool IsBaseSoundLose;
        [XmlElement("IsBaseSoundWin")] public bool IsBaseSoundWin;
    }

    public class CustomDiceColorOptionRoot
    {
        [XmlElement("IconId")] public string IconId = "";
        [XmlElement("TextColor")] public ColorRoot TextColor;
    }

    public class EmotionCardColorOptionRoot
    {
        [XmlElement("FrameHSVColor")] public HsvColorRoot FrameHSVColor;
        [XmlElement("FrameColor")] public ColorRoot FrameColor;


        [XmlElement("TextColor")] public ColorRoot TextColor;
    }

    public class EmotionCardOptionRoot
    {
        [XmlElement("CardId")] public List<int> CardId;
        [XmlElement("UsableByBookIds")] public List<LorIdRoot> UsableByBookIds;
        [XmlElement("Code")] public List<string> Code;
        [XmlElement("FloorCode")] public List<string> FloorCode;
        [XmlElement("ColorOptions")] public EmotionCardColorOptionRoot ColorOptions;
    }

    public class EgoCardOptionRoot
    {
        [XmlElement("CardId")] public List<int> CardId;
        [XmlElement("Code")] public List<string> Code;
        [XmlElement("FloorCode")] public List<string> FloorCode;
    }

    public class RewardOptionRoot
    {
        [XmlElement("Books")] public List<ItemQuantityRoot> Books = new List<ItemQuantityRoot>();
        [XmlElement("Cards")] public List<ItemQuantityRoot> Cards = new List<ItemQuantityRoot>();
        [XmlElement("Keypages")] public List<LorIdRoot> Keypages = new List<LorIdRoot>();
        [XmlElement("MessageId")] public string MessageId = "";
        [XmlElement("SingleTimeReward")] public bool SingleTimeReward = true;
    }

    public class ItemQuantityRoot
    {
        [XmlElement("LorId")] public LorIdRoot LorId;
        [XmlAttribute("Quantity")] public int Quantity;
    }

    public class SkinOptionRoot
    {
        [XmlElement("PackageId")] public string PackageId = "";
        [XmlElement("CustomHeight")] public int CustomHeight;

        [XmlElement("MotionSounds")]
        public List<MotionSoundOptionRoot> MotionSounds = new List<MotionSoundOptionRoot>();


        [XmlAttribute("SkinName")] public string SkinName = "";
    }

    public class MotionSoundOptionRoot
    {
        [XmlElement("Motion")] public MotionDetail Motion;
        [XmlElement("MotionSound")] public MotionSoundRoot MotionSound;
    }

    public class StageOptionRoot
    {
        [XmlElement("StageId")] public int StageId;
        [XmlElement("BannedEmotionLevel")] public bool BannedEmotionLevel;
        [XmlElement("StageColorOptions")] public ColorOptionsRoot StageColorOptions;
        [XmlElement("StageRequirements")] public StageRequirementRoot StageRequirements;
        [XmlElement("CustomDiceColorOptions")] public CustomDiceColorOptionRoot CustomDiceColorOptions;
        [XmlElement("StageRewardOptions")] public RewardOptionRoot StageRewardOptions;
        [XmlElement("PreBattleOptions")] public PreBattleOptionRoot PreBattleOptions;
    }

    public class StageRequirementRoot
    {
        [XmlElement("RequiredLibraryLevel")] public int? RequiredLibraryLevel;
        [XmlElement("RequiredStageId")] public List<LorIdRoot> RequiredStageIds = new List<LorIdRoot>();
    }

    public class PreBattleOptionRoot
    {
        [XmlElement("BattleType")] public PreBattleType BattleType;
        [XmlElement("CustomUnits")] public CustomUnitsRoot CustomUnits;
        [XmlElement("SetToggles")] public bool SetToggles;
        [XmlElement("SephirahUnits")] public SephiorahUnitsRoot SephirahUnits;
        [XmlElement("FillWithBaseUnits")] public bool FillWithBaseUnits;
        [XmlElement("OnlySephirah")] public bool OnlySephirah;
        [XmlElement("SephirahLocked")] public bool SephirahLocked;
        [XmlElement("UnlockedSephirah")] public List<SephirahType> UnlockedSephirah = new List<SephirahType>();
    }

    public class SephiorahUnitsRoot
    {
        [XmlElement("Floor")] public SephirahType Floor;
        [XmlElement("SephirahUnit")] public List<SephirahType> SephirahUnit;
    }

    public class CustomUnitsRoot
    {
        [XmlElement("Floor")] public SephirahType Floor;
        [XmlElement("CustomUnit")] public List<UnitModelRoot> CustomUnit;
    }

    public class UnitModelRoot
    {
        [XmlElement("CustomPos")] public XmlVector2 CustomPos;
        [XmlElement("SkinName")] public string SkinName = "";
        [XmlElement("IsMainEnemy")] public bool IsMainEnemy;
        [XmlElement("ForcedEgoOnStart")] public bool ForcedEgoOnStart;
        [XmlElement("AdditionalPassiveId")] public List<LorIdRoot> AdditionalPassiveIds;
        [XmlElement("AdditionalBuff")] public List<string> AdditionalBuffs = new List<string>();

        [XmlElement("LockedEmotion")] public bool LockedEmotion;
        [XmlElement("MaxEmotionLevel")] public int MaxEmotionLevel;
        [XmlElement("AutoPlay")] public bool AutoPlay;


        [XmlAttribute("Id")] public int Id;


        [XmlElement("SummonedOnPlay")] public bool SummonedOnPlay;
        [XmlElement("Name")] public string Name = "";


        [XmlAttribute("PackageId")] public string PackageId = "";


        [XmlElement("UnitNameId")] public int UnitNameId;
    }

    public class CategoryOptionRoot
    {
        [XmlAttribute("CategoryNumber")] public string AdditionalValue = "";
        [XmlElement("BaseIconSpriteId")] public string BaseIconSpriteId = "";
        [XmlElement("CategoryNameId")] public string CategoryNameId = "";
        [XmlElement("CategoryName")] public string CategoryName = "";
        [XmlElement("BookDataColor")] public ColorOptionsRoot BookDataColor;
        [XmlElement("BaseGameCategory")] public UIStoryLine? BaseGameCategory;


        [XmlElement("CredenzaType")] public CredenzaEnum CredenzaType = CredenzaEnum.ModifiedCredenza;
        [XmlElement("PackageId")] public string PackageId = "";
        [XmlElement("Chapter")] public int Chapter = 7;
        [XmlElement("CategoryBooksId")] public List<int> CategoryBooksId = new List<int>();


        [XmlElement("CredenzaBooksId")] public List<int> CredenzaBooksId = new List<int>();


        [XmlElement("CustomIconSpriteId")] public string CustomIconSpriteId = "";
    }

    public class CredenzaOptionRoot
    {
        [XmlElement("CredenzaBooksId")] public List<int> CredenzaBooksId = new List<int>();
        [XmlElement("CustomIconSpriteId")] public string CustomIconSpriteId = "";
        [XmlElement("BaseIconSpriteId")] public string BaseIconSpriteId = "";
        [XmlElement("CredenzaNameId")] public string CredenzaNameId = "";
        [XmlElement("CredenzaName")] public string CredenzaName = "";
        [XmlElement("BookDataColor")] public ColorOptionsRoot BookDataColor;
        [XmlElement("Chapter")] public int Chapter = 7;


        [XmlElement("CredenzaOption")] public CredenzaEnum CredenzaOption = CredenzaEnum.ModifiedCredenza;
    }

    public class CustomFloorOptionRoot
    {
        [XmlElement("FloorCode")] public string FloorCode;
        [XmlElement("IconId")] public string IconId;
        [XmlElement("FloorNameId")] public string FloorNameId;
        [XmlElement("FloorName")] public string FloorName;
        [XmlElement("CustomFloorMap")] public MapModelRoot CustomFloorMap;
        [XmlElement("CustomDiceColorOptions")] public CustomDiceColorOptionRoot CustomDiceColorOptions;


        [XmlElement("PackageId")] public string PackageId;
    }

    public class MapModelRoot
    {
        [XmlElement("IsPlayer")] public bool IsPlayer;
        [XmlElement("OneTurnEgo")] public bool OneTurnEgo;
        [XmlElement("Bgx")] public float Bgx = 0.5f;
        [XmlElement("Bgy")] public float Bgy = 0.5f;
        [XmlElement("Component")] public string Component;
        [XmlElement("Fx")] public float Fx = 0.5f;
        [XmlElement("Fy")] public float Fy = 407.5f / 1080f;
        [XmlElement("UnderX")] public float UnderX = 0.5f;
        [XmlElement("UnderY")] public float UnderY = 0.2777778f;
        [XmlElement("InitBgm")] public bool InitBgm;


        [XmlElement("Stage")] public string Stage;
        [XmlElement("OriginalMapStageId")] public List<LorIdRoot> OriginalMapStageIds;
    }

    public class CardOptionRoot
    {
        [XmlElement("Option")] public CardOption Option = CardOption.Basic;
        [XmlElement("Keywords")] public List<string> Keywords = new List<string>();
        [XmlElement("BookId")] public List<LorIdRoot> BookId = new List<LorIdRoot>();
        [XmlElement("OnlyAllyTargetCard")] public bool OnlyAllyTargetCard;
        [XmlElement("OneSideOnlyCard")] public bool OneSideOnlyCard;
        [XmlElement("IsBaseGameCard")] public bool IsBaseGameCard;
        [XmlElement("CardColorOptions")] public CardColorOptionRoot CardColorOptions;
        [XmlAttribute("Id")] public int CardId;
    }

    public class CardColorOptionRoot
    {
        [XmlElement("LeftFrame")] public string LeftFrame = "";
        [XmlElement("RightFrame")] public string RightFrame = "";
        [XmlElement("FrontFrame")] public string FrontFrame = "";
        [XmlElement("ApplyFrontColor")] public bool ApplyFrontColor;
        [XmlElement("ApplySideFrontColors")] public bool ApplySideFrontColors;
        [XmlElement("CardColor")] public ColorRoot CardColor;
        [XmlElement("CustomIcon")] public string CustomIcon = "";
        [XmlElement("CustomIconColor")] public ColorRoot CustomIconColor;


        [XmlElement("IconColor")] public HsvColorRoot IconColor;


        [XmlElement("UseHSVFilter")] public bool UseHSVFilter = true;
    }

    public class KeypageOptionRoot
    {
        [XmlElement("EquipRangeType")] public EquipRangeType? EquipRangeType;
        [XmlElement("MultiDeckOptions")] public MultiDeckOptionsRoot MultiDeckOptions;
        [XmlElement("BookCustomOptions")] public BookCustomOptionRoot BookCustomOptions;
        [XmlElement("BannedEgoFloorCards")] public bool BannedEgoFloorCards;
        [XmlElement("BannedEmotionCards")] public bool BannedEmotionCards;

        [XmlElement("EveryoneCanEquip")] public bool EveryoneCanEquip;
        [XmlElement("OnlySephirahCanEquip")] public bool OnlySephirahCanEquip;
        [XmlElement("BookIconId")] public string BookIconId = "";
        [XmlElement("IsDeckFixed")] public bool IsDeckFixed;
        [XmlElement("IsMultiDeck")] public bool IsMultiDeck;
        [XmlElement("CanNotEquip")] public bool? CanNotEquip;
        [XmlElement("IsBaseGameKeypage")] public bool IsBaseGameKeypage;

        [XmlElement("TargetableBySpecialCards")]
        public bool TargetableBySpecialCards = true;

        [XmlElement("KeypageColorOptions")] public ColorOptionsRoot KeypageColorOptions;
        [XmlElement("CustomFloorOptions")] public CustomFloorOptionRoot CustomFloorOptions;
        [XmlElement("CustomDiceColorOptions")] public CustomDiceColorOptionRoot CustomDiceColorOptions;


        [XmlElement("Editable")] public bool Editable = true;

        [XmlElement("ForceAggroLastDie")] public bool ForceAggroLastDie;

        [XmlElement("RedirectOnlyWithSlowerSpeed")]
        public bool RedirectOnlyWithSlowerSpeed;

        [XmlElement("EditErrorMessageId")] public string EditErrorMessageId = "";


        [XmlElement("SephirahType")] public SephirahType SephirahType;
        [XmlElement("ForceAggroSpeedDie")] public List<int> ForceAggroSpeedDie;


        [XmlAttribute("Id")] public int KeypageId;
    }

    public class BookCustomOptionRoot
    {
        [XmlElement("CustomFaceData")] public bool CustomFaceData;
        [XmlElement("OriginalSkin")] public string OriginalSkin = "";
        [XmlElement("EgoSkin")] public List<string> EgoSkin;
        [XmlElement("CustomDialogId")] public LorIdRoot CustomDialogId;
        [XmlElement("CustomDialog")] public BattleDialogCharacter CustomDialog;


        [XmlElement("NameTextId")] public int NameTextId;
        [XmlElement("Name")] public string Name = "";


        [XmlElement("OriginalSkinIsBaseGame")] public bool OriginalSkinIsBaseGame;
        [XmlElement("XiaoTaotieAction")] public ActionDetail XiaoTaotieAction = ActionDetail.NONE;
    }
}