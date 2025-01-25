using UnityEngine;

namespace MysticIsle.DreamEngine.Extension.Unity
{
    public static class EComponent
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (null == component)
                return null;

            if (!component.TryGetComponent<T>(out T com))
            {
                com = component.gameObject.AddComponent<T>();
            }

            return com;
        }

        public static T GetOrAddComponent<T>(this GameObject component) where T : Component
        {
            if (null == component)
                return null;

            if (!component.TryGetComponent<T>(out var com))
            {
                com = component.AddComponent<T>();
            }
            return com;
        }
    }
}
