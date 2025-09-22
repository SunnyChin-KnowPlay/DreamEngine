using System;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 面板默认打开模式枚举。
    /// 用于标注 `Control` 派生类型的首选打开语义，供 UI 管理器在没有明确指定模式时参考。
    /// </summary>
    public enum PanelOpenMode
    {
        /// <summary>
        /// 使用框架或管理器的默认策略（未显式指定时的占位值）。
        /// </summary>
        Default,

        /// <summary>
        /// 以普通显示 (Show) 的方式打开面板：不会自动隐藏或替换其他面板，适用于轻量级弹层或工具窗口。
        /// </summary>
        Show,

        /// <summary>
        /// 将该面板作为导航栈的新一层 (Push)：打开时通常会隐藏当前栈顶，便于后退时恢复。
        /// 适用于页面式导航场景。
        /// </summary>
        Push,
    }


    /// <summary>
    /// 面板属性，用于在 `Control` 类型上声明默认的打开模式和/或资源路径。
    /// 在框架中，`PanelAttribute` 可帮助 `Control.GetPath` 或 UI 管理器读取类型级别的元数据。
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

        public PanelAttribute(string path, PanelOpenMode mode)
        {
            Mode = mode;
            Path = path;
        }
    }
}
