using UnityEngine;
using ExtMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MH;
using DG.Tweening;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    ///<summary>
    /// 
    ///</summary>
    public class InfoPopText : MonoBehaviour, IRelease
    {
        #region "conf data" 
        
        [SerializeField][Tooltip("")]
        private string _prefix = "+";

        [SerializeField][Tooltip("")]
        private Text _lblValue;

        #endregion "conf data" 
        
        #region "data" 
        
        private Transform _tr;

        #endregion "data" 
        
        #region "prop" 
        
        #endregion "prop" 
        
        #region "unity methods" 
        
        void Awake()
        {
            _tr = transform;
            Dbg.CAssert(this, _lblValue != null, "InfoPopText.Awake: _lblValue not set");
        }

        #endregion "unity methods" 
        
        #region "methods" 

        public static InfoPopText Create(int v, Transform parentTr)
        {
            GameObject pf = null;
            if( v >= 0 )
                pf = FactoryMgr.Instance.pfAddCashText.gameObject;
            else
                pf = FactoryMgr.Instance.pfSubCashText.gameObject;

            GameObject newGO = PrefabPool.SpawnPrefab(pf, SpawnData.Create(parentTr.position, Quaternion.identity, parentTr));
            var newInfo = newGO.AssertGetComponent<InfoPopText>();
            newInfo._lblValue.text = newInfo._prefix + v.ToString();

            var newTr = newGO.transform;
            newTr.DOMove(newTr.position + Vector3.one * 3f, 1.5f);

            Color targetColor = newInfo._lblValue.color; targetColor.a = 0.4f;
            newInfo._lblValue.DOColor(targetColor, 1.5f).OnComplete( newInfo.Release );

            return newInfo;
        }
        
        public void Release()
        {
            Color c = _lblValue.color; c.a = 1f;
            _lblValue.color = c;
            Vector3 scale = _tr.localScale;
            PrefabPool.DespawnPrefab(gameObject);
            _tr.localScale = scale;
        }

        #endregion "methods" 
    }
}