using UnityEngine;

namespace BigDLL4221.Utils
{
    public class Assets : AssetBundleManager
    {
        public Assets(string packageId)
        {
            ModId = packageId;
        }

        public sealed override string ModId { get; set; }
        public string BundlePath => $"{AssetBundleFolder}/MyBundle.bundle";

        public GameObject GetAsset(string internalPath)
        {
            return GetAsset(BundlePath, internalPath);
        }
    }
}