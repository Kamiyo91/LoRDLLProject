using System.Linq;
using BigDLL4221.Models;

namespace BigDLL4221.Utils
{
    public static class PassiveUtil
    {
        public static void ChangeLoneFixerPassive(Faction unitFaction, PassiveAbilityBase passive)
        {
            foreach (var unit in BattleObjectManager.instance.GetAliveList(unitFaction))
            {
                if (!(unit.passiveDetail.PassiveList.Find(x => !x.destroyed && x is PassiveAbility_230008) is
                        PassiveAbility_230008
                        passiveLone)) continue;
                unit.passiveDetail.DestroyPassive(passiveLone);
                unit.passiveDetail.AddPassive(passive);
                //unit.passiveDetail.OnCreated();
            }
        }

        public static void ChangePassiveItem(string packageId)
        {
            if (!ModParameters.PassiveOptions.TryGetValue(packageId, out var passiveOptions)) return;
            foreach (var passive in PassiveXmlList.Instance.GetDataAll().Where(passive =>
                         passive.id.packageId == packageId))
            {
                var passiveOption =
                    passiveOptions.FirstOrDefault(x => x.PassiveId == passive.id.id && !x.IsBaseGamePassive);
                if (passiveOption == null) continue;
                passive.CanGivePassive = passiveOption.Transferable;
                if (passiveOption.InnerTypeId != 0) passive.InnerTypeId = passiveOption.InnerTypeId;
            }

            foreach (var passive in PassiveXmlList.Instance.GetDataAll()
                         .Where(passive => string.IsNullOrEmpty(passive.id.packageId)))
            {
                var passiveOption =
                    passiveOptions.FirstOrDefault(x => x.PassiveId == passive.id.id && x.IsBaseGamePassive);
                if (passiveOption == null || string.IsNullOrEmpty(passiveOption.PassiveScriptId)) continue;
                passive.script = passiveOption.PassiveScriptId;
            }
        }

        public static void AddPassiveItem(string packageId)
        {
            if (!ModParameters.CreatePassives.TryGetValue(packageId, out var passiveToCreate)) return;
            PassiveXmlList.Instance.AddPassivesByMod(passiveToCreate);
        }
    }
}