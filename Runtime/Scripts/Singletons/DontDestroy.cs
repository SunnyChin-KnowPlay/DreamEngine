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
        /// <summary>
        /// 在场景视图中显示一个Gizmo提示，方便在Inspector中识别该对象
        /// </summary>
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            // 设置Gizmo颜色
            Gizmos.color = Color.yellow;
            // 绘制一个包围盒
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            // 绘制图标，true表示允许在2D视图中显示
            Gizmos.DrawIcon(transform.position, "d_UnityEditor.ConsoleWindow", true);
            // 在对象上方绘制标签
            Handles.Label(transform.position + Vector3.up * 1.2f, "DontDestroy", EditorStyles.boldLabel);
#endif
        }
        #endregion
    }
}