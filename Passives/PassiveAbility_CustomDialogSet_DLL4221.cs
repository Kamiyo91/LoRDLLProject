using System.Linq;
using BigDLL4221.Models;

namespace BigDLL4221.Passives
{
    public class PassiveAbility_CustomDialogSet_DLL4221 : PassiveAbilityBase
    {
        private BattleDialogueModel _dlg;

        public override void OnWaveStart()
        {
            Hide();
            if (!ModParameters.KeypageOptions.TryGetValue(owner.Book.BookId.packageId, out var keypageOptions)) return;
            var keypageItem = keypageOptions.FirstOrDefault(x => x.KeypageId == owner.Book.BookId.id);
            if (keypageItem?.BookCustomOptions == null) return;
            _dlg = owner.UnitData.unitData.battleDialogModel;
            if (keypageItem.BookCustomOptions.CustomDialogId != null)
            {
                owner.UnitData.unitData.InitBattleDialogByDefaultBook(keypageItem.BookCustomOptions.CustomDialogId);
                return;
            }

            if (keypageItem.BookCustomOptions.CustomDialog == null) return;
            owner.UnitData.unitData.battleDialogModel =
                new BattleDialogueModel(keypageItem.BookCustomOptions.CustomDialog);
        }

        public override void OnBattleEnd()
        {
            owner.UnitData.unitData.battleDialogModel = _dlg;
        }
    }
}