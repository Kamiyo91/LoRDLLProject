namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_ImmunityToStatusAlimentUntilRoundEnd_DLL4221 : BattleUnitBuf
    {
        public override bool IsImmune(BufPositiveType posType)
        {
            return posType == BufPositiveType.Negative;
        }

        public override void OnRoundEnd()
        {
            _owner.bufListDetail.RemoveBuf(this);
        }
    }
}