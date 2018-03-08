using System;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.Collections;
using ReGoap.Unity.FactoryExample.Planners;
using ReGoap.Unity.FactoryExample.OtherScripts;
using ExtMethods;

namespace ReGoap.Unity.FactoryExample.Actions
{ 
    public class FactoryGetLoanAction : ReGoapAction<string, object>
    {
        private FactoryMB _factory;

        #region "Unity methods"
        
        protected override void Awake()
        {
            base.Awake();

            _factory = this.AssertGetComponentInParent<FactoryMB>();

            preconditions.Set(GOAPStateName.CanLoan, true);
            effects.Set(GOAPStateName.QuickMoney, true);
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

            StartCoroutine(_CoRun());
        }



        #endregion "ReGoapAction override"

        #region "Methods"

        private IEnumerator _CoRun()
        {
            Info.Log(string.Format("Factory {0} is going to get {1} loan", _factory.name, FactoryMB.ONE_LOAN) );

            yield return new WaitUntil( () => Input.anyKeyDown );

            _factory.GetLoan(FactoryMB.ONE_LOAN);

            doneCallback(this);
        }            

        #endregion "Methods"

    }
}
