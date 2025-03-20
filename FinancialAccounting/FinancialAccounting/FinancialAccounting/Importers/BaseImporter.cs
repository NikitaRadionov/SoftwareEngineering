using FinancialAccounting.Facades;

namespace FinancialAccounting.Importers
{
    public abstract class BaseImporter
    {
        protected readonly BankAccountFacade AccountFacade;
        protected readonly CategoryFacade CategoryFacade;
        protected readonly OperationFacade OperationFacade;

        protected BaseImporter(
            BankAccountFacade accountFacade,
            CategoryFacade categoryFacade,
            OperationFacade operationFacade)
        {
            AccountFacade = accountFacade;
            CategoryFacade = categoryFacade;
            OperationFacade = operationFacade;
        }

        public void ImportData(string filePath)
        {
            Console.WriteLine($"Начинаем импорт из файла: {filePath}");
            string fileContent = File.ReadAllText(filePath);

            ParseAndPopulate(fileContent);

            Console.WriteLine($"Импорт из файла {filePath} завершён.");
        }

        protected abstract void ParseAndPopulate(string content);
    }
}
