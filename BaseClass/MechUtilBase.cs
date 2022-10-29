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
            if (Model.Owner.bufListDetail.HasAssimilation()) return;
            if (Model.EgoOptions == null) return;
            Model.EgoOptions.EgoActivated = false;
            if (!string.IsNullOrEmpty(Model.EgoOptions?.SkinName))
                Model.Owner.view.SetAltSkin(Model.EgoOptions.SkinName);
            if (Model.EgoOptions?.EgoType != null)
                Model.Owner.bufListDetail.AddBufWithoutDuplication(Model.EgoOptions.EgoType);
            Model.Owner.cardSlotDetail.RecoverPlayPoint(Model.Owner.cardSlotDetail.GetMaxPlayPoint());
            foreach (var card in Model.PersonalCards.Where(x => x.Value.EgoPersonalCard))
                Model.Owner.personalEgoDetail.AddCard(card.Key);
            if (Model.EgoOptions == null) return;
            if (Model.EgoOptions.RefreshUI) UnitUtil.RefreshCombatUI();
            if (Model.EgoOptions.EgoAbDialogList.Any())
                UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.EgoOptions.EgoAbDialogList,
                    Model.EgoOptions.EgoAbColorColor);
        }

        public virtual void DeactiveEgo()
        {
            if (Model.EgoOptions == null || Model.EgoOptions.Duration == 0 ||
                Model.EgoOptions.Count != Model.EgoOptions.Duration) return;
            Model.EgoOptions.Count = 0;
            if (Model.EgoOptions?.EgoCardId != null) Model.Owner.personalEgoDetail.AddCard(Model.EgoOptions.EgoCardId);
            foreach (var card in Model.PersonalCards.Where(x => x.Value.EgoPersonalCard))
                Model.Owner.personalEgoDetail.RemoveCard(card.Key);
            Model.Owner.bufListDetail.RemoveBufAll(Model.EgoOptions?.EgoType.GetType());
            Model.Owner.passiveDetail.PassiveList.RemoveAll(x => Model.EgoOptions.AdditionalPassiveIds.Contains(x.id));
            if (!string.IsNullOrEmpty(Model.EgoOptions?.SkinName)) Model.Owner.view.CreateSkin();
            if (Model.EgoOptions.RefreshUI) UnitUtil.RefreshCombatUI();
            if (Model.EgoOptions.ActivatedMap != null) ReturnFromEgoAssimilationMap();
        }

        public virtual void EgoDurationCount()
        {
            if (Model.EgoOptions == null || Model.EgoOptions.Duration == 0) return;
            if (Model.Owner.bufListDetail.GetActivatedBufList()
                .Exists(x => x.GetType() == Model.EgoOptions.EgoType.GetType())) Model.EgoOptions.Count++;
        }

        public virtual void OnUseExpireCard(LorId cardId)
        {
            if (Model.PersonalCards.Any(x => x.Key == cardId && x.Value.ExpireAfterUse))
                Model.Owner.personalEgoDetail.RemoveCard(cardId);
            if (Model.EgoOptions?.EgoCardId == null || Model.EgoOptions.EgoCardId != cardId) return;
            Model.Owner.personalEgoDetail.RemoveCard(Model.EgoOptions.EgoCardId);
            foreach (var passiveId in Model.EgoOptions.AdditionalPassiveIds)
                Model.Owner.passiveDetail.AddPassive(passiveId);
            Model.Owner.breakDetail.ResetGauge();
            Model.Owner.breakDetail.RecoverBreakLife(1, true);
            Model.Owner.breakDetail.nextTurnBreak = false;
            Model.EgoOptions.EgoActivated = true;
        }

        public virtual void ReAddOnPlayCard()
        {
            foreach (var card in Model.PersonalCards.Where(x =>
                         x.Value.OnPlayCard && !x.Value.ExpireAfterUse && (!x.Value.EgoPersonalCard ||
                                                                           (Model.EgoOptions != null &&
                                                                            Model.Owner.bufListDetail
                                                                                .GetActivatedBufList()
                                                                                .Any(y => y.GetType() ==
                                                                                    Model.EgoOptions.EgoType
                                                                                        .GetType())))))
            {
                Model.Owner.personalEgoDetail.RemoveCard(card.Key);
                Model.Owner.personalEgoDetail.AddCard(card.Key);
            }
        }

        public virtual void AddExpireCards()
        {
            if (Model.EgoOptions?.EgoCardId != null) Model.Owner.personalEgoDetail.AddCard(Model.EgoOptions.EgoCardId);
            foreach (var card in Model.PersonalCards.Where(x => !x.Value.EgoPersonalCard))
                Model.Owner.personalEgoDetail.AddCard(card.Key);
        }

        public virtual void DoNotChangeSkinOnEgo()
        {
            if (Model.EgoOptions == null) return;
            Model.EgoOptions.SkinName = "";
        }

        public virtual bool CheckSkinChangeIsActive()
        {
            return !string.IsNullOrEmpty(Model.EgoOptions?.SkinName);
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
            Model.EgoOptions.EgoAbDialogList.Clear();
        }

        public virtual bool EgoCheck()
        {
            return Model.EgoOptions != null && Model.EgoOptions.EgoActivated;
        }

        public virtual void ForcedEgo()
        {
            Model.EgoOptions.EgoActivated = true;
        }

        public virtual void ChangeEgoAbDialog(List<AbnormalityCardDialog> value)
        {
            Model.EgoOptions.EgoAbDialogList = value;
        }

        public virtual void ReturnFromEgoMap()
        {
            if (Model.EgoOptions?.ActivatedMap == null) return;
            if (!Model.EgoOptions.ActivatedMap.OneTurnEgo && !Model.Owner.IsDead()) return;
            MapUtil.ReturnFromEgoMap(Model.EgoOptions.ActivatedMap.Stage,
                Model.EgoOptions.ActivatedMap.OriginalMapStageIds);
            Model.EgoOptions.ActivatedMap = null;
        }

        public virtual void ReturnFromEgoAssimilationMap()
        {
            if (Model.EgoOptions?.ActivatedMap == null) return;
            MapUtil.ReturnFromEgoMap(Model.EgoOptions.ActivatedMap.Stage,
                Model.EgoOptions.ActivatedMap.OriginalMapStageIds);
            Model.EgoOptions.ActivatedMap = null;
        }

        public virtual void ChangeToEgoMap(LorId cardId)
        {
            if (Model.EgoOptions == null || !Model.EgoOptions.EgoMaps.TryGetValue(cardId, out var mapModel) ||
                SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.isEgo) return;
            Model.EgoOptions.ActivatedMap = mapModel;
            MapUtil.ChangeMap(mapModel);
        }

        public virtual void PermanentBuffs()
        {
            foreach (var item in Model.PermanentBuffList.Select((buff, index) => (index, buff)).ToList())
            {
                if (Model.Owner.HasBuff(item.buff.GetType())) continue;
                Model.PermanentBuffList[item.index] = (BattleUnitBuf)Activator.CreateInstance(item.buff.GetType());
                Model.Owner.bufListDetail.AddBuf(Model.PermanentBuffList[item.index]);
            }
        }
    }
}