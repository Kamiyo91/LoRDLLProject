using System;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Buffs;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using LOR_XML;

namespace BigDLL4221.BaseClass
{
    public class MechUtilBase
    {
        public MechUtilBaseModel Model;

        public MechUtilBase(MechUtilBaseModel model)
        {
            Model = model;
        }

        public virtual void SurviveCheck(int dmg)
        {
            if (Model.Owner.hp - dmg > Model.SurviveHp || !Model.Survive) return;
            Model.Survive = false;
            UnitUtil.UnitReviveAndRecovery(Model.Owner, 0, Model.RecoverLightOnSurvive);
            if (Model.SurviveAbDialogList.Any())
                UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.SurviveAbDialogList,
                    Model.SurviveAbDialogColor);
            Model.Owner.SetHp(Model.RecoverToHp);
            if (Model.NearDeathBuffType != null)
                Model.Owner.bufListDetail.AddBufWithoutDuplication(Model.NearDeathBuffType);
            Model.Owner.bufListDetail.AddBufWithoutDuplication(
                new BattleUnitBuf_ImmunityToStatusAlimentUntilRoundEnd_DLL4221());
            Model.Owner.bufListDetail.AddBufWithoutDuplication(new BattleUnitBuf_ImmortalUntilRoundEnd_DLL4221());
        }

