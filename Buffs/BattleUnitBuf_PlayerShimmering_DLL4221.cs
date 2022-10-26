using BigDLL4221.Utils;

namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_PlayerShimmering_DLL4221 : BattleUnitBuf
    {
        public override void OnRoundStartAfter()
        {
            UnitUtil.DrawUntilX(_owner, 6);
        }

        public override int GetCardCostAdder(BattleDiceCardModel card)
        {
            return -999;
        }
    }
}