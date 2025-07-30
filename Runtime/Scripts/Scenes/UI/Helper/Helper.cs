using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MysticIsle.DreamEngine.UI
{
    internal static class Helper
    {
        /// <summary>
        /// 实例化游戏对象
        /// </summary>
        /// <param name="path">界面在资源文件夹下的路径</param>
        /// <returns>创建的GameObject</returns>
        public static GameObject InitializeGameObject(string path)
        {
            GameObject gameObject = null;
            string fullPath = path;
            Object target = AssetManager.LoadAsset<Object>(fullPath);

            if (null != target)
            {
                gameObject = (GameObject)Object.Instantiate(target);
                if (null != gameObject)
                {
                    gameObject.name = target.name;
                }
            }

            return gameObject;
        }

        /// <summary>
        /// 异步实例化游戏对象
        /// </summary>
        /// <param name="path">界面在资源文件夹下的路径</param>
        /// <returns>创建的GameObject</returns>
        public static async UniTask<GameObject> InitializeGameObjectAsync(string path)
        {
            GameObject gameObject = null;
            string fullPath = path;
            Object target = await AssetManager.LoadAssetAsync<Object>(fullPath);

            if (null != target)
            {
                gameObject = (GameObject)Object.Instantiate(target);
                if (null != gameObject)
                {
                    gameObject.name = target.name;
                }
            }

            return gameObject;
        }


        /// <summary>
        /// 销毁游戏对象
        /// </summary>
        /// <param name="go">要销毁的GameObject</param>
        public static void DestroyGameObject(GameObject go)
        {
            Object.Destroy(go);
        }
    }
}