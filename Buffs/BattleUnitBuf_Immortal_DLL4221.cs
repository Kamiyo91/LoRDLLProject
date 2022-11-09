namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_Immortal_DLL4221 : BattleUnitBuf_BaseBufChanged_DLL4221
    {
        private readonly bool _canRecoverBp;
        private readonly bool _canRecoverHp;
        private readonly bool _isImmortal;
        private readonly bool _isImmortalBp;
        private readonly bool _isImmortalHp;

        public BattleUnitBuf_Immortal_DLL4221(bool isImmortalHp = true, bool isImmortalBp = false,
            bool canRecoverHp = false, bool canRecoverBp = true, bool isImmortal = true,
            ActionDetail actionDetail = ActionDetail.NONE,
            bool infinite = false, bool lastOneScene = true, int lastForXScenes = 0) : base(actionDetail, infinite,
            lastOneScene,
            lastForXScenes)
        {
            _isImmortalHp = isImmortalHp;
            _isImmortalBp = isImmortalBp;
            _canRecoverHp = canRecoverHp;
            _canRecoverBp = canRecoverBp;
            _isImmortal = isImmortal;
        }

        public override bool IsImmortal()
        {
            return _isImmortal;
        }

        public override bool IsInvincibleHp(BattleUnitModel attacker)
        {
            return _isImmortalHp;
        }

        public override bool IsInvincibleBp(BattleUnitModel attacker)
        {
            return _isImmortalBp;
        }

        public override bool CanRecoverHp(int amount)
        {
            return _canRecoverHp;
        }

        public override bool CanRecoverBreak(int amount)
        {
            return _canRecoverBp;
        }
    }
}