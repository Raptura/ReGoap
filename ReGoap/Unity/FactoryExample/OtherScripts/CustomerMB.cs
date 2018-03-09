using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExtMethods;
using MH;
using DG.Tweening;

using Random = UnityEngine.Random;
using ReGoap.Unity.FactoryExample.Planners;
using ReGoap.Unity.FactoryExample.Strategy;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    /// <summary>
    /// * will buy stock in a varying interval,
    /// * will buy stock with the highest "perceived-value / price"
    /// </summary>
    public class CustomerMB : MonoBehaviour
    {
        public static readonly List<int> IDX = new List<int>();

        [SerializeField][Tooltip("Every turn the chance for this customer to buy")]
        private float _chanceToBuy = 0.5f;
        [SerializeField][Tooltip("")]
        private int _favorFeaturesCnt = 5;
        [SerializeField][Tooltip("")]
        private List<int> _favoriteFeatures = new List<int>();
        [SerializeField][Tooltip("every feature in the favoriteFeatures got a value boost")]
        private List<int> _addFavorValues = new List<int>();
        [SerializeField][Tooltip("from 1 to MAX_LEVEL, the standard perceived value for a feature")]
        private List<int> _standardValueOnLevel = new List<int>();

        private List<int> _perceivedFeatureValues = new List<int>(); //cached calculate result
        
        private Predicate<int> _D_NotInFavorList;
        private Predicate<int> D_NotInFavorList { get { if (_D_NotInFavorList == null) { _D_NotInFavorList = _NotInFavorList; } return _D_NotInFavorList; } }

        public float chanceToBuy { get { return _chanceToBuy; } set { _chanceToBuy = value; } }

        private Transform _tr;
        public Transform tr { get{ return _tr; } set{ _tr = value; } }

        private ICustomerStrategy _strategy = null;

        static CustomerMB()
        {
            IDX.AddRangeValue(0, 9);
        }

        public static void RandomGetFeatures(List<int> outIdxs, int cnt)
        {
            IDX.RandomGetElem(outIdxs, cnt);            
        }

        void Awake()
        {
            _tr = transform;

            IDX.RandomGetElem(_favoriteFeatures, _favorFeaturesCnt);
            _favoriteFeatures.ShuffleList();

            Dbg.CAssert(this, _standardValueOnLevel.Count == FactoryMB.MAX_RD_LEVEL, "CustomerMB.Awake: _standardValueOnLevel.Count = {0}, expected {1}", _standardValueOnLevel.Count, FactoryMB.MAX_RD_LEVEL);
            Dbg.CAssert(this, _addFavorValues.Count == _favorFeaturesCnt, "CustomerMB.Awake: _addFavorValues.Count == {0}, expected {1}", _addFavorValues.Count, _favorFeaturesCnt);
        }

        public void UpdateFavorList()
        {
            _favoriteFeatures.RemoveAt(0);
            int v = IDX.RandomGetElem(D_NotInFavorList);
            _favoriteFeatures.Add(v);
        }

        
        public void ChangeStrategy()
        {
            
        }
        
        public void Act()
        {
            var toBuy = _strategy.ChooseStock();
            if( toBuy.v0 == null )
            {
                Info.Log($"{name} decides to not buy anything.");
                return;
            }
            
            FactoryMB fac = toBuy.v0;
            Stock stock = toBuy.v1;
            Info.Log($"{name} decides to buy from {fac.name}");

            fac.stocks.Remove(stock);
            stock.tr.DOMove(_tr.position, 1.5f);
            stock.tr.DOScale(Vector3.one * 0.3f, 1.5f).OnComplete( () => GameObject.Destroy(stock.gameObject));

            fac.ModCash(stock.price);
        }

        private bool _NotInFavorList(int x)
        {
            return !_favoriteFeatures.Contains(x);
        }

        public int GetPerceivedValue(Stock aStock)
        {
            int totalValue = 0;
            var factories = FactoryMgr.Instance.allFactories;
            var stockFeatures = aStock.features;
            for(int i=0; i<stockFeatures.Count; ++i)
            {
                int aFeature = stockFeatures[i];
                int stdValue = _GetFeatureStandardValue(aFeature, factories);
                int addValue = _GetFeatureAddedFavorValue(aFeature);
                totalValue += stdValue + addValue;
            }

            return totalValue;
        }

        private int _GetFeatureAddedFavorValue(int aFeature)
        {
            int idx = _favoriteFeatures.FindIndex(x => x == aFeature);
            if (idx == -1)
                return 0;
            else
                return _addFavorValues[idx];
        }

        private int _GetFeatureStandardValue(int aFeature, List<FactoryMB> factories)
        {
            int minRDLevel = FactoryMB.MAX_RD_LEVEL;

            foreach(var aFac in factories)
            {
                int lv = aFac.RDlevels[aFeature];
                if( lv < minRDLevel )
                {
                    minRDLevel = lv;
                }
            }

            return _standardValueOnLevel[minRDLevel-1];
        }

    }
}
