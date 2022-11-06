using System.Collections.Generic;

namespace BigDLL4221.Models
{
    public class SummonedUnitStatModel
    {
        public SummonedUnitStatModel(bool dieAtSceneEndForPlayer = false, bool dieAtSceneEndForNpc = false,
            int reviveAfterScenesPlayer = -1, int reviveAfterScenesNpc = -1, bool useCustomData = true,
            DamageOptions damageOptions = null, int maxCounter = -1, List<LorId> massAttackCards = null,
            bool ignoreSephirah = false, bool aimAtTheSlowestDie = false, int loweredCardCost = 0, int maxCardCost = 0,
            int hpRecoveredWithRevive = 0, int additionalSpeedDie = 0, EgoOptions egoOptions = null,
            string originalSkinName = "")
        {
            DieAtSceneEndForPlayer = dieAtSceneEndForPlayer;
            DieAtSceneEndForNpc = dieAtSceneEndForNpc;
            ReviveCount = 0;
            ReviveAfterScenesPlayer = reviveAfterScenesPlayer;
            ReviveAfterScenesNpc = reviveAfterScenesNpc;
            UseCustomData = useCustomData;
            DamageOptions = damageOptions;
            Counter = 0;
            MaxCounter = maxCounter;
            MassAttackCards = massAttackCards ?? new List<LorId>();
            IgnoreSephirah = ignoreSephirah;
            AimAtTheSlowestDie = aimAtTheSlowestDie;
            OneTurnCard = false;
            LoweredCardCost = loweredCardCost;
            MaxCardCost = maxCardCost;
            HpRecoveredWithRevive = hpRecoveredWithRevive;
            AdditionalSpeedDie = additionalSpeedDie;
            EgoActivated = false;
            EgoOptions = egoOptions;
            OriginalSkinName = originalSkinName;
        }

        public string OriginalSkinName { get; set; }
        public bool DieAtSceneEndForPlayer { get; set; }
        public bool DieAtSceneEndForNpc { get; set; }
        public int ReviveCount { get; set; }
        public int ReviveAfterScenesPlayer { get; set; }
        public int ReviveAfterScenesNpc { get; set; }
        public int HpRecoveredWithRevive { get; set; }
        public bool RemoveFromUIAfterDeath { get; set; }
        public bool UseCustomData { get; set; }
        public int Counter { get; set; }
        public int MaxCounter { get; set; }
        public bool IgnoreSephirah { get; set; }
        public bool AimAtTheSlowestDie { get; set; }
        public bool OneTurnCard { get; set; }
        public int LoweredCardCost { get; set; }
        public int MaxCardCost { get; set; }
        public int AdditionalSpeedDie { get; set; }
        public List<LorId> MassAttackCards { get; set; }
        public DamageOptions DamageOptions { get; set; }
        public bool EgoActivated { get; set; }
        public EgoOptions EgoOptions { get; set; }
    }

    public class SummonedUnitStatModelLinked : SummonedUnitStatModel
    {
        public SummonedUnitStatModelLinked(PassiveAbilityBase linkedCharByPassive, int mainCharHpForRevive = 0,
            bool lowerOrHigherRange = false, bool dieAtSceneEndForPlayer = false, bool dieAtSceneEndForNpc = false,
            int reviveAfterScenesPlayer = -1, int reviveAfterScenesNpc = -1, bool useCustomData = true,
            int maxCounter = -1, List<LorId> massAttackCards = null, bool ignoreSephirah = false,
            bool aimAtTheSlowestDie = false,
            DamageOptions damageOptions = null, int loweredCardCost = 0, int maxCardCost = 0,
            int hpRecoveredWithRevive = 0, int additionalSpeedDie = 0, EgoOptions egoOptions = null,
            string originalSkinName = "") : base(
            dieAtSceneEndForPlayer, dieAtSceneEndForNpc,
            reviveAfterScenesPlayer, reviveAfterScenesNpc,
            useCustomData, damageOptions, maxCounter, massAttackCards, ignoreSephirah, aimAtTheSlowestDie,
            loweredCardCost, maxCardCost, hpRecoveredWithRevive, additionalSpeedDie, egoOptions, originalSkinName)
        {
            LinkedCharByPassive = linkedCharByPassive;
            MainCharHpForRevive = mainCharHpForRevive;
            LowerOrHigherRange = lowerOrHigherRange;
        }

        public PassiveAbilityBase LinkedCharByPassive { get; set; }
        public int MainCharHpForRevive { get; set; }
        public bool LowerOrHigherRange { get; set; }
    }
}