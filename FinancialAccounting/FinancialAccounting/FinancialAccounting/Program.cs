using FinancialAccounting.DI;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialAccounting
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var application = serviceProvider.GetRequiredService<AppRunner>();
            application.Run();
        }
    }
}
