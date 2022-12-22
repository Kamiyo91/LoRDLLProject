using System;
using BigDLL4221.Utils;
using UnityEngine;

namespace BigDLL4221.GameObjectUtils
{
    public class ChangeDiceOrderGameObject : MonoBehaviour
    {
        private readonly StartBattleEffect effect = new StartBattleEffect();
        public Type DieAbilityType;
        public bool IsFirst;
        public BattleUnitModel Owner;

        public void SetParameters(BattleUnitModel owner, Type dieAbilityType, bool isFirst)
        {
            Owner = owner;
            DieAbilityType = dieAbilityType;
            IsFirst = isFirst;
        }

        public void Init()
        {
            effect.isDone = false;
            Singleton<StageController>.Instance.RegisterStartBattleEffect(effect);
        }

        private void FixedUpdate()
        {
            if (Singleton<StageController>.Instance.Phase != StageController.StagePhase.WaitStartBattleEffect) return;
            if (IsFirst) CardUtil.PutCounterDieAsFirst(Owner, DieAbilityType);
            else CardUtil.PutCounterDieAsLast(Owner, DieAbilityType);
            effect.isDone = true;
            Destroy(gameObject);
        }
    }
}