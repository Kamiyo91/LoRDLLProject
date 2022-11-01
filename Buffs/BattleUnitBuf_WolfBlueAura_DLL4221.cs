using BigDLL4221.Utils;
using Sound;

namespace BigDLL4221.Buffs
{
    public class BattleUnitBuf_WolfBlueAura_DLL4221 : BattleUnitBuf
    {
        public override void Init(BattleUnitModel owner)
        {
            base.Init(owner);
            InitAuraAndPlaySound();
        }

        private void InitAuraAndPlaySound()
        {
            SingletonBehavior<SoundEffectManager>.Instance.PlayClip("Battle/Kali_Change");
            ArtUtil.MakeEffect(_owner, "6/BigBadWolf_Emotion_Aura", 1f, _owner);
        }
    }
}