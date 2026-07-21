using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{
    public class UnitySpawningExample : MonoBehaviour
    {
        public GameObject prefabToSpawn;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Instantiate(prefabToSpawn, worldpos, Quaternion.identity);
            }
        }
    }
}
