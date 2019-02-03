using System.Collections.Generic;
using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    public class ReGoapMemoryAdvanced<T, W> : ReGoapMemory<T, W>
    {
        [SerializeField][Tooltip("")]
        private bool _autoUpdateSensor = true;

        private List<IReGoapSensor<T, W>> sensors = new List<IReGoapSensor<T, W>>();

        public float SensorsUpdateDelay = 0.3f;
        private float sensorsUpdateCooldown;

        #region UnityFunctions
        protected override void Awake()
        {
            base.Awake();
            GetComponents<IReGoapSensor<T, W>>(sensors);
            foreach (var sensor in sensors)
            {
                sensor.Init(this);
            }
        }

        protected virtual void Update()
        {
            if( _autoUpdateSensor )
            {
                if (Time.time > sensorsUpdateCooldown)
                {
                    UpdateSensors();
                }
            }
        }

        public void UpdateSensors()
        {
            sensorsUpdateCooldown = Time.time + SensorsUpdateDelay;

            foreach (var sensor in sensors)
            {
                sensor.UpdateSensor();
            }
        }
        #endregion
    }
}
