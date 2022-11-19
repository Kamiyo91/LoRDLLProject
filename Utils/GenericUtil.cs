using System;
using System.Linq;
using System.Threading.Tasks;
using BigDLL4221.Models;

namespace BigDLL4221.Utils
{
    public static class GenericUtil
    {
        public static async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        public static void OtherModCheck()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Any(x => x.GetName().Name == "Daat Floor MOD")) StaticModsInfo.DaatFloorFound = true;
        }
    }
}