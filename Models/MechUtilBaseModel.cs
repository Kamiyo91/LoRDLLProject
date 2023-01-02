using System.Collections.Generic;
using BigDLL4221.Enum;
using CustomMapUtility;
using LOR_XML;
using UnityEngine;

namespace BigDLL4221.Models
{
    public class MechUtilBaseModel
    {
        public MechUtilBaseModel(Dictionary<int, EgoOptions> egoOptions = null, int additionalStartDraw = 0,
            int surviveHp = 0, int recoverToHp = 0, string originalSkinName = "", bool survive = false,
            bool recoverLightOnSurvive = false,
            bool dieOnFightEnd = false, DamageOptions damageOptions = null,
            List<AbnormalityCardDialog> surviveAbDialogList = null,
            AbColorType surviveAbDialogColor = AbColorType.Negative, BattleUnitBuf nearDeathBuffType = null,
            List<PermanentBuffOptions> permanentBuffList = null,
            Dictionary<LorId, PersonalCardOptions> personalCards = null, bool reusableEgo = true,
            bool reviveOnDeath = false, int recoverHpOnRevive = 0,
            List<AbnormalityCardDialog> reviveAbDialogList = null,
            AbColorType reviveAbDialogColor = AbColorType.Negative, LorId firstEgoFormCard = null,
            bool customData = false, Dictionary<LorId, MapModel> egoMaps = null, bool forceRetreatOnRevive = false,
            bool originalSkinIsBaseGame = false, Color? surviveAbDialogCustomColor = null,
            Color? reviveAbDialogCustomColor = null)
        {
            EgoOptions = egoOptions ?? new Dictionary<int, EgoOptions>();
            AdditionalStartDraw = additionalStartDraw;
            ReviveOnDeath = reviveOnDeath;
            SurviveHp = surviveHp;
            RecoverToHp = recoverToHp;
            OriginalSkinName = originalSkinName;
            Survive = survive;
            RecoverLightOnSurvive = recoverLightOnSurvive;
            DieOnFightEnd = dieOnFightEnd;
            DamageOptions = damageOptions;
            SurviveAbDialogList = surviveAbDialogList ?? new List<AbnormalityCardDialog>();
            SurviveAbDialogColor = surviveAbDialogColor;
            NearDeathBuffType = nearDeathBuffType;
            PermanentBuffList = permanentBuffList ?? new List<PermanentBuffOptions>();
            PersonalCards = personalCards ?? new Dictionary<LorId, PersonalCardOptions>();
            EgoPhase = 0;
            ActivatedMap = null;
            ReusableEgo = reusableEgo;
            RecoverHpOnRevive = recoverHpOnRevive;
            ReviveAbDialogList = reviveAbDialogList ?? new List<AbnormalityCardDialog>();
            ReviveAbDialogColor = reviveAbDialogColor;
            FirstEgoFormCard = firstEgoFormCard;
            CustomData = customData;
            EgoMaps = egoMaps ?? new Dictionary<LorId, MapModel>();
            ForceRetreatOnRevive = forceRetreatOnRevive;
            OriginalSkinIsBaseGame = originalSkinIsBaseGame;
            SurviveAbDialogCustomColor = surviveAbDialogCustomColor;
            ReviveAbDialogCustomColor = reviveAbDialogCustomColor;
        }

        public BattleUnitModel Owner { get; set; }
        public LorId ThisPassiveId { get; set; }
        public Dictionary<int, EgoOptions> EgoOptions { get; set; }
        public int AdditionalStartDraw { get; set; }
        public bool ReviveOnDeath { get; set; }
        public bool ForceRetreatOnRevive { get; set; }
        public int RecoverHpOnRevive { get; set; }
        public int SurviveHp { get; set; }
        public int RecoverToHp { get; set; }
        public string OriginalSkinName { get; set; }
        public bool Survive { get; set; }
        public bool RecoverLightOnSurvive { get; set; }
        public bool DieOnFightEnd { get; set; }
        public DamageOptions DamageOptions { get; set; }
        public List<AbnormalityCardDialog> ReviveAbDialogList { get; set; }
        public AbColorType ReviveAbDialogColor { get; set; }
        public Color? ReviveAbDialogCustomColor { get; set; }
        public List<AbnormalityCardDialog> SurviveAbDialogList { get; set; }
        public AbColorType SurviveAbDialogColor { get; set; }
        public Color? SurviveAbDialogCustomColor { get; set; }
        public BattleUnitBuf NearDeathBuffType { get; set; }
        public List<PermanentBuffOptions> PermanentBuffList { get; set; }
        public Dictionary<LorId, PersonalCardOptions> PersonalCards { get; set; }
        public LorId FirstEgoFormCard { get; set; }
        public int EgoPhase { get; set; }
        public MapModel ActivatedMap { get; set; }
        public bool ReusableEgo { get; set; }
        public bool CustomData { get; set; }
        public Dictionary<LorId, MapModel> EgoMaps { get; set; }
        public bool OriginalSkinIsBaseGame { get; set; }
    }

