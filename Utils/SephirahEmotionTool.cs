using BigDLL4221.GameObjectUtils;
using UnityEngine;

namespace BigDLL4221.Utils
{
    public static class SephirahEmotionTool
    {
        public static SephirahSelectableEmotionCardsGameObject Script;

        static SephirahEmotionTool()
        {
            var gameobj = new GameObject("SephirahEmotionSelectionScreen_DLL4221");
            Script = gameobj.AddComponent<SephirahSelectableEmotionCardsGameObject>();
            Object.Instantiate(gameobj);
            Script.Init();
            gameobj.SetActive(false);
        }

        public static void SetParameters(SephirahType sephirah, int emotionLevel)
        {
            Script.gameObject.SetActive(true);
            Script.ChangeSephirahTypeValues(true, sephirah);
            Script.SetEmotionLevel(emotionLevel);
            Script.ActiveEmotion();
        }
    }
}