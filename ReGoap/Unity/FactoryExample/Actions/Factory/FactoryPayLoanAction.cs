using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using ReGoap.Unity.FactoryExample.OtherScripts;
using ExtMethods;
using ReGoap.Unity.FactoryExample.Planners;

namespace ReGoap.Unity.FactoryExample.Actions
{
    public class FactoryPayLoanAction : ReGoapAction<string, object>
    {
        private FactoryMB _factory;

        #region "Unity methods"
        
        protected override void Awake()
        {
            base.Awake();

            _factory = this.AssertGetComponentInParent<FactoryMB>();
            
            preconditions.Set(GOAPStateName.CanPayLoan, true);
            effects.Set(GOAPStateName.PayLoan, true);
        }

        #endregion "Unity methods"

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

            StartCoroutine( _CoRun() );
        }

        #endregion "ReGoapAction override"

        #region "Methods"
        
        private IEnumerator _CoRun()
        {
            Info.Log(string.Format("Factory {0} is going to payback a loan", _factory.name) );

            yield return new WaitUntil( () => Input.anyKeyDown );

            _factory.PayLoan(FactoryMB.ONE_LOAN);

            doneCallback(this);
        }

        #endregion "Methods"
    }

}
