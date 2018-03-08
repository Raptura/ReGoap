using System;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.Collections;
using MH;
using ReGoap.Unity.FactoryExample.Planners;

namespace ReGoap.Unity.FactoryExample.Actions
{
    public class FactoryIdleAction : ReGoapAction<string, object>
    {
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

        #region "methods"

        private IEnumerator _CoRun()
        {
            Info.Log("FactoryIdleAction: Idling...");

            yield return new WaitForInput();

            doneCallback(this);
        }    

        #endregion
        
    }
}
