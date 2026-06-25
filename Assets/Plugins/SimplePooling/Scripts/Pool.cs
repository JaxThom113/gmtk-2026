using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using static Sezylrin.SimplePooling.Pooler;

namespace Sezylrin.SimplePooling
{

    public class Pool<T> : IPool where T : UnityEngine.Object
    {
        private ObjectPool<ReferenceMap> pool;
        private Action<T> onNewInstance;
        private Action<T> onGet;
        private Action<T> onRelease;
        private Action<T> onDestroy;
        private Action<GameObject> newPos;
        private TypeReference transformKey;
        private GameObject prefab;
        private bool single;
        /// <summary>
        /// Count all objects associated with pool including both inactive and active objects
        /// </summary>
        public int CountAll { get { return _countAll; } private set { _countAll = value; } }
        private int _countAll;
        /// <summary>
        /// Counts all Inactive object in the pool.
        /// </summary>
        public int CountInactive => pool.CountInactive;
        /// <summary>
        /// Counts all Active object in the pool.
        /// </summary>
        public int CountActive => _countAll - CountInactive;

        internal Pool(TypeReference targetRef, GameObject obj, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize, bool single, Action<GameObject> newPos)
        {
            this.single = single;
            this.onNewInstance = onNewInstance;
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.onDestroy = onDestroy;
            this.newPos = newPos;
            CreatePoolSingle(targetRef, obj, collectionCheck, defaultCapacity, maxSize, single);
        }
        private void CreatePoolSingle(TypeReference targetRef, GameObject obj, bool collectionCheck, int defaultCapacity, int maxSize, bool single)
        {
            CountAll = 0;
            transformKey = targetRef;
            prefab = obj;
            pool = new ObjectPool<ReferenceMap>(
            createFunc: () => CreateObject<T>(targetRef, obj, this.onNewInstance, ref _countAll, single, this.newPos),
            actionOnGet: (reference) => OnGet(reference, this.onGet, this.newPos, single),
            actionOnRelease: (reference) => OnRelease(reference, this.onRelease, single),
            actionOnDestroy: (reference) => OnDestroy(reference, this.onDestroy, ref _countAll, single),
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
            );
        }
        internal void SetNewActions(Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, Action<GameObject> newPos)
        {
            this.onNewInstance = onNewInstance;
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.onDestroy = onDestroy;
            this.newPos = newPos;
        }

        /// <summary>
        /// Set an action for when a new object is created for the first time/
        /// </summary>
        /// <param name="onNewInstance"></param>
        public void SetOnNewInstance(Action<T> onNewInstance)
        {
            this.onNewInstance = onNewInstance;
        }
        /// <summary>
        /// Set an action for when an object is retrieved from the pool/
        /// </summary>
        /// <param name="onGet"></param>
        public void SetOnGet(Action<T> onGet)
        {
            this.onGet = onGet;
        }
        /// <summary>
        /// Set an action for when an object is returned back into the pool/
        /// </summary>
        /// <param name="onRelease"></param>
        public void SetOnRelease(Action<T> onRelease)
        {
            this.onRelease = onRelease;
        }
        /// <summary>
        /// Set an action for when an object in the pool is to be destroyed when it exceeds max pool size
        /// or when local pools are destroyed on scene change.
        /// </summary>
        /// <param name="onDestroy"></param>
        public void SetOnDestroy(Action<T> onDestroy)
        {
            this.onDestroy = onDestroy;
        }
        internal ReferenceMap Get()
        {
            ReferenceMap map = pool.Get();
            map.SetActiveObjects();
            return map;
        }
        internal TypeReference GetTransformKey()
        {
            return transformKey;
        }
        private IEnumerable<T> GetObjectsPrivate()
        {
            if (single)
                return new T[] { GetObject<T>(pool.Get() as ReferenceMapSingle) };
            else
                return GetObjects<T>(pool.Get() as ReferenceMapMulti);
        }
        private T GetObjectPrivate()
        {
            if (single)
                return GetObject<T>(pool.Get() as ReferenceMapSingle);
            else
                return GetObjects<T>(pool.Get() as ReferenceMapMulti).First();
        }
        #region Object
        /// <summary>
        /// Get All object from the pool and calls the onGet action for each. onNewInstance will be called if a new object is created.
        /// If pool is single, returns an IEnumerable with only 1 object. If pool is multi, returns an IEnumerable with all active objects in the pool.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetObjects()
        {
            this.newPos = Object;
            return GetObjectsPrivate();
        }
        /// <summary>
        /// Get a single object from the pool and calls the onGet action for it. onNewInstance will be called if a new object is created.
        /// If pool is multi, returns the first detected component in the object. Use GetObjects() to get all active objects in a multi pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            this.newPos = Object;
            return GetObjectPrivate();
        }
        private void Object(GameObject obj)
        {
            obj.transform.parent = null;
        }
        #endregion

