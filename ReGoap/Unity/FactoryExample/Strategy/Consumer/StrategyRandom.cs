using UnityEngine;
using ExtMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using ReGoap.Unity.FactoryExample.OtherScripts;
using MH;

using Random = UnityEngine.Random;

namespace ReGoap.Unity.FactoryExample.Strategy
{
    public interface ICustomerStrategy : IRelease
    {
        STuple<FactoryMB, Stock> ChooseStock();
    }

    public class StrategyRandom : ICustomerStrategy
    {
        private CustomerMB _owner;

        public static StrategyRandom Create(CustomerMB owner)
        {
            var strategy = Mem.New<StrategyRandom>();
            strategy._owner = owner;
            return strategy;
        }

        public void Release()
        {
            _owner = null;
            Mem.Del(this);
        }

        private List<STuple<FactoryMB, Stock>> _cacheLst = new List<STuple<FactoryMB, Stock>>();
        public STuple<FactoryMB, Stock> ChooseStock()
        {
            var ret = new STuple<FactoryMB, Stock>(null, null);

            if( Random.value < _owner.chanceToBuy)
            {
                foreach (var aFac in FactoryMgr.Instance.allFactories)
                {
                    foreach(var aStock in aFac.stocks)
                    {
                        _cacheLst.Add(new STuple<FactoryMB, Stock>(aFac, aStock) );
                    }
                }
                ret = _cacheLst.RandomGetElem();
                _cacheLst.Clear();
            }

            return ret;
        }
    }
} 