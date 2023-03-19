using UI;

namespace BigDLL4221.Utils
{
    public static class UIPanelTool
    {
        public static T GetUIPanel<T>(UIPanelType type) where T : UIPanel
        {
            return UI.UIController.Instance.GetUIPanel(type) as T;
        }

        public static UIEnemyCharacterListPanel GetEnemyCharacterListPanel()
        {
            return GetUIPanel<UIEnemyCharacterListPanel>(UIPanelType.CharacterList);
        }

        public static UILibrarianCharacterListPanel GetLibrarianCharacterListPanel()
        {
            return GetUIPanel<UILibrarianCharacterListPanel>(UIPanelType.CharacterList_Right);
        }
    }
}