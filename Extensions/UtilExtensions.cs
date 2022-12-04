using System;
using System.Linq;
using BigDLL4221.Models;
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

        public static void SetPassiveCombatLog(this BattleUnitModel owner, PassiveAbilityBase passive)
        {
            var battleCardResultLog = owner.battleCardResultLog;
            battleCardResultLog?.SetPassiveAbility(passive);
        }

        //Not Working
        //public static void SetBuffCombatLog(this BattleUnitModel owner, BattleUnitBuf buf)
        //{
        //    var battleCardResultLog = owner.battleCardResultLog;
        //    battleCardResultLog?.SetNewBufs(buf);
        //}

        public static bool GetActivatedCustomEmotionCard(this BattleUnitModel owner, string packageId, int id,
            out EmotionCardXmlExtension xmlInfo)
        {
            var cards = owner.emotionDetail.GetSelectedCardList().Where(x => x.XmlInfo is EmotionCardXmlExtension)
                .Select(x => x.XmlInfo as EmotionCardXmlExtension);
            xmlInfo = cards.FirstOrDefault(x => x != null && x.LorId == new LorId(packageId, id));
            return xmlInfo != null;
        }

        public static bool GetEmotionCard(string packageId, int id, out EmotionCardXmlExtension xmlInfo)
        {
            if (!ModParameters.EmotionCards.TryGetValue(packageId, out var cards))
            {
                xmlInfo = null;
                return false;
            }

            xmlInfo = cards.Where(x => x.CardXml.LorId == new LorId(packageId, id)).Select(x => x.CardXml)
                .FirstOrDefault();
            return xmlInfo != null;
        }

        public static bool GetFloorEgoCard(string packageId, int id, out EmotionEgoCardXmlExtension xmlInfo)
        {
            if (!ModParameters.EmotionEgoCards.TryGetValue(packageId, out var cards))
            {
                xmlInfo = null;
                return false;
            }

            xmlInfo = cards.Where(x => x.CardXml.CardId == new LorId(packageId, id)).Select(x => x.CardXml)
                .FirstOrDefault();
            return xmlInfo != null;
        }

        public static void SetEmotionCombatLog(this BattleUnitModel owner, BattleEmotionCardModel emotionCard)
        {
            owner.battleCardResultLog.SetEmotionAbility(true, emotionCard, emotionCard.XmlInfo.id);
        }

        public static void SetDieAbility(this BattleUnitModel owner, DiceCardAbilityBase ability)
        {
            var battleCardResultLog = owner.battleCardResultLog;
            battleCardResultLog?.SetDiceBehaviourAbility(true, ability.behavior, ability.card.card);
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