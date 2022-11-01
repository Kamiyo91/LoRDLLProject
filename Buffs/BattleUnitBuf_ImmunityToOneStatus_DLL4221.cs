namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_ImmunityToOneStatus_DLL4221 : BattleUnitBuf_BaseBufChanged_DLL4221
    {
        private readonly BattleUnitBuf _immunityBuf;
        private readonly KeywordBuf _immunityType;

        public BattleUnitBuf_ImmunityToOneStatus_DLL4221(BattleUnitBuf immunityBuf = null,
            KeywordBuf immunityType = KeywordBuf.None, ActionDetail actionDetail = ActionDetail.NONE,
            bool infinite = false, bool lastOneScene = true, int lastForXScenes = 0) : base(actionDetail, infinite,
            lastOneScene, lastForXScenes)
        {
            _immunityBuf = immunityBuf;
            _immunityType = immunityType;
        }

        public override bool IsImmune(KeywordBuf buf)
        {
            if (_immunityType == KeywordBuf.None) return base.IsImmune(buf);
            return _immunityType == buf;
        }

        public override bool IsImmune(BattleUnitBuf buf)
        {
            if (_immunityBuf == null) return base.IsImmune(buf);
            return _immunityBuf.GetType() == buf.GetType();
        }
    }
}