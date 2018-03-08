using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    public class ReGoapMemoryAdvanced<T, W> : ReGoapMemory<T, W>
    {
        [SerializeField][Tooltip("")]
        private bool _autoUpdateSensor = true;

        private IReGoapSensor<T, W>[] sensors;

        public float SensorsUpdateDelay = 0.3f;
        private float sensorsUpdateCooldown;

        #region UnityFunctions
        protected override void Awake()
        {
            base.Awake();
            sensors = GetComponents<IReGoapSensor<T, W>>();
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
