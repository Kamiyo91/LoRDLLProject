using System.Linq;
using BigDLL4221.Buffs;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using Sound;

namespace BigDLL4221.BaseClass
{
    public class NpcMechUtilBase : MechUtilBase
    {
        private readonly StageLibraryFloorModel _floor;
        public new NpcMechUtilBaseModel Model;

        public NpcMechUtilBase(NpcMechUtilBaseModel model) : base(model)
        {
            Model = model;
            _floor = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
        }

        public virtual void OnUseCardResetCount(BattlePlayingCardDataInUnitModel curCard)
        {
            if (!Model.LorIdEgoMassAttack.ContainsKey(curCard.card.GetID())) return;
            Model.Counter = 0;
            Model.Owner.allyCardDetail.ExhaustACardAnywhere(curCard.card);
        }

        public virtual void MechHpCheck(int dmg)
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            if (Model.Owner.hp - dmg > mechOptions.MechHp) return;
            Model.Phase++;
            Model.PhaseChanging = true;
            Model.Owner.SetHp(mechOptions.MechHp);
            Model.Owner.breakDetail.ResetGauge();
            Model.Owner.breakDetail.RecoverBreakLife(1, true);
            Model.Owner.breakDetail.nextTurnBreak = false;
            Model.Owner.bufListDetail.AddBufWithoutDuplication(new BattleUnitBuf_ImmortalUntilRoundEnd_DLL4221());
        }

        public override void SurviveCheck(int dmg)
        {
            if (Model.Owner.hp - dmg > Model.SurviveHp || !Model.Survive) return;
            Model.Survive = false;
            if (Model.ReloadMassAttackOnLethal) SetCounter(Model.MaxCounter);
            Model.Owner.SetHp(Model.RecoverToHp);
            Model.Owner.breakDetail.ResetGauge();
            Model.Owner.breakDetail.RecoverBreakLife(1, true);
            Model.Owner.breakDetail.nextTurnBreak = false;
            Model.Owner.bufListDetail.AddBufWithoutDuplication(new BattleUnitBuf_ImmortalUntilRoundEnd_DLL4221());
            if (Model.SurviveAbDialogList.Any())
                UnitUtil.BattleAbDialog(Model.Owner.view.dialogUI, Model.SurviveAbDialogList,
                    Model.SurviveAbDialogColor);
            if (Model.NearDeathBuffType != null)
                Model.Owner.bufListDetail.AddBufWithoutDuplication(Model.NearDeathBuffType);
        }

        public virtual int AlwaysAimToTheSlowestDice(BattleUnitModel target)
        {
            var speedValue = 999;
            var finalTarget = 0;
            foreach (var dice in target.speedDiceResult.Select((x, i) => new { i, x }))
            {
                if (speedValue <= dice.x.value) continue;
                speedValue = dice.x.value;
                finalTarget = dice.i;
            }

            return finalTarget;
        }

        public virtual void RaiseCounter()
        {
            if (Model.MassAttackStartCount && Model.Counter < Model.MaxCounter) Model.Counter++;
        }

        public virtual void AddAdditionalPassive()
        {
            if (Model.EgoOptions == null) return;
            foreach (var passiveId in Model.EgoOptions.AdditionalPassiveIds)
                Model.Owner.passiveDetail.AddPassive(passiveId);
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
            Model.Counter = value;
        }

        public virtual void OnSelectCardPutMassAttack(ref BattleDiceCardModel origin)
        {
            if (!Model.MassAttackStartCount || Model.Counter < Model.MaxCounter || Model.OneTurnCard)
                return;
            origin = BattleDiceCardModel.CreatePlayingCard(
                ItemXmlDataList.instance.GetCardItem(
                    RandomUtil.SelectOne(Model.LorIdEgoMassAttack.Select(x => x.Key).ToList())));
            SetOneTurnCard(true);
        }

        public virtual void ExhaustEgoAttackCards()
        {
            var cards = Model.Owner.allyCardDetail.GetAllDeck()
                .Where(x => Model.LorIdEgoMassAttack.ContainsKey(x.GetID()));
            foreach (var card in cards) Model.Owner.allyCardDetail.ExhaustACardAnywhere(card);
        }

        public virtual BattleUnitModel IgnoreSephiraSelectionTarget(LorId cardId)
        {
            if (!Model.LorIdEgoMassAttack.TryGetValue(cardId, out var attackOptions)) return null;
            if (!attackOptions.IgnoreSephirah) return null;
            if (BattleObjectManager.instance
                .GetAliveList(Faction.Player).Any(x => !x.UnitData.unitData.isSephirah))
                return RandomUtil.SelectOne(BattleObjectManager.instance.GetAliveList(Faction.Player)
                    .Where(x => !x.UnitData.unitData.isSephirah).ToList());
            return null;
        }

