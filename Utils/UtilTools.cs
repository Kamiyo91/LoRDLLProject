using UnityEngine;
using UnityEngine.UI;

namespace BigDLL4221.Utils
{
    public static class UtilTools
    {
        public static Button CreateButton(Transform parent, Sprite Image, Vector2 scale, Vector2 position)
        {
            var image = CreateImage(parent, Image, scale, position);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        public static Image CreateImage(Transform parent, Sprite Image, Vector2 scale, Vector2 position)
        {
            var gameObject = new GameObject("Image");
            var image = gameObject.AddComponent<Image>();
            image.transform.SetParent(parent);
            new Texture2D(2, 2); //??
            image.sprite = Image;
            image.rectTransform.sizeDelta = new Vector2(Image.texture.width, Image.texture.height);
            gameObject.SetActive(true);
            gameObject.transform.localScale = scale;
            gameObject.transform.localPosition = position;
            return image;
        }
    }
}