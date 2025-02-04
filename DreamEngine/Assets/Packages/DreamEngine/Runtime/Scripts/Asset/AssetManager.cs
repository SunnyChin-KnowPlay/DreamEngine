using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// AssetManager is a static class that provides methods to load and manage assets using Unity Addressables.
    /// </summary>
    public static class AssetManager
    {
        // 用于跟踪已加载资源的字典，键是路径，值是对应的句柄
        private static readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();

        #region Load Asset

        /// <summary>
        /// Loads an asset synchronously.
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="path">The path of the asset to load.</param>
        /// <returns>The loaded asset, or null if the path is invalid or the load fails.</returns>
        public static T LoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = path.Replace("\\", "/");

            // 如果已经加载过相同路径的资源，直接使用已存在的 handle
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
        /// Loads an asset asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="path">The path of the asset to load.</param>
        /// <returns>A Task representing the asynchronous load operation. The Result property contains the loaded asset.</returns>
        public static async Task<T> LoadAssetAsync<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = path.Replace("\\", "/");

            // 如果已经加载过相同路径的资源，直接使用已存在的 handle
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
        /// Releases an asset to free up memory.
        /// </summary>
        /// <typeparam name="T">The type of the asset to release.</typeparam>
        /// <param name="asset">The asset to release.</param>
        public static void ReleaseAsset<T>(T asset) where T : Object
        {
            if (asset == null)
                return;

            // 创建一个待删除的键列表
            var keysToRemove = new List<string>();

            foreach (var kvp in _loadedAssets)
            {
                if (kvp.Value.Result == asset)
                {
                    Addressables.Release(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }

            // 在循环结束后一次性移除所有待删除的键
            foreach (var key in keysToRemove)
            {
                _loadedAssets.Remove(key);
            }
        }

        /// <summary>
        /// Releases all loaded assets to free up memory.
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

        #region Check Asset

        /// <summary>
        /// Checks if an asset is available at the given path.
        /// </summary>
        /// <param name="path">The path of the asset to check.</param>
        /// <returns>True if the asset exists, false otherwise.</returns>
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
