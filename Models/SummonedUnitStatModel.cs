using System.Collections.Generic;

namespace BigDLL4221.Models
{
    public class SummonedUnitStatModel
    {
        public SummonedUnitStatModel(bool dieAtSceneEndForPlayer = false, bool dieAtSceneEndForNpc = false,
            int reviveAfterScenesPlayer = -1, int reviveAfterScenesNpc = -1, bool useCustomData = true,
            DamageOptions damageOptions = null, int maxCounter = 0, List<LorId> massAttackCards = null,
            bool ignoreSephirah = false, bool aimAtTheSlowestDie = false)
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
        }

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
        public List<LorId> MassAttackCards { get; set; }
        public DamageOptions DamageOptions { get; set; }
    }

    public class SummonedUnitStatModelLinked : SummonedUnitStatModel
    {
        public SummonedUnitStatModelLinked(PassiveAbilityBase linkedCharByPassive, int mainCharHpForRevive = 0,
            bool lowerOrHigherRange = false, bool dieAtSceneEndForPlayer = false, bool dieAtSceneEndForNpc = false,
            int reviveAfterScenesPlayer = -1, int reviveAfterScenesNpc = -1, bool useCustomData = true,
            int maxCounter = 0, List<LorId> massAttackCards = null, bool ignoreSephirah = false,
            bool aimAtTheSlowestDie = false,
            DamageOptions damageOptions = null) : base(
            dieAtSceneEndForPlayer, dieAtSceneEndForNpc,
            reviveAfterScenesPlayer, reviveAfterScenesNpc,
            useCustomData, damageOptions, maxCounter, massAttackCards, ignoreSephirah, aimAtTheSlowestDie)
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