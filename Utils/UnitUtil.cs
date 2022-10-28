using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Buffs;
using BigDLL4221.Enum;
using BigDLL4221.Models;
using HarmonyLib;
using LOR_DiceSystem;
using LOR_XML;
using TMPro;
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

        public static void RefreshCombatUI(bool forceReturn = false)
        {
            foreach (var (battleUnit, num) in BattleObjectManager.instance.GetList()
                         .Select((value, i) => (value, i)))
            {
                SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(battleUnit.UnitData.unitData, num,
                    true);
                if (forceReturn)
                    battleUnit.moveDetail.ReturnToFormationByBlink(true);
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

        public static void UnitReviveAndRecovery(BattleUnitModel owner, int hp, bool recoverLight,
            bool skinChanged = false)
        {
            if (owner.IsDead())
            {
                owner.bufListDetail.GetActivatedBufList()
                    .RemoveAll(x => !x.CanRecoverHp(999) || !x.CanRecoverBreak(999));
                owner.Revive(hp);
                owner.moveDetail.ReturnToFormationByBlink(true);
                owner.view.EnableView(true);
                if (skinChanged)
                    CheckSkinProjection(owner);
                else
                    owner.view.CreateSkin();
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

        public static void BattleAbDialog(BattleDialogUI instance, List<AbnormalityCardDialog> dialogs,
            AbColorType colorType)
        {
            var component = instance.GetComponent<CanvasGroup>();
            var dialog = dialogs[Random.Range(0, dialogs.Count)].dialog;
            var txtAbnormalityDlg = (TextMeshProUGUI)typeof(BattleDialogUI).GetField("_txtAbnormalityDlg",
                AccessTools.all)?.GetValue(instance);
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
                var canvas = (Canvas)typeof(BattleDialogUI).GetField("_canvas", AccessTools.all)?.GetValue(instance);
                if (canvas != null) canvas.enabled = true;
                component.interactable = true;
                component.blocksRaycasts = true;
                txtAbnormalityDlg.GetComponent<AbnormalityDlgEffect>().Init();
            }

            var _ = (Coroutine)typeof(BattleDialogUI).GetField("_routine",
                AccessTools.all)?.GetValue(instance);
            var method = typeof(BattleDialogUI).GetMethod("AbnormalityDlgRoutine", AccessTools.all);
            if (method != null) instance.StartCoroutine(method.Invoke(instance, Array.Empty<object>()) as IEnumerator);
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
            unit.bufListDetail.AddBuf(new BattleUnitBuf_UntargetableUntilRoundEnd_DLL4221 { CantMove = breakUnit });
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
                    !x.passiveDetail.PassiveList.Exists(y =>
                        ModParameters.PassiveOptions.Any(z =>
                            z.Key == y.id.packageId &&
                            z.Value.Any(v => v.PassiveId == y.id.id && !v.CountForSoloAbilities))));
        }

        public static bool IsSupportCharCheck(BattleUnitModel owner)
        {
            return owner.passiveDetail.PassiveList.Exists(x =>
                ModParameters.PassiveOptions.Any(y =>
                    y.Key == x.id.packageId && y.Value.Any(z => z.PassiveId == x.id.id && !z.CountForSoloAbilities)));
        }

        public static List<BattleUnitModel> ExcludeSupportChars(BattleUnitModel owner, bool otherSide = false)
        {
            return BattleObjectManager.instance
                .GetAliveList(otherSide ? ReturnOtherSideFaction(owner.faction) : owner.faction).Where(x =>
                    !x.passiveDetail.PassiveList.Exists(y =>
                        ModParameters.PassiveOptions.Any(z =>
                            z.Key == y.id.packageId &&
                            z.Value.Any(v => v.PassiveId == y.id.id && !v.CountForSoloAbilities))))
                .ToList();
        }

        public static void AddCustomUnits(StageLibraryFloorModel instance, StageModel stage,
            List<UnitBattleDataModel> unitList, LorId dictionaryId, string packageId)
        {
            if (!ModParameters.StageOptions.TryGetValue(dictionaryId.packageId, out var stageParameters)) return;
            var stageOptions = stageParameters.FirstOrDefault(x => x.StageId == dictionaryId.id);
            if (stageOptions?.PreBattleOptions == null ||
                !stageOptions.PreBattleOptions.Sephirah.Contains(instance.Sephirah)) return;
            var localizedTryGet = ModParameters.LocalizedItems.TryGetValue(packageId, out var localizedItem);
            foreach (var unitParameters in stageOptions.PreBattleOptions.UnitModels)
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
                unitDataModel.CreateDeckByDeckInfo();
                unitDataModel.forceItemChangeLock = true;
                if (!string.IsNullOrEmpty(unitParameters.SkinName))
                    unitDataModel.bookItem.ClassInfo.CharacterSkin = new List<string> { unitParameters.SkinName };
                var unitBattleDataModel = new UnitBattleDataModel(stage, unitDataModel);
                unitBattleDataModel.Init();
                unitList.Add(unitBattleDataModel);
            }
        }

        public static BattleUnitModel AddNewUnitPlayerSideCustomData(StageLibraryFloorModel floor, UnitModel unit,
            int pos, bool addEmotionPassives = true)
        {
            var unitData = new UnitDataModel((int)floor.Sephirah * 10, floor.Sephirah);
            var customBook = Singleton<BookInventoryModel>.Instance.GetBookListAll()
                .FirstOrDefault(x => x.BookId.Equals(new LorId(unit.PackageId, unit.Id)));
            if (customBook != null)
            {
                customBook.owner = null;
                unitData.EquipBook(customBook);
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
                : floor.GetFormationPosition(allyUnit.index);
            var unitBattleData = new UnitBattleDataModel(Singleton<StageController>.Instance.GetStageModel(), unitData);
            unitBattleData.Init();
            allyUnit.SetUnitData(unitBattleData);
            allyUnit.OnCreated();
            if (unit.SummonedOnPlay) allyUnit.speedDiceResult = new List<SpeedDice>();
            BattleObjectManager.instance.RegisterUnit(allyUnit);
            allyUnit.passiveDetail.OnUnitCreated();
            LevelUpEmotion(allyUnit, unit.EmotionLevel);
            if (unit.LockedEmotion)
                allyUnit.emotionDetail.SetMaxEmotionLevel(unit.MaxEmotionLevel);
            allyUnit.allyCardDetail.DrawCards(allyUnit.UnitData.unitData.GetStartDraw());
            allyUnit.cardSlotDetail.RecoverPlayPoint(allyUnit.cardSlotDetail.GetMaxPlayPoint());
            if (addEmotionPassives)
                AddEmotionPassives(allyUnit);
            allyUnit.OnWaveStart();
            if (!unit.SummonedOnPlay) return allyUnit;
            allyUnit.OnRoundStart_speedDice();
            allyUnit.RollSpeedDice();
            if (unit.AutoPlay) SetAutoCardForPlayer(allyUnit);
            return allyUnit;
        }

        public static BattleUnitModel AddNewUnitWithDefaultData(StageLibraryFloorModel floor, UnitModel unit, int pos,
            bool addEmotionPassives = true, bool playerSide = true)
        {
            var unitData = new UnitDataModel(new LorId(unit.PackageId, unit.Id),
                playerSide ? floor.Sephirah : SephirahType.None);
            unitData.SetCustomName(ModParameters.LocalizedItems.TryGetValue(unit.PackageId, out var localizedItem)
                ? localizedItem.EnemyNames.TryGetValue(unit.UnitNameId, out var name) ? name : unit.Name
                : unit.Name);
            var allyUnit = BattleObjectManager.CreateDefaultUnit(playerSide ? Faction.Player : Faction.Enemy);
            allyUnit.index = pos;
            allyUnit.grade = unitData.grade;
            allyUnit.formation = unit.CustomPos != null
                ? new FormationPosition(new FormationPositionXmlData
                {
                    vector = unit.CustomPos
                })
                : floor.GetFormationPosition(allyUnit.index);
            var unitBattleData = new UnitBattleDataModel(Singleton<StageController>.Instance.GetStageModel(), unitData);
            unitBattleData.Init();
            allyUnit.SetUnitData(unitBattleData);
            allyUnit.OnCreated();
            if (unit.SummonedOnPlay) allyUnit.speedDiceResult = new List<SpeedDice>();
            BattleObjectManager.instance.RegisterUnit(allyUnit);
            allyUnit.passiveDetail.OnUnitCreated();
            LevelUpEmotion(allyUnit, unit.EmotionLevel);
            if (unit.LockedEmotion)
                allyUnit.emotionDetail.SetMaxEmotionLevel(unit.MaxEmotionLevel);
            allyUnit.allyCardDetail.DrawCards(allyUnit.UnitData.unitData.GetStartDraw());
            allyUnit.cardSlotDetail.RecoverPlayPoint(allyUnit.cardSlotDetail.GetMaxPlayPoint());
            if (addEmotionPassives)
                AddEmotionPassives(allyUnit);
            allyUnit.OnWaveStart();
            if (!unit.SummonedOnPlay) return allyUnit;
            allyUnit.OnRoundStart_speedDice();
            allyUnit.RollSpeedDice();
            if (unit.AutoPlay) SetAutoCardForPlayer(allyUnit);
            return allyUnit;
        }

        public static void AddSephirahUnits(StageModel stage,
            List<UnitBattleDataModel> unitList, PreBattleOptions options)
        {
            unitList?.AddRange(options.SephirahUnits.Select(sephirah => InitUnitDefault(stage,
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

        public static bool SpecialCaseEgo(Faction unitFaction, LorId passiveId, Type buffType)
        {
            var playerUnit = BattleObjectManager.instance
                .GetAliveList(ReturnOtherSideFaction(unitFaction)).FirstOrDefault(x =>
                    x.passiveDetail.PassiveList.Exists(y => y.id == passiveId));
            return playerUnit != null && playerUnit.bufListDetail.GetActivatedBufList()
                .Exists(x => !x.IsDestroyed() && x.GetType() == buffType);
        }

        public static void RemoveImmortalBuff(BattleUnitModel owner)
        {
            if (owner.bufListDetail.GetActivatedBufList().Find(x => x is BattleUnitBuf_ImmortalUntilRoundEnd_DLL4221) is
                BattleUnitBuf_ImmortalUntilRoundEnd_DLL4221 buf)
                owner.bufListDetail.RemoveBuf(buf);
            if (owner.bufListDetail.GetActivatedBufList()
                    .Find(x => x is BattleUnitBuf_ImmunityToStatusAlimentUntilRoundEnd_DLL4221) is
                BattleUnitBuf_ImmunityToStatusAlimentUntilRoundEnd_DLL4221 buf2)
                owner.bufListDetail.RemoveBuf(buf2);
        }

        public static LorId LorIdMaker(string packageId, int itemId)
        {
            return new LorId(packageId, itemId);
        }
        public static void ChangeAtkSound(BattleUnitModel model, MotionDetail changeDetail, MotionSound motionSound)
        {
            var audioClipWin = GetSound(motionSound.FileNameWin,motionSound.IsBaseSoundWin);
            var audioClipLose = GetSound(motionSound.FileNameLose, motionSound.IsBaseSoundLose);
            var list = (List<CharacterSound.Sound>)model.view.charAppearance.soundInfo.GetType().GetField("_motionSounds", AccessTools.all).GetValue(model.view.charAppearance.soundInfo);
            var item = list.FirstOrDefault(x => x.motion == changeDetail);
            var sound = new CharacterSound.Sound
            {
                motion = changeDetail,
                winSound = audioClipWin,
                loseSound = audioClipLose
            };
            if (item != null)
                list.Remove(item);
            list.Add(sound);
            ((Dictionary<MotionDetail, CharacterSound.Sound>)model.view.charAppearance.soundInfo.GetType().GetField("_dic", AccessTools.all).GetValue(model.view.charAppearance.soundInfo))[changeDetail] = sound;
        }

        public static AudioClip GetSound(string audioName,bool isBaseGame)
        {
            if (string.IsNullOrEmpty(audioName)) return null;
            return isBaseGame ? Resources.Load<AudioClip>("Sounds/MotionSound/" + audioName) : CustomMapHandler.GetAudioClip(audioName);
        }
    }
}