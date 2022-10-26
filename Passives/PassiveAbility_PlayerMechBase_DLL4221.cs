using System.Collections.Generic;
using System.Linq;
using BigDLL4221.BaseClass;
using BigDLL4221.Models;
using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_PlayerMechBase_DLL4221 : PassiveAbilityBase
    {
        public MechUtilBase Util;

        public override void OnBattleEnd()
        {
            if (!Util.CheckSkinChangeIsActive()) return;
            if (!ModParameters.KeypageOptions.TryGetValue(owner.Book.BookId.packageId, out var bookOptions)) return;
            var bookOption = bookOptions.FirstOrDefault(x => x.KeypageId == owner.Book.BookId.id);
            if (bookOption?.BookCustomOptions != null)
                owner.UnitData.unitData.bookItem.ClassInfo.CharacterSkin =
                    new List<string> { bookOption.BookCustomOptions.OriginalSkin };
        }

        public override void OnWaveStart()
        {
            if (UnitUtil.CheckSkinProjection(owner))
                Util.DoNotChangeSkinOnEgo();
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            Util.OnUseExpireCard(curCard.card.GetID());
            Util.ChangeToEgoMap(curCard.card.GetID());
        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            Util.DeactiveEgo();
            Util.ReturnFromEgoMap();
        }

        public override void OnRoundEndTheLast()
        {
            if (Util.EgoCheck()) Util.EgoActive();
        }

        public override void OnRoundEnd()
        {
            if (owner.faction != Faction.Enemy) return;
            if (Util.Model.EgoOptions == null) return;
            if (UnitUtil.SpecialCaseEgo(owner.faction, Util.Model.ThisPassiveId,
                    Util.Model.EgoOptions.EgoType.GetType())) Util.ForcedEgo();
        }

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {
            Util.SurviveCheck(dmg);
            return base.BeforeTakeDamage(attacker, dmg);
        }

        public override void OnRoundStartAfter()
        {
            Util.ReAddOnPlayCard();
        }

        public override void OnStartBattle()
        {
            UnitUtil.RemoveImmortalBuff(owner);
        }

        public override void OnRoundStart()
        {
            Util.EgoDurationCount();
            if (Util.EgoCheck()) Util.EgoActive();
        }

        public override void OnKill(BattleUnitModel target)
        {
            if (Util.CheckOnDieAtFightEnd())
                owner.Die();
        }

        public void ForcedEgo()
        {
            owner.personalEgoDetail.RemoveCard(Util.Model.EgoOptions.EgoCardId);
            Util.ForcedEgo();
        }

        public void SetDieAtEnd()
        {
            Util.TurnOnDieAtFightEnd();
        }
    }
}