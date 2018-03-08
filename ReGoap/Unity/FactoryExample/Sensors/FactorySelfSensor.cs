using ExtMethods;
using ReGoap.Core;
using ReGoap.Unity.FactoryExample.Agents;
using ReGoap.Unity.FactoryExample.OtherScripts;
using ReGoap.Unity.FactoryExample.Planners;
using System;
using System.Collections.Generic;

namespace ReGoap.Unity.FactoryExample.Sensors
{
    /// <summary>
    /// used to acquire self state into memory
    /// </summary>
    public class FactorySelfSensor : ReGoapSensor<string, object>
    {
        private FactoryMB _factory;

        void Awake()
        {
            _factory = this.AssertGetComponent<FactoryMB>();
        }

        public override void UpdateSensor()
        {
            var worldState = memory.GetWorldState();
            worldState.SetStructValue(GOAPStateName.Cash, StructValue.CreateIntArithmetic(_factory.cash));
            worldState.Set(GOAPStateName.HasStock, _factory.currentStock > 0);
            worldState.Set(GOAPStateName.HasStockSpace, _factory.currentStock < _factory.maxStock);
            worldState.Set(GOAPStateName.CanLoan, _factory.loan < 5 * FactoryMB.ONE_LOAN);
            worldState.Set(GOAPStateName.CanPayLoan, _factory.loan > 0 && _factory.cash >= FactoryMB.ONE_LOAN );
        }
    }


}
