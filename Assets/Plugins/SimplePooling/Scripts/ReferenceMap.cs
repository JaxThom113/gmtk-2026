using UnityEngine;

namespace Sezylrin.SimplePooling
{
    internal abstract class ReferenceMap
    {
        public ReferenceMap(TypeReference typeKey, Transform poolTransform)
        {
            //prefabObj = prefab;
            this.typeKey = typeKey;
            this.poolTransform = poolTransform;
        }
        //public GameObject prefabObj;
        public TypeReference typeKey;
        public Transform poolTransform;
        public Vector3 initialPos;
        public Quaternion initialRot;
        public Vector3 initialScale;
        internal virtual void SetActiveObjects()
        {

        }
        internal virtual void ReduceActiveObjects(int amount)
        {
        }
        internal abstract bool CanPoolAll();
    }

    internal class ReferenceMapSingle : ReferenceMap
    {
        public ReferenceMapSingle(GameObject instance, Component component, TypeReference typeKey, Transform poolTransform) : base(typeKey, poolTransform)
        {
            this.instance = instance;
            this.component = component;
        }

        public GameObject instance;
        public Component component;

        internal override bool CanPoolAll()
        {
            return true;
        }
    }

    internal class ReferenceMapMulti : ReferenceMap
    {
        public ReferenceMapMulti(GameObject head, Component[] components, TypeReference typeKey, Transform poolTransform) : base(typeKey, poolTransform)
        {
            this.components = components;
            this.head = head;
        }
        public GameObject head;
        public Component[] components;
        public Vector3[] localPos;
        internal int activeObj;
        internal override void SetActiveObjects()
        {
            activeObj = components.Length;
        }
        internal override void ReduceActiveObjects(int amount)
        {
            activeObj -= amount;
        }
        internal override bool CanPoolAll()
        {
            activeObj--;
            if (activeObj == 0)
            {
                foreach (Component component in components)
                    component.gameObject.SetActive(true);
                return true;
            }
            return false;
        }
    }
}
