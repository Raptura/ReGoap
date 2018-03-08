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
    public class FactoryPayLoanGoal : ReGoapGoal<string, object>
    {
        private FactoryMB _factory;

        protected override void Awake()
        {
            base.Awake();
            _factory = this.AssertGetComponentInParent<FactoryMB>();

            goal.Set(GOAPStateName.PayLoan, true); 
        }

        public override bool IsGoalPossible()
        {
            if (_factory.loan <= 0 || _factory.cash <= 0)
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
