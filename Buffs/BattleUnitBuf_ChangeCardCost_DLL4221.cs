using BigDLL4221.Utils;

namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_ChangeCardCost_DLL4221 : BattleUnitBuf_BaseBufChanged_DLL4221
    {
        public int AdditionalDraw;
        public int Cost;

        public BattleUnitBuf_ChangeCardCost_DLL4221(int cost = -999, int additionalDraw = 6,
            ActionDetail actionDetail = ActionDetail.NONE, bool infinite = true, bool lastOneScene = false,
            int lastForXScenes = 0) : base(actionDetail, infinite, lastOneScene,
            lastForXScenes)
        {
            Cost = cost;
            AdditionalDraw = additionalDraw;
        }

        public override void OnRoundStartAfter()
        {
            UnitUtil.DrawUntilX(_owner, AdditionalDraw);
        }

        public override int GetCardCostAdder(BattleDiceCardModel card)
        {
            return Cost;
        }
    }
}