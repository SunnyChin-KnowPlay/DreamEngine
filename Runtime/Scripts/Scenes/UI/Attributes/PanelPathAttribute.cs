using System;

namespace MysticIsle.DreamEngine.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class PanelPathAttribute : Attribute
    {
        public string Path { get; }

        public PanelPathAttribute(string path)
        {
            Path = path;
        }
    }

}