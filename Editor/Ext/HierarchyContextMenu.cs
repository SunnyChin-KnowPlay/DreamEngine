using UnityEditor;
using UnityEngine;

namespace MysticIsle.DreamEditor
{
    public static class HierarchyContextMenu
    {
        // 将菜单项添加到GameObject菜单，并为其设置优先级。优先级越小，菜单项越靠前。
        [MenuItem("GameObject/Copy Path", false, -100)]
        private static void CopyPath()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                string path = GetGameObjectPath(selectedObject);
                EditorGUIUtility.systemCopyBuffer = path;
                Debug.Log("Copied Path: " + path);
            }
        }

        // 确保菜单项仅当有GameObject选中时才可用
        [MenuItem("GameObject/Copy Path", true)]
        private static bool ValidateCopyPath()
        {
            return Selection.activeGameObject != null;
        }

        // 获取GameObject的完整路径
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            // 并且obj不能有NotEditable标记 如果有的话就直接跳出while循环
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;

                if ((obj.hideFlags & HideFlags.NotEditable) != 0)
                {
                    break;
                }

                path = obj.name + "/" + path;
            }
            return path;
        }
    }
}

