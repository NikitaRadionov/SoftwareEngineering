using FinancialAccounting.Exporters;
using FinancialAccounting.Facades;
using FinancialAccounting.Factories;
using FinancialAccounting.Importers;
using FinancialAccounting.Proxies;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialAccounting.DI
{
    public static class DependencyInjection
    {
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IBankAccountFactory, FinancialFactory>();
            services.AddSingleton<ICategoryFactory, FinancialFactory>();
            services.AddSingleton<IOperationFactory, FinancialFactory>();

            services.AddSingleton<InMemoryBankAccountRepository>();

            services.AddSingleton<IBankAccountRepository>(serviceProvider =>
            {
                var baseRepository = serviceProvider.GetRequiredService<InMemoryBankAccountRepository>();
                return new BankAccountRepositoryProxy(baseRepository);
            });

            services.AddSingleton<BankAccountFacade>();
            services.AddSingleton<CategoryFacade>();
            services.AddSingleton<OperationFacade>();
            services.AddSingleton<AnalyticsFacade>();

            services.AddSingleton<ExporterService>();
            services.AddSingleton<JsonImporter>();
            services.AddSingleton<CsvImporter>();

            services.AddSingleton<AppRunner>();
            return services.BuildServiceProvider();
        }
    }
}