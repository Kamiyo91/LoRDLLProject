using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_SupportChar_DLL4221 : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            ForceLoneFixerChange();
        }

        public void ForceLoneFixerChange()
        {
            PassiveUtil.ChangeLoneFixerPassive(owner.faction, new PassiveAbility_LoneFixer_DLL4221());
        }
    }
}