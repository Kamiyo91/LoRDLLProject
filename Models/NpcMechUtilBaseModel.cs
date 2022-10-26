using System;
using System.Collections.Generic;
using BigDLL4221.Enum;
using LOR_XML;

namespace BigDLL4221.Models
{
    public class NpcMechUtilBaseModel : MechUtilBaseModel
    {
        public NpcMechUtilBaseModel(BattleUnitModel owner, LorId thisPassiveId, string saveDataId,
            EgoOptions egoOptions = null, int surviveHp = 0, int recoverToHp = 0, bool survive = false,
            bool recoverLightOnSurvive = false, bool dieOnFightEnd = false,
            List<AbnormalityCardDialog> surviveAbDialogList = null,
            AbColorType surviveAbDialogColor = AbColorType.Negative, BattleUnitBuf nearDeathBuffType = null,
            Dictionary<LorId, PersonalCardOptions> personalCards = null,
            Dictionary<int, MechPhaseOptions> mechOptions = null, int maxCounter = 4,
            bool reloadMassAttackOnLethal = true, Dictionary<LorId, SpecialAttackCardOptions> lorIdEgoMassAttack = null,
            bool massAttackStartCount = false, SpecialCardOption specialCardOptions = null,
            bool onDeathOtherDies = false) : base(owner, thisPassiveId, egoOptions, surviveHp, recoverToHp, survive,
            recoverLightOnSurvive, dieOnFightEnd, surviveAbDialogList, surviveAbDialogColor, nearDeathBuffType,
            personalCards)
        {
            MechOptions = mechOptions ?? new Dictionary<int, MechPhaseOptions>();
            Counter = 0;
            MaxCounter = maxCounter;
            ReloadMassAttackOnLethal = reloadMassAttackOnLethal;
            OneTurnCard = false;
            LorIdEgoMassAttack = lorIdEgoMassAttack ?? new Dictionary<LorId, SpecialAttackCardOptions>();
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

        public Dictionary<LorId, SpecialAttackCardOptions> LorIdEgoMassAttack { get; set; }

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
        public List<LorId> AdditionalPassiveIds;

        public MechPhaseOptions(int speedDieAdder = 0, int mechHp = 0, bool startMassAttack = false,
            bool forceEgo = false, bool setCounterToMax = false, bool changeCardCost = false, int loweredCost = 0,
            int maxCost = 4, List<LorId> additionalPassiveIds = null, List<BattleUnitBuf> buffs = null,
            Dictionary<KeywordBuf, int> oneRoundBuffs = null, List<BattleUnitBuf> eachRoundStartBuffs = null,
            Dictionary<KeywordBuf, int> eachRoundStartKeywordBuffs = null, bool removeOtherUnits = false,
            List<UnitModel> summonUnit = null, List<UnitModel> summonPlayerUnit = null,
            List<string> soundEffectPath = null)
        {
            AdditionalPassiveIds = additionalPassiveIds ?? new List<LorId>();
            MechHp = mechHp;
            StartMassAttack = startMassAttack;
            ForceEgo = forceEgo;
            SetCounterToMax = setCounterToMax;
            ChangeCardCost = changeCardCost;
            LoweredCost = loweredCost;
            MaxCost = maxCost;
            SoundEffectPath = soundEffectPath ?? new List<string>();
            Buffs = buffs ?? new List<BattleUnitBuf>();
            OneRoundBuffs = oneRoundBuffs ?? new Dictionary<KeywordBuf, int>();
            SummonUnit = summonUnit ?? new List<UnitModel>();
            SummonPlayerUnit = summonPlayerUnit ?? new List<UnitModel>();
            RemoveOtherUnits = removeOtherUnits;
            EachRoundStartBuffs = eachRoundStartBuffs ?? new List<BattleUnitBuf>();
            EachRoundStartKeywordBuffs = eachRoundStartKeywordBuffs ?? new Dictionary<KeywordBuf, int>();
            SpeedDieAdder = speedDieAdder;
        }

        public int MechHp { get; set; }
        public bool StartMassAttack { get; set; }
        public bool ForceEgo { get; set; }
        public bool SetCounterToMax { get; set; }
        public bool ChangeCardCost { get; set; }
        public int LoweredCost { get; set; }
        public int MaxCost { get; set; }
        public List<string> SoundEffectPath { get; set; }
        public List<BattleUnitBuf> Buffs { get; set; }
        public Dictionary<KeywordBuf, int> OneRoundBuffs { get; set; }
        public List<UnitModel> SummonUnit { get; set; }
        public List<UnitModel> SummonPlayerUnit { get; set; }
        public bool RemoveOtherUnits { get; set; }
        public List<BattleUnitBuf> EachRoundStartBuffs { get; set; }
        public Dictionary<KeywordBuf, int> EachRoundStartKeywordBuffs { get; set; }
        public int SpeedDieAdder { get; set; }
    }

    public class SpecialAttackCardOptions
    {
        public SpecialAttackCardOptions(bool ignoreSephirah = false)
        {
            IgnoreSephirah = ignoreSephirah;
        }

        public bool IgnoreSephirah { get; set; }
    }
}