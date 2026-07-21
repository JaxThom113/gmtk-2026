using UnityEngine;

//use ``Using Sezylrin.SimplePooling;`` to access
//Do not use namespace in your own projects
namespace Sezylrin.SimplePooling.Demo 
{
    public class SpawnerOne : MonoBehaviour
    {
        public GameObject prefabExample1;
        // Start is called before the first frame update
        private int mode = 0;
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            //Getting position of the mouse relative to the game screen
            Vector2 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //on left click
            if (Input.GetMouseButtonDown(0))
            {
                switch (mode)
                {
                    //spawn object in global transform
                    case 0:
                        Pooler.GetObject(prefabExample1, worldpos, Quaternion.identity);
                        break;
                    //spawn object as children to the given transform
                    case 1:
                        Pooler.GetObject(prefabExample1, worldpos, Quaternion.identity, transform);
                        break;
                }
            }
            //switches spawn mode
            if (Input.GetMouseButtonDown(1))
            {
                mode++;
                mode %= 2;
            }
            float time = Mathf.PingPong(Time.time * 2, 1);
            transform.position = Vector3.Lerp(new Vector3(-5, 0, 0), new Vector3(5, 0, 0), time);
        }
    }
}

