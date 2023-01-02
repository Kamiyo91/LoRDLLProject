using BigDLL4221.Models;
using BigDLL4221.Utils;
using CustomMapUtility;

namespace BigDLL4221.CardAbility
{
    public class DiceCardSelfAbility_EgoOneSceneNoMass_DLL4221 : DiceCardSelfAbilityBase
    {
        public bool MapActivated;
        public virtual MapModel MapModel => null;
        public virtual string SkinName => "";
        public virtual string PackageId => "";

        public override void OnUseCard()
        {
            if (!string.IsNullOrEmpty(SkinName))
            {
                owner.view.StartEgoSkinChangeEffect("Character");
                owner.view.SetAltSkin(SkinName);
            }

            ChangeToEgoMap();
        }

        public override void OnEndBattle()
        {
            if (string.IsNullOrEmpty(SkinName)) return;
            owner.view.StartEgoSkinChangeEffect("Character");
            owner.view.CreateSkin();
        }

        public void ChangeToEgoMap()
        {
            if (MapModel == null || SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.isEgo) return;
            if (MapUtil.ChangeMap(CustomMapHandler.GetCMU(PackageId), MapModel)) MapActivated = true;
        }

        public virtual void ReturnFromEgoMap()
        {
            MapActivated = false;
            if (MapModel == null) return;
            MapUtil.ReturnFromEgoMap(CustomMapHandler.GetCMU(PackageId), MapModel.Stage, MapModel.OriginalMapStageIds);
        }

        public override void OnRoundEnd(BattleUnitModel unit, BattleDiceCardModel self)
        {
            if (MapActivated) ReturnFromEgoMap();
        }
    }
}