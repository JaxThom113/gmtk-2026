using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{
    public class ObjectExampleTwo : MonoBehaviour
    {
        public void HelloWorld()
        {
            Debug.Log("HelloWorld");
        }

        public void NewSpawn()
        {
            Debug.Log("I am a new spawned");
        }

        public void OnPooled()
        {
            Debug.Log("I have been Pooled");
        }

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

