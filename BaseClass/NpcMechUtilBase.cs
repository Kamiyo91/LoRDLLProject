using System;
using System.Linq;
using BigDLL4221.Buffs;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using Sound;

namespace BigDLL4221.BaseClass
{
    public class NpcMechUtilBase : MechUtilBase
    {
        public new StageLibraryFloorModel Floor = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
        public new NpcMechUtilBaseModel Model;

        public NpcMechUtilBase(NpcMechUtilBaseModel model) : base(model)
        {
            Model = model;
        }

        public virtual void OnUseCardResetCount(BattlePlayingCardDataInUnitModel curCard)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return;
            if (!mechPhaseOptions.EgoMassAttackCardsOptions.Any()) return;
            if (mechPhaseOptions.EgoMassAttackCardsOptions.All(x => x.CardId != curCard.card.GetID())) return;
            mechPhaseOptions.Counter = 0;
            Model.Owner.allyCardDetail.ExhaustACardAnywhere(curCard.card);
        }

        public virtual void AddStartBuffsToPlayerUnits()
        {
            foreach (var buff in Model.AddBuffsOnPlayerUnitsAtStart)
            foreach (var unit in BattleObjectManager.instance.GetAliveList(
                         UnitUtil.ReturnOtherSideFaction(Model.Owner.faction)))
                unit.bufListDetail.AddBuf(buff);
        }

        public virtual void MechHpCheck(int dmg)
        {
            if (Model.PhaseChanging) return;
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (mechOptions.MechHp == 0 || Model.Owner.hp - dmg > mechOptions.MechHp) return;
            Model.PhaseChanging = true;
            Model.Owner.SetHp(mechOptions.MechHp);
            Model.Owner.breakDetail.ResetGauge();
            Model.Owner.breakDetail.RecoverBreakLife(1, true);
            Model.Owner.breakDetail.nextTurnBreak = false;
            Model.Owner.bufListDetail.AddBufWithoutDuplication(new BattleUnitBuf_Immortal_DLL4221());
        }

