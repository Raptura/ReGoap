using System;
using System.Collections.Generic;
using UnityEngine;
using MH;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    [ScriptOrder(-1)]
    public class CustomerMgr : Singleton<CustomerMgr>
    {
        private List<CustomerMB> _allCustomers = new List<CustomerMB>(); 

        public List<CustomerMB> allCustomers { get { return _allCustomers; } }

        public override void Init()
        {
            base.Init();

            GetComponentsInChildren(_allCustomers);
        }

    }
}
