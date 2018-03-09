using UnityEngine;
using ExtMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MH;
using System.Text;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    public class InfoArea : MonoBehaviour
    {
        #region "conf data" 
        [SerializeField][Tooltip("")]
        private Text _lblInfo;
        [SerializeField][Tooltip("")]
        private Image _imgBG;
        #endregion "conf data" 
        
        #region "data" 
        StringBuilder _bld = new StringBuilder();        
        #endregion "data" 
        
        #region "prop" 
        
        #endregion "prop" 
        
        #region "unity methods" 
        
        void LateUpdate()
        {
            // find the object under cursor
            var hitGO = PhysxUtil.RaycastObject();
            if( ! hitGO )
                return;

            _bld.Remove(0, _bld.Length);

            var aFac = hitGO.GetComponentInParent<FactoryMB>();
            if( aFac )
            {
                _bld.AppendFormat("{0} : {1} / {2}\n", aFac.name, aFac.cash, aFac.loan);
                for(int i=0; i<FactoryMB.FeatureNames.Length; ++i)
                {
                    var aFeatLv = aFac.RDlevels[i];
                    _bld.AppendFormat("{0} : {1}\n", FactoryMB.GetFeatureName(i), aFeatLv);
                }
                _UpdateText();
                return;
            }

            var aCus = hitGO.GetComponentInParent<CustomerMB>();
            if( aCus )
            {
                _bld.AppendFormat("{0} : {1}\n", aCus.name, aCus.strategy?.GetType().Name);
                for(int i=0; i<aCus.favoriteFeatures.Count; ++i)
                {
                    var aFeat = aCus.favoriteFeatures[i];
                    _bld.AppendFormat("{0}\n", FactoryMB.GetFeatureName(aFeat));
                }
                _UpdateText();
                return;
            }

            var aStock = hitGO.GetComponentInParent<Stock>();
            if( aStock )
            {
                _bld.AppendFormat("Price : {0}\n", aStock.price);
                foreach(var aFeat in aStock.features)
                {
                    _bld.AppendFormat("{0} \n", FactoryMB.GetFeatureName(aFeat) );
                }
                _UpdateText();
                return;
            }
        }

        #endregion "unity methods" 
        
        #region "methods" 
        
        private void _UpdateText()
        {
            _lblInfo.text = _bld.ToString();
        }

        #endregion "methods" 
    }
} 