using System;
using System.Collections.Generic;
using BigDLL4221.Enum;
using LOR_XML;

namespace BigDLL4221.Models
{
    public class NpcMechUtilBaseModel : MechUtilBaseModel
    {
        public NpcMechUtilBaseModel(string saveDataId,
            Dictionary<int, EgoOptions> egoOptions = null,int additionalStartDraw = 0, int surviveHp = 0, int recoverToHp = 0,
            string originalSkinName = "", bool survive = false,
            bool recoverLightOnSurvive = false, bool dieOnFightEnd = false, DamageOptions damageOptions = null,
            List<AbnormalityCardDialog> surviveAbDialogList = null,
            AbColorType surviveAbDialogColor = AbColorType.Negative, BattleUnitBuf nearDeathBuffType = null,
            List<BattleUnitBuf> permanentBuffList = null,
            Dictionary<LorId, PersonalCardOptions> personalCards = null,
            Dictionary<int, MechPhaseOptions> mechOptions = null, int maxCounter = 4,
            bool reloadMassAttackOnLethal = true,
            bool massAttackStartCount = false, SpecialCardOption specialCardOptions = null,
            bool onDeathOtherDies = false) : base(egoOptions, additionalStartDraw,surviveHp, recoverToHp, originalSkinName, survive,
            recoverLightOnSurvive, dieOnFightEnd, damageOptions, surviveAbDialogList, surviveAbDialogColor,
            nearDeathBuffType,
            permanentBuffList,
            personalCards)
        {
            MechOptions = mechOptions ?? new Dictionary<int, MechPhaseOptions>();
            Counter = 0;
            MaxCounter = maxCounter;
            ReloadMassAttackOnLethal = reloadMassAttackOnLethal;
            OneTurnCard = false;
            MassAttackStartCount = massAttackStartCount;
            SpecialCardOptions = specialCardOptions;
            Phase = 0;
            PhaseChanging = false;
            OnDeathOtherDies = onDeathOtherDies;
            SaveDataId = saveDataId;
        }

        public Dictionary<int, MechPhaseOptions> MechOptions { get; set; }
        public int Counter { get; set; }
        public int MaxCounter { get; set; }
        public bool ReloadMassAttackOnLethal { get; set; }
        public bool OneTurnCard { get; set; }
        public bool MassAttackStartCount { get; set; }
        public SpecialCardOption SpecialCardOptions { get; set; }
        public int Phase { get; set; }
        public bool PhaseChanging { get; set; }
        public bool OnDeathOtherDies { get; set; }
        public string SaveDataId { get; set; }
    }

    public class SpecialCardOption
    {
        public SpecialCardOption(int specialCardCost = 0, Type specialBufType = null,
            Dictionary<KeywordBuf, int> buffs = null)
        {
            SpecialCardCost = specialCardCost;
            SpecialBufType = specialBufType;
            Buffs = buffs ?? new Dictionary<KeywordBuf, int>();
        }

        public int SpecialCardCost { get; set; }
        public Type SpecialBufType { get; set; }
        public Dictionary<KeywordBuf, int> Buffs { get; set; }
    }

    public class MechPhaseOptions
    {
        public MechPhaseOptions(int speedDieAdder = 0, int mechHp = 0,
            List<SpecialAttackCardOptions> egoMassAttackCardsOptions = null, bool? startMassAttack = null,
            bool forceEgo = false, bool hasCustomMap = false, bool setCounterToMax = false,
            bool alwaysAimSlowestTargetDie = false, bool changeCardCost = false,
            int loweredCost = 0,
            int maxCost = 4, List<LorId> additionalPassiveByIds = null, List<LorId> removePassiveByIds = null,
            MechBuffOptions buffOptions = null,
            bool removeOtherUnits = false,
            List<UnitModel> summonUnit = null, List<UnitModel> summonPlayerUnit = null,
            List<string> soundEffectPath = null, int extraLightRecoverEachScene = 0, int extraDrawEachScene = 0,
            int extraRecoverHp = 0, int extraRecoverStagger = 0, int mapOrderIndex = 0,
            DamageOptions damageOptions = null)
        {
            AdditionalPassiveByIds = additionalPassiveByIds ?? new List<LorId>();
            RemovePassiveByIds = removePassiveByIds ?? new List<LorId>();
            MechHp = mechHp;
            EgoMassAttackCardsOptions = egoMassAttackCardsOptions ?? new List<SpecialAttackCardOptions>();
            StartMassAttack = startMassAttack;
            ForceEgo = forceEgo;
            HasCustomMap = hasCustomMap;
            SetCounterToMax = setCounterToMax;
            AlwaysAimSlowestTargetDie = alwaysAimSlowestTargetDie;
            ChangeCardCost = changeCardCost;
            LoweredCost = -loweredCost;
            MaxCost = maxCost;
            SoundEffectPath = soundEffectPath ?? new List<string>();
            MechBuffOptions = buffOptions;
            SummonUnit = summonUnit ?? new List<UnitModel>();
            SummonPlayerUnit = summonPlayerUnit ?? new List<UnitModel>();
            RemoveOtherUnits = removeOtherUnits;
            SpeedDieAdder = speedDieAdder;
            ExtraLightRecoverEachScene = extraLightRecoverEachScene;
            ExtraDrawEachScene = extraDrawEachScene;
            ExtraRecoverHp = extraRecoverHp;
            ExtraRecoverStagger = extraRecoverStagger;
            MapOrderIndex = mapOrderIndex;
            DamageOptions = damageOptions;
        }

