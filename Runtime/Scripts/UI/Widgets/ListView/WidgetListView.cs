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
        public GameObject ItemPrefab;

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
        /// <param name="callback">回调函数，参数依次为当前数据项以及生成的 GameObject</param>
        public void Refresh<T>(IReadOnlyCollection<T> dataCollection, Action<T, GameObject> callback)
        {
            // 清除当前所有子对象（如果需要可添加延迟销毁），但跳过 ItemPrefab 本身
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child == ItemPrefab)
                    continue;
                DestroyImmediate(child);
            }

            // 判断是否设置了数据项预制体
            if (ItemPrefab == null)
            {
                Debug.LogError("ItemPrefab 未设置，无法刷新 WidgetListView。");
                return;
            }

            // 根据数据集合中的数据项，依次实例化预制体作为列表项
            foreach (T data in dataCollection)
            {
                // 实例化 ItemPrefab，并设置其 parent 为当前对象，同时保持局部坐标
                GameObject item = Instantiate(ItemPrefab, transform, false);
                // 调用回调函数，对 item 进行额外初始化，如绑定数据到组件或显示文本等操作
                callback?.Invoke(data, item);
            }
        }

        #endregion

        // 这里可以在此添加更多与 LayoutGroup 布局相关的逻辑
    }
}