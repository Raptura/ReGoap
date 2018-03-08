using System;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using ReGoap.Unity.FactoryExample.Planners;
using ReGoap.Unity.FactoryExample.Agents;

using Random = UnityEngine.Random;
using ReGoap.Unity.FactoryExample.OtherScripts;
using System.Collections;
using ExtMethods;
using MH;

namespace ReGoap.Unity.FactoryExample.Actions
{
    public class FactoryMakeAction : ReGoapAction<string, object>
    {
        private FactoryMB _factory;
        private List<int> _featIdxLst = new List<int>(); //cached to-be-built features

        protected override void Awake()
        {
            base.Awake();
            _factory = this.AssertGetComponentInParent<FactoryMB>();
        }

        #region "ReGoapAction override"

        public override ReGoapState<string, object> GetEffects(
           ReGoapState<string, object> goalState,
           IReGoapAction<string, object> next = null)
        {
            effects.Set(GOAPStateName.MakeStock, true);
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
            // try max 3 times, random 2~5 features, try making maximum available volume
            for(int t = 0; t < 3; ++t)
            {
                int cnt = Random.Range(2, 5+1);

                _featIdxLst.Clear();
                CustomerMB.RandomGetFeatures(_featIdxLst, cnt);
                int cost = _factory.GetCostForFeatures(_featIdxLst);

                if( cost <= _factory.cash )
                {
                    return true;
                }
            }

            return false;
        }

        public override IReGoapActionSettings<string, object> GetSettings(
            IReGoapAgent<string, object> goapAgent,
            ReGoapState<string, object> goalState)
        {
            return base.GetSettings(goapAgent, goalState);
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

        #region "public methods"

        private IEnumerator _CoRun()
        {
            var newStock = _factory.CreateNewStock(_featIdxLst);

            Dbg.Log("{0} made new goods: price {1}, cost {2}, features: {3}", _factory.name, newStock.price, newStock.cost, Misc.ListToString(_featIdxLst));

            yield return new WaitUntil( () => Input.anyKeyDown );

            doneCallback(this);
        }

        #endregion
    }
}
