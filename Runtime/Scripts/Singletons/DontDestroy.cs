using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 防止该GameObject在场景切换时被销毁。
    /// </summary>
    public class DontDestroy : MonoBehaviour
    {
        #region Unity Lifecycle
        /// <summary>
        /// Awake 在对象初始化时调用，解除对象父子关系并标记为不销毁。
        /// </summary>
        private void Awake()
        {
            // 解除与父对象的关系，确保对象独立存在
            transform.SetParent(null, false);
            // 标记该对象在加载新场景时不被销毁
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region Gizmos
        // 如有需要，可以在此处添加Gizmos绘制代码
        #endregion
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在Hierarchy窗口中为包含DontDestroy组件的对象显示一个标签提示。
    /// </summary>
    [InitializeOnLoad]
    static class DontDestroyHierarchyLabel
    {
        static DontDestroyHierarchyLabel()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj != null && obj.GetComponent<DontDestroy>() != null)
            {
                // 在Hierarchy中右侧显示提示标签
                Rect labelRect = new(selectionRect.x + selectionRect.width - 100, selectionRect.y, 100, selectionRect.height);
                EditorGUI.LabelField(labelRect, "DontDestroy", EditorStyles.boldLabel);
            }
        }
    }
#endif
}