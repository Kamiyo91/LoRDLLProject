namespace BigDLL4221.Models
{
    public class SummonedUnitStatModel
    {
        public SummonedUnitStatModel(bool dieAtSceneEndForPlayer = false, bool dieAtSceneEndForNpc = false,
            int reviveAfterScenesPlayer = -1, int reviveAfterScenesNpc = -1, int hpRecoveredWithRevive = 1,
            bool removeFromUIAfterDeath = false, bool useCustomData = true, DamageOptions damageOptions = null)
        {
            DieAtSceneEndForPlayer = dieAtSceneEndForPlayer;
            DieAtSceneEndForNpc = dieAtSceneEndForNpc;
            ReviveCount = 0;
            ReviveAfterScenesPlayer = reviveAfterScenesPlayer;
            ReviveAfterScenesNpc = reviveAfterScenesNpc;
            HpRecoveredWithRevive = hpRecoveredWithRevive;
            RemoveFromUIAfterDeath = removeFromUIAfterDeath;
            UseCustomData = useCustomData;
            DamageOptions = damageOptions;
        }

        public bool DieAtSceneEndForPlayer { get; set; }
        public bool DieAtSceneEndForNpc { get; set; }
        public int ReviveCount { get; set; }
        public int ReviveAfterScenesPlayer { get; set; }
        public int ReviveAfterScenesNpc { get; set; }
        public int HpRecoveredWithRevive { get; set; }
        public bool RemoveFromUIAfterDeath { get; set; }
        public bool UseCustomData { get; set; }
        public DamageOptions DamageOptions { get; set; }
    }
}