using UnityEngine;
using UnityEngine.UI;
using ExtMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using MH;
using ReGoap.Unity.FactoryExample.Planners;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    // the main flow control of FactoryExample
    public class FactoryExampleCtrl : MonoBehaviour
    {
        #region "conf data" 
        
        public Button _btnGO;

        #endregion "conf data" 
        
        #region "data" 
        
        #endregion "data" 
        
        #region "prop" 
        
        #endregion "prop" 
        
        #region "unity methods" 
        
        void Awake()
        {
            Dbg.CAssert( this, _btnGO != null, "FactoryExampleCtrl.Awake: not set _btnGO");
        }

        #endregion "unity methods" 

        #region "UI callback"
        
        public void OnClickBtnGO()
        {
            StartCoroutine(_CoProceed());
        }

        #endregion
        
        #region "methods" 
        
        private IEnumerator _CoProceed()
        {
            _btnGO.interactable = false;

            var allFactories = FactoryMgr.Instance.allFactories;
            var allCustomers = CustomerMgr.Instance.allCustomers;

            // Factory pay interest
            foreach(var aFac in allFactories)
            {
                int interest = Mathf.RoundToInt(aFac.loan * 0.1f);

                Info.Log(string.Format("Factory {0} pays interest {1}", aFac.name, interest) );
                yield return new WaitForInput();

                aFac.ModCash(-interest);
            }

            // Factory make decisions
            foreach(var aFac in allFactories)
            {
                Info.Log($"Factory {aFac.name} is ready to move...");

                yield return new WaitForInput();

                aFac.agent.DoPlan();

                yield return new WaitForInput();
    
            }

            // Customer make decisions
            foreach(var aCus in allCustomers)
            {
                var oldMat = aCus.GetMat();
                aCus.SetMat(CustomerMgr.Instance.matThinking);

                aCus.Act();

                yield return new WaitForInput();

                aCus.SetMat(oldMat);
            }

            _btnGO.interactable = true;
        }

        #endregion "methods" 
    }
}