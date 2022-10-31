using BigDLL4221.Utils;

namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_PlayerShimmering_DLL4221 : BattleUnitBuf
    {
        public int AdditionalDraw;
        public int LessCost;

        public BattleUnitBuf_PlayerShimmering_DLL4221(int lessCost = 999, int additionalDraw = 6)
        {
            LessCost = lessCost;
            AdditionalDraw = additionalDraw;
        }

        public override void OnRoundStartAfter()
        {
            UnitUtil.DrawUntilX(_owner, AdditionalDraw);
        }

        public override int GetCardCostAdder(BattleDiceCardModel card)
        {
            return -LessCost;
        }
    }
}