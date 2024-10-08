﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Buffs;
using BigDLL4221.Enum;
using BigDLL4221.Extensions;
using BigDLL4221.Models;
using BigDLL4221.Passives;
using CustomMapUtility;
using HarmonyLib;
using LOR_DiceSystem;
using LOR_XML;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BigDLL4221.Utils
{
    public static class UnitUtil
    {
        public static Faction ReturnOtherSideFaction(Faction faction)
        {
            return faction == Faction.Player ? Faction.Enemy : Faction.Player;
        }

        public static void PhaseChangeAllPlayerUnitRecoverBonus(int hp, int stagger, int light,
            bool fullLightRecover = false)
        {
            foreach (var unit in BattleObjectManager.instance.GetAliveList(Faction.Player))
            {
                unit.RecoverHP(hp);
                unit.breakDetail.RecoverBreak(stagger);
                var finalLightRecover = fullLightRecover ? unit.cardSlotDetail.GetMaxPlayPoint() : light;
                unit.cardSlotDetail.RecoverPlayPoint(finalLightRecover);
            }
        }

        public static void DrawUntilX(BattleUnitModel owner, int x)
        {
            var count = owner.allyCardDetail.GetHand().Count;
            var num = x - count;
            if (num > 0) owner.allyCardDetail.DrawCards(num);
        }

        public static void VipDeath(BattleUnitModel owner)
        {
            foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction)
                         .Where(x => x != owner))
                unit.Die();
            if (owner.faction == Faction.Enemy) return;
            StageController.Instance.GetCurrentStageFloorModel().Defeat();
            StageController.Instance.EndBattle();
        }

        public static void RefreshCombatUI(bool forceReturn = false, bool returnEffect = false)
        {
            foreach (var (battleUnit, num) in BattleObjectManager.instance.GetList()
                         .Select((value, i) => (value, i)))
            {
                SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(battleUnit.UnitData.unitData, num,
                    true);
                if (forceReturn)
                    battleUnit.moveDetail.ReturnToFormationByBlink(returnEffect);
            }

            try
            {
                BattleObjectManager.instance.InitUI();
            }
            catch (IndexOutOfRangeException)
            {
                // ignored
            }
        }

        public static void ChangeCardCostByValue(BattleUnitModel owner, int changeValue, int baseValue, bool startDraw)
        {
            foreach (var battleDiceCardModel in owner.allyCardDetail.GetAllDeck()
                         .Where(x => x.GetOriginCost() < baseValue))
            {
                battleDiceCardModel.GetBufList();
                battleDiceCardModel.AddCost(changeValue);
            }

            if (startDraw) owner.allyCardDetail.DrawCards(owner.UnitData.unitData.GetStartDraw());
        }

        public static void UnitReviveAndRecovery(BattleUnitModel owner, int hp, bool recoverLight)
        {
            hp = Mathf.Clamp(hp, 0, owner.MaxHp);
            if (owner.IsDead())
            {
                owner.bufListDetail.GetActivatedBufList()
                    .RemoveAll(x => !x.CanRecoverHp(999) || !x.CanRecoverBreak(999));
                owner.Revive(hp);
                owner.moveDetail.ReturnToFormationByBlink(true);
                owner.view.EnableView(true);
                owner.view.EnableStatNumber(true);
            }
            else
            {
                owner.bufListDetail.GetActivatedBufList()
                    .RemoveAll(x => !x.CanRecoverHp(999) || !x.CanRecoverBreak(999));
                owner.RecoverHP(hp);
            }

            owner.bufListDetail.RemoveBufAll(BufPositiveType.Negative);
            owner.bufListDetail.RemoveBufAll(typeof(BattleUnitBuf_sealTemp));
            owner.breakDetail.ResetGauge();
            owner.breakDetail.GetDefaultBreakGauge();
            owner.breakDetail.nextTurnBreak = false;
            owner.breakDetail.RecoverBreakLife(1, true);
            if (recoverLight) owner.cardSlotDetail.RecoverPlayPoint(owner.cardSlotDetail.GetMaxPlayPoint());
        }

        public static bool CheckSkinProjection(BattleUnitModel owner)
        {
            if (!string.IsNullOrEmpty(owner.UnitData.unitData.workshopSkin)) return true;
            if (owner.UnitData.unitData.bookItem == owner.UnitData.unitData.CustomBookItem) return false;
            owner.view.ChangeSkin(owner.UnitData.unitData.CustomBookItem.GetCharacterName());
            return true;
        }

        public static bool CheckSkinUnitData(UnitDataModel unitData)
        {
            if (!string.IsNullOrEmpty(unitData.workshopSkin)) return true;
            return unitData.bookItem != unitData.CustomBookItem;
        }

        public static bool CheckCardCost(BattleUnitModel owner, int baseValue)
        {
            return owner.allyCardDetail.GetAllDeck().Any(x => x.GetCost() > baseValue);
        }

        public static void LevelUpEmotion(BattleUnitModel owner, int value)
        {
            for (var i = 0; i < value; i++)
            {
                owner.emotionDetail.LevelUp_Forcely(1);
                owner.emotionDetail.CheckLevelUp();
            }

            StageController.Instance.GetCurrentStageFloorModel().team.UpdateCoin();
        }

        private static void AddEmotionPassives(BattleUnitModel unit)
        {
            var playerUnitsAlive = BattleObjectManager.instance.GetAliveList(Faction.Player);
            if (!playerUnitsAlive.Any()) return;
            foreach (var emotionCard in playerUnitsAlive.FirstOrDefault()
                         .emotionDetail.PassiveList.Where(x =>
                             x.XmlInfo.TargetType == EmotionTargetType.AllIncludingEnemy ||
                             x.XmlInfo.TargetType == EmotionTargetType.All))
            {
                if (unit.faction == Faction.Enemy &&
                    emotionCard.XmlInfo.TargetType == EmotionTargetType.All) continue;
                unit.emotionDetail.ApplyEmotionCard(emotionCard.XmlInfo);
            }
        }

        public static void SetAutoCardForPlayer(BattleUnitModel unit)
        {
            for (var j = 0; j < unit.speedDiceResult.Count; j++)
            {
                if (unit.speedDiceResult[j].breaked || unit.cardSlotDetail.cardAry[j] != null) continue;
                unit.cardOrder = j;
                unit.allyCardDetail.PlayTurnAutoForPlayer(j);
            }

            var selectedAllyDice = SingletonBehavior<BattleManagerUI>.Instance.selectedAllyDice;
            SingletonBehavior<BattleManagerUI>.Instance.ui_TargetArrow.UpdateTargetList();
            SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.UpdateCardsStateUI();
            SingletonBehavior<BattleManagerUI>.Instance.ui_unitInformationPlayer.ReleaseSelectedCard();
            SingletonBehavior<BattleManagerUI>.Instance.ui_unitInformationPlayer.CloseUnitInformation(true);
            SingletonBehavior<BattleManagerUI>.Instance.ui_unitCardsInHand.OnPointerOverInSpeedDice = null;
            SingletonBehavior<BattleManagerUI>.Instance.ui_unitCardsInHand.SetToDefault();
            if (selectedAllyDice != null) BattleUIInputController.Instance.ResetCharacterCursor(false);
        }

        public static void ReadyCounterCard(BattleUnitModel owner, int id, string packageId)
        {
            var card = BattleDiceCardModel.CreatePlayingCard(
                ItemXmlDataList.instance.GetCardItem(new LorId(packageId, id)));
            owner.cardSlotDetail.keepCard.AddBehaviours(card, card.CreateDiceCardBehaviorList());
            owner.allyCardDetail.ExhaustCardInHand(card);
        }

        public static void SetPassiveCombatLog(PassiveAbilityBase passive, BattleUnitModel owner)
        {
            var battleCardResultLog = owner.battleCardResultLog;
            battleCardResultLog?.SetPassiveAbility(passive);
        }

        //Not Working
        //public static void SetBuffCombatLog(BattleUnitBuf buf, BattleUnitModel owner)
        //{
        //    var battleCardResultLog = owner.battleCardResultLog;
        //    battleCardResultLog?.SetNewBufs(buf);
        //}

        public static void SetEmotionCombatLog(BattleEmotionCardModel emotionCard, BattleUnitModel owner)
        {
            owner.battleCardResultLog.SetEmotionAbility(true, emotionCard, emotionCard.XmlInfo.id);
        }

        public static void SetDieAbility(DiceCardAbilityBase ability, BattleUnitModel owner)
        {
            var battleCardResultLog = owner.battleCardResultLog;
            battleCardResultLog?.SetDiceBehaviourAbility(true, ability.behavior, ability.card.card);
        }

        public static void BattleAbDialog(BattleDialogUI instance, List<AbnormalityCardDialog> dialogs,
            AbColorType colorType)
        {
            var component = instance.GetComponent<CanvasGroup>();
            var dialog = dialogs[Random.Range(0, dialogs.Count)].dialog;
            var txtAbnormalityDlg = instance._txtAbnormalityDlg;
            if (txtAbnormalityDlg != null)
            {
                txtAbnormalityDlg.text = dialog;
                txtAbnormalityDlg.fontMaterial.SetColor("_GlowColor",
                    colorType == AbColorType.Negative
                        ? SingletonBehavior<BattleManagerUI>.Instance.negativeCoinColor
                        : SingletonBehavior<BattleManagerUI>.Instance.positiveCoinColor);
                txtAbnormalityDlg.color = colorType == AbColorType.Negative
                    ? SingletonBehavior<BattleManagerUI>.Instance.negativeTextColor
                    : SingletonBehavior<BattleManagerUI>.Instance.positiveTextColor;
                var canvas = instance._canvas;
                if (canvas != null) canvas.enabled = true;
                component.interactable = true;
                component.blocksRaycasts = true;
                txtAbnormalityDlg.GetComponent<AbnormalityDlgEffect>().Init();
            }

            var method = typeof(BattleDialogUI).GetMethod("AbnormalityDlgRoutine", AccessTools.all);
            if (method != null) instance.StartCoroutine(method.Invoke(instance, Array.Empty<object>()) as IEnumerator);
        }

        public static void BattleAbDialog(BattleDialogUI instance, List<AbnormalityCardDialog> dialogs,
            Color color)
        {
            var component = instance.GetComponent<CanvasGroup>();
            var dialog = dialogs[Random.Range(0, dialogs.Count)].dialog;
            var txtAbnormalityDlg = instance._txtAbnormalityDlg;
            if (txtAbnormalityDlg != null)
            {
                txtAbnormalityDlg.text = dialog;
                txtAbnormalityDlg.fontMaterial.SetColor("_GlowColor", color);
                txtAbnormalityDlg.color = color;
                var canvas = instance._canvas;
                if (canvas != null) canvas.enabled = true;
                component.interactable = true;
                component.blocksRaycasts = true;
                txtAbnormalityDlg.GetComponent<AbnormalityDlgEffect>().Init();
            }

            instance.AbnormalityDlgRoutine();
        }

        public static List<UnitBattleDataModel> UnitsToRecover(StageModel stageModel, UnitDataModel data,
            IEnumerable<SephirahType> unitTypes)
        {
            var list = new List<UnitBattleDataModel>();
            foreach (var sephirah in unitTypes)
                list.AddRange(stageModel.GetFloor(sephirah).GetUnitBattleDataList()
                    .Where(x => x.unitData == data));
            return list;
        }

        public static void RemoveDiceTargets(BattleUnitModel unit, bool breakUnit)
        {
            unit.view.speedDiceSetterUI.DeselectAll();
            foreach (var speedDice in unit.speedDiceResult)
                speedDice.breaked = true;
            unit.bufListDetail.AddBuf(new BattleUnitBuf_Untargetable_DLL4221(breakUnit));
            var actionableEnemyList = Singleton<StageController>.Instance.GetActionableEnemyList();
            if (unit.faction != Faction.Player)
                return;
            foreach (var actor in actionableEnemyList)
            {
                if (actor.turnState != BattleUnitTurnState.BREAK)
                    actor.turnState = BattleUnitTurnState.WAIT_CARD;
                try
                {
                    for (var index2 = 0; index2 < actor.speedDiceResult.Count; ++index2)
                    {
                        if (actor.speedDiceResult[index2].breaked || index2 >= actor.cardSlotDetail.cardAry.Count)
                            continue;
                        var cardDataInUnitModel = actor.cardSlotDetail.cardAry[index2];
                        if (cardDataInUnitModel?.card == null) continue;
                        if (cardDataInUnitModel.card.GetSpec().Ranged == CardRange.FarArea ||
                            cardDataInUnitModel.card.GetSpec().Ranged == CardRange.FarAreaEach)
                        {
                            if (cardDataInUnitModel.subTargets.Exists(x => x.target == unit))
                            {
                                cardDataInUnitModel.subTargets.RemoveAll(x => x.target == unit);
                            }
                            else if (cardDataInUnitModel.target == unit)
                            {
                                if (cardDataInUnitModel.subTargets.Count > 0)
                                {
                                    var subTarget = RandomUtil.SelectOne(cardDataInUnitModel.subTargets);
                                    cardDataInUnitModel.target = subTarget.target;
                                    cardDataInUnitModel.targetSlotOrder = subTarget.targetSlotOrder;
                                    cardDataInUnitModel.earlyTarget = subTarget.target;
                                    cardDataInUnitModel.earlyTargetOrder = subTarget.targetSlotOrder;
                                }
                                else
                                {
                                    actor.allyCardDetail.ReturnCardToHand(actor.cardSlotDetail.cardAry[index2].card);
                                    actor.cardSlotDetail.cardAry[index2] = null;
                                }
                            }
                        }
                        else
                        {
                            if (cardDataInUnitModel.subTargets.Exists(x => x.target == unit))
                                cardDataInUnitModel.subTargets.RemoveAll(x => x.target == unit);
                            if (cardDataInUnitModel.target == unit)
                            {
                                var targetByCard = BattleObjectManager.instance.GetTargetByCard(actor,
                                    cardDataInUnitModel.card, index2, actor.TeamKill());
                                if (targetByCard != null)
                                {
                                    var targetSlot = Random.Range(0, targetByCard.speedDiceResult.Count);
                                    var num = actor.ChangeTargetSlot(cardDataInUnitModel.card, targetByCard, index2,
                                        targetSlot, actor.TeamKill());
                                    cardDataInUnitModel.target = targetByCard;
                                    cardDataInUnitModel.targetSlotOrder = num;
                                    cardDataInUnitModel.earlyTarget = targetByCard;
                                    cardDataInUnitModel.earlyTargetOrder = num;
                                }
                                else
                                {
                                    actor.allyCardDetail.ReturnCardToHand(actor.cardSlotDetail.cardAry[index2].card);
                                    actor.cardSlotDetail.cardAry[index2] = null;
                                }
                            }
                            else if (cardDataInUnitModel.earlyTarget == unit)
                            {
                                var targetByCard = BattleObjectManager.instance.GetTargetByCard(actor,
                                    cardDataInUnitModel.card, index2, actor.TeamKill());
                                if (targetByCard != null)
                                {
                                    var targetSlot = Random.Range(0, targetByCard.speedDiceResult.Count);
                                    var num = actor.ChangeTargetSlot(cardDataInUnitModel.card, targetByCard, index2,
                                        targetSlot, actor.TeamKill());
                                    cardDataInUnitModel.earlyTarget = targetByCard;
                                    cardDataInUnitModel.earlyTargetOrder = num;
                                }
                                else
                                {
                                    cardDataInUnitModel.earlyTarget = cardDataInUnitModel.target;
                                    cardDataInUnitModel.earlyTargetOrder = cardDataInUnitModel.targetSlotOrder;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("target change error");
                }
            }

            unit.view.speedDiceSetterUI.BlockDiceAll(true);
            if (breakUnit) unit.view.speedDiceSetterUI.BreakDiceAll(true);
            SingletonBehavior<BattleManagerUI>.Instance.ui_TargetArrow.UpdateTargetList();
        }

        public static int SupportCharCheck(BattleUnitModel owner, bool otherSide = false)
        {
            return BattleObjectManager.instance
                .GetAliveList(otherSide ? ReturnOtherSideFaction(owner.faction) : owner.faction).Count(x =>
                    !x.passiveDetail.PassiveList.Any(y => y is PassiveAbility_SupportChar_DLL4221));
        }

        public static List<BattleUnitModel> ExcludeSupportChars(BattleUnitModel owner, bool otherSide = false)
        {
            return BattleObjectManager.instance
                .GetAliveList(otherSide ? ReturnOtherSideFaction(owner.faction) : owner.faction).Where(x =>
                    !x.passiveDetail.PassiveList.Exists(y => y is PassiveAbility_SupportChar_DLL4221))
                .ToList();
        }

        public static void AddCustomUnits(StageLibraryFloorModel instance, StageModel stage,
            List<UnitBattleDataModel> unitList, PreBattleOptions preBattleOptions, string packageId)
        {
            if (!preBattleOptions.CustomUnits.TryGetValue(instance.Sephirah, out var unitModels)) return;
            var localizedTryGet = ModParameters.LocalizedItems.TryGetValue(packageId, out var localizedItem);
            foreach (var unitParameters in unitModels)
            {
                var unitDataModel = new UnitDataModel(new LorId(packageId, unitParameters.Id),
                    instance.Sephirah, true);
                unitDataModel.SetTemporaryPlayerUnitByBook(new LorId(packageId,
                    unitParameters.Id));
                unitDataModel.bookItem.ClassInfo.categoryList.Add(BookCategory.DeckFixed);
                unitDataModel.isSephirah = false;
                unitDataModel.SetCustomName(localizedTryGet
                    ? localizedItem.EnemyNames.TryGetValue(unitParameters.UnitNameId, out var name)
                        ? name
                        : unitParameters.Name
                    : unitParameters.Name);
                if (unitParameters.AdditionalPassiveIds.Any())
                    foreach (var passive in unitParameters.AdditionalPassiveIds.Where(x =>
                                 !unitDataModel.bookItem.ClassInfo.EquipEffect.PassiveList.Contains(x)))
                        unitDataModel.bookItem.ClassInfo.EquipEffect.PassiveList.Add(passive);
                unitDataModel.CreateDeckByDeckInfo();
                unitDataModel.forceItemChangeLock = true;
                if (!string.IsNullOrEmpty(unitParameters.SkinName))
                    unitDataModel.bookItem.ClassInfo.CharacterSkin = new List<string> { unitParameters.SkinName };
                var unitBattleDataModel = new UnitBattleDataModel(stage, unitDataModel);
                unitBattleDataModel.Init();
                unitList.Add(unitBattleDataModel);
            }
        }

        public static void PreparePreBattleAllyUnits(StageLibraryFloorModel instance, List<UnitModel> unitModels,
            StageModel stage,
            List<UnitBattleDataModel> unitList)
        {
            foreach (var unitParameters in unitModels)
            {
                var localizedTryGet =
                    ModParameters.LocalizedItems.TryGetValue(unitParameters.PackageId, out var localizedItem);
                var unitDataModel = new UnitDataModel(new LorId(unitParameters.PackageId, unitParameters.Id),
                    instance.Sephirah, true);
                unitDataModel.SetTemporaryPlayerUnitByBook(new LorId(unitParameters.PackageId,
                    unitParameters.Id));
                unitDataModel.bookItem.ClassInfo.categoryList.Add(BookCategory.DeckFixed);
                unitDataModel.isSephirah = false;
                unitDataModel.SetCustomName(localizedTryGet
                    ? localizedItem.EnemyNames.TryGetValue(unitParameters.UnitNameId, out var name)
                        ? name
                        : unitParameters.Name
                    : unitParameters.Name);
                unitDataModel.CreateDeckByDeckInfo();
                unitDataModel.forceItemChangeLock = true;
                if (!string.IsNullOrEmpty(unitParameters.SkinName))
                    unitDataModel.bookItem.ClassInfo.CharacterSkin = new List<string> { unitParameters.SkinName };
                var unitBattleDataModel = new UnitBattleDataModel(stage, unitDataModel);
                unitBattleDataModel.Init();
                unitList.Add(unitBattleDataModel);
            }
        }

        public static void PreparePreBattleEnemyUnits(List<UnitModel> unitModels, StageModel stage,
            List<UnitBattleDataModel> unitList)
        {
            foreach (var unitParameters in unitModels)
            {
                var localizedTryGet =
                    ModParameters.LocalizedItems.TryGetValue(unitParameters.PackageId, out var localizedItem);
                var unitDataModel = new UnitDataModel(new LorId(unitParameters.PackageId, unitParameters.Id));
                unitDataModel.SetTemporaryPlayerUnitByBook(new LorId(unitParameters.PackageId,
                    unitParameters.Id));
                unitDataModel.bookItem.ClassInfo.categoryList.Add(BookCategory.DeckFixed);
                unitDataModel.isSephirah = false;
                unitDataModel.SetCustomName(localizedTryGet
                    ? localizedItem.EnemyNames.TryGetValue(unitParameters.UnitNameId, out var name)
                        ? name
                        : unitParameters.Name
                    : unitParameters.Name);
                unitDataModel.CreateDeckByDeckInfo();
                unitDataModel.forceItemChangeLock = true;
                if (!string.IsNullOrEmpty(unitParameters.SkinName))
                    unitDataModel.bookItem.ClassInfo.CharacterSkin = new List<string> { unitParameters.SkinName };
                var unitBattleDataModel = new UnitBattleDataModel(stage, unitDataModel);
                unitBattleDataModel.Init();
                unitList.Add(unitBattleDataModel);
            }
        }

        public static BattleUnitModel AddNewUnitPlayerSideCustomData(UnitModel unit,
            int pos, int emotionLevel = 0, bool addEmotionPassives = true, bool onWaveStartEffects = true)
        {
            var currentFloor = Singleton<StageController>.Instance.CurrentFloor;
            var unitData = new UnitDataModel((int)currentFloor * 10, currentFloor);
            var customBook = Singleton<BookInventoryModel>.Instance.GetBookListAll()
                .FirstOrDefault(x => x.BookId.Equals(new LorId(unit.PackageId, unit.Id)));
            if (customBook != null)
            {
                customBook.owner = null;
                unitData.EquipBook(customBook);
            }
            else
            {
                unitData = new UnitDataModel(new LorId(unit.PackageId, unit.Id), currentFloor);
            }

            unitData.SetCustomName(ModParameters.LocalizedItems.TryGetValue(unit.PackageId, out var localizedItem)
                ? localizedItem.EnemyNames.TryGetValue(unit.UnitNameId, out var name) ? name : unit.Name
                : unit.Name);
            var allyUnit = BattleObjectManager.CreateDefaultUnit(Faction.Player);
            allyUnit.index = pos;
            allyUnit.grade = unitData.grade;
            allyUnit.formation = unit.CustomPos != null
                ? new FormationPosition(new FormationPositionXmlData
                {
                    vector = unit.CustomPos
                })
                : Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetFormationPosition(allyUnit.index);
            var unitBattleData = new UnitBattleDataModel(Singleton<StageController>.Instance.GetStageModel(), unitData);
            unitBattleData.Init();
            allyUnit.SetUnitData(unitBattleData);
            if (unit.AdditionalPassiveIds.Any())
                foreach (var passiveId in unit.AdditionalPassiveIds.Where(x =>
                             !allyUnit.passiveDetail.PassiveList.Exists(y => y.id == x)))
                {
                    allyUnit.passiveDetail.AddPassive(passiveId);
                    allyUnit.passiveDetail.OnCreated();
                }

            allyUnit.OnCreated();
            if (unit.SummonedOnPlay) allyUnit.speedDiceResult = new List<SpeedDice>();
            BattleObjectManager.instance.RegisterUnit(allyUnit);
            allyUnit.passiveDetail.OnUnitCreated();
            LevelUpEmotion(allyUnit, emotionLevel);
            if (unit.LockedEmotion)
                allyUnit.emotionDetail.SetMaxEmotionLevel(unit.MaxEmotionLevel);
            allyUnit.allyCardDetail.DrawCards(allyUnit.UnitData.unitData.GetStartDraw());
            allyUnit.cardSlotDetail.RecoverPlayPoint(allyUnit.cardSlotDetail.GetMaxPlayPoint());
            if (addEmotionPassives)
                AddEmotionPassives(allyUnit);
            if (onWaveStartEffects) allyUnit.OnWaveStart();
            if (unit.AdditionalBuffs.Any())
                foreach (var buff in unit.AdditionalBuffs.Where(x => !allyUnit.HasBuff(x.GetType(), out _)))
                    allyUnit.bufListDetail.AddBuf(buff);
            if (unit.ForcedEgoOnStart && !unit.SummonedOnPlay)
                allyUnit.GetActivePassive<PassiveAbility_PlayerMechBase_DLL4221>()?.ForcedEgo(0);
            if (!unit.SummonedOnPlay) return allyUnit;
            allyUnit.OnRoundStart_speedDice();
            allyUnit.RollSpeedDice();
            if (unit.AutoPlay) SetAutoCardForPlayer(allyUnit);
            if (!unit.ForcedEgoOnStart) return allyUnit;
            var passive = allyUnit.GetActivePassive<PassiveAbility_PlayerMechBase_DLL4221>();
            if (passive == null) return allyUnit;
            passive.ForcedEgo(0);
            passive.Util.EgoActive();
            return allyUnit;
        }

        public static BattleUnitModel AddOriginalPlayerUnit(int index, int emotionLevel)
        {
            var allyUnit = Singleton<StageController>.Instance.CreateLibrarianUnit_fromBattleUnitData(index);
            allyUnit.OnWaveStart();
            allyUnit.allyCardDetail.DrawCards(allyUnit.UnitData.unitData.GetStartDraw());
            LevelUpEmotion(allyUnit, emotionLevel);
            allyUnit.cardSlotDetail.RecoverPlayPoint(allyUnit.cardSlotDetail.GetMaxPlayPoint());
            AddEmotionPassives(allyUnit);
            return allyUnit;
        }

        public static BattleUnitModel AddNewUnitWithDefaultData(UnitModel unit, int pos,
            bool addEmotionPassives = true, int emotionLevel = 0, Faction unitSide = Faction.Player,
            bool onWaveStartEffects = true)
        {
            var currentFloor = Singleton<StageController>.Instance.CurrentFloor;
            var unitData = new UnitDataModel(new LorId(unit.PackageId, unit.Id),
                unitSide == Faction.Player ? currentFloor : SephirahType.None);
            unitData.SetCustomName(ModParameters.LocalizedItems.TryGetValue(unit.PackageId, out var localizedItem)
                ? localizedItem.EnemyNames.TryGetValue(unit.UnitNameId, out var name) ? name : unit.Name
                : unit.Name);
            var allyUnit = BattleObjectManager.CreateDefaultUnit(unitSide);
            allyUnit.index = pos;
            allyUnit.grade = unitData.grade;
            allyUnit.formation = unit.CustomPos != null
                ? new FormationPosition(new FormationPositionXmlData
                {
                    vector = unit.CustomPos
                })
                : unitSide == Faction.Player
                    ? Singleton<StageController>.Instance.GetCurrentStageFloorModel()
                        .GetFormationPosition(allyUnit.index)
                    : Singleton<StageController>.Instance.GetCurrentWaveModel().GetFormationPosition(allyUnit.index);
            var unitBattleData = new UnitBattleDataModel(Singleton<StageController>.Instance.GetStageModel(), unitData);
            unitBattleData.Init();
            allyUnit.SetUnitData(unitBattleData);
            if (unit.AdditionalPassiveIds.Any())
                foreach (var passiveId in unit.AdditionalPassiveIds.Where(x =>
                             !allyUnit.passiveDetail.PassiveList.Exists(y => y.id == x)))
                {
                    allyUnit.passiveDetail.AddPassive(passiveId);
                    allyUnit.passiveDetail.OnCreated();
                }

            allyUnit.OnCreated();
            if (unit.SummonedOnPlay) allyUnit.speedDiceResult = new List<SpeedDice>();
            BattleObjectManager.instance.RegisterUnit(allyUnit);
            allyUnit.passiveDetail.OnUnitCreated();
            LevelUpEmotion(allyUnit, emotionLevel);
            if (unit.LockedEmotion)
                allyUnit.emotionDetail.SetMaxEmotionLevel(unit.MaxEmotionLevel);
            allyUnit.allyCardDetail.DrawCards(allyUnit.UnitData.unitData.GetStartDraw());
            allyUnit.cardSlotDetail.RecoverPlayPoint(allyUnit.cardSlotDetail.GetMaxPlayPoint());
            if (addEmotionPassives)
                AddEmotionPassives(allyUnit);
            if (onWaveStartEffects) allyUnit.OnWaveStart();
            if (unit.AdditionalBuffs.Any())
                foreach (var buff in unit.AdditionalBuffs.Where(x => !allyUnit.HasBuff(x.GetType(), out _)))
                    allyUnit.bufListDetail.AddBuf(buff);
            if (unit.ForcedEgoOnStart && !unit.SummonedOnPlay)
            {
                if (unitSide == Faction.Player)
                {
                    var passive = allyUnit.GetActivePassive<PassiveAbility_PlayerMechBase_DLL4221>();
                    if (passive != null)
                    {
                        passive.ForcedEgo(0);
                        passive.Util.EgoActive();
                    }
                }
                else
                {
                    var passive = allyUnit.GetActivePassive<PassiveAbility_NpcMechBase_DLL4221>();
                    if (passive == null) return allyUnit;
                    passive.Util.EgoActive();
                    passive.Util.Model.EgoPhase++;
                }
            }

            if (!unit.SummonedOnPlay) return allyUnit;
            allyUnit.OnRoundStart_speedDice();
            allyUnit.RollSpeedDice();
            if (unit.AutoPlay) SetAutoCardForPlayer(allyUnit);
            if (!unit.ForcedEgoOnStart) return allyUnit;
            if (unitSide == Faction.Player)
            {
                var passive = allyUnit.GetActivePassive<PassiveAbility_PlayerMechBase_DLL4221>();
                if (passive == null) return allyUnit;
                passive.ForcedEgo(0);
                passive.Util.EgoActive();
            }
            else
            {
                var passive = allyUnit.GetActivePassive<PassiveAbility_NpcMechBase_DLL4221>();
                if (passive == null) return allyUnit;
                passive.Util.EgoActive();
                passive.Util.Model.EgoPhase++;
            }

            return allyUnit;
        }

        public static void AddSephirahUnits(StageLibraryFloorModel instance, StageModel stage,
            List<UnitBattleDataModel> unitList, PreBattleOptions options)
        {
            if (!options.SephirahUnits.TryGetValue(instance.Sephirah, out var sephirahUnitTypes)) return;
            unitList?.AddRange(sephirahUnitTypes.Select(sephirah => InitUnitDefault(stage,
                LibraryModel.Instance.GetOpenedFloorList()
                    .FirstOrDefault(x => x.Sephirah == sephirah)
                    ?.GetUnitDataList()
                    .FirstOrDefault(y => y.isSephirah))));
        }

        public static UnitBattleDataModel InitUnitDefault(StageModel stage, UnitDataModel data)
        {
            var unitBattleDataModel = new UnitBattleDataModel(stage, data);
            unitBattleDataModel.Init();
            return unitBattleDataModel;
        }

        public static bool SpecialCaseEgo(Faction unitFaction, LorId passiveId, Dictionary<int, EgoOptions> egoOptions,
            out int egoPhase)
        {
            egoPhase = 0;
            var playerUnit = BattleObjectManager.instance
                .GetAliveList(ReturnOtherSideFaction(unitFaction)).FirstOrDefault(x =>
                    x.passiveDetail.PassiveList.Exists(y => y.id == passiveId));
            if (playerUnit == null) return false;
            var egoOption = egoOptions.FirstOrDefault(x => playerUnit.bufListDetail.GetActivatedBufList()
                .Exists(y => !y.IsDestroyed() && x.GetType() == x.Value.EgoType.GetType()));
            if (egoOption.Value == null) return false;
            egoPhase = egoOption.Key;
            return true;
        }

        public static void RemoveImmortalBuff(BattleUnitModel owner)
        {
            if (owner.bufListDetail.GetActivatedBufList().Find(x => x is BattleUnitBuf_Immortal_DLL4221) is
                BattleUnitBuf_Immortal_DLL4221 buf)
                if (buf.LastOneScene)
                    owner.bufListDetail.RemoveBuf(buf);
            if (!(owner.bufListDetail.GetActivatedBufList()
                        .Find(x => x is BattleUnitBuf_ImmunityToStatusAlimentType_DLL4221) is
                    BattleUnitBuf_ImmunityToStatusAlimentType_DLL4221 buf2)) return;
            if (buf2.LastOneScene) owner.bufListDetail.RemoveBuf(buf2);
        }

        public static void PrepareSounds(string packageId, List<CharacterSound.Sound> motionSounds,
            Dictionary<MotionDetail, CharacterSound.Sound> dicMotionSounds,
            Dictionary<MotionDetail, MotionSound> customMotionSounds)
        {
            var cmh = CustomMapHandler.GetCMU(packageId);
            foreach (var customMotionSound in customMotionSounds)
                try
                {
                    var audioClipWin = GetSound(cmh, customMotionSound.Value.FileNameWin,
                        customMotionSound.Value.IsBaseSoundWin);
                    var audioClipLose = GetSound(cmh, customMotionSound.Value.FileNameLose,
                        customMotionSound.Value.IsBaseSoundLose);
                    var item = motionSounds.FirstOrDefault(x => x.motion == customMotionSound.Key);
                    var sound = new CharacterSound.Sound
                    {
                        motion = customMotionSound.Key,
                        winSound = audioClipWin,
                        loseSound = audioClipLose
                    };
                    if (item != null)
                        motionSounds.Remove(item);
                    motionSounds.Add(sound);
                    if (dicMotionSounds.ContainsKey(customMotionSound.Key))
                        dicMotionSounds.Remove(customMotionSound.Key);
                    dicMotionSounds.Add(customMotionSound.Key, sound);
                }
                catch (Exception)
                {
                    // ignored
                }
        }

        public static AudioClip GetSound(CustomMapHandler cmh, string audioName, bool isBaseGame)
        {
            if (string.IsNullOrEmpty(audioName)) return null;
            if (isBaseGame) return Resources.Load<AudioClip>("Sounds/MotionSound/" + audioName);
            cmh.LoadEnemyTheme(audioName, out var audioClip);
            return audioClip;
        }

        public static BattleUnitModel IgnoreSephiraSelectionTarget(bool ignore)
        {
            if (!ignore) return null;
            if (BattleObjectManager.instance
                .GetAliveList(Faction.Player).Any(x => !x.UnitData.unitData.isSephirah))
                return RandomUtil.SelectOne(BattleObjectManager.instance.GetAliveList(Faction.Player)
                    .Where(x => !x.UnitData.unitData.isSephirah).ToList());
            return null;
        }

        public static int AlwaysAimToTheSlowestDice(BattleUnitModel target, int targetSlot, bool aim)
        {
            if (!aim) return targetSlot;
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

        public static void ApplyEmotionCards(BattleUnitModel unit, IEnumerable<BattleEmotionCardModel> emotionCardList)
        {
            foreach (var card in emotionCardList) unit.emotionDetail.ApplyEmotionCard(card.XmlInfo);
        }

        public static IEnumerable<BattleEmotionCardModel> GetEmotionCardByUnit(BattleUnitModel unit)
        {
            return unit.emotionDetail.PassiveList.ToList();
        }

        public static List<BattleEmotionCardModel> AddValueToEmotionCardList(
            IEnumerable<BattleEmotionCardModel> addedEmotionCards, List<BattleEmotionCardModel> savedEmotionCards,
            bool ignoreDuplication = false)
        {
            savedEmotionCards.AddRange(addedEmotionCards.Where(emotionCard =>
                !savedEmotionCards.Exists(x => x.XmlInfo.Equals(emotionCard.XmlInfo) || ignoreDuplication)));
            return savedEmotionCards;
        }

        public static bool NotTargetableCharCheck(BattleUnitModel target)
        {
            if (!ModParameters.KeypageOptions.TryGetValue(target.Book.BookId.packageId, out var keypageOptions))
                return true;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == target.Book.BookId.id);
            return keypageItem == null || keypageItem.TargetableBySpecialCards;
        }

        public static bool IsLocked(StageRequirements stageExtra)
        {
            if (stageExtra.RequiredLibraryLevel.HasValue &&
                LibraryModel.Instance.GetLibraryLevel() < stageExtra.RequiredLibraryLevel.Value)
                return true;
            return stageExtra.RequiredStageIds.Any(num => LibraryModel.Instance.ClearInfo.GetClearCount(num) <= 0);
        }

        public static bool CheckTargetSpeedByCard(BattlePlayingCardDataInUnitModel card, int value)
        {
            var speedDiceResultValue = card.speedDiceResultValue;
            var target = card.target;
            var targetSlotOrder = card.targetSlotOrder;
            if (targetSlotOrder < 0 || targetSlotOrder >= target.speedDiceResult.Count) return false;
            var speedDice = target.speedDiceResult[targetSlotOrder];
            var targetDiceBroken = target.speedDiceResult[targetSlotOrder].breaked;
            return speedDiceResultValue - speedDice.value > value || targetDiceBroken;
        }
    }
}