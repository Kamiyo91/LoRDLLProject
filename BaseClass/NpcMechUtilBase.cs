﻿using System;
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
        public new NpcMechUtilBaseModel Model;

        public NpcMechUtilBase(NpcMechUtilBaseModel model, string packageId) : base(model, packageId)
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

        public virtual bool MechHpCheck(int dmg)
        {
            if (Model.PhaseChanging) return true;
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return true;
            if (mechOptions.MechHp == 0 || Model.Owner.hp - dmg > mechOptions.MechHp) return true;
            Model.PhaseChanging = true;
            Model.Owner.SetHp(mechOptions.MechHp);
            Model.Owner.breakDetail.ResetGauge();
            Model.Owner.breakDetail.RecoverBreakLife(1, true);
            Model.Owner.breakDetail.nextTurnBreak = false;
            Model.Owner.bufListDetail.AddBufWithoutDuplication(new BattleUnitBuf_Immortal_DLL4221());
            return false;
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
                if (Model.SurviveAbDialogCustomColor.HasValue)
                    UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.SurviveAbDialogList,
                        Model.SurviveAbDialogCustomColor.Value);
                else
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
            if (!MassAttackExtraCondition()) return;
            if (Model.OneTurnCard) return;
            if (mechPhaseOptions.SingletonBufMech != null &&
                mechPhaseOptions.SingletonBufMech.Buff.stack >= mechPhaseOptions.SingletonBufMech.MassAttackStacks)
            {
                var card = RandomUtil.SelectOne(mechPhaseOptions.SingletonBufMech.MassAttackCards);
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
            foreach (var mechOptions in Model.MechOptions.Where(x => x.Value.SingletonBufMech != null))
            foreach (var card in mechOptions.Value.SingletonBufMech.MassAttackCards.SelectMany(cardItem =>
                         Model.Owner.allyCardDetail.GetAllDeck().Where(x => cardItem.CardId == x.GetID())))
                Model.Owner.allyCardDetail.ExhaustACardAnywhere(card);
        }

        public virtual BattleUnitModel IgnoreSephiraSelectionTarget(LorId cardId)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechPhaseOptions)) return null;
            if (!mechPhaseOptions.EgoMassAttackCardsOptions.Any() && (mechPhaseOptions.SingletonBufMech == null ||
                                                                      !mechPhaseOptions.SingletonBufMech.MassAttackCards
                                                                          .Any()))
                return null;
            var card = mechPhaseOptions.EgoMassAttackCardsOptions.FirstOrDefault(x => x.CardId == cardId);
            if (card == null && mechPhaseOptions.SingletonBufMech != null)
                card = mechPhaseOptions.SingletonBufMech.MassAttackCards.FirstOrDefault(x => x.CardId == cardId);
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
            //if (Singleton<StageController>.Instance.EnemyStageManager is
            //    EnemyTeamStageManager_RushBattleWithCMUOnly_DLL4221 manager)
            //    if (mechOptions.MusicOptions != null)
            //        manager.ChangeMusic(Model.Owner.Book.BookId.packageId, mechOptions.MusicOptions.MusicFileName,
            //            mechOptions.MusicOptions.MapName);
            if (mechOptions.MechOnDeath) UnitUtil.UnitReviveAndRecovery(Model.Owner, 1, true);
            if (mechOptions.ForcedRetreatOnDeath) Model.Owner.forceRetreat = true;
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
                foreach (var buff in mechOptions.MechBuffOptions.OtherSideBuffs)
                foreach (var unit in BattleObjectManager.instance
                             .GetAliveList(UnitUtil.ReturnOtherSideFaction(Model.Owner.faction)).Where(x =>
                                 (buff.OnlySephirah && x.UnitData.unitData.isSephirah) ||
                                 (buff.Index.HasValue && x.index == buff.Index.Value) ||
                                 (!buff.OnlySephirah && !buff.Index.HasValue)))
                    unit.bufListDetail.AddBuf(buff.Buff);
                foreach (var buff in mechOptions.MechBuffOptions.Buffs)
                    Model.Owner.bufListDetail.AddBuf(buff);
                foreach (var buff in mechOptions.MechBuffOptions.OneRoundBuffs)
                    Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            }

            if (mechOptions.SingletonBufMech != null)
                MechBuffPhaseChange(mechOptions);
            foreach (var passive in mechOptions.AdditionalPassiveByIds
                         .Select(passiveId => Model.Owner.passiveDetail.AddPassive(passiveId))
                         .Where(passive => mechOptions.OnWaveStartEffectOnAddedPassives))
                passive.OnWaveStart();
            foreach (var passiveId in mechOptions.RemovePassiveByIds)
                Model.Owner.DestroyPassive(passiveId);
            if (mechOptions.RemoveOtherUnits)
                foreach (var unit in BattleObjectManager.instance.GetList(Model.Owner.faction)
                             .Where(x => x != Model.Owner))
                    BattleObjectManager.instance.UnregisterUnit(unit);
            foreach (var path in mechOptions.SoundEffectPath.Where(x => !string.IsNullOrEmpty(x)))
                SoundEffectPlayer.PlaySound(path);
            if (mechOptions.HasExtraFunctionRoundEnd) ExtraMethodOnPhaseChangeRoundEnd(mechOptions);
        }

        public virtual void InitMechRoundStart(MechPhaseOptions mechOptions)
        {
            foreach (var unitModel in mechOptions.SummonUnit)
                UnitUtil.AddNewUnitWithDefaultData(unitModel,
                    BattleObjectManager.instance.GetList(Model.Owner.faction).Count, true,
                    mechOptions.SummonedEmotionLevelEnemy ?? Model.Owner.emotionDetail.EmotionLevel,
                    Model.Owner.faction);
            foreach (var unitModel in mechOptions.SummonPlayerUnit)
                UnitUtil.AddNewUnitWithDefaultData(unitModel,
                    BattleObjectManager.instance.GetList(UnitUtil.ReturnOtherSideFaction(Model.Owner.faction)).Count,
                    emotionLevel: mechOptions.SummonedEmotionLevelAlly ?? Model.Owner.emotionDetail.EmotionLevel);
            foreach (var index in mechOptions.SummonOriginalUnitByIndex)
                UnitUtil.AddOriginalPlayerUnit(index,
                    mechOptions.SummonedEmotionLevelAlly ?? Model.Owner.emotionDetail.EmotionLevel);
            if (mechOptions.SummonUnit.Any() || mechOptions.SummonPlayerUnit.Any() ||
                mechOptions.SummonOriginalUnitByIndex.Any()) UnitUtil.RefreshCombatUI();
            if (mechOptions.OnPhaseChangeDialogList.Any())
                if (mechOptions.OnPhaseChangeDialogCustomColor.HasValue)
                    UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, mechOptions.OnPhaseChangeDialogList,
                        mechOptions.OnPhaseChangeDialogCustomColor.Value);
                else
                    UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, mechOptions.OnPhaseChangeDialogList,
                        mechOptions.OnPhaseChangeDialogColor);
            if (mechOptions.HasExtraFunctionRoundStart) ExtraMethodOnPhaseChangeRoundStart(mechOptions);
        }

        public virtual void InitMechRoundStartAfter(MechPhaseOptions mechOptions, bool restart = false)
        {
            foreach (var buff in Model.Owner.bufListDetail.GetActivatedBufList()
                         .Where(x => mechOptions.RemoveBuffs.Exists(y => x.GetType() == y.GetType())).ToList())
                Model.Owner.bufListDetail.RemoveBuf(buff);
            if (mechOptions.HpRecoverOnChangePhase != 0 && !restart)
                UnitUtil.UnitReviveAndRecovery(Model.Owner, mechOptions.HpRecoverOnChangePhase, true);
        }

        public virtual void InitExtraMechRoundPreEnd()
        {
            if (!Model.PhaseChanging) return;
            if (!Model.MechOptions.TryGetValue(Model.Phase + 1, out var mechPhase)) return;
            if (mechPhase.HasExtraFunctionRoundPreEnd) ExtraMechRoundPreEnd(mechPhase);
        }

        public virtual void ExtraMechRoundPreEnd(MechPhaseOptions mechOptions)
        {
        }

        public virtual void CheckPhaseRoundEnd()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase + 1, out var mechOptions)) return;
            if (!Model.PhaseChanging) return;
            Model.PhaseChanging = false;
            Model.PhaseChangingRoundStart = true;
            Model.PhaseChangingRoundStartAfter = true;
            InitMechRoundEnd(mechOptions);
        }

        public virtual void CheckSpecialPhaseRoundEnd()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (mechOptions.HasSpecialChangePhaseCondition && SpecialChangePhaseCondition()) Model.PhaseChanging = true;
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
            InitMechRoundStart(mechOptions);
        }

        public virtual void CheckPhaseRoundStartAfter()
        {
            if (!Model.PhaseChangingRoundStartAfter) return;
            if (!Model.MechOptions.TryGetValue(Model.Phase + 1, out var mechOptions)) return;
            Model.PhaseChangingRoundStartAfter = false;
            Model.Phase++;
            InitMechRoundStartAfter(mechOptions);
        }

        public virtual void InitRestartMech(int mechPhase)
        {
            if (!Model.MechOptions.TryGetValue(mechPhase, out var mechOptions)) return;
            InitMechRoundEnd(mechOptions);
            InitMechRoundStart(mechOptions);
            InitMechRoundStartAfter(mechOptions, true);
        }

        public virtual void OnEndBattle()
        {
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var currentWaveModel = Singleton<StageController>.Instance.GetCurrentWaveModel();
            if (currentWaveModel == null || currentWaveModel.IsUnavailable()) return;
            stageModel.SetStageStorgeData(Model.SaveDataId, Model.Phase);
            stageModel.SetStageStorgeData(Model.EgoSaveDataId, Model.EgoSaveDataId);
            var list = BattleObjectManager.instance.GetAliveList(Model.Owner.faction)
                .Where(unit => !unit.IsSupportCharCheck()).Select(unit => unit.UnitData)
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
            if (mechOptions.SingletonBufMech == null) return;
            if (Model.Owner.HasBuff(mechOptions.SingletonBufMech.Buff.GetType(), out var buff))
            {
                mechOptions.SingletonBufMech.Buff = buff;
                return;
            }

            mechOptions.SingletonBufMech.Buff =
                (BattleUnitBuf)Activator.CreateInstance(mechOptions.SingletonBufMech.Buff.GetType());
            Model.Owner.bufListDetail.AddBuf(mechOptions.SingletonBufMech.Buff);
        }

        public virtual void MechBuffPhaseChange(MechPhaseOptions mechOptions)
        {
            if (mechOptions.SingletonBufMech == null) return;
            if (Model.Owner.HasBuff(mechOptions.SingletonBufMech.Buff.GetType(), out var buff))
            {
                mechOptions.SingletonBufMech.Buff = buff;
            }
            else
            {
                mechOptions.SingletonBufMech.Buff =
                    (BattleUnitBuf)Activator.CreateInstance(mechOptions.SingletonBufMech.Buff.GetType());
                Model.Owner.bufListDetail.AddBuf(mechOptions.SingletonBufMech.Buff);
            }

            if (mechOptions.SingletonBufMech.MaxStackOnPhaseChange) mechOptions.SingletonBufMech.Buff.OnAddBuf(9999);
        }

        public virtual bool OnUseMechBuffAttackCard(BattlePlayingCardDataInUnitModel card)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return false;
            if (mechOptions.SingletonBufMech == null) return false;
            if (!mechOptions.SingletonBufMech.MassAttackCards.Exists(x => x.CardId == card.card.GetID())) return false;
            //mechOptions.SingletonBufMech.Buff.OnAddBuf(-9999);
            mechOptions.SingletonBufMech.Buff.stack = 0;
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

        public new virtual void ExtraMethodOnRoundEndTheLast()
        {
        }

        public new virtual void ExtraMethodOnOtherUnitDie(BattleUnitModel unit)
        {
        }

        public new virtual void ExtraMethodOnKill(BattleUnitModel unit)
        {
        }

        public override void ReviveCheck()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (!mechOptions.MechOnDeath) return;
            UnitUtil.UnitReviveAndRecovery(Model.Owner, Model.RecoverHpOnRevive, Model.RecoverLightOnSurvive);
            if (Model.ReviveAbDialogList.Any())
                if (Model.ReviveAbDialogCustomColor.HasValue)
                    UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.ReviveAbDialogList,
                        Model.ReviveAbDialogCustomColor.Value);
                else
                    UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.ReviveAbDialogList,
                        Model.ReviveAbDialogColor);
            if (Model.ForceRetreatOnRevive) Model.Owner.forceRetreat = true;
            if (!Model.EgoOptions.TryGetValue(Model.EgoPhase, out var egoOptions)) return;
            if (egoOptions.ActiveEgoOnDeath) EgoActive();
        }

        public int GetMapPhase(int phase)
        {
            if (Model.MechOptions == null ||
                !Model.MechOptions.TryGetValue(phase, out var mechPhaseOptions)) return -1;
            if (phase == 0 && !mechPhaseOptions.HasCustomMap) return -1;
            if (mechPhaseOptions.HasCustomMap) return mechPhaseOptions.MapOrderIndex;
            var subPhase = Model.MechOptions.Where(x => x.Key < phase).Reverse().Any(x => x.Value.HasCustomMap);
            if (!subPhase) return -1;
            return (from phaseOption in Model.MechOptions.Where(x => x.Key < phase).Reverse()
                where phaseOption.Value.HasCustomMap
                select phaseOption.Value.MapOrderIndex).FirstOrDefault();
        }
    }
}