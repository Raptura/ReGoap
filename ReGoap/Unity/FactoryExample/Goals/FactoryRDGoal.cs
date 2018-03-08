using System;
using System.Collections.Generic;
using UnityEngine;
using ExtMethods;
using ReGoap.Unity.FactoryExample.OtherScripts;

using ReGoap.Planner;
using ReGoap.Unity.FactoryExample.Planners;

using Random = UnityEngine.Random;

namespace ReGoap.Unity.FactoryExample.Goals
{
    public class FactoryRDGoal : ReGoapGoal<string, object>
    {
        private FactoryMB _factory;

        protected override void Awake()
        {
            base.Awake();
            _factory = this.AssertGetComponentInParent<FactoryMB>();

            goal.Set(GOAPStateName.RD, true);
        }

        public override bool IsGoalPossible()
        {
            var RDs = _factory.RDlevels;
            if (RDs.TrueForAll(x => x >= FactoryMB.MAX_RD_LEVEL))
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