        public virtual void EgoActive()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            //if (Model.Owner.bufListDetail.HasAssimilation()) return;
            egoOptions.EgoActivated = false;
            if (!string.IsNullOrEmpty(egoOptions.EgoSkinName))
                Model.Owner.view.SetAltSkin(egoOptions.EgoSkinName);
            if (egoOptions.EgoType != null)
                Model.Owner.bufListDetail.AddBufWithoutDuplication(egoOptions.EgoType);
            Model.Owner.cardSlotDetail.RecoverPlayPoint(Model.Owner.cardSlotDetail.GetMaxPlayPoint());
            foreach (var card in Model.PersonalCards.Where(x =>
                         x.Value.EgoPersonalCard && x.Value.EgoPhase == Model.EgoPhase))
                Model.Owner.personalEgoDetail.AddCard(card.Key);
            if (Model.EgoOptions == null) return;
            if (egoOptions.RefreshUI) UnitUtil.RefreshCombatUI();
            if (egoOptions.EgoAbDialogList.Any())
                UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, egoOptions.EgoAbDialogList,
                    egoOptions.EgoAbColorColor);
        }

        public virtual void DeactiveEgo()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            if (egoOptions.Duration == 0 ||
                egoOptions.Count != egoOptions.Duration) return;
            egoOptions.Count = 0;
            foreach (var card in Model.PersonalCards.Where(x => x.Value.EgoPersonalCard))
                Model.Owner.personalEgoDetail.RemoveCard(card.Key);
            var egoCard = Model.PersonalCards.FirstOrDefault(x => x.Value.ActiveEgoCard && x.Value.EgoPhase == 0);
            if (egoCard.Key != null) Model.Owner.personalEgoDetail.AddCard(egoCard.Key);
            foreach (var item in Model.EgoOptions)
                Model.Owner.bufListDetail.RemoveBufAll(item.Value.EgoType.GetType());
            foreach (var item in Model.EgoOptions)
                Model.Owner.passiveDetail.PassiveList.RemoveAll(x => item.Value.AdditionalPassiveIds.Contains(x.id));
            if (!string.IsNullOrEmpty(egoOptions.EgoSkinName))
            {
                Model.Owner.view.model.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                    new List<string> { Model.OriginalSkinName };
                Model.Owner.view.CreateSkin();
            }

            if (egoOptions.RefreshUI) UnitUtil.RefreshCombatUI();
            if (Model.ActivatedMap != null) ReturnFromEgoAssimilationMap();
        }

        public virtual void EgoDurationCount()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            if (egoOptions.Duration == 0) return;
            if (Model.Owner.bufListDetail.GetActivatedBufList()
                .Exists(x => x.GetType() == egoOptions.EgoType.GetType())) egoOptions.Count++;
        }

        public virtual void OnUseExpireCard(LorId cardId)
        {
            if (Model.PersonalCards.Any(x => x.Key == cardId && x.Value.ExpireAfterUse))
                Model.Owner.personalEgoDetail.RemoveCard(cardId);
            var egoCard = Model.PersonalCards.FirstOrDefault(x => x.Key == cardId);
            if (egoCard.Key == null) return;
            if (!egoCard.Value.ActiveEgoCard) return;
            Model.EgoPhase = egoCard.Value.EgoPhase + 1;
            if (Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions))
            {
                foreach (var passiveId in egoOptions.AdditionalPassiveIds)
                    Model.Owner.passiveDetail.AddPassive(passiveId);
                Model.Owner.breakDetail.ResetGauge();
                Model.Owner.breakDetail.RecoverBreakLife(1, true);
                Model.Owner.breakDetail.nextTurnBreak = false;
                egoOptions.EgoActivated = true;
            }

            Model.Owner.personalEgoDetail.RemoveCard(egoCard.Key);
        }

        public virtual void ReAddOnPlayCard()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            foreach (var card in Model.PersonalCards.Where(x =>
                         x.Value.OnPlayCard && !x.Value.ExpireAfterUse && (!x.Value.EgoPersonalCard ||
                                                                           (Model.EgoOptions != null &&
                                                                            Model.Owner.bufListDetail
                                                                                .GetActivatedBufList()
                                                                                .Any(y => y.GetType() ==
                                                                                    egoOptions.EgoType
                                                                                        .GetType())))))
            {
                Model.Owner.personalEgoDetail.RemoveCard(card.Key);
                Model.Owner.personalEgoDetail.AddCard(card.Key);
            }
        }

        public virtual void AddExpireCards()
        {
            var egoCard = Model.PersonalCards.FirstOrDefault(x => x.Value.ActiveEgoCard && x.Value.EgoPhase == 0);
            if(egoCard.Key != null) Model.Owner.personalEgoDetail.AddCard(egoCard.Key);
            foreach (var card in Model.PersonalCards.Where(x => !x.Value.EgoPersonalCard))
                Model.Owner.personalEgoDetail.AddCard(card.Key);
        }

        public virtual void DoNotChangeSkinOnEgo()
        {
            if (!Model.EgoOptions.Any()) return;
            foreach (var item in Model.EgoOptions)
                item.Value.EgoSkinName = "";
        }

        public virtual bool CheckSkinChangeIsActive()
        {
            return Model.EgoOptions.Any() && Model.EgoOptions
                .Select(item => !string.IsNullOrEmpty(item.Value.EgoSkinName)).FirstOrDefault();
        }

        public virtual bool CheckOnDieAtFightEnd()
        {
            return Model.DieOnFightEnd;
        }

        public virtual void TurnOnDieAtFightEnd()
        {
            Model.DieOnFightEnd = true;
        }

        public virtual void TurnEgoAbDialogOff()
        {
            if (!Model.EgoOptions.Any()) return;
            foreach (var item in Model.EgoOptions)
                item.Value.EgoAbDialogList.Clear();
        }

        public virtual bool EgoCheck()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return false;
            return egoOptions != null && egoOptions.EgoActivated;
        }

        public virtual void ForcedEgo()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            egoOptions.EgoActivated = true;
        }

        public virtual void ChangeEgoAbDialog(List<AbnormalityCardDialog> value)
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            egoOptions.EgoAbDialogList = value;
        }

        public virtual void ReturnFromEgoMap()
        {
            if (Model.ActivatedMap == null) return;
            if (!Model.ActivatedMap.OneTurnEgo && !Model.Owner.IsDead()) return;
            MapUtil.ReturnFromEgoMap(Model.ActivatedMap.Stage,
                Model.ActivatedMap.OriginalMapStageIds);
            Model.ActivatedMap = null;
        }

        public virtual void ReturnFromEgoAssimilationMap()
        {
            if (Model.ActivatedMap == null) return;
            MapUtil.ReturnFromEgoMap(Model.ActivatedMap.Stage,
                Model.ActivatedMap.OriginalMapStageIds);
            Model.ActivatedMap = null;
        }

        public virtual void ChangeToEgoMap(LorId cardId)
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            if (Model.EgoOptions == null || !egoOptions.EgoMaps.TryGetValue(cardId, out var mapModel) ||
                SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.isEgo) return;
            Model.ActivatedMap = mapModel;
            MapUtil.ChangeMap(mapModel);
        }

        public virtual void PermanentBuffs()
        {
            foreach (var item in Model.PermanentBuffList.Select((buff, index) => (index, buff)).ToList()
                         .Where(item => !Model.Owner.HasBuff(item.buff.GetType())))
            {
                Model.PermanentBuffList[item.index] = (BattleUnitBuf)Activator.CreateInstance(item.buff.GetType());
                Model.Owner.bufListDetail.AddBuf(Model.PermanentBuffList[item.index]);
            }
        }

        public virtual void EgoRoundStartBuffs()
        {
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            if (egoOptions.AdditionalBuffs == null) return;
            foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartBuffs)
                Model.Owner.bufListDetail.AddBuf(buff);
            foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartKeywordBuffs)
                Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            if (UnitUtil.SupportCharCheck(Model.Owner) > 1)
            {
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartBuffsNotAlone)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartKeywordBuffsNotAlone)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }
            else
            {
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartBuffsAlone)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartKeywordBuffsAlone)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }

            if (BattleObjectManager.instance.GetAliveList(Model.Owner.faction).Count > 1)
            {
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartBuffsNotAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartKeywordBuffsNotAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }
            else
            {
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartBuffsAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in egoOptions.AdditionalBuffs.EachRoundStartKeywordBuffsAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }
        }
    }
}