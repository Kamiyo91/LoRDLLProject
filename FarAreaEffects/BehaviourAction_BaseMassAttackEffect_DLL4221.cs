using System;
using System.Collections.Generic;
using System.IO;
using Battle.DiceAttackEffect;
using BigDLL4221.Utils;
using Sound;
using UnityEngine;

namespace BigDLL4221.FarAreaEffects
{
    public class BehaviourAction_BaseMassAttackEffect_DLL4221 : BehaviourActionBase
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

    public class FarAreaEffect_BaseMassAttackEffect_DLL4221 : FarAreaEffect
    {
        private string _attackEffect;
        private float _attackEffectScale;

        private ActionDetail _attackMotion;
        private string _audioFileName;

        private ActionDetail _beforeMotion;

        private CameraFilterPack_FX_EarthQuake _camFilter;
        private bool _characterMove;
        private float _elapsed;
        private bool _followUnits;
        private bool _isBaseGameAudio;
        private bool _slowMotion;
        private bool _zoom;

        public void SetParameters(ActionDetail attackMotion, string audioFileName, string attackEffect,
            float attackEffectScale = 1f, bool isBaseGameAudio = false, bool slowMotion = true, bool zoom = true,
            bool characterMove = true, bool followUnits = true)
        {
            _attackMotion = attackMotion;
            _audioFileName = audioFileName;
            _attackEffect = attackEffect;
            _attackEffectScale = attackEffectScale;
            _isBaseGameAudio = isBaseGameAudio;
            _slowMotion = slowMotion;
            _zoom = zoom;
            _characterMove = characterMove;
            _followUnits = followUnits;
        }

        public override void Init(BattleUnitModel self, params object[] args)
        {
            base.Init(self, args);
            if (_characterMove) self.moveDetail.Move(Vector3.zero, 200f);
            OnEffectStart();
            _elapsed = 0f;
            Singleton<BattleFarAreaPlayManager>.Instance.SetActionDelay(0f);
            var list = new List<BattleUnitModel> { self };
            list.AddRange(
                BattleObjectManager.instance.GetAliveList(
                    self.faction == Faction.Enemy ? Faction.Player : Faction.Enemy));
            if (_followUnits) SingletonBehavior<BattleCamManager>.Instance.FollowUnits(false, list);
            _beforeMotion = ActionDetail.Default;
        }

        protected override void Update()
        {
            switch (state)
            {
                case EffectState.Start:
                {
                    if (_self.moveDetail.isArrived) state = EffectState.GiveDamage;
                    break;
                }
                case EffectState.GiveDamage:
                {
                    _elapsed += Time.deltaTime;
                    if (_elapsed >= 0.25f)
                    {
                        _beforeMotion = _self.view.charAppearance.GetCurrentMotionDetail();
                        _self.view.charAppearance.ChangeMotion(_attackMotion);
                        _elapsed = 0f;
                        isRunning = false;
                        state = EffectState.End;
                        if (_zoom)
                        {
                            var instance = SingletonBehavior<BattleCamManager>.Instance;
                            var camera = instance != null ? instance.EffectCam : null;
                            if (camera != null)
                                _camFilter = camera.gameObject.AddComponent<CameraFilterPack_FX_EarthQuake>();
                        }

                        if (_slowMotion) TimeManager.Instance.SlowMotion(0.25f, 0.125f, true);
                        var audioClip = UnitUtil.GetSound(_audioFileName, _isBaseGameAudio);
                        SingletonBehavior<SoundEffectManager>.Instance.PlayClip(audioClip);
                        SingletonBehavior<DiceEffectManager>.Instance.CreateBehaviourEffect(_attackEffect,
                            _attackEffectScale, _self.view, null);
                    }

                    break;
                }
                case EffectState.End:
                {
                    _elapsed += Time.deltaTime;
                    if (_camFilter != null && _zoom)
                    {
                        _camFilter.Speed = 30f * (1f - _elapsed);
                        _camFilter.X = 0.1f * (1f - _elapsed);
                        _camFilter.Y = 0.1f * (1f - _elapsed);
                    }

                    if (_elapsed > 1f)
                    {
                        if (_camFilter != null)
                        {
                            Destroy(_camFilter);
                            _camFilter = null;
                        }

                        _self.view.charAppearance.ChangeMotion(_beforeMotion);
                        state = EffectState.None;
                        _elapsed = 0f;
                    }

                    break;
                }
                case EffectState.None:
                {
                    if (_followUnits)
                        SingletonBehavior<BattleCamManager>.Instance.FollowUnits(false,
                            BattleObjectManager.instance.GetAliveList());
                    if (_self.view.FormationReturned) Destroy(gameObject);
                    break;
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_camFilter == null) return;
            Destroy(_camFilter);
            _camFilter = null;
        }
    }

    public class DiceAttackEffect_BaseAreaAtk_DLL4221 : DiceAttackEffect
    {
        public float? Duration;
        public float OffsetX;
        public float OffsetY;
        public string Path;
        public float PositionX;
        public float PositionY;
        public float Scale;

        public void SetParameters(string path, float positionX = 0.3f, float positionY = 0.6f, float scale = 1f,
            float? duration = null, float offsetX = -20f, float offsetY = 0.5f)
        {
            Path = path;
            PositionX = positionX;
            PositionY = positionY;
            OffsetX = offsetX;
            OffsetY = offsetY;
            Scale = scale;
            Duration = duration;
        }

        public override void Initialize(BattleUnitView self, BattleUnitView target, float destroyTime)
        {
            _self = self.model;
            var atkEffectRoot = self.atkEffectRoot;
            if (self.charAppearance != null)
                atkEffectRoot = self.charAppearance.atkEffectRoot;
            var texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(File.ReadAllBytes(Path + "/CustomEffect/" +
                                                  GetType().Name.Replace("DiceAttackEffect_", "") + ".png"));
            spr.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
                new Vector2(PositionX, PositionY));
            gameObject.layer = LayerMask.NameToLayer("Effect");
            ResetLocalTransform(transform);
            transform.parent = atkEffectRoot;
            transform.localScale = Vector3.one + additionalScale;
            transform.localPosition = Vector3.zero + new Vector3(OffsetX, OffsetY, 0f);
            transform.localRotation = Quaternion.identity;
            _destroyTime = Duration ?? destroyTime;
            animator.speed = 1f / _destroyTime;
            _elapsed = 0f;
        }

        public override void SetScale(float scaleFactor)
        {
            scaleFactor *= Scale;
            base.SetScale(scaleFactor);
        }
    }
}