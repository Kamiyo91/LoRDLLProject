using UI;
using UnityEngine;

namespace BigDLL4221.Models
{
    public static class HSVColors
    {
        public static HSVColor Black = new HSVColor(0, 0, 0.05f);
        public static HSVColor White = new HSVColor(0, 0, 1);
        public static HSVColor Purple = new HSVColor(60, 1, 1);
        public static HSVColor Blue = new HSVColor(145, 1, 1);
        public static HSVColor Green = new HSVColor(220, 1, 1);
        public static HSVColor Yellow = new HSVColor(305, 1.5f, 1.2f);
    }

    public class HSVColor
    {
        public HSVColor(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }

        public float H { get; set; }
        public float S { get; set; }
        public float V { get; set; }
    }

    public static class LoRColorUtil
    {
        public static Color HighlightColor = UIColorManager.Manager.GetUIColor(UIColor.Highlighted);
    }
}