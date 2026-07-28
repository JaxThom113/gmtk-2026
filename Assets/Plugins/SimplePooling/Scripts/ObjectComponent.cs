using UnityEngine;

namespace Sezylrin.SimplePooling
{
    internal struct ObjectComponent
    {
        public ObjectComponent(GameObject instance, Component component)
        {
            this.instance = instance;
            this.component = component;
        }
        public GameObject instance;
        public Component component;
    }
}
