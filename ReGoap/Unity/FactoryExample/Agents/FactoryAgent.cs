using ReGoap.Unity.FactoryExample.Sensors;
using System;
using System.Collections.Generic;
using UnityEngine;
using ExtMethods;
using ReGoap.Unity.FactoryExample.OtherScripts;

namespace ReGoap.Unity.FactoryExample.Agents
{
    public class FactoryAgent : ReGoapAgent<string, object>
    {
        private FactorySelfSensor _selfSensor;
        private FactoryMB _factory;
        
        public FactoryMB factory { get{ return _factory; } set{ _factory = value; } }

        protected override void Awake()
        {
            base.Awake();
            _factory = this.AssertGetComponent<FactoryMB>();
            _selfSensor = this.AssertGetComponent<FactorySelfSensor>();
        }

        [ContextMenu("Do Plan")]
        public void DoPlan()
        {
            _selfSensor.UpdateSensor();

            allowPlanToken++; //enable plan for once
            CalculateNewGoal();
        }

        public bool isIdle {
            get{
                var curGoal = GetCurrentGoal();
                return curGoal?.GetPlan() == null || curGoal.GetPlan().Count == 0;
            }
        }
    }
}
