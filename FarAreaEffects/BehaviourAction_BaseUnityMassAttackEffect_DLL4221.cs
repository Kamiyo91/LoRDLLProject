using System;
using BigDLL4221.Models;
using UnityEngine;

namespace BigDLL4221.FarAreaEffects
{
    public class BehaviourAction_BaseUnityMassAttackEffect_DLL4221 : BehaviourActionBase
    {
        public Type FarEffectType;

        public void SetParameters(Type farEffectType)
        {
            FarEffectType = farEffectType;
        }

        public override FarAreaEffect SetFarAreaAtkEffect(BattleUnitModel self)
        {
            _self = self;
            var massAttackEffect = new GameObject().AddComponent(FarEffectType) as FarAreaEffect;
            if (massAttackEffect != null)
                massAttackEffect.Init(self, Array.Empty<object>());
            return massAttackEffect;
        }
    }

    public class FarAreaEffect_BaseUnityMassAttackEffect_DLL4221 : FarAreaEffect
    {
        private bool _damaged;
        private GameObject _effect;
        private float _elapsed;
        public float Duration;
        public float EnemyHit;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float Scale;

        public void SetParameters(float positionX = 0f, float positionY = 9f, float positionZ = 2f,
            float scale = 1.2f, float duration = 6.3f, float enemyHit = 2.2f)
        {
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            Scale = scale;
            Duration = duration;
            EnemyHit = enemyHit;
            _damaged = false;
        }

        public override void Init(BattleUnitModel self, params object[] args)
        {
            base.Init(self, args);
            OnEffectStart();
            _elapsed = 0f;
            if (!ModParameters.AssetBundle.TryGetValue(GetType().Name.Replace("FarAreaEffect_", ""),
                    out var assetBundle)) return;
            for (var i = 0; i < assetBundle.GetAllAssetNames().Length; i++)
            {
                var assetName = assetBundle.GetAllAssetNames()[i];
                _effect = Instantiate(assetBundle.LoadAsset<GameObject>(assetName));
            }

            _effect.transform.SetParent(SingletonBehavior<BattleSceneRoot>.Instance.transform);
            _effect.transform.localPosition = new Vector3(PositionX, PositionY, PositionZ);
            foreach (var component in _effect.GetComponentsInChildren<Component>())
                component.transform.localScale *= Scale;
            _effect.layer = LayerMask.NameToLayer("Effect");
            _effect.SetActive(true);
        }

        protected override void Update()
        {
            base.Update();
            _elapsed += Time.deltaTime;
            if (_elapsed >= Duration)
            {
                OnEffectEnd();
                Destroy(_effect);
                //Destroy(camFilter); ?? camFilter not found?
                Destroy(gameObject);
            }

            if (!(_elapsed >= EnemyHit) || _damaged) return;
            _damaged = true;
            OnGiveDamage();
        }

        public override void OnGiveDamage()
        {
            base.OnGiveDamage();
            isRunning = false;
        }

        public override void OnEffectEnd()
        {
            _isDoneEffect = true;
        }
    }
}