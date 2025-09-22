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
    /// 标记面板默认的打开模式（OpenMode）
    /// 使用示例：
    /// [PanelOpenMode(PanelOpenMode.Push)]
    /// public class InventoryControl : Control { }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class PanelOpenModeAttribute : Attribute
    {
        public PanelOpenMode Mode { get; }

        public PanelOpenModeAttribute(PanelOpenMode mode)
        {
            Mode = mode;
        }
    }
}
