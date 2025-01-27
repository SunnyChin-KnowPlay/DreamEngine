using UnityEngine;

namespace MysticIsle.DreamEngine.Extension.Unity
{
    public static class ETransform
    {
        /// <summary>
        /// 销毁所有子节点
        /// </summary>
        /// <param name="transform"></param>
        public static void DestroyChildren(this Transform transform)
        {
            if (null == transform)
                return;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
            transform.DetachChildren();
        }



        /// <summary>
        /// 递归设置Transform及子节点的layer
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="layerMask"></param>
        public static void SetLayerWithChildren(this Transform trans, int layerMask)
        {
            if (layerMask < 0)
            {
                return;
            }
            trans.gameObject.layer = layerMask;
            for (int i = 0; i < trans.childCount; ++i)
            {
                SetLayerWithChildren(trans.GetChild(i), layerMask);
            }
        }
    }
}
