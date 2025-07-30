using Dream.FixMath;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine.Utilities
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static partial class Utility
    {
        /// <summary>
        /// 深拷贝一个 List<ICloneable> 中的所有元素到另一个 List<ICloneable>
        /// </summary>
        /// <param name="sourceList">源列表</param>
        /// <returns>包含深拷贝元素的目标列表</returns>
        public static bool DeepCloneList(this List<ICloneable> sourceList, List<ICloneable> clonedList)
        {
            if (sourceList.IsNullOrEmpty()) return false;
            if (clonedList.IsNullOrEmpty()) return false;

            foreach (ICloneable item in sourceList)
            {
                clonedList.Add((ICloneable)item.Clone());
            }

            return true;
        }

        public static Vector3 ToVector3(this FixVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Vector3 ToVector3(this FixVector2 value)
        {
            return new Vector3(value.X, value.Y, 0);
        }

        public static Vector2 ToVector2(this FixVector3 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public static Vector2 ToVector2(this FixVector2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public static FixVector3 ToFixVector3(this Vector3 value)
        {
            return new FixVector3(value.x, value.y, value.z);
        }

        public static FixVector3 ToFixVector3(this Vector2 value)
        {
            return new FixVector3(value.x, value.y, 0);
        }

        public static FixVector2 ToFixVector2(this Vector3 value)
        {
            return new FixVector2(value.x, value.y);
        }

        public static FixVector2 ToFixVector2(this Vector2 value)
        {
            return new FixVector2(value.x, value.y);
        }



        public static Vector3 ConvertVector3BetweenCameras(Vector3 position, Camera fromCamera, Camera toCamera)
        {
            // 将 position 从 fromCamera 的世界坐标转换为屏幕坐标
            Vector3 screenPosition = fromCamera.WorldToScreenPoint(position);

            // 将屏幕坐标转换为 toCamera 的世界坐标
            Vector3 worldPosition = toCamera.ScreenToWorldPoint(screenPosition);

            return worldPosition;
        }
    }
}
