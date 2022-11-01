namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_Uncontrollable_DLL4221 : BattleUnitBuf_BaseBufChanged_DLL4221
    {
        public BattleUnitBuf_Uncontrollable_DLL4221(ActionDetail actionDetail = ActionDetail.NONE,
            bool infinite = false, bool lastOneScene = true, int lastForXScenes = 0) : base(actionDetail, infinite,
            lastOneScene, lastForXScenes)
        {
        }

        public override bool IsControllable => false;
    }
}