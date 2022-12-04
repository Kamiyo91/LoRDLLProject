using BigDLL4221.Extensions;
using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_SupportChar_DLL4221 : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            PassiveUtil.ChangeLoneFixerPassive(owner.faction, new PassiveAbility_LoneFixer_DLL4221());
            if (owner.GetActivatedCustomEmotionCard("modId", 1, out var emotionCard))
            {
            }
        }
    }
}