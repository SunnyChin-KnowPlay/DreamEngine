#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MysticIsle.DreamEngine.UI
{
    public partial class WidgetPanel
    {
        [TitleGroup("Panel", "Widget Camera Settings", Order = 2)]
        [Button("Configure", ButtonSizes.Medium)]
        private void ConfigureWidgetPanel()
        {
            // 如果处于运行状态，不允许使用该编辑器功能
            if (Application.isPlaying)
            {
                Debug.LogError("运行时不能使用此功能，请在编辑器非运行模式下操作。");
                return;
            }

            // 如果当前对象为预制体资产，则加载、处理并保存预制体
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                Debug.LogWarning("当前对象为预制体资产，将对预制体进行处理。");
                string assetPath = AssetDatabase.GetAssetPath(gameObject);
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(assetPath);
                if (prefabContents.TryGetComponent<WidgetPanel>(out var panel))
                {
                    panel.DoConfigureWidgetPanel();
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, assetPath);
                    Debug.Log("预制体更新成功：" + assetPath);
                }
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return;
            }

            // 当前对象在场景中，直接调用内部方法
            DoConfigureWidgetPanel();
        }

        /// <summary>
        /// 配置 Widget Panel 的主要入口：配置 CanvasScaler 和摄像机
        /// </summary>
        private void DoConfigureWidgetPanel()
        {
            if (!TryGetComponent<Canvas>(out var canvas))
            {
                Debug.LogError("当前对象没有 Canvas 组件，无法配置 Widget Panel。");
                return;
            }

            // 判断父节点是否为 "Canvas (Environment)"，是则解除父子关系
            if (transform.parent != null && transform.parent.name == "Canvas (Environment)")
            {
                transform.SetParent(null, false);
            }

            // 配置 CanvasScaler（默认分辨率 1920x1080）
            ConfigureCanvasScaler();

            // 配置 Canvas 的摄像机（如果不存在则自动创建）
            ConfigureCanvasCamera(canvas);

            canvas.vertexColorAlwaysGammaSpace = true;

            EditorGUIUtility.PingObject(this.gameObject);
        }

        /// <summary>
        /// 配置 CanvasScaler，如果组件不存在则添加并设置默认参考分辨率 1920x1080
        /// </summary>
        private void ConfigureCanvasScaler()
        {
            if (!TryGetComponent<CanvasScaler>(out var scaler))
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                Debug.Log("自动添加并配置 CanvasScaler, 参考分辨率设为 1920x1080");
            }
        }

        /// <summary>
        /// 配置 Canvas 的摄像机；如果 Canvas.worldCamera 不存在则查找或创建预设摄像机
        /// </summary>
        /// <param name="canvas">目标 Canvas</param>
        private void ConfigureCanvasCamera(Canvas canvas)
        {
            if (canvas.worldCamera != null)
            {
                return;
            }

            Camera cam = GetComponentInChildren<Camera>();
            if (cam == null)
            {
                cam = CreatePresetCamera();
            }
            else
            {
                Debug.Log("在子物体中已找到摄像机: " + cam.name);
            }
            canvas.worldCamera = cam;
        }

        /// <summary>
        /// 创建预设摄像机并配置默认参数
        /// </summary>
        /// <returns>新创建的摄像机</returns>
        private Camera CreatePresetCamera()
        {
            GameObject camObj = new("PanelCamera");
            camObj.transform.SetParent(transform, false);
            camObj.transform.SetSiblingIndex(0);
            Camera cam = camObj.AddComponent<Camera>();

            cam.clearFlags = CameraClearFlags.Depth;
            cam.cullingMask = LayerMask.GetMask("UI");
            cam.orthographic = true;
            cam.nearClipPlane = 0f;

            Debug.Log("自动添加预设摄像机: " + cam.name);
            return cam;
        }
    }
}
#endif