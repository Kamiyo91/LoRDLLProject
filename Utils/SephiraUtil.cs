using System;
using System.Linq;
using BigDLL4221.Models;
using TMPro;
using UI;

namespace BigDLL4221.Utils
{
    public static class SephiraUtil
    {
        public static void SetOperationPanel(UIOriginEquipPageSlot instance,
            UICustomGraphicObject button_Equip, TextMeshProUGUI txt_equipButton, BookModel bookDataModel)
        {
            if (bookDataModel == null || !ModParameters.PackageIds.Contains(bookDataModel.ClassInfo.id.packageId) ||
                instance.BookDataModel == null ||
                instance.BookDataModel.owner != null) return;
            var currentUnit = UI.UIController.Instance.CurrentUnit;
            if (currentUnit == null) return;
            if (!ModParameters.KeypageOptions.TryGetValue(bookDataModel.ClassInfo.id.packageId, out var keypageOptions))
                return;
            var keypage = keypageOptions.FirstOrDefault(x => x.KeypageId == bookDataModel.ClassInfo.id.id);
            if (keypage == null) return;
            if (!keypage.EveryoneCanEquip && keypage.SephirahType == currentUnit.OwnerSephirah && !currentUnit.isSephirah)
            {
                button_Equip.interactable = false;
                txt_equipButton.text = TextDataModel.GetText("ui_equippage_notequip", Array.Empty<object>());
                return;
            }

            if (!IsLockedCharacter(currentUnit)) return;
            button_Equip.interactable = true;
            txt_equipButton.text = TextDataModel.GetText("ui_bookinventory_equipbook", Array.Empty<object>());
        }

        private static bool IsLockedCharacter(UnitDataModel unitData)
        {
            return unitData.isSephirah && (unitData.OwnerSephirah == SephirahType.Binah ||
                                           unitData.OwnerSephirah == SephirahType.Keter);
        }
    }
}