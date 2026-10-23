using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ArrowOut.Runtime
{
    // Original logical addresses, imported assets loaded on demand.
    public static class AddressableAssetService
    {
        [Serializable] private sealed class Index { public Entry[] entries; }
        [Serializable] private sealed class Entry { public string address; public string path; public string subAsset; public string sprite; }
        private static Dictionary<string, Entry> entries;
        private static Task indexLoading;
        private static int generation;
        private static readonly Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();
        private static readonly Dictionary<string, Task<UnityEngine.Object>> inFlight = new Dictionary<string, Task<UnityEngine.Object>>();
        public static string NormalizeKey(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            string key = path.Trim().Replace('\\', '/').TrimStart('/');
            const string prefix = "withdrawal-framework2/";
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) key = key.Substring(prefix.Length);
            int bracket = key.LastIndexOf('[');
            if (bracket > key.LastIndexOf('/') && key.EndsWith("]", StringComparison.Ordinal)) return key.Substring(0, bracket).ToLowerInvariant() + key.Substring(bracket);
            int extension = key.LastIndexOf('.');
            if (extension > key.LastIndexOf('/')) key = key.Substring(0, extension);
            return key.ToLowerInvariant();
        }
        private static Task<UnityEngine.Object> LoadResourceAsync(string path, Type type)
        {
            var source = new TaskCompletionSource<UnityEngine.Object>();
            ResourceRequest request = Resources.LoadAsync(path, type);
            if (request.isDone) source.SetResult(request.asset);
            else request.completed += _ => source.TrySetResult(request.asset);
            return source.Task;
        }
        private static async Task LoadIndexAsync()
        {
            TextAsset asset = await LoadResourceAsync("Framework2Replica/AddressMap", typeof(TextAsset)) as TextAsset;
            if (asset == null) throw new InvalidOperationException("Framework 2 Resources address index is missing.");
            Index index = JsonUtility.FromJson<Index>(asset.text);
            var map = new Dictionary<string, Entry>(StringComparer.Ordinal);
            foreach (Entry entry in index.entries) if (entry != null && !string.IsNullOrEmpty(entry.address) && !string.IsNullOrEmpty(entry.path)) map[NormalizeKey(entry.address)] = entry;
            entries = map;
        }
        public static Task<T> LoadAsync<T>(string logicalPath) where T : UnityEngine.Object => LoadCoreAsync<T>(logicalPath, true);
        public static Task<T> LoadOptionalAsync<T>(string logicalPath) where T : UnityEngine.Object => LoadCoreAsync<T>(logicalPath, false);
        private static async Task<T> LoadCoreAsync<T>(string logicalPath, bool required) where T : UnityEngine.Object
        {
            int requestGeneration = generation;
            if (entries == null) { if (indexLoading == null) indexLoading = LoadIndexAsync(); await indexLoading; }
            if (requestGeneration != generation) return null;
            string address = NormalizeKey(logicalPath);
            Entry entry;
            if (!entries.TryGetValue(address, out entry)) { if (required) Debug.LogError("Framework 2 unmapped asset address: " + logicalPath); return null; }
            string key = address + "|" + typeof(T).FullName;
            UnityEngine.Object value;
            if (cache.TryGetValue(key, out value) && value != null) return value as T;
            Task<UnityEngine.Object> task;
            if (!inFlight.TryGetValue(key, out task)) { task = LoadResourceAsync(entry.path, typeof(T)); inFlight[key] = task; }
            try
            {
                value = await task;
                if (requestGeneration != generation) return null;
                string subAssetName = !string.IsNullOrEmpty(entry.subAsset) ? entry.subAsset : entry.sprite;
                if (!string.IsNullOrEmpty(subAssetName))
                {
                    T[] assets = Resources.LoadAll<T>(entry.path);
                    value = null;
                    for (int i = 0; i < assets.Length; i++) if (assets[i].name == subAssetName) { value = assets[i]; break; }
                }
                if (value != null) cache[key] = value;
                else if (required) Debug.LogError("Framework 2 asset missing or wrong type: " + logicalPath + " -> " + entry.path);
                return value as T;
            }
            finally
            {
                if (inFlight.TryGetValue(key, out Task<UnityEngine.Object> current) && ReferenceEquals(current, task))
                    inFlight.Remove(key);
            }
        }
        public static async Task<GameObject> InstantiateAsync(string path, Transform parent = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GameObject prefab = await LoadAsync<GameObject>(path);
            cancellationToken.ThrowIfCancellationRequested();
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab, parent, false);
        }
        public static void ReleaseCache()
        {
            generation++;
            cache.Clear();
            inFlight.Clear();
        }
    }
}
