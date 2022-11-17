using System;
using System.Collections.Generic;
using Mod;
using UnityEngine;

namespace BigDLL4221.Utils
{
    //Made by uGuardian
    public abstract class AssetBundleManager
    {
        protected readonly Dictionary<string, AssetBundle> BundleDic = new Dictionary<string, AssetBundle>();

        protected readonly Dictionary<(string bundle, string asset), WeakReference<GameObject>> CacheDic =
            new Dictionary<(string bundle, string asset), WeakReference<GameObject>>();

        public abstract string ModId { get; set; }
        protected virtual string ModPath => Singleton<ModContentManager>.Instance.GetModPath(ModId);
        public virtual string AssetBundleFolder => $"{ModPath}/Resource/AssetBundle";

        public GameObject GetAsset(string bundlePath, string internalPath)
        {
            GameObject result;
            var cacheDicKey = (bundlePath, internalPath);
            if (CacheDic.TryGetValue(cacheDicKey, out var cache))
            {
                if (cache.TryGetTarget(out result)) return result;
            }
            else
            {
                cache = new WeakReference<GameObject>(null);
            }

            AssetBundle bundle;
            try
            {
                if (!BundleDic.TryGetValue(bundlePath, out bundle))
                {
                    bundle = AssetBundle.LoadFromFile(bundlePath);
                    result = bundle?.LoadAsset<GameObject>(internalPath);
                }
                else
                {
                    result = bundle.LoadAsset<GameObject>(internalPath);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning("Attempting to reload asset bundle");
                bundle = AssetBundle.LoadFromFile(bundlePath);
                result = bundle?.LoadAsset<GameObject>(internalPath);
            }

            if (result == null) throw new NullReferenceException("AssetBundle returned null");
            BundleDic[bundlePath] = bundle;
            cache.SetTarget(result);
            CacheDic[cacheDicKey] = cache;
            return result;
        }
    }
}