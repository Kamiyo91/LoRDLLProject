using UnityEngine;

namespace BigDLL4221.Utils
{
    public class Assets : AssetBundleManager
    {
        public Assets(string packageId,string bundleName)
        {
            ModId = packageId;
            BundleName = bundleName;
        }

        public sealed override string ModId { get; set; }
        public string BundleName { get; set; }
        public string BundlePath => $"{AssetBundleFolder}/{BundleName}.bundle";

        public GameObject GetAsset(string internalPath)
        {
            return GetAsset(BundlePath, internalPath);
        }
    }
}