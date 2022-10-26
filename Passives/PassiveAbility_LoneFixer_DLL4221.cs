using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_LoneFixer_DLL4221 : PassiveAbilityBase
    {
        public override void OnRoundEnd()
        {
            if (UnitUtil.SupportCharCheck(owner) == 1)
                owner.bufListDetail.AddKeywordBufByEtc(KeywordBuf.Strength, 3);
        }
    }
}