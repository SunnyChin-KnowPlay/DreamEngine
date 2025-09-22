using System;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 面板打开模式（用于标记 Control 类型的默认打开语义）
    /// </summary>
    public enum PanelOpenMode
    {
        // 进入新面板并隐藏下层（常用于 Screen）
        Push,
        // 直接显示，不隐藏下层（Overlay/HUD/Tooltip）
        Show,
    }


    /// <summary>
    /// 面板属性（用于标记 Control 类型的打开模式和路径）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class PanelAttribute : Attribute
    {
        /// <summary>
        /// 打开模式
        /// </summary>
        public PanelOpenMode Mode { get; }

        /// <summary>
        /// 面板路径（可选）。若声明了 Path，则 Control.GetPath 将读取此值。
        /// </summary>
        public string Path { get; }

        public PanelAttribute(PanelOpenMode mode)
            : this(mode, null)
        {
        }

        public PanelAttribute(string path)
            : this(PanelOpenMode.Show, path)
        {
        }

        public PanelAttribute(PanelOpenMode mode, string path)
        {
            Mode = mode;
            Path = path;
        }
    }
}
