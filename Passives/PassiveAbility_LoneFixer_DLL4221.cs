using BigDLL4221.Utils;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_LoneFixer_DLL4221 : PassiveAbility_SupportChar_DLL4221
    {
        public override void OnWaveStart()
        {
            switch (GlobalGameManager.Instance.CurrentOption.language)
            {
                case "en":
                    name = "Lone Fixer";
                    desc = "At the end of each Scene, gain 3 Strength if no other allies are present.";
                    break;
                case "cn":
                    name = "";
                    desc = "";
                    break;
                case "jp":
                    name = "孤独なフィクサー";
                    desc = "幕の終了時、他の味方がいなければパワー3を得る";
                    break;
                case "kr":
                    name = "고독한 해결사";
                    desc = "막 종료시 다른 아군이 없으면 다음 막에 힘 3을 얻음";
                    break;
                case "trcn":
                    name = "";
                    desc = "";
                    break;
            }
        }

        public override void OnRoundEnd()
        {
            if (UnitUtil.SupportCharCheck(owner) == 1)
                owner.bufListDetail.AddKeywordBufByEtc(KeywordBuf.Strength, 3);
        }
    }
}