namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_CannotActUnitUntilRoundEnd_DLL4221 : BattleUnitBuf
    {
        private int _breakedDice;

        public override bool IsTargetable()
        {
            return false;
        }

        public override void OnRoundEnd()
        {
            _owner.bufListDetail.RemoveBuf(this);
        }

        public override void OnRollSpeedDice()
        {
            _breakedDice = _owner.view.speedDiceSetterUI.SpeedDicesCount;
            for (var i = 0; i < _breakedDice; i++)
            {
                _owner.speedDiceResult[i].value = 0;
                _owner.speedDiceResult[i].breaked = true;
                _owner.view.speedDiceSetterUI.GetSpeedDiceByIndex(i).BreakDice(true, true);
            }
        }

        public override int SpeedDiceBreakedAdder()
        {
            return _breakedDice;
        }

        public override void OnRoundStart()
        {
            _owner.turnState = BattleUnitTurnState.BREAK;
        }
    }
}