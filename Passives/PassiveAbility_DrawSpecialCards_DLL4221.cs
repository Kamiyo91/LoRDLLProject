using System.Collections.Generic;
using System.Linq;
using BigDLL4221.Buffs;
using HarmonyLib;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_DrawSpecialCards_DLL4221 : PassiveAbilityBase
    {
        public List<LorId> CardIds;

        public void SetCards(List<LorId> cardsIds)
        {
            CardIds = cardsIds;
        }

        public override void OnRoundStartAfter()
        {
            var cardNumber = RandomUtil.SelectOne(CardIds);
            var card = owner.allyCardDetail.AddNewCard(cardNumber);
            card.AddBuf(new BattleDiceCardBuf_TempCard_DLL4221());
        }

        public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
        {
            if (curCard.card.HasBuf<BattleDiceCardBuf_TempCard_DLL4221>())
                owner.allyCardDetail.ExhaustACardAnywhere(curCard.card);
        }

        public override void OnRoundEndTheLast()
        {
            owner.allyCardDetail.GetAllDeck()
                .Where(x => x.HasBuf<BattleDiceCardBuf_TempCard_DLL4221>())
                .Do(x => owner.allyCardDetail.ExhaustACardAnywhere(x));
        }
    }
}