using UnityEngine;
using Sezylrin.SimplePooling;
namespace Sezylrin.SimplePooling.Demo
{    
    public class ObjectExampleOne : MonoBehaviour
    {
        bool triggeredPooling = false;
        // Update is called once per frame
        void Update()
        {
            if (isActiveAndEnabled)
            {
                if (!triggeredPooling)
                {
                    Invoke("PoolSelf", 1);
                    triggeredPooling = true;
                }
            }
        }

        public void PoolSelf()
        {
            triggeredPooling = false;
            Pooler.PoolObject(gameObject);
        }
    }

}
