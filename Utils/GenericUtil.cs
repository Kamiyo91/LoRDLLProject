using System.Threading.Tasks;

namespace BigDLL4221.Utils
{
    public static class GenericUtil
    {
        public static async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }
    }
}