SimplePooling is a pooling system that operates on any desired prefabs and
can store any given compnent that exist on the root prefab item. This system does
not need you to place any additional object into the scene, it purely operates
on c#.

Refer to the sample scenes for examples on how to use different features available

How to:
1. Import the package
2. Use namespace "Sezylrin.SimplePooling" 
3. Replace any instantiate call with "Pooler.GetObject"
4. Replace any Destroy call with "Pooler.PoolObject"
5. Replace Start and Awake call with custom Initialize and Reset methods and assign them to onNewInstance and onGet Action (refer to example2 folders) 

Documentation is included in a separate PDF

SimplePooling contains additional functions for more utility
Utility:
-Assign "Action" events for when the pooler spawns a object, gets an object, releases an object or destroy an object.
-Retrieve counts for all, active and inactive objects within a pool.
-Allows Pre-Loading objects into the pool whenever you want with "Pooler.PreloadObjects" - 
    (Note. This feature does not work as intended in start,awake and onEnable calls when in Editor view and user has domain reloads turned off. It is an editor only problem)
-Allows adding and removing Pools from to be DontDestroyOnLoad with "Pooler.SetDontDestroyOnLoad" and "Pooler.RemoveDontDestroyOnLoad"
-Allows clearing and destroying pool at will with "Pooler.ClearObject" and "Pooler.DestroyPool"
-Supports child object pooling with restrictions, refer to PoolingExampleFour

-Get and store a local reference to a pool<T> with "Pooler.GetOrCreatePool" where T is just the component of the prefab you wish to store
-Can use nearly every function instead of calling "Pooler" by replacing "Pooler." with "[pool variable name]."

FAQ

Q. Why am i getting null reference exception from the object retrieved from the pool

A. Unity order of execution has Start, Awake, and OnEnable calls occur at the end of frame. When a new object is spawned and the target component is 
retrieved, any functions executed is triggering before those starter calls. Use an action assignment with OnNewInstance when using GetObjects instead
of default monobehaviour Start, Awake or OnEnable calls or manually call a start equivalent function.