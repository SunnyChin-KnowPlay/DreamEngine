using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    /// <summary>
    /// 单条工具提示条目。
    /// </summary>
    [System.Serializable]
    public class TooltipEntry
    {
        [HorizontalGroup("Tooltip", 0.5f), OdinSerialize, LabelText("Title"), Tooltip("Tooltip title localization key / text."), LabelWidth(50)]
        public string Title;

        [HorizontalGroup("Tooltip", 0.5f), OdinSerialize, LabelText("Desc"), Tooltip("Tooltip description localization key / text."), LabelWidth(60)]
        public string Description;

        public TooltipEntry() { }
        public TooltipEntry(string title, string desc)
        {
            Title = title;
            Description = desc;
        }

        public TooltipEntry(string key)
        {
            Title = key + "_Name";
            Description = key + "_Des";
        }
    }
}
