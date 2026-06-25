using UnityEngine;
using System.Linq;

namespace Sezylrin.SimplePooling.Demo
{
    public class SpawnerThree : MonoBehaviour
    {
        public GameObject ExampleObject;
        //A pool is a local reference to a pool, it contains all equivalent functions to the pooler except PoolObject and DestroyPool
        //and can be used directly instead. PoolObject and DestroyPool is only available from Pooler
        private Pool<GameObject> pool;

        private BoxCollider2D col;
        void Start()
        {
            //creating a pool for objects that dont support child object pooling
            pool = Pooler.CreatePoolSingle(ExampleObject);
            //preloading object into pool
            pool.PreloadObject(100);
            col = GetComponentInChildren<BoxCollider2D>();
        }
        void Update()
        {
            //spawn the object wherever you click
            Vector2 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && !col.OverlapPoint(worldpos))
            {
                pool.GetObject().transform.position = worldpos;
            }
        }
        //preload objects into the pool
        public void PreloadObject()
        {
            pool.PreloadObject(100);
        }
        //clear all objects in the pool, does not destroy the pool
        //will invoke any listeners to OnDestroy for each cleared object
        //will not clear any objects not yet returned to the pool
        public void Clear()
        {
            pool.Clear();
        }
        //Destroy the pool and all pooled object
        //Does not invoke any listeners to OnDestroy
        //Will cause issues if there are still active objects that have
        //not been pooled. Use only after all objects are pooled
        public void DestroyPool()
        {
            Pooler.DestroyPool(ref pool);
            pool = Pooler.CreatePoolSingle(ExampleObject);
        }
        //counts all object, active and inactive, in the pool
        public void CountAll()
        {
            Debug.Log($"The pool contains a total of {pool.CountAll} objects");
        }
        //counts active objects in the pool
        public void CountActive()
        {
            Debug.Log($"The pool contains a total of {pool.CountActive} active objects");
        }
        //counts inactive objects in the pool
        public void CountInactive()
        {
            Debug.Log($"The pool contains a total of {pool.CountInactive} inactive objects");
        }
        //pooled objects dont destroy on load
        //Will destroy any unpooled object on scene change
        //which will result in warning logs.
        //Recommended to change scene after all objects are pooled.
        public void MakePoolDontDestroyOnLoad()
        {
            Pooler.SetPoolDontDestroyOnLoad(pool);
        }
        //Resets pooled object to destroy on load
        public void RemovePoolFromDontDestroyOnLoad()
        {
            Pooler.RemoveDontDestroyOnLoad(pool);
        }
    }
}

