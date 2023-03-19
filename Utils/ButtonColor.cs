using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BigDLL4221.Utils
{
    public class ButtonColor : EventTrigger
    {
        public static Color OnEnterColor;

        public Color DefaultColor = new Color(1f, 1f, 1f);

        public Image Image;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            Image.color = OnEnterColor;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            Image.color = DefaultColor;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            Image.color = DefaultColor;
        }
    }
}