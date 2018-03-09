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
    public class StrategyLowestPrice : ICustomerStrategy
    {
        private CustomerMB _owner;

        public static StrategyLowestPrice Create(CustomerMB owner)
        {
            var o = Mem.New<StrategyLowestPrice>();
            o._owner = owner;
            return o;
        }

        public STuple<FactoryMB, Stock> ChooseStock()
        {
            var ret = new STuple<FactoryMB, Stock>(null, null);
            
            int lowestPrice = int.MaxValue;
            if( Random.value < _owner.chanceToBuy)
            {
                foreach (var aFac in FactoryMgr.Instance.allFactories)
                {
                    foreach(var aStock in aFac.stocks)
                    {
                        if(lowestPrice > aStock.price)
                        {
                            lowestPrice = aStock.price;
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