        #region Object Transform
        /// <summary>
        /// Get All object from the pool and calls the onGet action for each. onNewInstance will be called if a new object is created.
        /// If pool is single, returns an IEnumerable with only 1 object. If pool is multi, returns an IEnumerable with all active objects in the pool.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetObjects(Transform transform)
        {
            this.newPos = (obj) => obj.transform.SetParent(transform);
            return GetObjectsPrivate();
        }
        /// <summary>
        /// Get a single object from the pool and calls the onGet action for it. onNewInstance will be called if a new object is created.
        /// If pool is multi, returns the first detected component in the object. Use GetObjects() to get all active objects in a multi pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject(Transform transform)
        {
            this.newPos = (obj) => obj.transform.SetParent(transform);
            return GetObjectPrivate();
        }

        #endregion

        #region Object Transform WorldSpace
        /// <summary>
        /// Get All object from the pool and calls the onGet action for each. onNewInstance will be called if a new object is created.
        /// If pool is single, returns an IEnumerable with only 1 object. If pool is multi, returns an IEnumerable with all active objects in the pool.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetObjects(Transform transform, bool instantiateInWorldSpace)
        {
            this.newPos = (obj) => obj.transform.SetParent(transform, instantiateInWorldSpace);
            return GetObjectsPrivate();
        }
        /// <summary>
        /// Get a single object from the pool and calls the onGet action for it. onNewInstance will be called if a new object is created.
        /// If pool is multi, returns the first detected component in the object. Use GetObjects() to get all active objects in a multi pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject(Transform transform, bool instantiateInWorldSpace)
        {
            this.newPos = (obj) => obj.transform.SetParent(transform, instantiateInWorldSpace);
            return GetObjectPrivate();
        }
        #endregion

        #region Object Position Rotation
        /// <summary>
        /// Get All object from the pool and calls the onGet action for each. onNewInstance will be called if a new object is created.
        /// If pool is single, returns an IEnumerable with only 1 object. If pool is multi, returns an IEnumerable with all active objects in the pool.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetObjects(Vector3 position, Quaternion rotation)
        {
            this.newPos = (obj) => ObjectNewPos(obj, position, rotation);
            return GetObjectsPrivate();
        }
        /// <summary>
        /// Get a single object from the pool and calls the onGet action for it. onNewInstance will be called if a new object is created.
        /// If pool is multi, returns the first detected component in the object. Use GetObjects() to get all active objects in a multi pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject(Vector3 position, Quaternion rotation)
        {
            this.newPos = (obj) => ObjectNewPos(obj, position, rotation);
            return GetObjectPrivate();
        }

        private void ObjectNewPos(GameObject obj, Vector3 position, Quaternion rotation)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        #endregion

        #region Object Position Rotation Transform
        /// <summary>
        /// Get All object from the pool and calls the onGet action for each. onNewInstance will be called if a new object is created.
        /// If pool is single, returns an IEnumerable with only 1 object. If pool is multi, returns an IEnumerable with all active objects in the pool.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetObjects(Vector3 position, Quaternion rotation, Transform transform)
        {
            this.newPos = (obj) => ObjectNewPos(obj, position, rotation, transform);
            return GetObjectsPrivate();
        }
        /// <summary>
        /// Get a single object from the pool and calls the onGet action for it. onNewInstance will be called if a new object is created.
        /// If pool is multi, returns the first detected component in the object. Use GetObjects() to get all active objects in a multi pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject(Vector3 position, Quaternion rotation, Transform transform)
        {
            this.newPos = (obj) => ObjectNewPos(obj, position, rotation, transform);
            return GetObjectPrivate();
        }

        private void ObjectNewPos(GameObject obj, Vector3 position, Quaternion rotation, Transform transform)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(transform);
        }

        #endregion
        void IPool.Release(ReferenceMap reference)
        {
            Release(reference);
        }

        private void Release(ReferenceMap reference)
        {
            pool.Release(reference);
        }
        /// <summary>
        /// Destroys every instance in the pool and calls the onDestroy Action for each. Does not destroy unpooled objects
        /// </summary>
        public void Clear()
        {
            _countAll -= CountActive;
            pool.Clear();
        }
        /// <summary>
        /// Pre-Initialise a set amount of object
        /// </summary>
        /// <param name="amount"></param>
        public void PreloadObject(int amount, Action<T> onNewInstance = null)
        {
            if (amount <= 0)
                return;
            this.onNewInstance = onNewInstance;
            for (int i = 0; i < amount; i++)
                pool.Release(CreateObject<T>(transformKey, prefab, this.onNewInstance, ref _countAll, single, null));
        }

        public void ResetCount()
        {
            CountAll = CountInactive;
        }
    }
}
