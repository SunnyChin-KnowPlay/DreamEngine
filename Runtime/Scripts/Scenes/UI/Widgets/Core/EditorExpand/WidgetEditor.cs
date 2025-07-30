#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    public partial class Widget
    {
        #region GUI
        private void OnDrawReferenceDragArea()
        {
#if UNITY_EDITOR
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(32));
            EditorGUI.HelpBox(rect, $"Drag and drop nodes here to automatically add them", MessageType.Info);
            CatchDragAndDrop(rect);
#endif
        }

#if UNITY_EDITOR
        private void CatchDragAndDrop(Rect val)
        {
            Event val2 = Event.current;
            EventType type = val2.type;
            if (type - EventType.DragUpdated <= 1 && val.Contains(val2.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (val2.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    AddDrops(DragAndDrop.objectReferences);
                }
            }
        }

        private void AddDrops(UnityEngine.Object[] drops)
        {
            if (drops != null && drops.Length != 0)
            {
                foreach (var d in drops)
                {
                    this.AddReference(d);
                }
            }
        }
#endif
        #endregion
    }
}