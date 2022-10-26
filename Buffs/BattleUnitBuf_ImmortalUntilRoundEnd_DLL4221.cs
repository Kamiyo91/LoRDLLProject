namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_ImmortalUntilRoundEnd_DLL4221 : BattleUnitBuf
    {
        public override bool IsImmortal()
        {
            return true;
        }

        public override bool IsInvincibleHp(BattleUnitModel attacker)
        {
            return true;
        }

        public override void OnRoundEnd()
        {
            _owner.bufListDetail.RemoveBuf(this);
        }

        public override bool CanRecoverHp(int amount)
        {
            return false;
        }
    }
}