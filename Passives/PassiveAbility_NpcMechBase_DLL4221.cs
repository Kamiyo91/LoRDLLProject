using System.Collections.Generic;
using System.Linq;
using BigDLL4221.BaseClass;
using BigDLL4221.Models;
using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_NpcMechBase_DLL4221 : PassiveAbilityBase
    {
        public NpcMechUtilBase Util;

        public override void OnWaveStart()
        {
            Util.Restart();
        }

        public override void OnRoundStart()
        {
            Util.UseSpecialBuffCard();
            Util.RoundStartBuffs();
            if (!Util.EgoCheck()) return;
            Util.EgoActive();
        }

        public override int SpeedDiceNumAdder()
        {
            return !Util.Model.MechOptions.TryGetValue(Util.Model.Phase, out var mechOptions)
                ? 0
                : mechOptions.SpeedDieAdder;
        }

        public override void OnRoundEnd()
        {
            Util.ExhaustEgoAttackCards();
            Util.SetOneTurnCard(false);
            Util.RaiseCounter();
        }

        public override void OnBattleEnd()
        {
            if (Util.CheckSkinChangeIsActive())
                if (ModParameters.KeypageOptions.TryGetValue(owner.Book.BookId.packageId, out var bookOptions))
                {
                    var bookOption = bookOptions.FirstOrDefault(x => x.KeypageId == owner.Book.BookId.id);
                    if (bookOption?.BookCustomOptions != null)
                        owner.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                            new List<string> { bookOption.BookCustomOptions.OriginalSkin };
                }

            Util.OnEndBattle();
        }

        public override BattleDiceCardModel OnSelectCardAuto(BattleDiceCardModel origin, int currentDiceSlotIdx)
        {
            Util.OnSelectCardPutMassAttack(ref origin);
            return base.OnSelectCardAuto(origin, currentDiceSlotIdx);
        }

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {
            Util.MechHpCheck(dmg);
            Util.SurviveCheck(dmg);
            return base.BeforeTakeDamage(attacker, dmg);
        }

        public override void OnStartBattle()
        {
            UnitUtil.RemoveImmortalBuff(owner);
        }

        public override void OnRoundEndTheLast()
        {
            Util.CheckPhase();
        }

        public override void OnDie()
        {
            if (!Util.Model.OnDeathOtherDies) return;
            foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction))
                unit.DieFake();
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            Util.OnUseCardResetCount(curCard);
            Util.ChangeToEgoMap(curCard.card.GetID());
        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            Util.ReturnFromEgoMap();
        }
    }
}