        public int MechHp { get; set; }
        public List<SpecialAttackCardOptions> EgoMassAttackCardsOptions { get; set; }
        public bool? StartMassAttack { get; set; }
        public bool ForceEgo { get; set; }
        public bool HasCustomMap { get; set; }
        public bool SetCounterToMax { get; set; }
        public bool AlwaysAimSlowestTargetDie { get; set; }
        public bool ChangeCardCost { get; set; }
        public int LoweredCost { get; set; }
        public int MaxCost { get; set; }
        public List<string> SoundEffectPath { get; set; }
        public MechBuffOptions MechBuffOptions { get; set; }
        public List<LorId> AdditionalPassiveByIds { get; set; }
        public List<LorId> RemovePassiveByIds { get; set; }
        public List<UnitModel> SummonUnit { get; set; }
        public List<UnitModel> SummonPlayerUnit { get; set; }
        public bool RemoveOtherUnits { get; set; }
        public int SpeedDieAdder { get; set; }
        public int ExtraLightRecoverEachScene { get; set; }
        public int ExtraDrawEachScene { get; set; }
        public int ExtraRecoverHp { get; set; }
        public int ExtraRecoverStagger { get; set; }
        public int MapOrderIndex { get; set; }
        public DamageOptions DamageOptions { get; set; }
    }

    public class MechBuffOptions
    {
        public MechBuffOptions(List<BattleUnitBuf> buffs = null, Dictionary<KeywordBuf, int> oneRoundBuffs = null,
            List<BattleUnitBuf> eachRoundStartBuffs = null
            , Dictionary<KeywordBuf, int> eachRoundStartKeywordBuffs = null,
            List<BattleUnitBuf> eachRoundStartBuffsNotAlone = null,
            Dictionary<KeywordBuf, int> eachRoundStartKeywordBuffsNotAlone = null,
            List<BattleUnitBuf> eachRoundStartBuffsAlone = null,
            Dictionary<KeywordBuf, int> eachRoundStartKeywordBuffsAlone = null,
            List<BattleUnitBuf> eachRoundStartBuffsAloneCountSupportChar = null,
            Dictionary<KeywordBuf, int> eachRoundStartKeywordBuffsAloneCountSupportChar = null,
            List<BattleUnitBuf> eachRoundStartBuffsNotAloneCountSupportChar = null,
            Dictionary<KeywordBuf, int> eachRoundStartKeywordBuffsNotAloneCountSupportChar = null)
        {
            Buffs = buffs ?? new List<BattleUnitBuf>();
            OneRoundBuffs = oneRoundBuffs ?? new Dictionary<KeywordBuf, int>();
            EachRoundStartBuffs = eachRoundStartBuffs ?? new List<BattleUnitBuf>();
            EachRoundStartKeywordBuffs = eachRoundStartKeywordBuffs ?? new Dictionary<KeywordBuf, int>();
            EachRoundStartBuffsNotAlone = eachRoundStartBuffsNotAlone ?? new List<BattleUnitBuf>();
            EachRoundStartKeywordBuffsNotAlone =
                eachRoundStartKeywordBuffsNotAlone ?? new Dictionary<KeywordBuf, int>();
            EachRoundStartBuffsAlone = eachRoundStartBuffsAlone ?? new List<BattleUnitBuf>();
            EachRoundStartKeywordBuffsAlone = eachRoundStartKeywordBuffsAlone ?? new Dictionary<KeywordBuf, int>();
            EachRoundStartBuffsNotAloneCountSupportChar =
                eachRoundStartBuffsNotAloneCountSupportChar ?? new List<BattleUnitBuf>();
            EachRoundStartKeywordBuffsNotAloneCountSupportChar =
                eachRoundStartKeywordBuffsNotAloneCountSupportChar ?? new Dictionary<KeywordBuf, int>();
            EachRoundStartBuffsAloneCountSupportChar =
                eachRoundStartBuffsAloneCountSupportChar ?? new List<BattleUnitBuf>();
            EachRoundStartKeywordBuffsAloneCountSupportChar = eachRoundStartKeywordBuffsAloneCountSupportChar ??
                                                              new Dictionary<KeywordBuf, int>();
        }

        public List<BattleUnitBuf> Buffs { get; set; }
        public Dictionary<KeywordBuf, int> OneRoundBuffs { get; set; }
        public List<BattleUnitBuf> EachRoundStartBuffs { get; set; }
        public Dictionary<KeywordBuf, int> EachRoundStartKeywordBuffs { get; set; }
        public List<BattleUnitBuf> EachRoundStartBuffsNotAlone { get; set; }
        public Dictionary<KeywordBuf, int> EachRoundStartKeywordBuffsNotAlone { get; set; }
        public List<BattleUnitBuf> EachRoundStartBuffsAlone { get; set; }
        public Dictionary<KeywordBuf, int> EachRoundStartKeywordBuffsAlone { get; set; }
        public List<BattleUnitBuf> EachRoundStartBuffsNotAloneCountSupportChar { get; set; }
        public Dictionary<KeywordBuf, int> EachRoundStartKeywordBuffsNotAloneCountSupportChar { get; set; }
        public List<BattleUnitBuf> EachRoundStartBuffsAloneCountSupportChar { get; set; }
        public Dictionary<KeywordBuf, int> EachRoundStartKeywordBuffsAloneCountSupportChar { get; set; }
    }

    public class SpecialAttackCardOptions
    {
        public SpecialAttackCardOptions(LorId cardId, bool ignoreSephirah = false)
        {
            CardId = cardId;
            IgnoreSephirah = ignoreSephirah;
        }

        public LorId CardId { get; set; }
        public bool IgnoreSephirah { get; set; }
    }
}