using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 游戏对象池管理器
    /// </summary>
    public abstract partial class GameObjectPoolAbstractManager<T> : SceneSingleton<T> where T : GameObjectPoolAbstractManager<T>
    {
        #region Params
        private readonly Dictionary<string, GameObjectPool> pools = new();
        #endregion

        #region Mono
        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.ClearAllPools();
        }
        #endregion

        #region Logic
        // 实现类似数组索引的方式获取对象池
        public GameObjectPool this[string poolName]
        {
            get { return GetPool(poolName); }
        }

        // 从管理器中获取一个对象池
        public GameObjectPool GetPool(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                return pools[poolName];
            }

            GameObject go = new(poolName);
            if (null != go)
            {
                go.transform.SetParent(this.transform, false);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.Setup(Instantiate(poolName));
                pools.Add(poolName, pool);
                return pool;
            }

            return null;
        }

        // 移除管理器中的一个对象池
        public void RemovePool(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                GameObjectPool pool = pools[poolName];
                pools.Remove(poolName);
                Destroy(pool.gameObject); // 销毁对象池游戏对象
            }
            else
            {
                Debug.LogWarning("Pool with name " + poolName + " does not exist in the pool manager.");
            }
        }

        private void ClearAllPools()
        {
            foreach (KeyValuePair<string, GameObjectPool> poolEntry in pools)
            {
                Destroy(poolEntry.Value.gameObject);
            }
            pools.Clear();
        }

        protected abstract GameObject Instantiate(string path);
        #endregion
    }

}
