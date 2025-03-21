using Sirenix.OdinInspector;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 界面控制器
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(WidgetPanel))]
    public partial class PanelControl : Control
    {
        #region Const

        /// <summary>
        /// 通用的获取路径方法，依赖于每个子类的 Path 字段
        /// </summary>
        /// <typeparam name="T">PanelControl的子类</typeparam>
        /// <returns>路径字符串</returns>
        public static string GetPath<T>() where T : PanelControl
        {
            // 获取路径
            var pathAttribute = typeof(T).GetCustomAttribute<PanelPathAttribute>();
            if (pathAttribute == null)
            {
                throw new InvalidOperationException($"Panel path not defined for {typeof(T).Name}");
            }
            return pathAttribute.Path;
        }
        #endregion

        #region Params

        /// <summary>
        /// panel组件
        /// </summary>
        public WidgetPanel Panel => GetWidget<WidgetPanel>();

        /// <summary>
        /// 标题文本
        /// </summary>
        private TMPro.TMP_Text titleText;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Unity的Awake方法
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (null != this.Panel.TitleText)
            {
                titleText = this.Panel.TitleText.GetComponent<TMP_Text>();
            }

            WidgetButton exitButton = this.Panel.ExitButton;
            exitButton?.onClick.AddListener(OnClickExit);
        }

        /// <summary>
        /// Unity的OnEnable方法
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        /// <summary>
        /// Unity的OnDisable方法
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        #endregion

        #region Logic
        /// <summary>
        /// 关闭面板
        /// </summary>
        public override void Close()
        {
            base.Close();
        }

        /// <summary>
        /// 设置标题文本
        /// </summary>
        /// <param name="text">标题文本</param>
        protected void SetTitleText(string text)
        {
            if (null != this.titleText)
            {
                this.titleText.text = text;
            }
        }

        #endregion

        #region Event Listeners

        /// <summary>
        /// 退出按钮点击事件
        /// </summary>
        protected virtual void OnClickExit()
        {
            this.Close();
        }

        /// <summary>
        /// 首页按钮点击事件
        /// </summary>
        protected virtual void OnClickHome()
        {
            UIManager uiManager = this.FirstWidget.UIManager;
            if (null != uiManager)
            {
                uiManager.PopUntilTop();
            }
        }

        #endregion
    }
}