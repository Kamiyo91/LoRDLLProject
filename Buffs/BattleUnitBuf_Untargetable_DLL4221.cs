namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_Untargetable_DLL4221 : BattleUnitBuf_BaseBufChanged_DLL4221
    {
        private readonly bool _cantMove;

        public BattleUnitBuf_Untargetable_DLL4221(bool cantMove = true, ActionDetail actionDetail = ActionDetail.NONE,
            bool infinite = false, bool lastOneScene = true, int lastForXScenes = 0) : base(actionDetail, infinite,
            lastOneScene,
            lastForXScenes)
        {
            _cantMove = cantMove;
        }

        public override bool IsTargetable()
        {
            return false;
        }

        public override int SpeedDiceBreakedAdder()
        {
            return _cantMove ? 10 : 0;
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