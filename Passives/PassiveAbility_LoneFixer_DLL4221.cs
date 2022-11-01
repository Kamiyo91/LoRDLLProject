using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_LoneFixer_DLL4221 : PassiveAbility_SupportChar_DLL4221
    {
        public override void OnWaveStart()
        {
        }

        public override void OnCreated()
        {
            name = Singleton<PassiveDescXmlList>.Instance.GetName(230008);
            desc = Singleton<PassiveDescXmlList>.Instance.GetDesc(230008);
        }

        public override void OnRoundEnd()
        {
            if (UnitUtil.SupportCharCheck(owner) == 1)
                owner.bufListDetail.AddKeywordBufByEtc(KeywordBuf.Strength, 3);
        }
    }
}