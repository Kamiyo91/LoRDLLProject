namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_ImmunityToStatusAlimentType_DLL4221 : BattleUnitBuf_BaseBufChanged_DLL4221
    {
        private readonly BufPositiveType _bufType;

        public BattleUnitBuf_ImmunityToStatusAlimentType_DLL4221(BufPositiveType bufType = BufPositiveType.Negative,
            ActionDetail actionDetail = ActionDetail.NONE, bool infinite = false, bool lastOneScene = true,
            int lastForXScenes = 0) : base(actionDetail, infinite, lastOneScene,
            lastForXScenes)
        {
            _bufType = bufType;
        }

        public override bool IsImmune(BufPositiveType posType)
        {
            if (_bufType == BufPositiveType.None) return base.IsImmune(posType);
            return posType == _bufType;
        }
    }
}