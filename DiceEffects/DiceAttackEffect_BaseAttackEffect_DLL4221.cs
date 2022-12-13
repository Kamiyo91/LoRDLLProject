using System.IO;
using Battle.DiceAttackEffect;
using UnityEngine;

namespace BigDLL4221.DiceEffects
{
    public class DiceAttackEffect_BaseAttackEffect_DLL4221 : DiceAttackEffect
    {
        private float _duration;
        public float? Duration;
        public bool FixedScale;
        public bool OverSelf;
        public string Path;
        public float PositionX;
        public float PositionY;
        public float Scale;

        public void SetParameters(string path, float positionX = 0.3f, float positionY = 0.6f, float scale = 0.5f,
            bool overSelf = true, float? duration = null, bool fixedScale = false)
        {
            Path = path;
            PositionX = positionX;
            PositionY = positionY;
            Scale = scale;
            OverSelf = overSelf;
            Duration = duration;
            FixedScale = fixedScale;
        }

        public override void Initialize(BattleUnitView self, BattleUnitView target, float destroyTime)
        {
            base.Initialize(self, target, destroyTime);
            _self = self.model;
            _selfTransform = self.atkEffectRoot;
            _targetTransform = OverSelf ? self.atkEffectRoot : target.atkEffectRoot;
            transform.parent = OverSelf ? self.charAppearance.transform : target.transform;
            _duration = Duration ?? _destroyTime;
            var texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(File.ReadAllBytes(Path + "/CustomEffect/" +
                                                  GetType().Name.Replace("DiceAttackEffect_", "") + ".png"));
            spr.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
                new Vector2(PositionX, PositionY));
            gameObject.layer = LayerMask.NameToLayer("Effect");
            ResetLocalTransform(transform);
        }


        public override void Update()
        {
            base.Update();
            _duration -= Time.deltaTime;
            spr.color = new Color(1f, 1f, 1f, _duration * 2f);
        }


        public override void SetScale(float scaleFactor)
        {
            base.SetScale(FixedScale ? Scale : scaleFactor * Scale);
        }
    }
}