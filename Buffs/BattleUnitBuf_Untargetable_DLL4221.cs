namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_Untargetable_DLL4221 : BattleUnitBuf
    {
        public override bool IsTargetable()
        {
            return false;
        }

        public override void OnRoundEnd()
        {
            _owner.bufListDetail.RemoveBuf(this);
        }

        public override bool IsImmortal()
        {
            return true;
        }
    }
}