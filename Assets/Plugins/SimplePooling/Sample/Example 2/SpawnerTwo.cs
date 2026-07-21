using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Sezylrin.SimplePooling.Demo
{    public class SpawnerTwo : MonoBehaviour
    {
        public GameObject PrefabToSpawn;
        public TMP_Text text;
        // Start is called before the first frame update
        void Start()
        {

        }
        private int mode = 0;
        // Update is called once per frame
        void Update()
        {
            Vector2 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                switch (mode)
                {
                    case 0:
                        ExampleOne(worldpos);
                        break;
                    case 1:
                        ExampleTwo(worldpos);
                        break;
                    case 2:
                        ExampleThree(worldpos);
                        break;
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                mode++;
                mode %= 3;
                text.text = $"Current Mode:{mode}";
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Pooler.ClearAllObject(PrefabToSpawn);
            }
        }
        //Example 1. Initialize New Spawn
        //spawns in object where when a new 
        //object is instantiated, a method is called
        //it then calls 
        public void ExampleOne(Vector3 worldpos)
        {
            //Different way to assign Actions without lambda expression.
            Pooler.GetObject<ObjectExampleTwo>(
                PrefabToSpawn,
                worldpos,
                Quaternion.identity,
                onNewInstance: ExampleOneExtension
                ).HelloWorld();
        }
        //This method executes whenever a new object is instantiated
        public void ExampleOneExtension(ObjectExampleTwo script)
        {
            Debug.Log("A new object has been instantiated");
            script.NewSpawn();
            //other programming logic
        }
        //Example 2. Basic component retrival
        //Simple retrival of pooled object
        //similar to Type name = Instantiate().GetComponent()
        public void ExampleTwo(Vector3 worldpos)
        {
            ObjectExampleTwo objectExample = Pooler.GetObject<ObjectExampleTwo>(PrefabToSpawn, worldpos, Quaternion.identity);
            objectExample.HelloWorld();
        }
        //Example 3. Lambda Expression
        //Component retrival with assigned Actions using Lambda expression
        public void ExampleThree(Vector3 worldPos)
        {
            Pooler.GetObject<ObjectExampleTwo>(
                PrefabToSpawn,
                worldPos,
                Quaternion.identity,
                //using named arguments and lambda expression
                onNewInstance: (script) => script.NewSpawn(),
                onGet: (script) => script.HelloWorld(),
                onRelease: (script) => script.OnPooled()
                );
        }
    }
}

