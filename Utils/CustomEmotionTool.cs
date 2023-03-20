using BigDLL4221.GameObjectUtils;
using UnityEngine;

namespace BigDLL4221.Utils
{
    public static class CustomEmotionTool
    {
        public static SelectableEmotionCardsGameObject Script;

        static CustomEmotionTool()
        {
            var gameobj = new GameObject("CustomEmotionSelectionScreen_DLL4221");
            Script = gameobj.AddComponent<SelectableEmotionCardsGameObject>();
            Object.Instantiate(gameobj);
            Script.Init();
            gameobj.SetActive(false);
        }

        public static void SetParameters(CustomEmotionParameters parameters)
        {
            Script.gameObject.SetActive(true);
            Script.ChangeParametersValues(true, parameters);
            Script.ActiveEmotion();
        }
    }

    public class CustomEmotionParameters
    {
        public string PoolName { get; set; }
        public int EmotionLevel { get; set; }
        public LorId BookId { get; set; }
        public bool IsOnlyForUser { get; set; }
    }
}