    public class PermanentBuffOptions
    {
        public PermanentBuffOptions(BattleUnitBuf buff = null, bool isActive = true)
        {
            Buff = buff;
            IsActive = isActive;
        }

        public BattleUnitBuf Buff { get; set; }
        public bool IsActive { get; set; }
    }

    public class EgoOptions
    {
        public EgoOptions(BattleUnitBuf egoType = null, string egoSkinName = "",
            bool refreshUI = false, List<LorId> additionalPassiveIds = null,
            MechBuffOptions additionalBuffs = null,
            List<AbnormalityCardDialog> egoAbDialogList = null, AbColorType egoAbColorColor = AbColorType.Negative,
            int duration = 0, bool activeEgoOnDeath = false, bool activeEgoOnSurvive = false,
            List<UnitModel> summonUnitDefaultData = null, List<UnitModel> summonUnitCustomData = null,
            List<LorId> unitsThatDieTogetherByPassive = null, bool removeEgoWhenSolo = false,
            bool deactiveEgoOnBreak = false, int recoverHpOnEgo = 0, int extraMaxHp = 0, int extraMaxStagger = 0,
            int activeEgoOnHpRange = 0, bool activeEgoOnStart = false, LorId assimilationEgoWithMap = null,
            bool isBaseGameSkin = false, Color? egoAbColorCustomColor = null)
        {
            EgoType = egoType;
            EgoActivated = false;
            AdditionalPassiveIds = additionalPassiveIds ?? new List<LorId>();
            AdditionalBuffs = additionalBuffs;
            RefreshUI = refreshUI;
            EgoSkinName = egoSkinName;
            EgoAbDialogList = egoAbDialogList ?? new List<AbnormalityCardDialog>();
            EgoAbColorColor = egoAbColorColor;
            Duration = duration;
            Count = 0;
            ActiveEgoOnDeath = activeEgoOnDeath;
            ActiveEgoOnSurvive = activeEgoOnSurvive;
            SummonUnitDefaultData = summonUnitDefaultData ?? new List<UnitModel>();
            SummonUnitCustomData = summonUnitCustomData ?? new List<UnitModel>();
            UnitsThatDieTogetherByPassive = unitsThatDieTogetherByPassive ?? new List<LorId>();
            RemoveEgoWhenSolo = removeEgoWhenSolo;
            DeactiveEgoOnBreak = deactiveEgoOnBreak;
            RecoverHpOnEgo = recoverHpOnEgo;
            ExtraMaxHp = extraMaxHp;
            ExtraMaxStagger = extraMaxStagger;
            ActiveEgoOnHpRange = activeEgoOnHpRange;
            ActiveEgoOnStart = activeEgoOnStart;
            AssimilationEgoWithMap = assimilationEgoWithMap;
            EgoActive = false;
            IsBaseGameSkin = isBaseGameSkin;
            EgoAbColorCustomColor = egoAbColorCustomColor;
        }

        public bool ActiveEgoOnStart { get; set; }
        public bool ActiveEgoOnDeath { get; set; }
        public bool ActiveEgoOnSurvive { get; set; }
        public int ActiveEgoOnHpRange { get; set; }
        public BattleUnitBuf EgoType { get; set; }
        public bool EgoActivated { get; set; }
        public List<LorId> AdditionalPassiveIds { get; set; }
        public MechBuffOptions AdditionalBuffs { get; set; }
        public bool RefreshUI { get; set; }
        public string EgoSkinName { get; set; }
        public int RecoverHpOnEgo { get; set; }
        public List<AbnormalityCardDialog> EgoAbDialogList { get; set; }
        public AbColorType EgoAbColorColor { get; set; }
        public Color? EgoAbColorCustomColor { get; set; }
        public int Duration { get; set; }
        public int Count { get; set; }
        public List<UnitModel> SummonUnitCustomData { get; set; }
        public List<UnitModel> SummonUnitDefaultData { get; set; }
        public List<LorId> UnitsThatDieTogetherByPassive { get; set; }
        public bool RemoveEgoWhenSolo { get; set; }
        public bool DeactiveEgoOnBreak { get; set; }
        public int ExtraMaxHp { get; set; }
        public int ExtraMaxStagger { get; set; }
        public LorId AssimilationEgoWithMap { get; set; }
        public bool EgoActive { get; set; }
        public bool EgoInDeactiveState { get; set; }
        public bool IsBaseGameSkin { get; set; }
    }

