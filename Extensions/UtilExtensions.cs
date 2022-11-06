using System;
using System.Linq;
using BigDLL4221.Passives;

namespace BigDLL4221.Extensions
{
    public static class UtilExtensions
    {
        public static T GetActivePassive<T>(this BattleUnitModel owner) where T : PassiveAbilityBase
        {
            return (T)owner.passiveDetail.PassiveList.FirstOrDefault(x => x is T && !x.destroyed);
        }

        public static T GetActiveBuff<T>(this BattleUnitModel owner) where T : BattleUnitBuf
        {
            return (T)owner.bufListDetail.GetActivatedBufList().FirstOrDefault(x => x is T && !x.IsDestroyed());
        }

        public static bool HasPassive(this BattleUnitModel owner, Type passiveType, out PassiveAbilityBase passive)
        {
            passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.GetType() == passiveType && !x.destroyed);
            return passive != null;
        }

        public static void RemovePassive(this BattleUnitModel owner, LorId passiveId)
        {
            var passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.id == passiveId);
            owner.passiveDetail.PassiveList.Remove(passive);
        }

        public static bool IsSupportCharCheck(this BattleUnitModel owner)
        {
            return owner.passiveDetail.PassiveList.Exists(x => x is PassiveAbility_SupportChar_DLL4221);
        }

        public static void DestroyPassive(this BattleUnitModel owner, LorId passiveId)
        {
            var passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.id == passiveId && !x.destroyed);
            if (passive != null) passive.destroyed = true;
        }

        public static bool HasBuff(this BattleUnitModel owner, Type buffType, out BattleUnitBuf buf)
        {
            buf = owner.bufListDetail.GetActivatedBufList()
                .FirstOrDefault(x => x.GetType() == buffType && !x.IsDestroyed());
            return buf != null;
        }

        public static bool HasPassivePlayerMech(this BattleUnitModel owner,
            out PassiveAbility_PlayerMechBase_DLL4221 passive)
        {
            passive = null;
            if (owner.passiveDetail.PassiveList.Find(x => x is PassiveAbility_PlayerMechBase_DLL4221 && !x.destroyed) is
                PassiveAbility_PlayerMechBase_DLL4221 outPassive)
                passive = outPassive;
            return passive != null;
        }

        public static bool HasPassiveNpcMech(this BattleUnitModel owner,
            out PassiveAbility_NpcMechBase_DLL4221 passive)
        {
            passive = null;
            if (owner.passiveDetail.PassiveList.Find(x => x is PassiveAbility_NpcMechBase_DLL4221 && !x.destroyed) is
                PassiveAbility_NpcMechBase_DLL4221 outPassive)
                passive = outPassive;
            return passive != null;
        }

        public static bool ForceEgoPlayer(this BattleUnitModel owner, int egoPhase)
        {
            if (!(owner.passiveDetail.PassiveList.Find(x => x is PassiveAbility_PlayerMechBase_DLL4221 && !x.destroyed)
                    is
                    PassiveAbility_PlayerMechBase_DLL4221 passive)) return false;
            passive.ForcedEgo(egoPhase);
            return true;
        }
    }
}