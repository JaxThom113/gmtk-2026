using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{    public class ObjectExampleThree : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        }

        bool triggeredPooling = false;
        // Update is called once per frame
        void Update()
        {
            if (isActiveAndEnabled)
            {
                if (!triggeredPooling)
                {
                    Invoke("PoolSelf", 5);
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

