using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Sezylrin.SimplePooling
{
    public class Pooler
    { 
        #region Initialisation
        private static Dictionary<TypeReference, IPool> objectPool;
        private static Dictionary<TypeReference, Transform> transformDict;
        //gameobject instance reference
        private static Dictionary<GameObject, ReferenceMap> referenceDict;
        //prefab key to all the different stored variation
        private static Dictionary<GameObject, List<TypeReference>> prefabReference;
        private static List<TypeReference> localPools;
        private static GameObject GlobalPool;
        private static GameObject LocalPool;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            objectPool = new Dictionary<TypeReference, IPool>();
            transformDict = new Dictionary<TypeReference, Transform>();
            prefabReference = new Dictionary<GameObject, List<TypeReference>>();
            referenceDict = new Dictionary<GameObject, ReferenceMap>();
            localPools = new List<TypeReference>();
            GlobalPool = null;
            LocalPool = null;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

#if UNITY_EDITOR
        [InitializeOnLoad]
        public class StartupMessage
        {
            static StartupMessage()
            {
                if (!SessionState.GetBool("Sezylrin.SimplePooling.FirstInitDone", false))
                {
                    bool disableWarning = EditorPrefs.GetBool("SezylrinPoolingSettings");

                    if ((EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) != 0 && !disableWarning)
                    {
                        Debug.LogWarning("User has disabled domain reloading. Preloading will not function as intended during start and awake calls in editor.");
                        Debug.Log("You can disable this warning from the tool bar Tools/Sezylrin/Pooling");
                    }
                    SessionState.SetBool("Sezylrin.SimplePooling.FirstInitDone", true);
                }
            }
        }
#endif
        private static void SceneUnloaded(Scene current)
        {
            foreach (TypeReference obj in localPools)
            {
                DestroyPool(obj);
            }
            localPools.Clear();
            LocalPool = null;
            var keysToRemove = referenceDict.Keys.Where(k => k == null).ToList();
            foreach (var key in keysToRemove) 
                referenceDict.Remove(key);
            foreach (var pool in objectPool.Values)
            {
                pool.ResetCount();
            }
        }
        #endregion

        #region Object Pool
        private static void GenerateHeadPool(GameObject prefab)
        {
            if (GlobalPool == null)
            {
                GlobalPool = new GameObject("GlobalPool");
                GameObject.DontDestroyOnLoad(GlobalPool);
            }
            if (LocalPool == null)
            {
                LocalPool = new GameObject("LocalPool");
            }
        }
        private static Pool<T> CreateNewPool<T>(TypeReference targetRef ,GameObject obj, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize, bool single, Action<GameObject> newPos) where T : UnityEngine.Object
        {
            GenerateHeadPool(obj);
            Pool<T> pool = new Pool<T>(targetRef, obj, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize, single, newPos);

            if (!transformDict.ContainsKey(targetRef))
            {
                Transform transform = CreateParents(obj, targetRef);
                transformDict.Add(targetRef, transform);
            }
            objectPool.Add(targetRef, pool);
            return pool;
        }
        internal static ReferenceMap CreateObject<T>(TypeReference typeReference, GameObject prefab, Action<T> onNewInstance, ref int count, bool single, Action<GameObject> SetPosition) where T : UnityEngine.Object
        {
            count++;
            GameObject obj = GameObject.Instantiate(prefab);
            SetPosition?.Invoke(obj);
            return InitialiseObject<T>(typeReference, ref obj, ref prefab, ref onNewInstance, single);
        }
        #region Utility
        private static TypeReference GetTypeReference(GameObject prefab, Type typeToCheck, bool generateNew = true)
        {
            if (prefab.scene.IsValid())
                throw new ArgumentException($"The given gameobject: {prefab.name} is not a prefab");

            if (prefabReference.TryGetValue(prefab, out var reference))
            {
                foreach (TypeReference type in reference)
                {
                    if (type.desiredType == typeToCheck)
                        return type;
                }
            }
            else if (generateNew)
            {
                prefabReference.Add(prefab, new List<TypeReference>());
            }
            if (generateNew)
            {
                TypeReference newRef = new TypeReference(typeToCheck);
                prefabReference[prefab].Add(newRef);
                return newRef;
            }
            else
                return null;
        }
        private static Transform CreateParents(GameObject prefab, TypeReference type)
        {
            GameObject newObj = new GameObject(prefab.name +":"+type.desiredType.Name);
            newObj.transform.parent = LocalPool.transform;
            localPools.Add(type);

            return newObj.transform;
        }

        private static ReferenceMap InitialiseObject<T>(TypeReference type, ref GameObject newObj, ref GameObject prefab, ref Action<T> onNewInstance, bool single) where T : UnityEngine.Object
        {
            newObj.name = prefab.name;
            newObj.SetActive(false);
            ReferenceMap associatedObj;
            if (single)
            {
                UnityEngine.Object targetReference = null; 
                if (typeof(T) == typeof(GameObject))
                    targetReference = newObj;
                else if (typeof(Component).IsAssignableFrom(typeof(T)))
                    targetReference = newObj.GetComponent(typeof(T)); 
                associatedObj = new ReferenceMapSingle(newObj, targetReference as Component, type, transformDict[type]);
                
                if (onNewInstance != null && targetReference != null)
                    onNewInstance.Invoke((T)targetReference);
            }
            else
            {
                UnityEngine.Object[] targetReference = newObj.GetComponentsInChildren(typeof(T));
                associatedObj = new ReferenceMapMulti(newObj, targetReference as Component[],type, transformDict[type]);
                Vector3[] local = new Vector3[targetReference.Length];
                for (int i = 0; i < targetReference.Length; i++)
                {
                    local[i] = (targetReference as Component[])[i].transform.localPosition;
                }
                (associatedObj as ReferenceMapMulti).localPos = local;
                if (onNewInstance != null && targetReference != null)
                {
                    foreach(UnityEngine.Object comp in targetReference)
                        onNewInstance.Invoke((T)comp);
                }
            }
            associatedObj.initialPos = newObj.transform.position;
            associatedObj.initialRot = newObj.transform.rotation;
            associatedObj.initialScale = newObj.transform.localScale;
            referenceDict.Add(newObj, associatedObj);



            return associatedObj;
        }
        private static T[] GetReference<T>(ReferenceMap obj, bool single) where T : UnityEngine.Object
        {
            T[] targetReference = null;
            if (!single && obj is ReferenceMapMulti multi)
                targetReference = multi.components.OfType<T>().ToArray();
            else if (obj is ReferenceMapSingle objSingle && typeof(T) == typeof(GameObject))
                targetReference = new T[] { (T)(UnityEngine.Object)objSingle.instance};
            else if (obj is ReferenceMapSingle objSingle2 &&  typeof(Component).IsAssignableFrom(typeof(T)))
                targetReference = new T[] { (T)(UnityEngine.Object)objSingle2.component };
            return targetReference;
        }
        #endregion
        #region OnRelease
        internal static void OnRelease<T>(ReferenceMap obj, Action<T> OnRelease, bool single) where T : UnityEngine.Object
        {
            GameObject instance = single ? (obj as ReferenceMapSingle).instance : (obj as ReferenceMapMulti).head;     
            instance.SetActive(false);
            instance.transform.parent = obj.poolTransform;
            foreach(T objs in GetReference<T>(obj, single))
            {
                OnRelease?.Invoke(objs);
            }
        }
        #endregion
        #region OnGet
        internal static void OnGet<T>(ReferenceMap obj, Action<T> OnGet, Action<GameObject> newPos,bool single) where T : UnityEngine.Object
        {
            GameObject instance = single ? (obj as ReferenceMapSingle).instance : (obj as ReferenceMapMulti).head;

            if (instance == null)
                return;
            instance.SetActive(true); 
            foreach (T objs in GetReference<T>(obj, single))
            {
                instance.transform.position = obj.initialPos;
                instance.transform.rotation = obj.initialRot;
                instance.transform.localScale = obj.initialScale;
                if (obj is ReferenceMapSingle)
                    newPos?.Invoke((obj as ReferenceMapSingle).instance);
                else
                {
                    ReferenceMapMulti reference = obj as ReferenceMapMulti;
                    for (int i = 0; i < reference.localPos.Length; i++)
                    {
                        Component comp = reference.components[i];
                        if(comp.transform.parent != reference.head)
                            comp.transform.localPosition = reference.localPos[i];
                    }
                    newPos?.Invoke(reference.head);
                }
                OnGet?.Invoke(objs);
            }
        }
        #endregion
        #region OnDestroy
        internal static void OnDestroy<T>(ReferenceMap obj, Action<T> onDestroy, ref int countAll, bool single) where T : UnityEngine.Object
        {
            countAll--;
            GameObject instance = single ? (obj as ReferenceMapSingle).instance : (obj as ReferenceMapMulti).head;

            referenceDict.Remove(instance);
            foreach (T objs in GetReference<T>(obj, single))
            {
                onDestroy?.Invoke(objs);
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
#endif
                GameObject.Destroy(instance);
        }
        #endregion
        #endregion

        #region GetObjects

        internal static T GetObject<T>(ReferenceMapSingle pooledObj) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(GameObject))
            {
                return pooledObj.instance as T;
            }
            if (pooledObj.component == null)
            {
                Debug.LogError($"Object {pooledObj.instance.name} doesn't have component of type {typeof(T)}");
                return null;
            }
            return pooledObj.component as T;            
        }
        internal static IEnumerable<T> GetObjects<T>(ReferenceMapMulti pooledObj) where T : UnityEngine.Object
        {
            if (pooledObj.components.Length == 0)
            {
                Debug.LogError($"Object {pooledObj.head.name} doesn't have any child components of type {typeof(T)}");
                return null;
            }
            return pooledObj.components.Cast<T>();
        }
        private static ReferenceMapSingle ValidateObject<T>(Pool<T> pool) where T : UnityEngine.Object
        {
            ReferenceMapSingle pooledObj = pool.Get() as ReferenceMapSingle;
            //count while loop exiting safety
            int i = 0;
            while (pooledObj.instance == null && i < pool.CountAll + 100)
            {
                Debug.Log($"A pooled obj: {pooledObj.instance.name} has been destroyed through external means, while this isnt fatal and has been handled, try and debug the cause");
                referenceDict.Remove(pooledObj.instance);
                pooledObj = pool.Get() as ReferenceMapSingle;
                i++;
            }
            if(pooledObj.instance == null)
            {
                Debug.LogWarning("Pool entered infinite loop and failed to generated a object");
            }
            return pooledObj;
        }
        private static ReferenceMapSingle GetValidReference<T>(GameObject prefab, Action<GameObject> newPos, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            if (prefab.scene.IsValid())
            {
                Debug.LogWarning($"The given gameobject: {prefab.name} is not a prefab");
                return null;
            }
            Pool<T> pool = GetOrCreatePool<T>(GetTypeReference(prefab, typeof(T)), prefab, true, newPos, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);

            return ValidateObject<T>(pool);
        }

        private static ReferenceMapMulti ValidateObjects<T>(Pool<T> pool) where T : UnityEngine.Object
        {
            ReferenceMapMulti pooledObj = pool.Get() as ReferenceMapMulti;
            //count while loop exiting safety
            int i = 0;
            while (pooledObj.head == null && i < pool.CountAll + 100)
            {
                Debug.Log($"A pooled obj: {pooledObj.head.name} has been destroyed through external means, while this isnt fatal and has been handled, try and debug the cause");
                referenceDict.Remove(pooledObj.head);
                pooledObj = pool.Get() as ReferenceMapMulti;
                i++;
            }
            if (pooledObj.head == null)
            {
                Debug.LogWarning("Pool entered infinite loop and failed to generated a object");
            }
            return pooledObj;
        }
        private static ReferenceMapMulti GetValidReferences<T>(GameObject prefab, Action<GameObject> newPos, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            if (prefab.scene.IsValid())
            {
                Debug.LogWarning($"The given gameobject: {prefab.name} is not a prefab");
                return null;
            }
            Pool<T> pool = GetOrCreatePool<T>(GetTypeReference(prefab, typeof(T)), prefab, false, newPos, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);

            return ValidateObjects<T>(pool);
        }
        #region Object
        private static T GetObjectInternal<T>(GameObject prefab, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            ReferenceMapSingle pooledObj = GetValidReference<T>(prefab, (obj) => obj.transform.parent = null, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            return GetObject<T>(pooledObj);
        }
        /// <summary>
        /// Retreives a Selected component from the given Prefab.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static T GetObject<T>(GameObject prefab, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {            
            return GetObjectInternal<T>(prefab, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives a Gameobject cloned from the Prefab.
        /// </summary>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static GameObject GetObject(GameObject prefab, Action<GameObject> onNewInstance = null, Action<GameObject> onGet = null, Action<GameObject> onRelease = null, Action<GameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            return GetObjectInternal<GameObject>(prefab, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }

        /// <summary>
        /// Retreives all selected component from the given Prefab's children.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetObjects<T>(GameObject prefab, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            ReferenceMapMulti pooledObj = GetValidReferences<T>(prefab, (obj) => obj.transform.parent = null, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            return GetObjects<T>(pooledObj);
        }
        #endregion

        #region Object Transform
        private static T GetObjectInternal<T>(GameObject prefab, Transform transform, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            ReferenceMapSingle pooledObj = GetValidReference<T>(prefab, (obj) => NewPosTransform(obj, transform), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            return GetObject<T>(pooledObj);
        }
        /// <summary>
        /// Retreives a Selected component from the given Prefab.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="transform">parent transform</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static T GetObject<T>(GameObject prefab, Transform transform, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            return GetObjectInternal<T>(prefab, transform, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives a Gameobject cloned from the Prefab.
        /// </summary>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="transform">parent transform</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static GameObject GetObject(GameObject prefab, Transform transform, Action<GameObject> onNewInstance = null, Action<GameObject> onGet = null, Action<GameObject> onRelease = null, Action<GameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            return GetObjectInternal<GameObject>(prefab, transform, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives all selected component from the given Prefab's children.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="transform">parent transform</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetObjects<T>(GameObject prefab, Transform transform, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            ReferenceMapMulti pooledObj = GetValidReferences<T>(prefab, (obj) => NewPosTransform(obj, transform), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            return GetObjects<T>(pooledObj);
        }

        private static void NewPosTransform(GameObject obj, Transform transform)
        {
            obj.transform.parent = transform;
        }
        #endregion

        #region Object Transform WorldSpace
        private static T GetObjectInternal<T>(GameObject prefab, Transform transform, bool instantiateInWorldSpace, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            ReferenceMapSingle pooledObj = GetValidReference<T>(prefab, (obj) => NewPosTransformWorldSpace(obj, prefab.transform.position, transform, instantiateInWorldSpace), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            return GetObject<T>(pooledObj);
        }
        /// <summary>
        /// Retreives a Selected component from the given Prefab.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="transform">parent transform</param>
        /// <param name="instantiateInWorldSpace">Whether the object spawn position is in localspace or worldspace</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static T GetObject<T>(GameObject prefab, Transform transform, bool instantiateInWorldSpace, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            return GetObjectInternal<T>(prefab, transform, instantiateInWorldSpace, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives a Gameobject cloned from the Prefab.
        /// </summary>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="transform">parent transform</param>
        /// <param name="instantiateInWorldSpace">Whether the object spawn position is in localspace or worldspace</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static GameObject GetObject(GameObject prefab, Transform transform, bool instantiateInWorldSpace, Action<GameObject> onNewInstance = null, Action<GameObject> onGet = null, Action<GameObject> onRelease = null, Action<GameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            return GetObjectInternal<GameObject>(prefab, transform, instantiateInWorldSpace, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives all selected component from the given Prefab's children.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="transform">parent transform</param>
        /// <param name="instantiateInWorldSpace">Whether the object spawn position is in localspace or worldspace</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetObjects<T>(GameObject prefab, Transform transform, bool instantiateInWorldSpace, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            ReferenceMapMulti pooledObj = GetValidReferences<T>(prefab, (obj) => NewPosTransformWorldSpace(obj, prefab.transform.position, transform, instantiateInWorldSpace), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;

            return GetObjects<T>(pooledObj);
        }

        private static void NewPosTransformWorldSpace(GameObject obj, Vector3 localPos,Transform transform, bool instantiateInWorldSpace)
        {
            obj.transform.parent = null;
            if (instantiateInWorldSpace)
                obj.transform.position = localPos;
            else
                obj.transform.localPosition = localPos;
            
        }
        #endregion

        #region Object Vector Rotation
        private static T GetObjectInternal<T>(GameObject prefab, Vector3 pos, Quaternion rot, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            ReferenceMapSingle pooledObj = GetValidReference<T>(prefab, (obj) => newPosPosRot(obj, pos, rot), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;

            return GetObject<T>(pooledObj);
        }
        /// <summary>
        /// Retreives a Selected component from the given Prefab.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="pos">World position</param>
        /// <param name="rot">Rotation</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static T GetObject<T>(GameObject prefab, Vector3 pos, Quaternion rot, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            return GetObjectInternal<T>(prefab, pos, rot, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives a Gameobject cloned from the Prefab.
        /// </summary>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="pos">World position</param>
        /// <param name="rot">Rotation</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static GameObject GetObject(GameObject prefab, Vector3 pos, Quaternion rot, Action<GameObject> onNewInstance = null, Action<GameObject> onGet = null, Action<GameObject> onRelease = null, Action<GameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            return GetObjectInternal<GameObject>(prefab, pos, rot, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives all selected component from the given Prefab's children.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="pos">World position</param>
        /// <param name="rot">Rotation</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetObjects<T>(GameObject prefab, Vector3 pos, Quaternion rot, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            ReferenceMapMulti pooledObj = GetValidReferences<T>(prefab, (obj) => newPosPosRot(obj, pos, rot), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            return GetObjects<T>(pooledObj);
        }

        private static void newPosPosRot(GameObject obj, Vector3 pos, Quaternion rot)
        {
            obj.transform.parent = null;
            obj.transform.position = pos;
            obj.transform.rotation = rot;
        }
        #endregion

        #region Object Vector Rotation Transform
        public static T GetObjectInternal<T>(GameObject prefab, Vector3 pos, Quaternion rot, Transform transform, Action<T> onNewInstance, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool collectionCheck, int defaultCapacity, int maxSize) where T : UnityEngine.Object
        {
            ReferenceMapSingle pooledObj = GetValidReference<T>(prefab, (obj) => NewPosPosRotTran(obj, pos, rot, transform), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;

            return GetObject<T>(pooledObj);
        }
        /// <summary>
        /// Retreives a Selected component from the given Prefab.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="pos">World position</param>
        /// <param name="rot">Rotation</param>
        /// <param name="transform">parent transform</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static T GetObject<T>(GameObject prefab, Vector3 pos, Quaternion rot, Transform transform, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            return GetObjectInternal<T>(prefab, pos, rot, transform, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives a Gameobject cloned from the Prefab.
        /// </summary>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="pos">World position</param>
        /// <param name="rot">Rotation</param>
        /// <param name="transform">parent transform</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static GameObject GetObject(GameObject prefab, Vector3 pos, Quaternion rot, Transform transform, Action<GameObject> onNewInstance = null, Action<GameObject> onGet = null, Action<GameObject> onRelease = null, Action<GameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            return GetObjectInternal<GameObject>(prefab, pos, rot, transform, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        }
        /// <summary>
        /// Retreives all selected component from the given Prefab's children.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object you wish to retrive</typeparam>
        /// <param name="prefab">Prefab to generate, Only prefabs are allowed</param>
        /// <param name="pos">World position</param>
        /// <param name="rot">Rotation</param>
        /// <param name="transform">parent transform</param>
        /// <param name="onNewInstance">Action delegate called when a new object is intantiated.</param>
        /// <param name="onGet">Action delegate when an object is retrived from pool.</param>
        /// <param name="onRelease">Action delegate when an object is released back into the pool.</param>
        /// <param name="onDestroy">Action delegate when object is destroyed, Occurs on release of excessive objects or scene transition for local pools.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetObjects<T>(GameObject prefab, Vector3 pos, Quaternion rot, Transform transform, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            ReferenceMapMulti pooledObj = GetValidReferences<T>(prefab, (obj) => NewPosPosRotTran(obj, pos, rot, transform), onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
            if (pooledObj == null)
                return null;
            
            return GetObjects<T>(pooledObj);
        }

        private static void NewPosPosRotTran(GameObject obj, Vector3 pos, Quaternion rot, Transform transform)
        {
            obj.transform.parent = transform;
            obj.transform.position = pos;
            obj.transform.rotation = rot;
        }
        #endregion
        #endregion

        #region Get Pool


        /// <summary>
        /// Retrieves or creates a pool of gameobjects based on the given prefab
        /// </summary>
        /// <param name="prefab">Prefab</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static Pool<GameObject> CreatePoolSingle(GameObject prefab, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            TypeReference reference = GetTypeReference(prefab, typeof(GameObject));
            return GetOrCreatePool<GameObject>(reference, prefab, true, null, collectionCheck: collectionCheck, defaultCapacity: defaultCapacity, maxSize: maxSize);
        }
        /// <summary>
        /// Retrieves or creates a pool of components based on the given prefab and component.
        /// </summary>
        /// <typeparam name="T">Component on Gameobject</typeparam>
        /// <param name="prefab">Prefab</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static Pool<T> CreatePoolSingle<T>(GameObject prefab, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            TypeReference reference = GetTypeReference(prefab, typeof(T));
            return GetOrCreatePool<T>(reference, prefab, true, null,collectionCheck: collectionCheck, defaultCapacity: defaultCapacity, maxSize: maxSize);   
        }
        /// <summary>
        /// Retrieves or creates a pool which supports child object component pooling in the given Prefab.
        /// </summary>
        /// <typeparam name="T">Component in child objects</typeparam>
        /// <param name="prefab">Prefab</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        /// <param name="defaultCapacity">Initial size of the pool size for memory allocation.</param>
        /// <param name="maxSize">Maximum allowed size of pool. Excessive objects are instead destroyed on release.</param>
        /// <returns></returns>
        public static Pool<T> CreatePoolMulti<T>(GameObject prefab, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Component
        {
            TypeReference reference = GetTypeReference(prefab, typeof(T));
            return GetOrCreatePool<T>(reference, prefab, false, null,collectionCheck: collectionCheck, defaultCapacity: defaultCapacity, maxSize: maxSize);
        }



        private static Pool<T> GetOrCreatePool<T>(TypeReference reference, GameObject prefab, bool single, Action<GameObject> newPos, Action<T> onNewInstance = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) where T : UnityEngine.Object
        {
            
            if (TryGetPool<T>(prefab, out Pool<T> pool))
            {
                pool.SetNewActions(onNewInstance, onGet, onRelease, onDestroy, newPos);
                return pool;
            }
            else
                return CreateNewPool<T>(reference, prefab, onNewInstance, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize, single, newPos);
        }
        private static bool TryGetPool<T>(GameObject prefab, out Pool<T> pool) where T : UnityEngine.Object
        {
            if (objectPool.TryGetValue(GetTypeReference(prefab, typeof(T), false), out IPool Ipool))
            {
                pool = (Pool<T>)Ipool;
                return true;
            }
            else
            {
                pool = null;
                return false;
            }
        }
        #endregion

        #region Pool and Clearing
        /// <summary>
        /// Returns object back to the pool, target gameobject Active status will automatically be set to false.
        /// </summary>
        /// <param name="instance">The gotten object instance</param>
        public static void PoolObject(GameObject instance)
        {
            GameObject initial = instance;
            Transform transform = instance.transform;
            while (transform != null)
            {
                instance = transform.gameObject;
                if (referenceDict.TryGetValue(instance, out ReferenceMap referenceObj))
                {
                    if(referenceObj.CanPoolAll())
                    {
                        (objectPool[referenceObj.typeKey]).Release(referenceObj);
                    }
                    else
                    {
                        initial.SetActive(false);
                        Component[] otherChild = initial.GetComponentsInChildren(referenceObj.typeKey);
                        foreach (Component comp in otherChild)
                        {
                            if (comp.gameObject == initial)
                            {
                                referenceObj.ReduceActiveObjects(1);
                                comp.gameObject.SetActive(false);
                            }
                        }

                    }
                    return;
                }
                transform = transform.parent;
            }
            Debug.LogError($"Trying to return {instance.name} to a non existing pool.\n Ensure this prefab is spawned via the Pooling system or that you dont destroy pools with active objects ");

        }
        /// <summary>
        /// Clears every pool that uses the given prefab 
        /// and destroys all instances of the pooled prefab. 
        /// Succesful clears retun true.
        /// </summary>
        /// <param name="prefab">Target Prefab to clear pools</param>
        /// <returns></returns>
        public static bool ClearAllObject(GameObject prefab)
        {
            if (prefab.scene.IsValid())
            {
                Debug.LogWarning($"The given gameobject: {prefab.name} is not a prefab");
                return false;
            }
            if (prefabReference.TryGetValue(prefab, out List<TypeReference> pools))
            {
                if (pools.Count > 0)
                {
                    foreach (TypeReference type in pools)
                    {
                        ClearObject(objectPool[type]);
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Clears the pool of type gameobject for
        /// the given prefab and destroys all instances
        /// of the pooled prefab. 
        /// Does not clear active objects and does not destroy pool. 
        /// Succesful clears retun true.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static bool ClearObject(GameObject prefab)
        {
            return ClearObjectInternal<GameObject>(prefab);
        }
        /// <summary>
        /// Clears the target pool of the given prefab and component combo and destroys all instances of the pooled prefab. 
        /// Does not clear active objects and does not destroy pool. 
        /// Succesful clears retun true.
        /// </summary>
        /// <typeparam name="T">Component on Prefab</typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static bool ClearObject<T>(GameObject prefab) where T : UnityEngine.Component
        {            
            return ClearObjectInternal<T>(prefab);  
        }
        
        private static bool ClearObjectInternal<T>(GameObject prefab) where T : UnityEngine.Object
        {
            TypeReference typeRef = GetTypeReference(prefab, typeof(T), false);
            if (typeRef == null)
                return false;
            else
            {
                ClearObject(objectPool[typeRef]);
                return true;
            }
        }
        private static void ClearObject(IPool pool)
        {
            pool.Clear();            
        }

        private static bool DestroyPoolInternal<T>(GameObject prefab) where T : UnityEngine.Object
        {
            if(prefab.scene.IsValid())
            {
                Debug.LogWarning($"The given gameobject: {prefab.name} is not a prefab");
                return false;
            }
            if (TryGetPool<T>(prefab, out Pool<T> pool))
            {
                DestroyPool(pool);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Destroys the pool and all its pooled object.
        /// Does not invoke onDestroy actions.
        /// </summary>
        /// <param name="prefab">Prefab</param>
        public static bool DestroyPool(GameObject prefab)
        {
            return DestroyPoolInternal<GameObject>(prefab);
        }

        /// <summary>
        /// Destroys the pool and all its pooled object.
        /// Does not invoke onDestroy actions.
        /// </summary>
        /// <typeparam name="T">Component</typeparam>
        /// <param name="prefab">Prefab</param>
        public static bool DestroyPool<T>(GameObject prefab) where T : UnityEngine.Component
        {
            return DestroyPoolInternal<T>(prefab);
        }
        /// <summary>
        /// Destroys the pool and all its pooled object.
        /// Does not invoke onDestroy actions.
        /// Warning: Ensure all objects are pooled before Destorying.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool">Target pool</param>
        public static void DestroyPool<T>(ref Pool<T> pool) where T : UnityEngine.Component
        {
            if(pool == null)
            {
                Debug.LogWarning($"Trying to destroy a non existing pool");
                return;
            }
            DestroyPool(pool.GetTransformKey());
            pool = null;
        }
        /// <summary>
        /// Destroys the pool and all its pooled object.
        /// Does not invoke onDestroy actions.
        /// Warning: Ensure all objects are pooled before Destorying.
        /// </summary>
        /// <param name="pool">Target pool</param>
        public static void DestroyPool(ref Pool<GameObject> pool)
        {
            if (pool == null)
            {
                Debug.LogWarning($"Trying to destroy a non existing pool");
                return;
            }
            DestroyPool(pool.GetTransformKey());
            pool = null;
        }
        internal static void DestroyPool<T>(Pool<T> pool) where T : UnityEngine.Object
        {
            DestroyPool(pool.GetTransformKey());
        }
        internal static void DestroyPool(TypeReference typeref)
        {
            objectPool.Remove(typeref);
            if (transformDict[typeref])
                GameObject.Destroy(transformDict[typeref].gameObject);
            transformDict.Remove(typeref);
        }

        #endregion

        #region Count
        #region CountActive
        private static int CountActiveInternal<T>(GameObject prefab) where T : UnityEngine.Object
        {
            if (TryGetPool<T>(prefab, out Pool<T> pool))
            {
                return CountActive(pool);
            }
            else
            {
                Debug.LogWarning($"The given prefab: {prefab.name} has not been pooled yet");
                return 0;
            }
        }

        /// <summary>
        /// Count all active objects of every pool based on the prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountActiveOfAll(GameObject prefab)
        {
            int final = 0;
            if(prefabReference.TryGetValue(prefab, out List<TypeReference> list))
            {
                foreach(TypeReference type in list)
                    final += objectPool[type].CountActive;
                return final;
            }
            else
            {
                Debug.LogWarning($"The given prefab: {prefab.name} has not been pooled yet");
                return 0;
            }
        }

        /// <summary>
        /// Count all active objects in the pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountActive(GameObject prefab) => CountActiveInternal<GameObject>(prefab);
        /// <summary>
        /// Count all active objects in the pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountActive<T>(GameObject prefab) where T : UnityEngine.Component => CountActiveInternal<T>(prefab);

        private static int CountActive(IPool pool) 
        {
            return pool.CountActive;
        }
        #endregion

        #region CountAll
        private static int CountAllInternal<T>(GameObject prefab) where T : UnityEngine.Object
        {
            if (TryGetPool<T>(prefab, out Pool<T> pool))
            {
                return CountAll(pool);
            }
            else
            {
                Debug.LogWarning($"The given prefab: {prefab.name} has not been pooled yet");
                return 0;
            }
        }
        /// <summary>
        /// Count all objects of every pool based on the prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountAllOfAll(GameObject prefab)
        {
            int final = 0;
            if (prefabReference.TryGetValue(prefab, out List<TypeReference> list))
            {
                foreach (TypeReference type in list)
                    final += objectPool[type].CountAll;
                return final;
            }
            else
            {
                Debug.LogWarning($"The given prefab: {prefab.name} has not been pooled yet");
                return 0;
            }
        }
        /// <summary>
        /// Count all objects in the pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountAll(GameObject prefab) => CountAllInternal<GameObject>(prefab);
        /// <summary>
        /// Count all objects in the pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountAll<T>(GameObject prefab) where T : UnityEngine.Component => CountAllInternal<T>(prefab);


        private static int CountAll(IPool pool)
        {
            return pool.CountActive;
        }
        #endregion

        #region CountInactive
        private static int CountInactiveInternal<T>(GameObject prefab) where T : UnityEngine.Object
        {
            if (TryGetPool<T>(prefab, out Pool<T> pool))
            {
                return pool.CountInactive;
            }
            else
            {
                Debug.LogWarning($"The given prefab: {prefab.name} has not been pooled yet");
                return 0;
            }
        }

        /// <summary>
        /// Count all inactive objects of every pool based on the prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountInactiveOfAll(GameObject prefab)
        {
            int final = 0;
            if (prefabReference.TryGetValue(prefab, out List<TypeReference> list))
            {
                foreach (TypeReference type in list)
                    final += objectPool[type].CountInactive;
                return final;
            }
            else
            {
                Debug.LogWarning($"The given prefab: {prefab.name} has not been pooled yet");
                return 0;
            }
        }
        /// <summary>
        /// Count all inactive objects in the pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountInactive(GameObject prefab)
        {
            return CountInactiveInternal<GameObject>(prefab);
        }
        /// <summary>
        /// Count all inactive objects in the pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int CountInactive<T>(GameObject prefab) where T : UnityEngine.Component
        {
            return CountInactiveInternal<T>(prefab);
        }
        #endregion

        #endregion

        #region DontDestroyOnLoad
        /// <summary>
        /// Sets a pool to be DontDestroyOnLoad
        /// </summary>
        /// <param name="pool">Pool instance</param>
        public static void SetPoolDontDestroyOnLoad(Pool<GameObject> pool)
        {
            if(pool != null)
                transformDict[pool.GetTransformKey()].parent = GlobalPool.transform;
            else
                Debug.LogWarning($"Trying to set a non existing pool to DontDestroyOnLoad");
        }
        /// <summary>
        /// Sets a pool to be DontDestroyOnLoad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool">Pool instance</param>
        public static void SetPoolDontDestroyOnLoad<T>(Pool<T> pool) where T : UnityEngine.Component
        {
            if (pool != null)
                transformDict[pool.GetTransformKey()].parent = GlobalPool.transform;
            else
                Debug.LogWarning($"Trying to set a non existing pool to DontDestroyOnLoad");
        }
        /// <summary>
        /// Removes a pool from DontDestroyOnLoad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool">Pool instance</param>
        public static void RemoveDontDestroyOnLoad(Pool<GameObject> pool)
        {
            if (pool != null)
                transformDict[pool.GetTransformKey()].parent = LocalPool.transform;
            else
                Debug.LogWarning($"Trying to remove a non existing pool from DontDestroyOnLoad");
        }
        /// <summary>
        /// Removes a pool from DontDestroyOnLoad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool">Pool instance</param>
        public static void RemoveDontDestroyOnLoad<T>(Pool<T> pool) where T : UnityEngine.Component
        {
            if (pool != null)
                transformDict[pool.GetTransformKey()].parent = LocalPool.transform;
            else
                Debug.LogWarning($"Trying to remove a non existing pool from DontDestroyOnLoad");
        }
        #endregion

        #region Preload
        /// <summary>
        /// Pre-Initialise a set amount of object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool"></param>
        /// <param name="amount"></param>
        public static void PreloadObject<T>(Pool<T> pool, int amount, Action<T> onNewInstance = null) where T : UnityEngine.Component
        {
            if (pool != null)
                pool.PreloadObject(amount, onNewInstance);
            else
                Debug.LogWarning($"Trying to preload a non existing pool");
        }
        /// <summary>
        /// Pre-Initialise a set amount of object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool"></param>
        /// <param name="amount"></param>
        public static void PreloadObject(Pool<GameObject> pool, int amount, Action<GameObject> onNewInstance = null)
        {
            if (pool != null)
                pool.PreloadObject(amount, onNewInstance);
            else
                Debug.LogWarning($"Trying to preload a non existing pool");
        }
        #endregion
    }
}

