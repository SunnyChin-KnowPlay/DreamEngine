using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 对象池
    /// </summary>
    public class GameObjectPool : MonoBehaviour
    {
        #region Params
        public int poolSize = 10; // 池的大小
        private readonly Queue<GameObject> objectPool = new();
        /// <summary>
        /// 预制体对象
        /// </summary>
        public GameObject PrefabObject => prefabObject;
        private GameObject prefabObject;

        private readonly List<DelayedRecycleItem> delayedRecycles = new();
        #endregion

        #region Mono
        private void Start()
        {

        }

        private void Update()
        {
            UpdateDelayedRecycleQueue();
        }

        private void OnDestroy()
        {
            foreach (var item in delayedRecycles)
            {
                if (item.Object != null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(item.Object);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(item.Object);
                    }
                }
            }
            delayedRecycles.Clear();
        }
        #endregion

        #region Logic
        internal void Setup(GameObject prefabObject)
        {
            this.prefabObject = prefabObject;
        }

        private GameObject InstantiateObject()
        {
            if (null == prefabObject)
            {
                return null;
            }
            return GameObject.Instantiate(prefabObject);
        }

        // 从池中获取对象
        public GameObject Allocate()
        {
            if (objectPool.Count > 0)
            {
                GameObject obj = objectPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // 如果池中没有对象，可以选择扩展池的大小并创建新对象
                GameObject newObj = InstantiateObject();
                return newObj;
            }
        }

        // 将对象放回池中
        public void Recycle(GameObject obj)
        {
            if (null == obj)
            {
                return;
            }

            obj.SetActive(false);
            if (objectPool.Count >= poolSize)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(obj);
                }
                else
                {
                    GameObject.DestroyImmediate(obj);
                }
            }
            else
            {
                obj.transform.SetParent(this.transform, false);
                objectPool.Enqueue(obj);
            }
        }

        public void RecycleDelay(GameObject obj, float delay)
        {
            delayedRecycles.Add(new DelayedRecycleItem(obj, Time.time + delay));
        }

        private void UpdateDelayedRecycleQueue()
        {
            delayedRecycles.RemoveAll(OnRemoveItem);
        }

        private bool OnRemoveItem(DelayedRecycleItem item)
        {
            float currentTime = Time.time;
            if (currentTime >= item.RecycleTime)
            {
                Recycle(item.Object);
                return true;
            }
            return false;
        }
        #endregion
    }

}
