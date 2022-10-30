using System;
using System.Collections.Generic;
using System.Linq;

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

        public static bool HasBuff(this BattleUnitModel owner, Type buffType)
        {
            return owner.bufListDetail.GetActivatedBufList().Exists(x => x.GetType() == buffType);
        }
    }
}