using System;
using System.Collections.Generic;
using UnityEngine;
using MH;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    [ScriptOrder(-1)]
    public class FactoryMgr : Singleton<FactoryMgr>
    {
        [SerializeField][Tooltip("")]
        private List<int> _featureCost4Level = new List<int>();

        [SerializeField][Tooltip("")]
        private InfoPopText _pfAddCashText;
        [SerializeField][Tooltip("")]
        private InfoPopText _pfSubCashText;

        private List<FactoryMB> _allFactories = new List<FactoryMB>();

        public List<FactoryMB> allFactories { get { return _allFactories; } }
        public List<int> featureCost4Level { get{ return _featureCost4Level; } set{ _featureCost4Level = value; } }
        public InfoPopText pfAddCashText { get{ return _pfAddCashText; } set{ _pfAddCashText = value; } }
        public InfoPopText pfSubCashText { get{ return _pfSubCashText; } set{ _pfSubCashText = value; } }

        public override void Init()
        {
            base.Init();
            
            Dbg.CAssert(this, _featureCost4Level.Count == FactoryMB.MAX_RD_LEVEL, "FactoryMgr.Init: featureCost4Level, expected {0}, get {1}", FactoryMB.MAX_RD_LEVEL, _featureCost4Level.Count);
            Dbg.CAssert(this, _pfAddCashText != null, "FactoryMgr.Init : not set _pfAddCashText");
            Dbg.CAssert(this, _pfSubCashText != null, "FactoryMgr.Init : not set _pfSubCashText");
            GetComponentsInChildren(_allFactories);
        }

    }
}
