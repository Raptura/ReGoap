using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FactoryExample.OtherScripts
{
    public class Stock : MonoBehaviour
    {
        [SerializeField][Tooltip("")]
        private List<int> _features = new List<int>();
        [SerializeField][Tooltip("")]
        private int _cost = 0;
        [SerializeField][Tooltip("")]
        private int _price = 0;

        public List<int> features { get { return _features; } }
        public int cost { get { return _cost; } set { _cost = value; } }
        public int price { get { return _price; } set { _price = value; } }


        private Transform _tr;
        public Transform tr { get{ return _tr; } set{ _tr = value; } }

        void Awake()
        {
            _tr = transform;
        }

        public void SetFeatures(List<int> featLst)
        {
            _features.Clear();
            _features.AddRange(featLst);
        }
    }
}
