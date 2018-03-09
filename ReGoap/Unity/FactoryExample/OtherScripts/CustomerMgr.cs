using System;
using System.Collections.Generic;
using UnityEngine;
using MH;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    [ScriptOrder(-1)]
    public class CustomerMgr : Singleton<CustomerMgr>
    {
        [SerializeField][Tooltip("")]
        private Material _matThinking;
        public Material matThinking { get{ return _matThinking; } set{ _matThinking = value; } }

        private List<CustomerMB> _allCustomers = new List<CustomerMB>(); 

        public List<CustomerMB> allCustomers { get { return _allCustomers; } }

        private void DoValidate()
        {
            Dbg.CAssert(this, _matThinking != null, "CustomerMgr.DoValidate : not set _matThinking");
        }

        public override void Init()
        {
            base.Init();
            DoValidate();

            GetComponentsInChildren(_allCustomers);
        }

    }
}