    public class DamageOptions
    {
        public DamageOptions(int lessMassAttackDamage = 0, int lessMassAttackIndividualDamage = 0,
            int lessSpecialRangeAttackDamage = 0, int lessRangedAttackDamage = 0, int lessMeleeAttackDamage = 0,
            int lessHitAttackDamage = 0, int lessPierceAttackDamage = 0, int lessSlashAttackDamage = 0,
            int lessMassAttackBreakDamage = 0, int lessMassAttackIndividualBreakDamage = 0,
            int lessSpecialRangeAttackBreakDamage = 0, int lessRangedAttackBreakDamage = 0,
            int lessMeleeAttackBreakDamage = 0, int lessHitAttackBreakDamage = 0, int lessPierceAttackBreakDamage = 0,
            int lessSlashAttackBreakDamage = 0)
        {
            LessMassAttackDamage = lessMassAttackDamage;
            LessMassAttackIndividualDamage = lessMassAttackIndividualDamage;
            LessSpecialRangeAttackDamage = lessSpecialRangeAttackDamage;
            LessRangedAttackDamage = lessRangedAttackDamage;
            LessMeleeAttackDamage = lessMeleeAttackDamage;
            LessHitAttackDamage = lessHitAttackDamage;
            LessPierceAttackDamage = lessPierceAttackDamage;
            LessSlashAttackDamage = lessSlashAttackDamage;
            LessMassAttackBreakDamage = lessMassAttackBreakDamage;
            LessMassAttackIndividualBreakDamage = lessMassAttackIndividualBreakDamage;
            LessSpecialRangeAttackBreakDamage = lessSpecialRangeAttackBreakDamage;
            LessRangedAttackBreakDamage = lessRangedAttackBreakDamage;
            LessMeleeAttackBreakDamage = lessMeleeAttackBreakDamage;
            LessHitAttackBreakDamage = lessHitAttackBreakDamage;
            LessPierceAttackBreakDamage = lessPierceAttackBreakDamage;
            LessSlashAttackBreakDamage = lessSlashAttackBreakDamage;
        }

        public int LessMassAttackDamage { get; set; }
        public int LessMassAttackIndividualDamage { get; set; }
        public int LessSpecialRangeAttackDamage { get; set; }
        public int LessRangedAttackDamage { get; set; }
        public int LessMeleeAttackDamage { get; set; }
        public int LessHitAttackDamage { get; set; }
        public int LessPierceAttackDamage { get; set; }
        public int LessSlashAttackDamage { get; set; }
        public int LessMassAttackBreakDamage { get; set; }
        public int LessMassAttackIndividualBreakDamage { get; set; }
        public int LessSpecialRangeAttackBreakDamage { get; set; }
        public int LessRangedAttackBreakDamage { get; set; }
        public int LessMeleeAttackBreakDamage { get; set; }
        public int LessHitAttackBreakDamage { get; set; }
        public int LessPierceAttackBreakDamage { get; set; }
        public int LessSlashAttackBreakDamage { get; set; }
    }

    public class PersonalCardOptions
    {
        public PersonalCardOptions(bool egoPersonalCard = false, bool onPlayCard = false, bool expireAfterUse = false,
            int egoPhase = 0, bool activeEgoCard = false)
        {
            ExpireAfterUse = expireAfterUse;
            EgoPersonalCard = egoPersonalCard;
            OnPlayCard = onPlayCard;
            EgoPhase = egoPhase;
            ActiveEgoCard = activeEgoCard;
        }

        public int EgoPhase { get; set; }
        public bool ActiveEgoCard { get; set; }
        public bool ExpireAfterUse { get; set; }
        public bool EgoPersonalCard { get; set; }
        public bool OnPlayCard { get; set; }
    }
}