        public override void SurviveCheck(int dmg)
        {
            if (Model.Owner.hp - dmg > Model.SurviveHp || !Model.Survive) return;
            Model.Survive = false;
            if (Model.ReloadMassAttackOnLethal)
                if (Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions))
                    SetCounter(mechPhaseOptions.MaxCounter);
            Model.Owner.SetHp(Model.RecoverToHp);
            Model.Owner.breakDetail.ResetGauge();
            Model.Owner.breakDetail.RecoverBreakLife(1, true);
            Model.Owner.breakDetail.nextTurnBreak = false;
            Model.Owner.bufListDetail.AddBufWithoutDuplication(new BattleUnitBuf_Immortal_DLL4221());
            if (Model.SurviveAbDialogList.Any())
                UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.SurviveAbDialogList,
                    Model.SurviveAbDialogColor);
            if (Model.NearDeathBuffType != null)
                Model.Owner.bufListDetail.AddBufWithoutDuplication(Model.NearDeathBuffType);
        }

        public virtual int AlwaysAimToTheSlowestDice(BattleUnitModel target, int targetSlot)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return targetSlot;
            return !mechPhaseOptions.AlwaysAimSlowestTargetDie
                ? targetSlot
                : UnitUtil.AlwaysAimToTheSlowestDice(target, targetSlot, true);
        }

        public virtual void RaiseCounter()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return;
            if (Model.MassAttackStartCount && mechPhaseOptions.Counter < mechPhaseOptions.MaxCounter)
                mechPhaseOptions.Counter++;
        }

        public virtual void SetMassAttack(bool value)
        {
            Model.MassAttackStartCount = value;
        }

        public virtual void SetOneTurnCard(bool value)
        {
            Model.OneTurnCard = value;
        }

        public virtual void SetCounter(int value)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return;
            mechPhaseOptions.Counter = value;
        }

        public virtual void OnSelectCardPutMassAttack(ref BattleDiceCardModel origin)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return;
            if (MassAttackExtraCondition()) return;
            if (Model.OneTurnCard) return;
            if (mechPhaseOptions.BuffMech != null &&
                mechPhaseOptions.BuffMech.Buff.stack >= mechPhaseOptions.BuffMech.MassAttackStacks)
            {
                var card = RandomUtil.SelectOne(mechPhaseOptions.BuffMech.MassAttackCards);
                origin = BattleDiceCardModel.CreatePlayingCard(
                    ItemXmlDataList.instance.GetCardItem(card.CardId));
                SetOneTurnCard(true);
            }

            if (!Model.MassAttackStartCount || mechPhaseOptions.Counter < mechPhaseOptions.MaxCounter) return;
            if (!mechPhaseOptions.EgoMassAttackCardsOptions.Any()) return;
            origin = BattleDiceCardModel.CreatePlayingCard(
                ItemXmlDataList.instance.GetCardItem(RandomUtil.SelectOne(mechPhaseOptions.EgoMassAttackCardsOptions)
                    .CardId));
            SetOneTurnCard(true);
        }

        public virtual bool MassAttackExtraCondition()
        {
            return true;
        }

        public virtual void ExhaustEgoAttackCards()
        {
            foreach (var mechOptions in Model.MechOptions.Where(x => x.Value.EgoMassAttackCardsOptions.Any()))
            foreach (var card in mechOptions.Value.EgoMassAttackCardsOptions.SelectMany(cardItem =>
                         Model.Owner.allyCardDetail.GetAllDeck().Where(x => cardItem.CardId == x.GetID())))
                Model.Owner.allyCardDetail.ExhaustACardAnywhere(card);
        }

        public virtual void ExhaustMechBufAttackCards()
        {
            foreach (var mechOptions in Model.MechOptions.Where(x => x.Value.BuffMech != null))
            foreach (var card in mechOptions.Value.BuffMech.MassAttackCards.SelectMany(cardItem =>
                         Model.Owner.allyCardDetail.GetAllDeck().Where(x => cardItem.CardId == x.GetID())))
                Model.Owner.allyCardDetail.ExhaustACardAnywhere(card);
        }

        public virtual BattleUnitModel IgnoreSephiraSelectionTarget(LorId cardId)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return null;
            if (!mechPhaseOptions.EgoMassAttackCardsOptions.Any() && (mechPhaseOptions.BuffMech == null ||
                                                                      !mechPhaseOptions.BuffMech.MassAttackCards.Any()))
                return null;
            var card = mechPhaseOptions.EgoMassAttackCardsOptions.FirstOrDefault(x => x.CardId == cardId) ??
                       mechPhaseOptions.BuffMech.MassAttackCards.FirstOrDefault(x => x.CardId == cardId);
            if (card == null || !card.IgnoreSephirah) return null;
            return UnitUtil.IgnoreSephiraSelectionTarget(true);
        }

        public override bool EgoActive()
        {
            var egoActivated = base.EgoActive();
            if (egoActivated) Model.EgoPhase++;
            return egoActivated;
        }

        public virtual void RoundStartBuffs()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (mechOptions.MechBuffOptions == null) return;
            foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartBuffs)
                Model.Owner.bufListDetail.AddBuf(buff);
            foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartKeywordBuffs)
                Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            if (UnitUtil.SupportCharCheck(Model.Owner) > 1)
            {
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartBuffsNotAlone)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartKeywordBuffsNotAlone)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }
            else
            {
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartBuffsAlone)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartKeywordBuffsAlone)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }

            if (BattleObjectManager.instance.GetAliveList(Model.Owner.faction).Count > 1)
            {
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartBuffsNotAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartKeywordBuffsNotAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }
            else
            {
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartBuffsAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in mechOptions.MechBuffOptions.EachRoundStartKeywordBuffsAloneCountSupportChar)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }
        }

        public virtual bool UseSpecialBuffCard()
        {
            if (Model.SpecialCardOptions == null) return false;
            if (Model.Owner.cardSlotDetail.PlayPoint < Model.SpecialCardOptions.SpecialCardCost) return false;
            if (Model.SpecialCardOptions.SpecialBufType != null)
            {
                if (!Model.Owner
                        .bufListDetail
                        .GetActivatedBufList()
                        .Exists(x =>
                            Model.SpecialCardOptions.SpecialBufType.IsInstanceOfType(x) &&
                            x.stack >= Model.SpecialCardOptions.SpecialStackNeeded)) return false;
                Model.Owner.bufListDetail.RemoveBufAll(Model.SpecialCardOptions.SpecialBufType);
            }

            if (Model.SpecialCardOptions.SpecialKeywordBuf != KeywordBuf.None)
            {
                if (Model.Owner.bufListDetail.GetActivatedBuf(Model.SpecialCardOptions.SpecialKeywordBuf)?.stack <
                    Model.SpecialCardOptions.SpecialStackNeeded) return false;
                Model.Owner.bufListDetail.RemoveBufAll(Model.SpecialCardOptions.SpecialBufType);
            }

            Model.Owner.cardSlotDetail.RecoverPlayPoint(-Model.SpecialCardOptions.SpecialCardCost);
            foreach (var buff in Model.SpecialCardOptions.Buffs)
                Model.Owner.bufListDetail.AddBuf(buff);
            foreach (var keywordBuff in Model.SpecialCardOptions.KeywordBuffs)
                Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(keywordBuff.Key, keywordBuff.Value, Model.Owner);
            return true;
        }

        public virtual void InitMechRoundEnd(MechPhaseOptions mechOptions)
        {
            if (mechOptions.MechOnDeath) UnitUtil.UnitReviveAndRecovery(Model.Owner, 1, true);
            if (mechOptions.SetEmotionLevel != 0)
            {
                var emotionLevels = mechOptions.SetEmotionLevel - Model.Owner.emotionDetail.EmotionLevel;
                if (emotionLevels > 0) UnitUtil.LevelUpEmotion(Model.Owner, emotionLevels);
            }

            if (mechOptions.ForceEgo) ForcedEgo();
            if (mechOptions.StartMassAttack.HasValue) SetMassAttack(mechOptions.StartMassAttack.Value);
            if (mechOptions.SetCounterToMax) mechOptions.Counter = mechOptions.MaxCounter;
            if (mechOptions.ChangeCardCost)
                UnitUtil.ChangeCardCostByValue(Model.Owner, mechOptions.LoweredCost, mechOptions.MaxCost, true);
            if (mechOptions.MechBuffOptions != null)
            {
                foreach (var buff in mechOptions.MechBuffOptions.Buffs)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in mechOptions.MechBuffOptions.OneRoundBuffs)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }

            if (mechOptions.BuffMech != null)
            {
                MechBuff();
                if (mechOptions.BuffMech.MaxStackOnPhaseChange) mechOptions.BuffMech.Buff.OnAddBuf(9999);
            }

            foreach (var passive in mechOptions.AdditionalPassiveByIds
                         .Select(passiveId => Model.Owner.passiveDetail.AddPassive(passiveId))
                         .Where(passive => mechOptions.OnWaveStartEffectOnAddedPassives))
                passive.OnWaveStart();
            foreach (var passiveId in mechOptions.RemovePassiveByIds)
                Model.Owner.RemovePassive(passiveId);
            foreach (var buff in Model.Owner.bufListDetail.GetActivatedBufList()
                         .Where(x => mechOptions.RemoveBuffs.Exists(y => x.GetType() == y.GetType())))
                Model.Owner.bufListDetail.RemoveBuf(buff);
            if (mechOptions.RemoveOtherUnits)
                foreach (var unit in BattleObjectManager.instance.GetList(Model.Owner.faction)
                             .Where(x => x != Model.Owner))
                    BattleObjectManager.instance.UnregisterUnit(unit);
            foreach (var path in mechOptions.SoundEffectPath.Where(x => !string.IsNullOrEmpty(x)))
                SoundEffectPlayer.PlaySound(path);
            if (mechOptions.HasExtraFunctionRoundEnd) ExtraMethodOnPhaseChangeRoundEnd(mechOptions);
        }

        public virtual void InitMechRoundStart(MechPhaseOptions mechOptions, bool restart = false)
        {
            if (mechOptions.HpRecoverOnChangePhase != 0 && !restart)
                UnitUtil.UnitReviveAndRecovery(Model.Owner, mechOptions.HpRecoverOnChangePhase, true);
            foreach (var unitModel in mechOptions.SummonUnit)
                UnitUtil.AddNewUnitWithDefaultData(Floor, unitModel,
                    BattleObjectManager.instance.GetList(Model.Owner.faction).Count, true,
                    mechOptions.SummonedEmotionLevelEnemy ?? Model.Owner.emotionDetail.EmotionLevel, false);
            foreach (var unitModel in mechOptions.SummonPlayerUnit)
                UnitUtil.AddNewUnitWithDefaultData(Floor, unitModel,
                    BattleObjectManager.instance.GetList(UnitUtil.ReturnOtherSideFaction(Model.Owner.faction)).Count,
                    emotionLevel: mechOptions.SummonedEmotionLevelAlly ?? Model.Owner.emotionDetail.EmotionLevel);
            foreach (var index in mechOptions.SummonOriginalUnitByIndex)
                UnitUtil.AddOriginalPlayerUnit(index,
                    mechOptions.SummonedEmotionLevelAlly ?? Model.Owner.emotionDetail.EmotionLevel);
            if (mechOptions.SummonUnit.Any() || mechOptions.SummonPlayerUnit.Any()) UnitUtil.RefreshCombatUI();
            if (mechOptions.OnPhaseChangeDialogList.Any())
                UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, mechOptions.OnPhaseChangeDialogList,
                    mechOptions.OnPhaseChangeDialogColor);
            if (mechOptions.HasExtraFunctionRoundStart) ExtraMethodOnPhaseChangeRoundStart(mechOptions);
        }

        public virtual void InitExtraMechRoundPreEnd()
        {
            if (!Model.PhaseChanging) return;
            if (!Model.MechOptions.TryGetValue(Model.Phase + 1, out var mechOptions)) return;
            if (mechOptions.HasExtraFunctionRoundPreEnd) ExtraMechRoundPreEnd(mechOptions);
        }

        public virtual void ExtraMechRoundPreEnd(MechPhaseOptions mechOptions)
        {
        }

        public virtual void CheckPhaseRoundEnd()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase + 1, out var mechOptions)) return;
            if (mechOptions.HasSpecialChangePhaseCondition && SpecialChangePhaseCondition()) Model.PhaseChanging = true;
            if (!Model.PhaseChanging) return;
            Model.PhaseChanging = false;
            Model.PhaseChangingRoundStart = true;
            InitMechRoundEnd(mechOptions);
        }

        public virtual bool SpecialChangePhaseCondition()
        {
            return false;
        }

        public virtual void CheckPhaseRoundStart()
        {
            if (!Model.PhaseChangingRoundStart) return;
            if (!Model.MechOptions.TryGetValue(Model.Phase + 1, out var mechOptions)) return;
            Model.PhaseChangingRoundStart = false;
            Model.Phase++;
            InitMechRoundStart(mechOptions);
        }

        public virtual void InitRestartMech(int mechPhase)
        {
            if (!Model.MechOptions.TryGetValue(mechPhase, out var mechOptions)) return;
            InitMechRoundEnd(mechOptions);
            InitMechRoundStart(mechOptions, true);
        }

        public virtual void OnEndBattle()
        {
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var currentWaveModel = Singleton<StageController>.Instance.GetCurrentWaveModel();
            if (currentWaveModel == null || currentWaveModel.IsUnavailable()) return;
            stageModel.SetStageStorgeData(Model.SaveDataId, Model.Phase);
            stageModel.SetStageStorgeData(Model.EgoSaveDataId, Model.EgoSaveDataId);
            var list = BattleObjectManager.instance.GetAliveList(Model.Owner.faction)
                .Where(unit => !UnitUtil.IsSupportCharCheck(unit)).Select(unit => unit.UnitData)
                .ToList();
            currentWaveModel.ResetUnitBattleDataList(list);
        }

        public virtual void Restart()
        {
            var curPhaseTryGet = Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData<int>(Model.SaveDataId, out var curPhase);
            if (!curPhaseTryGet)
            {
                Model.Phase = 0;
                InitRestartMech(Model.Phase);
                return;
            }

            var curEgoPhaseTryGet = Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData<int>(Model.EgoSaveDataId, out var curEgoPhase);
            if (curEgoPhaseTryGet && curEgoPhase != 0)
            {
                Model.EgoPhase = curEgoPhase - 1;
                ForcedEgo();
            }

            Model.Phase = curPhase;
            if (Model.Phase < 1) return;
            InitRestartMech(Model.Phase);
        }

        public virtual void ExtraRecovery()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (mechOptions.ExtraLightRecoverEachScene != 0)
                Model.Owner.cardSlotDetail.RecoverPlayPoint(mechOptions.ExtraLightRecoverEachScene);
            if (mechOptions.ExtraDrawEachScene != 0)
                Model.Owner.allyCardDetail.DrawCards(mechOptions.ExtraDrawEachScene);
            if (mechOptions.ExtraRecoverHp != 0) Model.Owner.RecoverHP(mechOptions.ExtraRecoverHp);
            if (mechOptions.ExtraRecoverStagger != 0)
                Model.Owner.breakDetail.RecoverBreak(mechOptions.ExtraRecoverStagger);
        }

        public virtual void MechBuff()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (mechOptions.BuffMech == null) return;
            var buffType = mechOptions.BuffMech.Buff.GetType();
            if (Model.Owner.HasBuff(buffType, out var buff))
            {
                mechOptions.BuffMech.Buff = buff;
                return;
            }

            mechOptions.BuffMech.Buff = (BattleUnitBuf)Activator.CreateInstance(buffType);
            Model.Owner.bufListDetail.AddBuf(mechOptions.BuffMech.Buff);
        }

        public virtual bool OnUseMechBuffAttackCard(BattlePlayingCardDataInUnitModel card)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return false;
            if (mechOptions.BuffMech == null) return false;
            if (!mechOptions.BuffMech.MassAttackCards.Exists(x => x.CardId == card.card.GetID())) return false;
            mechOptions.BuffMech.Buff.OnAddBuf(-9999);
            return true;
        }

        public virtual void ChangePhaseForSceneCounter()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (!mechOptions.MechOnScenesCount) return;
            if (mechOptions.SceneCounter >= mechOptions.ScenesBeforeNextPhase) Model.PhaseChanging = true;
        }

        public virtual void SceneCounter()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (mechOptions.MechOnScenesCount) mechOptions.SceneCounter++;
        }

        public virtual void ExtraMethodOnPhaseChangeRoundEnd(MechPhaseOptions mechOptions)
        {
        }

        public virtual void ExtraMethodOnPhaseChangeRoundStart(MechPhaseOptions mechOptions)
        {
        }

        public new virtual void ExtraMethodOnRoundEndTheLastIgnoreDead()
        {
        }

        public new virtual void ExtraMethodOnOtherUnitDie(BattleUnitModel unit)
        {
        }

        public new virtual void ExtraMethodOnKill(BattleUnitModel unit)
        {
        }
    }
}