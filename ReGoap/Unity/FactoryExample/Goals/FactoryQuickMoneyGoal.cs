using System;
using System.Collections.Generic;
using UnityEngine;
using ExtMethods;
using ReGoap.Unity.FactoryExample.OtherScripts;

using Random = UnityEngine.Random;
using ReGoap.Planner;
using ReGoap.Unity.FactoryExample.Planners;

namespace ReGoap.Unity.FactoryExample.Goals
{
    public class FactoryQuickMoneyGoal : ReGoapGoal<string, object>
    {
        private FactoryMB _factory;

        protected override void Awake()
        {
            base.Awake();
            _factory = this.AssertGetComponentInParent<FactoryMB>();

            goal.Set(GOAPStateName.QuickMoney, true);
        }

        public override bool IsGoalPossible()
        {
            return true;
        }

        public override float GetPriority()
        {
            return Random.value;
        }

        public override void Precalculations(IGoapPlanner<string, object> goapPlanner)
        {
            base.Precalculations(goapPlanner);

            Priority = Random.value;
        }

    }
}
