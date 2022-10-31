using UnityEngine;

namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_BaseBufChanged_DLL4221 : BattleUnitBuf
    {
        public virtual int MinStack => 0;
        public virtual int MaxStack => 25;
        public virtual bool DestroyedAt0Stack => false;

        public override void OnAddBuf(int addedStack)
        {
            stack += addedStack;
            stack = Mathf.Clamp(stack, MinStack, MaxStack);
            if (DestroyedAt0Stack && stack == 0) _owner.bufListDetail.RemoveBuf(this);
        }
    }
}