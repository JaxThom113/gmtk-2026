using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{
    public class PoolerObjectExample : MonoBehaviour
    {
        private Rigidbody2D rb;
        //We replace start and awake methods with initializeObject.
        //These are stuff we wish to run only once when an object is
        //newly spawned.
        public void InitializeObject()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 1.0f;
        }
        //We need a ResetFunction to reset values back to their initial value
        public void ResetValues()
        {
            rb.linearVelocity = Vector3.zero;
            Invoke("PoolSelf", 2);
        }
        //equivalent to Destroy
        public void PoolSelf()
        {
            Pooler.PoolObject(gameObject);
        }
    }
}
