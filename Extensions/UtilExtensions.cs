using System;
using System.Linq;
using BigDLL4221.Passives;

namespace BigDLL4221.Extensions
{
    public static class UtilExtensions
    {
        public static T GetActivePassive<T>(this BattleUnitModel owner) where T : PassiveAbilityBase
        {
            return (T)owner.passiveDetail.PassiveList.FirstOrDefault(x => x is T);
        }

        public static T GetActiveBuff<T>(this BattleUnitModel owner) where T : BattleUnitBuf
        {
            return (T)owner.bufListDetail.GetActivatedBufList().FirstOrDefault(x => x is T);
        }

        public static bool HasPassive(this BattleUnitModel owner, Type passiveType, out PassiveAbilityBase passive)
        {
            passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.GetType() == passiveType);
            return passive != null;
        }

        public static void RemovePassive(this BattleUnitModel owner, LorId passiveId)
        {
            var passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.id == passiveId);
            owner.passiveDetail.PassiveList.Remove(passive);
        }

        public static void DestroyPassive(this BattleUnitModel owner, LorId passiveId)
        {
            var passive = owner.passiveDetail.PassiveList.FirstOrDefault(x => x.id == passiveId && !x.destroyed);
            if (passive != null) passive.destroyed = true;
        }

        public static bool HasBuff(this BattleUnitModel owner, Type buffType, out BattleUnitBuf buf)
        {
            buf = owner.bufListDetail.GetActivatedBufList().FirstOrDefault(x => x.GetType() == buffType);
            return buf != null;
        }

        public static bool HasPassivePlayerMech(this BattleUnitModel owner,
            out PassiveAbility_PlayerMechBase_DLL4221 passive)
        {
            passive = null;
            var basePassive =
                owner.passiveDetail.PassiveList.FirstOrDefault(x =>
                    x.GetType() == typeof(PassiveAbility_PlayerMechBase_DLL4221));
            if (basePassive != null) passive = (PassiveAbility_PlayerMechBase_DLL4221)basePassive;
            return passive != null;
        }

        public static bool HasPassiveNpcMech(this BattleUnitModel owner,
            out PassiveAbility_NpcMechBase_DLL4221 passive)
        {
            passive = null;
            var basePassive =
                owner.passiveDetail.PassiveList.FirstOrDefault(x =>
                    x.GetType() == typeof(PassiveAbility_NpcMechBase_DLL4221));
            if (basePassive != null) passive = (PassiveAbility_NpcMechBase_DLL4221)basePassive;
            return passive != null;
        }

        public static bool ForceEgoPlayer(this BattleUnitModel owner, int egoPhase)
        {
            var basePassive =
                owner.passiveDetail.PassiveList.FirstOrDefault(x =>
                    x.GetType() == typeof(PassiveAbility_PlayerMechBase_DLL4221));
            if (basePassive == null) return false;
            var passive = (PassiveAbility_PlayerMechBase_DLL4221)basePassive;
            passive.ForcedEgo(egoPhase);
            return true;
        }
    }
}