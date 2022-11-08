using Battle.DiceAttackEffect;
using BigDLL4221.Models;
using UnityEngine;

namespace BigDLL4221.DiceEffects
{
    public class DiceAttackEffect_BaseUnityEffect_DLL4221 : DiceAttackEffect
    {
        private BattleUnitView _targetView;
        public float? Duration;
        public GameObject GameObject;
        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public override void Initialize(BattleUnitView self, BattleUnitView target, float destroyTime)
        {
            base.Initialize(self, target, Duration ?? _destroyTime);
            _targetView = target;
        }

        public void SetParameters(float positionX = 0f, float positionY = 180f, float positionZ = 0f,
            float? duration = null)
        {
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            Duration = duration;
        }

        protected override void Start()
        {
            if (!ModParameters.AssetBundle.TryGetValue(GetType().Name.Replace("DiceAttackEffect_", ""),
                    out var assetBundle)) return;
            for (var i = 0; i < assetBundle.GetAllAssetNames().Length; i++)
            {
                var name = assetBundle.GetAllAssetNames()[i];
                GameObject = Instantiate(assetBundle.LoadAsset<GameObject>(name));
            }

            GameObject.transform.parent = _selfTransform;
            GameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            if (_self.view.WorldPosition.x > _targetView.WorldPosition.x)
                GameObject.transform.localEulerAngles = new Vector3(PositionX, PositionY, PositionZ);
            GameObject.layer = LayerMask.NameToLayer("Effect");
            GameObject.SetActive(true);
        }
    }
}