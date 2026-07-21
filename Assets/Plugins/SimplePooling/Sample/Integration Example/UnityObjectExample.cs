using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{
    public class UnityObjectExample : MonoBehaviour
    {
        Rigidbody2D rb;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 1.0f;
            Invoke("DestroySelf", 2);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
