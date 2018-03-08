using ExtMethods;
using ReGoap.Unity.FactoryExample.OtherScripts;
using UnityEngine;
using System;
using System.Collections.Generic;

using Random = UnityEngine.Random;
using ReGoap.Planner;
using ReGoap.Unity.FactoryExample.Planners;

namespace ReGoap.Unity.FactoryExample.Goals
{
    public class FactoryMakeGoal : ReGoapGoal<string, object>
    {
        private FactoryMB _factory;

        protected override void Awake()
        {
            base.Awake();
            _factory = this.AssertGetComponentInParent<FactoryMB>();

            goal.Set(GOAPStateName.MakeStock, true);
        }

        public override bool IsGoalPossible()
        {
            if (_factory.currentStock >= _factory.maxStock)
                return false;

            return true;
        }

        public override void Precalculations(IGoapPlanner<string, object> goapPlanner)
        {
            base.Precalculations(goapPlanner);

            Priority = Random.value; 
        }
    }
}
