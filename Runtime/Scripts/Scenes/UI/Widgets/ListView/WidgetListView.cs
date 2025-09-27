using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 用于支持 LayoutGroup 布局的 WidgetListView。
    /// 该组件依赖于 LayoutGroup 来自动组织子对象的位置和大小。
    /// 同时支持对象池机制，实现轮换刷新（适用于可拖动列表）。
    /// </summary>
    [RequireComponent(typeof(LayoutGroup))]
    public class WidgetListView : Widget
    {
        #region Fields
        /// <summary>
        /// 获取当前对象的 LayoutGroup 组件。
        /// </summary>
        public LayoutGroup LayoutGroupComponent => GetComponent<LayoutGroup>();

        /// <summary>
        /// ItemPrefab 用于实例化列表项。
        /// 请在 Inspector 中赋值此预制体。
        /// </summary>
        public GameObject itemPrefab;

        /// <summary>
        /// 对象池，保存已实例化的列表项，用于重复使用，避免重复销毁与实例化。
        /// </summary>
        private readonly Queue<GameObject> itemPool = new();
        #endregion

        #region Mono Methods
        protected override void Awake()
        {
            base.Awake();
            if (itemPrefab != null && itemPrefab.scene.IsValid())
            {
                // 隐藏预制体本身，避免在层级中显示
                itemPrefab.SetActive(false);
            }
        }
        #endregion

        #region Refresh Logic
        /// <summary>
        /// 用于实例化数据项，并刷新列表视图。
        /// 该方法通过对象池机制复用已生成的列表项，
        /// 每次刷新时先检查池中是否有足够的实例，
        /// 不够的会新实例化，使用完毕后所有项重新入池。
        /// </summary>
        /// <typeparam name="T">数据项类型</typeparam>
        /// <param name="dataCollection">数据集合，包含需要显示的数据</param>
        /// <param name="callback">
        /// 回调函数，参数依次为当前数据项以及生成的 GameObject，
        /// 可在此处进行额外的初始化，例如绑定数据或设置文本。
        /// </param>
        public void Refresh<T>(IReadOnlyCollection<T> dataCollection, Action<T, GameObject> callback)
        {
            // 隐藏池中所有对象，确保它们不显示
            foreach (GameObject obj in itemPool)
            {
                obj.SetActive(false);
            }

            if (null != dataCollection)
            {
                // 存放本次刷新需要使用的列表项
                List<GameObject> activeItems = new();

                // 遍历数据集合，为每个数据项获取一个列表项
                foreach (T data in dataCollection)
                {
                    GameObject item;
                    if (itemPool.Count > 0)
                    {
                        // 如果池中有对象，则复用
                        item = itemPool.Dequeue();
                        item.SetActive(true);
                    }
                    else
                    {
                        // 池中不足则新实例化一个
                        if (itemPrefab == null)
                        {
                            Debug.LogError("ItemPrefab 未设置，无法刷新 WidgetListView。");
                            return;
                        }
                        item = Instantiate(itemPrefab, transform, false);
                        item.SetActive(true);
                    }

                    // 调用回调函数，对列表项做额外初始化操作
                    callback?.Invoke(data, item);

                    // 将该对象加入本次刷新列表
                    activeItems.Add(item);
                }

                // 将使用过的对象重新归还到对象池
                foreach (GameObject item in activeItems)
                {
                    itemPool.Enqueue(item);
                }
            }
        }
        #endregion
    }
}