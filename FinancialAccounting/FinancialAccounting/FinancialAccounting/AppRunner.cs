using FinancialAccounting.Commands;
using FinancialAccounting.Domain;
using FinancialAccounting.Exporters;
using FinancialAccounting.Facades;
using FinancialAccounting.Importers;

namespace FinancialAccounting
{
    public class AppRunner
    {
        private readonly BankAccountFacade _accountFacade;
        private readonly CategoryFacade _categoryFacade;
        private readonly OperationFacade _operationFacade;
        private readonly AnalyticsFacade _analytics;
        private readonly ExporterService _exporterService;
        private readonly JsonImporter _jsonImporter;
        private readonly CsvImporter _csvImporter;

        public AppRunner(
            BankAccountFacade accountFacade,
            CategoryFacade categoryFacade,
            OperationFacade operationFacade,
            AnalyticsFacade analytics,
            ExporterService exporterService,
            JsonImporter jsonImporter,
            CsvImporter csvImporter)
        {
            _accountFacade = accountFacade;
            _categoryFacade = categoryFacade;
            _operationFacade = operationFacade;
            _analytics = analytics;
            _exporterService = exporterService;
            _jsonImporter = jsonImporter;
            _csvImporter = csvImporter;
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("======= МЕНЮ =======");
                Console.WriteLine("1. Создать счёт (без декоратора)");
                Console.WriteLine("2. Создать счёт (с декоратором TimingCommand)");
                Console.WriteLine("3. Список счетов");
                Console.WriteLine("4. Создать категорию");
                Console.WriteLine("5. Список категорий");
                Console.WriteLine("6. Добавить операцию (доход/расход)");
                Console.WriteLine("7. Список операций");
                Console.WriteLine("8. Удалить счёт");
                Console.WriteLine("9. Удалить категорию");
                Console.WriteLine("10. Удалить операцию");
                Console.WriteLine("I. Импорт из JSON");
                Console.WriteLine("C. Импорт из CSV");
                Console.WriteLine("E. Экспорт всех данных в JSON");
                Console.WriteLine("W. Экспорт всех данных в CSV");
                Console.WriteLine("A. Показать суммы доходов/расходов и общий баланс");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт: ");

                var choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1": CreateAccount(false); break;
                    case "2": CreateAccount(true); break;
                    case "3": ListBankAccounts(); break;
                    case "4": CreateCategory(); break;
                    case "5": ShowCategories(); break;
                    case "6": AddOperation(); break;
                    case "7": ShowOperations(); break;
                    case "8": DeleteAccount(); break;
                    case "9": DeleteCategory(); break;
                    case "10": DeleteOperation(); break;
                    case "I": ImportJson(); break;
                    case "C": ImportCsv(); break;
                    case "E": ExportToJson(); break;
                    case "W": ExportToCsv(); break;
                    case "A": ShowAnalytics(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Неизвестный пункт меню!");
                        break;
                }
            }
        }

