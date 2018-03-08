using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExtMethods;
using ReGoap.Unity.FactoryExample.Agents;
using MH;
using DG.Tweening;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    
    public class FactoryMB : MonoBehaviour
    {
        public const int ONE_LOAN = 100;
        public const int MAX_RD_LEVEL = 5;
        public static readonly string[] FeatureNames = new string[]
        { "WaterProof", "HiResCam", "BIG-SCREEN", "HIFI", "BigBattery", "HugeStorage", "Cool", "HiPerf", "GreatUX", "StrongSignal" };
        public static int FeatureCnt { get { return FeatureNames.Length; } }

        [SerializeField][Tooltip("")]
        private GameObject _pfStock;
        [SerializeField][Tooltip("the position new stock is spawned")]
        private Transform _trStockSite;
        [SerializeField][Tooltip("")]
        private Text _lblBalance;

        [SerializeField][Tooltip("")]
        private int _cash;
        public int cash { get { return _cash; } set { _cash = value; } }

        [SerializeField][Tooltip("")]
        private int _loan = 0;
        public int loan { get { return _loan; } set { _loan = value; } }

        [SerializeField][Tooltip("current R&D level")]
        private List<int> _RDlevels = new List<int>();
        public List<int> RDlevels { get { return _RDlevels; }  }

        [SerializeField][Tooltip("")]
        private List<Stock> _stocks = new List<Stock>();
        public List<Stock> stocks { get { return _stocks; } }

        [SerializeField][Tooltip("")]
        private int _maxStock = 3;
        public int maxStock { get { return _maxStock; } set { _maxStock = value; } }

        public int currentStock { get { return _stocks.Count; } }

        private FactoryAgent _facAgent;
        public FactoryAgent agent {get{return _facAgent;}}

        private Canvas _canvas;

        private Transform _tr;

        void Awake()
        {
            _tr = transform;
            _facAgent = this.AssertGetComponent<FactoryAgent>();
            _canvas = this.AssertGetComponentInChildren<Canvas>();

            Dbg.CAssert(this, _pfStock != null, "FactoryMB.Awake: _pfStock not set");
            Dbg.CAssert(this, _trStockSite != null, "FactroyMB.Awake: _trStockSite not set");
            Dbg.CAssert(this, _lblBalance != null, "FactoryMB.Awake : not set _lblBalance");
        }

        void Update()
        {
            _lblBalance.text = string.Format(" {0} / {1}", _cash, _loan);
        }

        public static string GetFeatureName(int idx)
        {
            return FeatureNames[idx];
        }

        public int GetCostForFeatures(List<int> featureIdxs)
        {
            var featCost4Level = FactoryMgr.Instance.featureCost4Level;
            int totalCost = 0;
            foreach( var aFeat in featureIdxs)
            {
                int rdLevel = _RDlevels[aFeat];
                int cost = featCost4Level[rdLevel-1];
                totalCost += cost;
            }
            return totalCost;
        }

        

        public Stock CreateNewStock(List<int> featureIdxs)
        {
            GameObject newStock = GameObject.Instantiate(_pfStock, _trStockSite.position + Vector3.up * 2f, Random.rotation);
            var newTr = newStock.transform;
            newTr.DOScale(Vector3.one * 0.1f, 0.2f).From();

            var stock = newStock.AssertGetComponent<Stock>();
            stock.SetFeatures(featureIdxs);

            int cost = GetCostForFeatures(featureIdxs);
            int price = Mathf.RoundToInt(cost * 1.5f);

            stock.cost = cost;
            stock.price = price;

            ModCash(-cost);

            _stocks.Add(stock);

            return stock;
        }

        ///<summary>
        /// modify the cash of this factory, 
        /// meanwhile show a pop text
        ///</summary>
        public void ModCash(int mod)
        {
            _cash += mod;

            InfoPopText.Create(mod, _canvas.transform);
        }

        ///<summary>
        /// get loan
        ///</summary>
        public void GetLoan(int v)
        {
            ModCash(v);
            _loan += v;
        }

        ///<summary>
        /// payback loan
        ///</summary>
        public void PayLoan(int v)
        {
            ModCash(-v);
            _loan -= v;
        }

        public void UpgradeFeature(int featIdx)
        {
            _RDlevels[featIdx] ++;
        }

        [ContextMenu("EDITOR Reset Features")]
        private void EDITOR_ResetFeatures()
        {
            _RDlevels.Clear();
            _RDlevels.SetValue(0, FeatureNames.Length, 1);
        }

    }

}
