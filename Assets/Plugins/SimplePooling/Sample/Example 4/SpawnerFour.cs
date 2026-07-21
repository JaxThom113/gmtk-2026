using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;


namespace Sezylrin.SimplePooling.Demo
{    public class SpawnerFour : MonoBehaviour
    {
        public GameObject prefabExample4;
        int i = 1;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                i = 1;
                //Use Pooler.GetObjects in order to support 
                //child pooling in the prefab.
                //Recommended that the parent object does not contain the
                //target script for predictable behaviours.
                IEnumerable<ObjectExampleFour> example = Pooler.GetObjects<ObjectExampleFour>(prefabExample4, worldpos, Quaternion.identity);
                foreach (ObjectExampleFour exampleFour in example)
                {
                    exampleFour.ResetObj(i * 2);
                    i++;
                }             

            }
        }
    }
}
