using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_LoneFixer_DLL4221 : PassiveAbilityBase
    {
        public override void OnCreated()
        {
            rare = Rarity.Uncommon;
            name = Singleton<PassiveDescXmlList>.Instance.GetName(230008);
            desc = Singleton<PassiveDescXmlList>.Instance.GetDesc(230008);
        }

        public override void OnRoundEnd()
        {
            if (UnitUtil.SupportCharCheck(owner) < 2)
                owner.bufListDetail.AddKeywordBufByEtc(KeywordBuf.Strength, 3);
        }
    }
}