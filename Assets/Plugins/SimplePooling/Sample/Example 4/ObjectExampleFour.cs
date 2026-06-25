using Sezylrin.SimplePooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{    public class ObjectExampleFour : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ResetObj(int duration)
        {
            Invoke("PoolSelf", duration);
        }
        public void PoolSelf()
        {
            Pooler.PoolObject(gameObject);
        }
    }
}
