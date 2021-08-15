using System;
using System.Threading.Tasks;

namespace Datack.Agent.Console
{
    public class Program
    {
        private static async Task Main(String[] args)
        {
            await Datack.Agent.Program.Start(args);
        }
    }
}