        private void CreateAccount(bool useTimingDecorator)
        {
            Console.Write("Введите название счёта: ");
            var name = Console.ReadLine();

            Console.Write("Введите начальный баланс: ");
            var balanceText = Console.ReadLine();

            if (decimal.TryParse(balanceText, out var balance) == false)
            {
                Console.WriteLine("Некорректная сумма. Отмена.");
                return;
            }

            ICommand command = new CreateBankAccountCommand(_accountFacade, name, balance);

            if (useTimingDecorator)
            {
                command = new TimingCommandDecorator(command);
            }

            try
            {
                command.Execute();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
        }

        private void ListBankAccounts()
        {
            List<BankAccount> allAccounts = _accountFacade.GetAccounts().ToList();

            if (allAccounts.Any() == false)
            {
                Console.WriteLine("Нет счетов.");
            }
            else
            {
                allAccounts.ForEach(account => Console.WriteLine(account));
            }
        }

        private void CreateCategory()
        {
            Console.Write("Введите название категории: ");
            var categoryName = Console.ReadLine();

            Console.Write("Тип категории (1=доход, 2=расход): ");
            var typeText = Console.ReadLine();

            OperationType type = OperationType.Income;

            if (typeText == "2")
            {
                type = OperationType.Expense;
            }

            try
            {
                var category = _categoryFacade.CreateCategory(type, categoryName);
                Console.WriteLine($"Создана категория: {category}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
        }

        private void ShowCategories()
        {
            var categories = _categoryFacade.GetCategories().ToList();

            if (categories.Any() == false)
            {
                Console.WriteLine("Нет категорий.");
            }
            else
            {
                categories.ForEach(category => Console.WriteLine(category));
            }
        }

        private void AddOperation()
        {
            var allAccounts = _accountFacade.GetAccounts().ToList();

            if (allAccounts.Any() == false)
            {
                Console.WriteLine("Сначала создайте счет!");
                return;
            }

            Console.WriteLine("Список счетов:");

            for (int i = 0; i < allAccounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {allAccounts[i]}");
            }

            Console.Write("Выберите счёт: ");
            string accountChoice = Console.ReadLine();

            if (int.TryParse(accountChoice, out var accIdx) == false || accIdx < 1 || accIdx > allAccounts.Count)
            {
                Console.WriteLine("Некорректный выбор счёта.");
                return;
            }

            BankAccount chosenAccount = allAccounts[accIdx - 1];
            List<Category> categories = _categoryFacade.GetCategories().ToList();

            if (categories.Any() == false)
            {
                Console.WriteLine("Сначала создайте категорию!");
                return;
            }

            Console.WriteLine("Список категорий:");

            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i]}");
            }

            Console.Write("Выберите категорию: ");
            var categoryChoice = Console.ReadLine();

            if (int.TryParse(categoryChoice, out var catIdx) == false || catIdx < 1 || catIdx > categories.Count)
            {
                Console.WriteLine("Некорректный выбор категории.");
                return;
            }

            var chosenCategory = categories[catIdx - 1];

            Console.Write("Введите сумму операции (>0): ");
            var amountText = Console.ReadLine();

            if (decimal.TryParse(amountText, out var amount) == false || amount <= 0)
            {
                Console.WriteLine("Некорректная сумма.");
                return;
            }

            Console.Write("Описание (необязательно): ");
            string description = Console.ReadLine();

            try
            {
                Operation operation = _operationFacade.CreateOperation(chosenCategory.Type, chosenAccount.Id, amount, DateTime.Now, description, chosenCategory.Id);
                Console.WriteLine($"Добавлена операция: {operation}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
        }

        private void ShowOperations()
        {
            List<Operation> operations = _operationFacade.GetOperations().ToList();

            if (operations.Any() == false)
            {
                Console.WriteLine("Нет операций.");
            }
            else
            {
                operations.ForEach(operation => Console.WriteLine(operation));
            }
        }

        private void DeleteAccount()
        {
            List<BankAccount> allAccounts = _accountFacade.GetAccounts().ToList();

            if (allAccounts.Any() == false)
            {
                Console.WriteLine("Нет счетов для удаления.");
                return;
            }

            for (int i = 0; i < allAccounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {allAccounts[i]}");
            }

            Console.Write("Выберите счёт для удаления: ");
            string choice = Console.ReadLine();

            if (int.TryParse(choice, out var idx) == false || idx < 1 || idx > allAccounts.Count)
            {
                Console.WriteLine("Некорректно.");
                return;
            }

            var account = allAccounts[idx - 1];
            _accountFacade.DeleteAccount(account.Id);
            Console.WriteLine("Счёт удалён.");
        }

        private void DeleteCategory()
        {
            List<Category> categories = _categoryFacade.GetCategories().ToList();

            if (categories.Any() == false)
            {
                Console.WriteLine("Нет категорий для удаления.");
                return;
            }

            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i]}");
            }

            Console.Write("Выберите категорию для удаления: ");
            string choice = Console.ReadLine();

            if (int.TryParse(choice, out var idx) == false || idx < 1 || idx > categories.Count)
            {
                Console.WriteLine("Некорректно.");
                return;
            }

            var category = categories[idx - 1];
            _categoryFacade.DeleteCategory(category.Id);
            Console.WriteLine("Категория удалена.");
        }

        private void DeleteOperation()
        {
            List<Operation> operations = _operationFacade.GetOperations().ToList();

            if (operations.Any() == false)
            {
                Console.WriteLine("Нет операций для удаления.");
                return;
            }

            for (int i = 0; i < operations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {operations[i]}");
            }

            Console.Write("Выберите операцию для удаления: ");
            string choice = Console.ReadLine();

            if (int.TryParse(choice, out var idx) == false || idx < 1 || idx > operations.Count)
            {
                Console.WriteLine("Некорректно.");
                return;
            }

            var operation = operations[idx - 1];

            _operationFacade.DeleteOperation(operation.Id);
            Console.WriteLine("Операция удалена.");
        }

        private void ImportJson()
        {
            const string filePath = "demo.json";
            _jsonImporter.ImportData(filePath);
        }

        private void ImportCsv()
        {
            const string filePath = "demo.csv";
            _csvImporter.ImportData(filePath);
        }

        private void ExportToJson()
        {
            var accounts = _accountFacade.GetAccounts();
            var categories = _categoryFacade.GetCategories();
            var operations = _operationFacade.GetOperations();

            string json = _exporterService.ExportAllToJson(accounts, categories, operations);
            Console.WriteLine("=== Экспорт в JSON ===");

            const string exportFile = "demo.json";
            File.WriteAllText(exportFile, json);
            Console.WriteLine($"JSON также записан в файл: {exportFile}");
        }

        private void ExportToCsv()
        {
            var accounts = _accountFacade.GetAccounts();
            var categories = _categoryFacade.GetCategories();
            var operations = _operationFacade.GetOperations();

            var csv = _exporterService.ExportAllToCsv(accounts, categories, operations);

            Console.WriteLine("=== Экспорт в CSV ===");

            const string exportCsvFile = "demo.csv";
            File.WriteAllText(exportCsvFile, csv);
            Console.WriteLine($"CSV-файл сохранён: {exportCsvFile}");
        }

        private void ShowAnalytics()
        {
            decimal incomeSum = _analytics.GetTotalByType(OperationType.Income);
            decimal expenseSum = _analytics.GetTotalByType(OperationType.Expense);
            decimal totalBalance = _analytics.GetTotalBalance();

            Console.WriteLine($"Всего доходов: {incomeSum:F2}");
            Console.WriteLine($"Всего расходов: {expenseSum:F2}");
            Console.WriteLine($"Суммарный баланс (по всем счетам): {totalBalance:F2}");
        }
    }
}
