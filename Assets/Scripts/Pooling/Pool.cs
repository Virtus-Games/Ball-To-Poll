using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace GDTools.ObjectPooling
{

    public class Pool : MonoBehaviour
    {
        [SerializeField] private int capacity = 5;
        [SerializeField] private bool autoGrow = false;
        [SerializeField] private GameObject poolObjectPrefab;

        private int _activeObjects = 0;

        public int activeObjects { get { return _activeObjects; } }

        [HideInInspector] public UnityAction<PoolObject> onObjectInstantiated;
        [HideInInspector] public UnityAction<PoolObject> onObjectDestroyed;

        private Queue<PoolObject> queue = new Queue<PoolObject>();
        public CoinTranslateManager translateManager;


        private void Awake()
        {
            InitializeQueue(translateManager.CoinPrefabUIParent);
        }

        private void InitializeQueue(Transform parent)
        {
            for (int i = 0; i < capacity; i++)
                InsertObjectToQueue(parent);
        }

        private void InsertObjectToQueue(Transform parent)
        {
            PoolObject poolObj = Instantiate(poolObjectPrefab, parent).GetComponent<PoolObject>();
            poolObj.pool = this;
            poolObj.OnObjectDestroy();

            queue.Enqueue(poolObj);
        }



        // // Instaciate object : ------------------------------------------------
        // public PoolObject InstantiateObject () {
        //    return InstantiateObject (Vector3.zero, Quaternion.identity) ;
        // }

        public PoolObject InstantiateObject(Vector3 position, Transform transform)
        {
            return InstantiateObject(position, Quaternion.identity, transform);
        }

        // public PoolObject GetInstantiate (Transform transform) {
        //    return InstantiateObject (transform) ;
        // }

        public PoolObject InstantiateObject(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (queue.Count == 0)
            {
                if (autoGrow)
                {
                    capacity++;
                    InsertObjectToQueue(translateManager.CoinPrefabUIParent);

                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError(@"[ <color=#ff5566><b>Pool out of objects error</b></color> ] : no more gameobjects available in the <i>" + this.name + "</i> pool.\n"
                    + "Make sure to increase the <b>Capacity</b> or check the <b>Auto Grow</b> checkbox in the inspector.\n\n", gameObject);
#endif
                    return null;
                }
            }

            PoolObject poolObj = queue.Dequeue();

            poolObj.transform.position = position;
            poolObj.transform.rotation = rotation;
            poolObj.OnObjectInstantiate();

            if (!object.ReferenceEquals(onObjectInstantiated, null))
                onObjectInstantiated.Invoke(poolObj);

            _activeObjects++;

            return poolObj;
        }



        // Destroy object : ------------------------------------------------
        public void DestroyObject(PoolObject poolObj)
        {
            poolObj.OnObjectDestroy();

            queue.Enqueue(poolObj);

            if (!object.ReferenceEquals(onObjectDestroyed, null))
                onObjectDestroyed.Invoke(poolObj);

            _activeObjects--;
        }

        public void DestroyObject(PoolObject poolObj, float delay)
        {
            poolObj.__InvokeDestroy(delay);
        }

    }

}