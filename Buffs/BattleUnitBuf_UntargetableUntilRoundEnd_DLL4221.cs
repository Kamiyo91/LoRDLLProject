namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_UntargetableUntilRoundEnd_DLL4221 : BattleUnitBuf
    {
        public bool CantMove = true;

        public override bool IsTargetable()
        {
            return false;
        }

        public override int SpeedDiceBreakedAdder()
        {
            return CantMove ? 10 : 0;
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