using ReGoap.Core;
using ReGoap.Unity.FactoryExample.Planners;
using System;
using System.Collections.Generic;
using MH;
using ExtMethods;
using System.Collections;
using ReGoap.Unity.FactoryExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FactoryExample.Actions
{
    public class FactoryBigSaleAction : ReGoapAction<string, object>
    {
        private FactoryMB _factory;

        protected override void Awake()
        {
            base.Awake();

            _factory = this.AssertGetComponentInParent<FactoryMB>();

            preconditions.Set(GOAPStateName.HasStock, true);
            effects.Set(GOAPStateName.QuickMoney, true);
            effects.Set(GOAPStateName.HasStock, false);
        }

        #region "ReGoapAction override"

        public override ReGoapState<string, object> GetEffects(
           ReGoapState<string, object> goalState,
           IReGoapAction<string, object> next = null)
        {
            return effects;
        }

        public override ReGoapState<string, object> GetPreconditions(
            ReGoapState<string, object> goalState,
            IReGoapAction<string, object> next = null)
        {
            return preconditions;
        }

        public override bool CheckProceduralCondition(
            IReGoapAgent<string, object> goapAgent,
            ReGoapState<string, object> goalState,
            IReGoapAction<string, object> next = null)
        {
            return true;
        }

        public override IReGoapActionSettings<string, object> GetSettings(
            IReGoapAgent<string, object> goapAgent,
            ReGoapState<string, object> goalState)
        {
            return settings;
        }

        public override void Run(
            IReGoapAction<string, object> previous, 
            IReGoapAction<string, object> next, 
            IReGoapActionSettings<string, object> settings, 
            ReGoapState<string, object> goalState, 
            Action<IReGoapAction<string, object>> done,
            Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);

            StartCoroutine(_CoRun());
        }

        #endregion "ReGoapAction override"

        #region "Methods"

        private IEnumerator _CoRun()
        {
            Info.Log(string.Format("Factory {0} starts a big sale...", _factory.name));

            yield return new WaitUntil(()=>Input.anyKeyDown);

            foreach(var aStock in _factory.stocks)
            {
                int halfPrice = Mathf.RoundToInt(aStock.price * 0.5f);
                Info.Log(string.Format("Factory {0} sells stock with reduced price: {1}", _factory.name, halfPrice));

                _factory.ModCash(halfPrice);

                yield return new WaitUntil( () => Input.anyKeyDown);
            }

            doneCallback(this);
        }

        #endregion "Methods"
    }
}
