using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// AssetManager 是基于 Unity Addressables 的资源加载与管理工具类（含场景加载）。
    /// </summary>
    public static class AssetManager
    {
        // 用于跟踪已加载资源的字典，键为路径，值为对应的句柄
        private static readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();

        #region Load Asset

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <typeparam name="T">要加载的资源类型。</typeparam>
        /// <param name="path">资源路径。</param>
        /// <returns>加载成功返回资源实例；若路径无效或加载失败返回 null。</returns>
        public static T LoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = path.Replace("\\", "/");

            // 如果已加载过相同路径的资源，直接复用现有句柄
            if (_loadedAssets.TryGetValue(path, out var existingHandle))
            {
                T existingAsset = existingHandle.Result as T;
                if (existingAsset != null)
                {
                    return existingAsset;
                }
            }

            // 加载新的资源
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(path);
            T asset = handle.WaitForCompletion();

            if (asset != null)
            {
                _loadedAssets[path] = handle;
            }

            return asset;
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <typeparam name="T">要加载的资源类型。</typeparam>
        /// <param name="path">资源路径。</param>
        /// <returns>异步任务，完成时返回加载到的资源实例。</returns>
        public static async Task<T> LoadAssetAsync<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = path.Replace("\\", "/");

            // 如果已加载过相同路径的资源，直接复用现有句柄
            if (_loadedAssets.TryGetValue(path, out var existingHandle))
            {
                T existingAsset = existingHandle.Result as T;
                if (existingAsset != null)
                {
                    return existingAsset;
                }
            }

            // 加载新的资源
            var handle = Addressables.LoadAssetAsync<T>(path);
            await handle.Task;

            if (handle.Result != null)
            {
                _loadedAssets[path] = handle;
            }

            return handle.Result;
        }

        #endregion

        #region Unload Asset
        /// <summary>
        /// 释放资源以回收内存。
        /// </summary>
        /// <typeparam name="T">要释放的资源类型。</typeparam>
        /// <param name="asset">要释放的资源实例。</param>
        public static void ReleaseAsset<T>(T asset) where T : Object
        {
            if (asset == null)
                return;

            // 创建待删除键列表，避免遍历时修改字典
            var keysToRemove = new List<string>();

            foreach (var kvp in _loadedAssets)
            {
                if (kvp.Value.Result == asset)
                {
                    Addressables.Release(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }

            // 统一移除待删除键
            foreach (var key in keysToRemove)
            {
                _loadedAssets.Remove(key);
            }
        }

        /// <summary>
        /// 释放所有已加载资源。
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var handle in _loadedAssets.Values)
            {
                Addressables.Release(handle);
            }
            _loadedAssets.Clear();

            // 垃圾回收
            System.GC.Collect();
        }

        #endregion

        #region Load Scene

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="scenePath">场景路径（Addressables 键或地址）。</param>
        /// <param name="activateOnLoad">加载完成后是否立即激活场景。</param>
        /// <param name="loadSceneMode">场景加载模式。</param>
        /// <returns>异步场景加载句柄。</returns>
        public static async Task<AsyncOperationHandle<SceneInstance>> LoadSceneAsync(string scenePath, bool activateOnLoad = true, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(scenePath))
                return default;

            scenePath = scenePath.Replace("\\", "/");

            var handle = Addressables.LoadSceneAsync(scenePath, loadSceneMode, activateOnLoad);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[scenePath] = handle;
            }

            return handle;
        }

        /// <summary>
        /// 同步加载场景。
        /// </summary>
        /// <param name="scenePath">场景路径（Addressables 键或地址）。</param>
        /// <param name="activateOnLoad">加载完成后是否立即激活场景。</param>
        /// <param name="loadSceneMode">场景加载模式。</param>
        /// <returns>场景加载句柄。</returns>
        public static AsyncOperationHandle<SceneInstance> LoadScene(string scenePath, bool activateOnLoad = true, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(scenePath))
                return default;

            scenePath = scenePath.Replace("\\", "/");

            var handle = Addressables.LoadSceneAsync(scenePath, loadSceneMode, activateOnLoad);
            handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[scenePath] = handle;
            }

            return handle;
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="scenePath">场景路径（Addressables 键或地址）。</param>
        /// <returns>异步卸载句柄。</returns>
        public static AsyncOperationHandle UnloadScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return default;

            scenePath = scenePath.Replace("\\", "/");

            if (_loadedAssets.TryGetValue(scenePath, out var handle))
            {
                var unloadHandle = Addressables.UnloadSceneAsync(handle);
                _loadedAssets.Remove(scenePath);
                return unloadHandle;
            }

            return default;
        }

        #endregion

        #region Check Asset

        /// <summary>
        /// 检查指定路径的资源是否可用。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>若资源存在返回 true，否则返回 false。</returns>
        public static async Task<bool> IsAssetAvailableAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            path = path.Replace("\\", "/");
            var handle = Addressables.LoadResourceLocationsAsync(path);
            await handle.Task;

            bool exists = handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Count > 0;
            Addressables.Release(handle);
            return exists;
        }

        #endregion
    }
}
