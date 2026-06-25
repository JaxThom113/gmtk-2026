using UnityEngine;
namespace Sezylrin.SimplePooling.Demo
{
    public class PoolerSpawningExample : MonoBehaviour
    {

        public GameObject prefabToSpawn;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //note when spawning an object, everything that occurs
                //in this code block executes before Start, Awake and OnEnable
                //execute in the spawned object scripts.
                Pooler.GetObject<PoolerObjectExample>(
                    prefabToSpawn,
                    worldpos,
                    Quaternion.identity,
                    //we assign the "start" function to the event onNewInstance
                    onNewInstance: (obj) => { obj.InitializeObject(); },
                    //we assign the "reset" function to OnGet
                    onGet: ResetObject
                    );
            }             
        }
        public void ResetObject(PoolerObjectExample obj)
        {
            obj.ResetValues();
            //other functions cna go here
        }
    }

}
