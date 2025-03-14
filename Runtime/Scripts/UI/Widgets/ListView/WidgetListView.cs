using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 用于支持 LayoutGroup 布局的 WidgetListView。
    /// 该组件依赖于 LayoutGroup 来自动组织子对象的位置和大小。
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
        #endregion

        #region Mono Methods
        protected override void Awake()
        {
            base.Awake();
            if (null != this.itemPrefab)
            {
                // 隐藏预制体自己，避免在层级中显示
                this.itemPrefab.SetActive(false);
            }
        }
        #endregion

        #region Refresh Logic
        /// <summary>
        /// 用于实例化数据项，并刷新列表视图。
        /// 在刷新前，会清除已有的子物体（不销毁 ItemPrefab 本身），
        /// 并根据传入的数据集合实例化新的 GameObject 对象，
        /// 将其设置为当前对象的子物体，并通过回调函数对每个实例项进行初始化处理。
        /// </summary>
        /// <typeparam name="T">数据项类型</typeparam>
        /// <param name="dataCollection">数据集合，包含需要显示的数据</param>
        /// <param name="callback">
        /// 回调函数，参数依次为当前数据项以及生成的 GameObject，
        /// 可在此处进行额外的初始化，例如绑定数据或设置文本。
        /// </param>
        public void Refresh<T>(IReadOnlyCollection<T> dataCollection, Action<T, GameObject> callback)
        {
            // 清除当前所有子对象（如果需要可添加延迟销毁），但跳过 ItemPrefab 本身
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child == itemPrefab)
                    continue;
                DestroyImmediate(child);
            }

            // 判断是否设置了数据项预制体
            if (itemPrefab == null)
            {
                Debug.LogError("ItemPrefab 未设置，无法刷新 WidgetListView。");
                return;
            }

            // 遍历数据集合，依次实例化预制体作为列表项
            foreach (T data in dataCollection)
            {
                // 实例化 ItemPrefab，并设置其 parent 为当前对象，同时保持局部坐标
                GameObject item = Instantiate(itemPrefab, transform, false);
                item.SetActive(true);
                // 调用回调函数，对 item 进行额外初始化，如绑定数据到组件或设置显示内容
                callback?.Invoke(data, item);
            }
        }
        #endregion
    }
}