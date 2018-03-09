using UnityEngine;
using ExtMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using MH;
using ReGoap.Unity.FactoryExample.OtherScripts;

using Random = UnityEngine.Random;

namespace ReGoap.Unity.FactoryExample.Strategy
{
    public class StrategyMaxUtil : ICustomerStrategy
    {
        private CustomerMB _owner;

        public static StrategyMaxUtil Create(CustomerMB owner)
        {
            var o = Mem.New<StrategyMaxUtil>();
            o._owner = owner;
            return o;
        }

        public STuple<FactoryMB, Stock> ChooseStock()
        {
            var ret = new STuple<FactoryMB, Stock>(null, null);
            if (Random.value < _owner.chanceToBuy)
            {
                float bestPricePerf = 0;
                foreach (var aFac in FactoryMgr.Instance.allFactories)
                {
                    foreach (var aStock in aFac.stocks)
                    {
                        int price = aStock.price;
                        int perceivedValue = _owner.GetPerceivedValue(aStock);
                        float newPricePerf = perceivedValue / (float)price;
                        if (newPricePerf > bestPricePerf)
                        {
                            bestPricePerf = newPricePerf;
                            ret.v0 = aFac;
                            ret.v1 = aStock;
                        }
                    }
                }
            }
            return ret;
        }

        public void Release()
        {
            _owner = null;
            Mem.Del(this);
        }
    }
}