        public virtual void RoundStartBuffs()
        {
            if (!Model.MechOptions.TryGetValue(Model.Phase, out var mechOptions)) return;
            foreach (var buff in mechOptions.EachRoundStartBuffs)
                Model.Owner.bufListDetail.AddBuf(buff);
            foreach (var buff in mechOptions.EachRoundStartKeywordBuffs)
                Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
        }

        public virtual bool UseSpecialBuffCard()
        {
            if (Model.SpecialCardOptions == null) return false;
            if (Model.Owner.cardSlotDetail.PlayPoint < Model.SpecialCardOptions.SpecialCardCost || !Model.Owner
                    .bufListDetail
                    .GetActivatedBufList()
                    .Exists(x => Model.SpecialCardOptions.SpecialBufType.IsInstanceOfType(x))) return false;
            Model.Owner.bufListDetail.RemoveBufAll(Model.SpecialCardOptions.SpecialBufType);
            Model.Owner.cardSlotDetail.RecoverPlayPoint(-Model.SpecialCardOptions.SpecialCardCost);
            foreach (var buff in Model.SpecialCardOptions.Buffs)
                Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            return true;
        }

        public virtual void CheckPhase()
        {
            if (!Model.PhaseChanging) return;
            Model.PhaseChanging = false;
            if (!Model.MechOptions.TryGetValue(Model.Phase - 1, out var mechOptions)) return;
            if (mechOptions.ForceEgo) ForcedEgo();
            SetMassAttack(mechOptions.StartMassAttack);
            if (mechOptions.SetCounterToMax) SetCounter(Model.MaxCounter);
            if (mechOptions.ChangeCardCost)
                UnitUtil.ChangeCardCostByValue(Model.Owner, mechOptions.LoweredCost, mechOptions.MaxCost, true);
            foreach (var buff in mechOptions.Buffs)
                Model.Owner.bufListDetail.AddBuf(buff);
            foreach (var buff in mechOptions.OneRoundBuffs)
                Model.Owner.bufListDetail.AddKeywordBufThisRoundByEtc(buff.Key, buff.Value, Model.Owner);
            foreach (var passiveId in mechOptions.AdditionalPassiveIds)
                Model.Owner.passiveDetail.AddPassive(passiveId);
            if (mechOptions.RemoveOtherUnits)
                foreach (var unit in BattleObjectManager.instance.GetList(Model.Owner.faction)
                             .Where(x => x != Model.Owner))
                    BattleObjectManager.instance.UnregisterUnit(unit);
            foreach (var unitModel in mechOptions.SummonUnit)
                UnitUtil.AddNewUnitWithDefaultData(_floor, unitModel,
                    BattleObjectManager.instance.GetList(Model.Owner.faction).Count, true, false);
            foreach (var unitModel in mechOptions.SummonPlayerUnit)
                UnitUtil.AddNewUnitWithDefaultData(_floor, unitModel,
                    BattleObjectManager.instance.GetList(UnitUtil.ReturnOtherSideFaction(Model.Owner.faction)).Count);
            foreach (var path in mechOptions.SoundEffectPath.Where(x => !string.IsNullOrEmpty(x)))
                SoundEffectPlayer.PlaySound(path);
        }

        public virtual void OnEndBattle()
        {
            var stageModel = Singleton<StageController>.Instance.GetStageModel();
            var currentWaveModel = Singleton<StageController>.Instance.GetCurrentWaveModel();
            if (currentWaveModel == null || currentWaveModel.IsUnavailable()) return;
            stageModel.SetStageStorgeData(Model.SaveDataId, Model.Phase);
            var list = BattleObjectManager.instance.GetAliveList(Model.Owner.faction)
                .Where(unit => !UnitUtil.IsSupportCharCheck(unit)).Select(unit => unit.UnitData)
                .ToList();
            currentWaveModel.ResetUnitBattleDataList(list);
        }

        public virtual void Restart()
        {
            Singleton<StageController>.Instance.GetStageModel()
                .GetStageStorageData<int>(Model.SaveDataId, out var curPhase);
            Model.Phase = curPhase;
            if (Model.Phase < 1) return;
            Model.PhaseChanging = true;
            CheckPhase();
        }
    }
}