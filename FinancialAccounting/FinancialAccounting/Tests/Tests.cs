using FinancialAccounting;
using FinancialAccounting.Commands;
using FinancialAccounting.DI;
using FinancialAccounting.Domain;
using FinancialAccounting.Exporters;
using FinancialAccounting.Facades;
using FinancialAccounting.Factories;
using FinancialAccounting.Importers;
using FinancialAccounting.Proxies;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Text.Json;

namespace Tests
{
    #region Domain Tests

    public class Tests
    {
        [Fact]
        public void BankAccount_IncreaseAndDecreaseBalance_WorksCorrectly()
        {
            var account = new BankAccount(Guid.NewGuid(), "TestAccount", 100m);

            account.IncreaseBalance(50m);
            account.DecreaseBalance(20m);

            Assert.Equal(130m, account.Balance);
        }

        [Fact]
        public void Category_ToString_ReturnsNameAndType()
        {
            var category = new Category(Guid.NewGuid(), OperationType.Expense, "Food");
            string result = category.ToString();
            Assert.Contains("Food", result);
            Assert.Contains("Expense", result);
        }

        [Fact]
        public void Operation_ToString_ReturnsExpectedFormat()
        {
            var operation = new Operation(Guid.NewGuid(), OperationType.Income, Guid.NewGuid(), 100m,
                                     DateTime.Parse("2025-03-09"), "Salary", Guid.NewGuid());
            string result = operation.ToString();
            Assert.Contains("Income", result);
            Assert.Contains("100", result);
            Assert.Contains("2025-03-09", result);
        }
    }

    #endregion

    #region Factory Tests

    public class FactoryTests
    {
        private readonly FinancialFactory _factory = new FinancialFactory();

        [Fact]
        public void CreateBankAccount_ReturnsValidAccount()
        {
            var account = _factory.Create("Test", 100m);
            Assert.NotEqual(Guid.Empty, account.Id);
            Assert.Equal("Test", account.Name);
            Assert.Equal(100m, account.Balance);
        }

        [Fact]
        public void CreateBankAccount_WithNegativeBalance_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _factory.Create("Test", -10m));
        }

        [Fact]
        public void ImportBankAccount_CreatesAccountWithGivenId()
        {
            string id = Guid.NewGuid().ToString();
            var account = _factory.Create(id, "ImportTest", 200m);
            Assert.Equal(Guid.Parse(id), account.Id);
            Assert.Equal("ImportTest", account.Name);
            Assert.Equal(200m, account.Balance);
        }

        [Fact]
        public void CreateCategory_ReturnsValidCategory()
        {
            var category = _factory.Create(OperationType.Income, "Salary");
            Assert.NotEqual(Guid.Empty, category.Id);
            Assert.Equal("Salary", category.Name);
            Assert.Equal(OperationType.Income, category.Type);
        }

        [Fact]
        public void ImportCategory_CreatesCategoryWithGivenId()
        {
            string id = Guid.NewGuid().ToString();
            var category = _factory.Create(id, OperationType.Expense, "Food");
            Assert.Equal(Guid.Parse(id), category.Id);
            Assert.Equal("Food", category.Name);
            Assert.Equal(OperationType.Expense, category.Type);
        }

        [Fact]
        public void CreateOperation_ReturnsValidOperation()
        {
            var account = _factory.Create("Acc", 1000m);
            var category = _factory.Create(OperationType.Income, "Salary");
            var operation = _factory.Create(OperationType.Income, account, 500m, DateTime.Parse("2025-03-09"), "TestOp", category);
            Assert.NotEqual(Guid.Empty, operation.Id);
            Assert.Equal(account.Id, operation.BankAccountId);
            Assert.Equal(category.Id, operation.CategoryId);
            Assert.Equal(500m, operation.Amount);
        }
    }

    #endregion

    #region Repository Tests

    public class RepositoryTests
    {
        [Fact]
        public void InMemoryBankAccountRepository_AddGetDelete_Works()
        {
            var repository = new InMemoryBankAccountRepository();
            var account = new BankAccount(Guid.NewGuid(), "RepoAcc", 500m);
            repository.Add(account);
            var fetched = repository.Get(account.Id);
            Assert.Equal(account, fetched);
            repository.Delete(account.Id);
            Assert.Null(repository.Get(account.Id));
        }

        [Fact]
        public void BankAccountRepositoryProxy_WorksCorrectly()
        {
            var baseRepository = new InMemoryBankAccountRepository();
            var proxy = new BankAccountRepositoryProxy(baseRepository);
            var account = new BankAccount(Guid.NewGuid(), "ProxyAcc", 300m);
            proxy.Add(account);
            var fetched = proxy.Get(account.Id);
            Assert.Equal(account, fetched);
            proxy.Delete(account.Id);
            Assert.Null(proxy.Get(account.Id));
        }
    }

    #endregion

    #region Facade Tests

    public class FacadeTests
    {
        private readonly FinancialFactory _factory = new FinancialFactory();
        private readonly InMemoryBankAccountRepository _baseRepository = new InMemoryBankAccountRepository();
        private readonly BankAccountRepositoryProxy _proxy;
        private readonly BankAccountFacade _accountFacade;
        private readonly CategoryFacade _categoryFacade;
        private readonly OperationFacade _operationFacade;
        private readonly AnalyticsFacade _analytics;

        public FacadeTests()
        {
            _proxy = new BankAccountRepositoryProxy(_baseRepository);
            _accountFacade = new BankAccountFacade(_factory, _proxy);
            _categoryFacade = new CategoryFacade(_factory);
            _operationFacade = new OperationFacade(_factory, _accountFacade, _categoryFacade);
            _analytics = new AnalyticsFacade(_operationFacade, _accountFacade);
        }

        [Fact]
        public void BankAccountFacade_CreateAndDeleteAccount_Works()
        {
            var account = _accountFacade.CreateAccount("TestAcc", 1000m);
            Assert.Contains(account, _accountFacade.GetAccounts());
            _accountFacade.DeleteAccount(account.Id);
            Assert.DoesNotContain(account, _accountFacade.GetAccounts());
        }

        [Fact]
        public void CategoryFacade_CreateAndDeleteCategory_Works()
        {
            var category = _categoryFacade.CreateCategory(OperationType.Income, "TestCat");
            Assert.Contains(category, _categoryFacade.GetCategories());
            _categoryFacade.DeleteCategory(category.Id);
            Assert.DoesNotContain(category, _categoryFacade.GetCategories());
        }

        [Fact]
        public void OperationFacade_CreateAndDeleteOperation_Works()
        {
            var account = _accountFacade.CreateAccount("OpAcc", 1000m);
            var category = _categoryFacade.CreateCategory(OperationType.Expense, "OpCat");
            var operation = _operationFacade.CreateOperation(OperationType.Expense, account.Id, 200m, DateTime.Parse("2025-03-09"), "TestOp", category.Id);
            Assert.Contains(operation, _operationFacade.GetOperations());
            _operationFacade.DeleteOperation(operation.Id);
            Assert.DoesNotContain(operation, _operationFacade.GetOperations());
        }

        [Fact]
        public void AnalyticsFacade_ReturnsCorrectTotals()
        {
            var account1 = _accountFacade.CreateAccount("Acc1", 1000m);
            var categoryIncome = _categoryFacade.CreateCategory(OperationType.Income, "Salary");
            var categoryExpense = _categoryFacade.CreateCategory(OperationType.Expense, "Food");
            _operationFacade.CreateOperation(OperationType.Income, account1.Id, 500m, DateTime.Now, "IncomeOp", categoryIncome.Id);
            _operationFacade.CreateOperation(OperationType.Expense, account1.Id, 200m, DateTime.Now, "ExpenseOp", categoryExpense.Id);

            decimal totalIncome = _analytics.GetTotalByType(OperationType.Income);
            decimal totalExpense = _analytics.GetTotalByType(OperationType.Expense);
            decimal totalBalance = _analytics.GetTotalBalance();

            Assert.Equal(500m, totalIncome);
            Assert.Equal(200m, totalExpense);
            Assert.Equal(1000m + 500m - 200m, totalBalance);
        }

        [Fact]
        public void AnalyticsFacade_ReturnsZero_WhenNoAccountsOrOperations()
        {
            var analytics = new AnalyticsFacade(_operationFacade, _accountFacade);

            decimal totalIncome = analytics.GetTotalByType(OperationType.Income);
            decimal totalExpense = analytics.GetTotalByType(OperationType.Expense);
            decimal totalBalance = analytics.GetTotalBalance();

            Assert.Equal(0m, totalIncome);
            Assert.Equal(0m, totalExpense);
            Assert.Equal(0m, totalBalance);
        }

        [Fact]
        public void BankAccountFacade_GetAccounts_ReturnsCorrectList()
        {
            var account1 = _accountFacade.CreateAccount("Acc1", 1000m);
            var account2 = _accountFacade.CreateAccount("Acc2", 2000m);

            var accounts = _accountFacade.GetAccounts();

            Assert.Contains(account1, accounts);
            Assert.Contains(account2, accounts);
            Assert.Equal(2, accounts.Count());
        }

        [Fact]
        public void OperationFacade_GetOperations_ReturnsFilteredOperations()
        {
            var account = _accountFacade.CreateAccount("Acc", 1000m);
            var categoryIncome = _categoryFacade.CreateCategory(OperationType.Income, "Salary");
            var categoryExpense = _categoryFacade.CreateCategory(OperationType.Expense, "Food");

            var incomeOp = _operationFacade.CreateOperation(OperationType.Income, account.Id, 500m, DateTime.Now, "Salary", categoryIncome.Id);
            var expenseOp = _operationFacade.CreateOperation(OperationType.Expense, account.Id, 200m, DateTime.Now, "Lunch", categoryExpense.Id);

            var operations = _operationFacade.GetOperations().ToList();

            Assert.Contains(incomeOp, operations);
            Assert.Contains(expenseOp, operations);
            Assert.Equal(2, operations.Count);
        }

        [Fact]
        public void CategoryFacade_GetCategories_ReturnsCorrectList()
        {
            var category1 = _categoryFacade.CreateCategory(OperationType.Income, "Bonus");
            var category2 = _categoryFacade.CreateCategory(OperationType.Expense, "Groceries");

            var categories = _categoryFacade.GetCategories();

            Assert.Contains(category1, categories);
            Assert.Contains(category2, categories);
            Assert.Equal(2, categories.Count());
        }

        [Fact]
        public void OperationFacade_CreateOperation_WithUnknownAccount_Throws()
        {
            var unknownAccountId = Guid.NewGuid();
            var category = _categoryFacade.CreateCategory(OperationType.Expense, "UnknownAccCat");

            Assert.Throws<ArgumentException>(() =>
                _operationFacade.CreateOperation(OperationType.Expense, unknownAccountId, 100m, DateTime.Now, "Desc", category.Id));
        }

        [Fact]
        public void OperationFacade_CreateOperation_WithUnknownCategory_Throws()
        {
            var account = _accountFacade.CreateAccount("RealAcc", 1000m);
            var unknownCategoryId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                _operationFacade.CreateOperation(OperationType.Expense, account.Id, 100m, DateTime.Now, "Desc", unknownCategoryId));
        }

        [Fact]
        public void OperationFacade_CreateOperation_WithZeroAmount_Throws()
        {
            var account = _accountFacade.CreateAccount("ZeroAcc", 500m);
            var category = _categoryFacade.CreateCategory(OperationType.Expense, "ZeroCat");

            Assert.Throws<ArgumentException>(() =>
                _operationFacade.CreateOperation(OperationType.Expense, account.Id, 0m, DateTime.Now, "ZeroSum", category.Id));
        }

        [Fact]
        public void OperationFacade_CreateOperation_WithNegativeAmount_Throws()
        {
            var account = _accountFacade.CreateAccount("NegAcc", 500m);
            var category = _categoryFacade.CreateCategory(OperationType.Expense, "NegCat");

            Assert.Throws<ArgumentException>(() =>
                _operationFacade.CreateOperation(OperationType.Expense, account.Id, -50m, DateTime.Now, "NegSum", category.Id));
        }

        [Fact]
        public void CategoryFacade_CreateCategory_WithEmptyName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                _categoryFacade.CreateCategory(OperationType.Income, ""));
        }

        [Fact]
        public void CategoryFacade_CreateCategory_WithWhitespaceName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                _categoryFacade.CreateCategory(OperationType.Income, "   "));
        }
    }

    #endregion

    #region Command Tests

    public class CommandTests
    {
        [Fact]
        public void CreateAccountCommand_ExecutesAndCallsFacade()
        {
            var factorySub = Substitute.For<IBankAccountFactory>();
            var repoSub = Substitute.For<IBankAccountRepository>();

            var expectedAccount = new BankAccount(Guid.NewGuid(), "CmdAcc", 100m);
            factorySub.Create("CmdAcc", 100m).Returns(expectedAccount);

            var facade = new BankAccountFacade(factorySub, repoSub);

            var command = new CreateBankAccountCommand(facade, "CmdAcc", 100m);
            command.Execute();

            repoSub.Received(1).Add(expectedAccount);
        }

        [Fact]
        public void TimingCommandDecorator_ExecutesInnerCommand()
        {
            bool isExecuted = false;
            var dummyCommand = Substitute.For<FinancialAccounting.Commands.ICommand>();
            dummyCommand.When(x => x.Execute()).Do(_ => isExecuted = true);

            var decorated = new TimingCommandDecorator(dummyCommand);
            decorated.Execute();

            Assert.True(isExecuted);
            dummyCommand.Received(1).Execute();
        }
    }

    #endregion

    #region Importer and Exporter Tests

    public class ImporterExporterTests
    {
        public class TestJsonImporter : JsonImporter
        {
            public TestJsonImporter(BankAccountFacade accountFacade, CategoryFacade categoryFacade, OperationFacade operationFacade)
                : base(accountFacade, categoryFacade, operationFacade)
            { }

            public void TestParseAndPopulate(string content)
            {
                base.ParseAndPopulate(content);
            }
        }

        public class TestCsvImporter : CsvImporter
        {
            public TestCsvImporter(BankAccountFacade accountFacade, CategoryFacade categoryFacade, OperationFacade operationFacade)
                : base(accountFacade, categoryFacade, operationFacade)
            { }

            public void TestParseAndPopulate(string content)
            {
                base.ParseAndPopulate(content);
            }
        }

        private readonly FinancialFactory _factory = new FinancialFactory();
        private readonly InMemoryBankAccountRepository _baseRepository = new InMemoryBankAccountRepository();
        private readonly BankAccountRepositoryProxy _proxy;
        private readonly BankAccountFacade _accountFacade;
        private readonly CategoryFacade _categoryFacade;
        private readonly OperationFacade _operationFacade;
        private readonly ExporterService _exporterService;

        private readonly TestCsvImporter _csvImporter;
        private readonly TestJsonImporter _jsonImporter;

        public ImporterExporterTests()
        {
            _proxy = new BankAccountRepositoryProxy(_baseRepository);
            _accountFacade = new BankAccountFacade(_factory, _proxy);
            _categoryFacade = new CategoryFacade(_factory);
            _operationFacade = new OperationFacade(_factory, _accountFacade, _categoryFacade);
            _exporterService = new ExporterService();

            _csvImporter = new TestCsvImporter(_accountFacade, _categoryFacade, _operationFacade);
            _jsonImporter = new TestJsonImporter(_accountFacade, _categoryFacade, _operationFacade);
        }

        [Fact]
        public void JsonExportVisitor_GeneratesValidJsonArray()
        {
            var account = _accountFacade.CreateAccount("TestAcc", 1000m);
            var category = _categoryFacade.CreateCategory(OperationType.Income, "Salary");
            var operation = _operationFacade.CreateOperation(OperationType.Income, account.Id, 500m, DateTime.Parse("2025-03-09"), "TestOp", category.Id);

            var visitor = new JsonExportVisitor();
            account.Accept(visitor);
            category.Accept(visitor);
            operation.Accept(visitor);
            string result = visitor.GetResult();

            Assert.StartsWith("[", result);
            Assert.EndsWith("]", result);
            var parsed = JsonDocument.Parse(result);
            Assert.Equal(JsonValueKind.Array, parsed.RootElement.ValueKind);
        }

        [Fact]
        public void CsvExportVisitor_GeneratesExpectedCsvFormat()
        {
            var account = _accountFacade.CreateAccount("CsvAcc", 2000m);
            var category = _categoryFacade.CreateCategory(OperationType.Expense, "Food");
            var operation = _operationFacade.CreateOperation(OperationType.Expense, account.Id, 300m, DateTime.Parse("2025-03-09"), "Lunch", category.Id);

            var visitor = new CsvExportVisitor();
            account.Accept(visitor);
            category.Accept(visitor);
            operation.Accept(visitor);
            string result = visitor.GetResult();

            Assert.Contains("Account;", result);
            Assert.Contains("Category;", result);
            Assert.Contains("Operation;", result);
        }

        [Fact]
        public void ExporterService_ExportAllToJson_ReturnsValidStructuredJson()
        {
            var account = _accountFacade.CreateAccount("ExportAcc", 3000m);
            var category = _categoryFacade.CreateCategory(OperationType.Income, "Bonus");
            var operation = _operationFacade.CreateOperation(OperationType.Income, account.Id, 1000m, DateTime.Parse("2025-03-09"), "Bonus", category.Id);

            string json = _exporterService.ExportAllToJson(_accountFacade.GetAccounts(), _categoryFacade.GetCategories(), _operationFacade.GetOperations());
            var document = JsonDocument.Parse(json);

            Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);

            bool hasBankAccount = false;
            bool hasCategory = false;
            bool hasOperation = false;

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("type", out var typeProperty))
                {
                    string type = typeProperty.GetString();

                    if (type.Equals("BankAccount", StringComparison.OrdinalIgnoreCase))
                        hasBankAccount = true;
                    else if (type.Equals("Category", StringComparison.OrdinalIgnoreCase))
                        hasCategory = true;
                    else if (type.Equals("Operation", StringComparison.OrdinalIgnoreCase))
                        hasOperation = true;
                }
            }

            Assert.True(hasBankAccount, "Exported JSON does not contain any BankAccount objects.");
            Assert.True(hasCategory, "Exported JSON does not contain any Category objects.");
            Assert.True(hasOperation, "Exported JSON does not contain any Operation objects.");
        }

        [Fact]
        public void ExporterService_ExportAllToCsv_ReturnsExpectedCsv()
        {
            var account = _accountFacade.CreateAccount("CsvExportAcc", 4000m);
            var category = _categoryFacade.CreateCategory(OperationType.Expense, "Transport");
            var operation = _operationFacade.CreateOperation(OperationType.Expense, account.Id, 150m, DateTime.Parse("2025-03-09"), "Bus ticket", category.Id);

            string csv = _exporterService.ExportAllToCsv(_accountFacade.GetAccounts(), _categoryFacade.GetCategories(), _operationFacade.GetOperations());
            Assert.Contains("Account;", csv);
            Assert.Contains("Category;", csv);
            Assert.Contains("Operation;", csv);
        }

        [Fact]
        public void CsvImporter_UnknownLineType_IsSkipped()
        {
            string content = "UnknownType;id;SomeName;100\n";

            _csvImporter.TestParseAndPopulate(content);

            Assert.Empty(_accountFacade.GetAccounts());
            Assert.Empty(_categoryFacade.GetCategories());
            Assert.Empty(_operationFacade.GetOperations());
        }

        [Fact]
        public void CsvImporter_InsufficientColumnsForAccount_SkipsLine()
        {
            string content = "Account;OnlyTwoCols\n";
            _csvImporter.TestParseAndPopulate(content);
            Assert.Empty(_accountFacade.GetAccounts());
        }

        [Fact]
        public void CsvImporter_InsufficientColumnsForOperation_SkipsLine()
        {
            string content = "Operation;id;income;100\n";
            _csvImporter.TestParseAndPopulate(content);
            Assert.Empty(_operationFacade.GetOperations());
        }

        [Fact]
        public void CsvImporter_InvalidGuid_SkipsOperation()
        {
            string content = "Operation;cccccccc-cccc-cccc-cccc-cccccccccccc;income;850;2025-03-09;NOTAGUID;aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa;Test operation\n";
            _csvImporter.TestParseAndPopulate(content);
            Assert.Empty(_operationFacade.GetOperations());
        }

        [Fact]
        public void JsonImporter_UnknownType_SkipsObject()
        {
            string content = @"[
                { ""type"": ""UnknownThing"", ""id"": ""11111111-1111-1111-1111-111111111111"", ""name"": ""???"" }
            ]";

            _jsonImporter.TestParseAndPopulate(content);

            Assert.Empty(_accountFacade.GetAccounts());
            Assert.Empty(_categoryFacade.GetCategories());
            Assert.Empty(_operationFacade.GetOperations());
        }

        [Fact]
        public void JsonImporter_MissingFields_SkipsObject()
        {
            string content = @"[
              { ""type"": ""BankAccount"", ""id"": ""11111111-1111-1111-1111-111111111111"", ""name"": ""NoBalance"" }
            ]";

            _jsonImporter.TestParseAndPopulate(content);

            Assert.Empty(_accountFacade.GetAccounts());
        }

        [Fact]
        public void CsvImporter_ValidOperationLine_CallsImportOperation()
        {
            var accountId = Guid.NewGuid().ToString();
            var categoryId = Guid.NewGuid().ToString();

            _accountFacade.ImportAccount(accountId, "CsvAcc", 1000m);
            _categoryFacade.ImportCategory(categoryId, OperationType.Income, "SalaryCat");

            string line =
                "Operation;99999999-9999-9999-9999-999999999999;income;850;2025-03-09;" +
                $"{accountId};{categoryId};Test operation\n";

            _csvImporter.TestParseAndPopulate(line);

            var operations = _operationFacade.GetOperations().ToList();
            Assert.Single(operations);
            Assert.Equal(Guid.Parse("99999999-9999-9999-9999-999999999999"), operations[0].Id);
            Assert.Equal(OperationType.Income, operations[0].Type);
            Assert.Equal(850m, operations[0].Amount);

            Assert.Equal(Guid.Parse(accountId), operations[0].BankAccountId);
            Assert.Equal(Guid.Parse(categoryId), operations[0].CategoryId);
        }

        [Fact]
        public void JsonImporter_ValidOperation_CallsImportOperation()
        {
            var accountId = Guid.NewGuid().ToString();
            var categoryId = Guid.NewGuid().ToString();

            _accountFacade.ImportAccount(accountId, "JsonAcc", 500m);
            _categoryFacade.ImportCategory(categoryId, OperationType.Expense, "JsonCat");

            string content = $@"
            [
              {{
                ""type"": ""Operation"",
                ""id"": ""88888888-8888-8888-8888-888888888888"",
                ""opType"": ""Expense"",
                ""amount"": 999
              }}
            ]";

            _jsonImporter.TestParseAndPopulate(content);

            var operations = _operationFacade.GetOperations().ToList();
            Assert.Single(operations);
            Assert.Equal(Guid.Parse("88888888-8888-8888-8888-888888888888"), operations[0].Id);
            Assert.Equal(OperationType.Expense, operations[0].Type);
            Assert.Equal(999m, operations[0].Amount);

            Assert.Equal(Guid.Parse(accountId), operations[0].BankAccountId);
            Assert.Equal(Guid.Parse(categoryId), operations[0].CategoryId);
        }
    }

    #endregion

    #region DI and Negative Importer Tests

    public class DIContainerTests
    {
        [Fact]
        public void DependencyInjection_ResolvesAllServices()
        {
            var serviceProvider = FinancialAccounting.DI.DependencyInjection.ConfigureServices();
            Assert.NotNull(serviceProvider.GetRequiredService<IBankAccountFactory>());
            Assert.NotNull(serviceProvider.GetRequiredService<ICategoryFactory>());
            Assert.NotNull(serviceProvider.GetRequiredService<IOperationFactory>());
            Assert.NotNull(serviceProvider.GetRequiredService<BankAccountFacade>());
            Assert.NotNull(serviceProvider.GetRequiredService<CategoryFacade>());
            Assert.NotNull(serviceProvider.GetRequiredService<OperationFacade>());
            Assert.NotNull(serviceProvider.GetRequiredService<AnalyticsFacade>());
            Assert.NotNull(serviceProvider.GetRequiredService<ExporterService>());
            Assert.NotNull(serviceProvider.GetRequiredService<JsonImporter>());
            Assert.NotNull(serviceProvider.GetRequiredService<CsvImporter>());
            Assert.NotNull(serviceProvider.GetRequiredService<AppRunner>());
        }
    }

    public class TestJsonImporter : JsonImporter
    {
        public TestJsonImporter(BankAccountFacade accountFacade, CategoryFacade categoryFacade, OperationFacade operationFacade)
            : base(accountFacade, categoryFacade, operationFacade)
        { }

        public void TestParseAndPopulate(string content)
        {
            base.ParseAndPopulate(content);
        }
    }

    public class TestCsvImporter : CsvImporter
    {
        public TestCsvImporter(BankAccountFacade accountFacade, CategoryFacade categoryFacade, OperationFacade operationFacade)
            : base(accountFacade, categoryFacade, operationFacade)
        { }

        public void TestParseAndPopulate(string content)
        {
            base.ParseAndPopulate(content);
        }
    }

    public class NegativeImporterTests
    {
        private readonly FinancialFactory _factory = new FinancialFactory();

        [Fact]
        public void CsvImporter_EmptyContent_CreatesNoObjects()
        {
            var baseRepository = new InMemoryBankAccountRepository();
            var proxy = new BankAccountRepositoryProxy(baseRepository);
            var accountFacade = new BankAccountFacade(_factory, proxy);
            var categoryFacade = new CategoryFacade(_factory);
            var operationFacade = new OperationFacade(_factory, accountFacade, categoryFacade);
            var importer = new TestCsvImporter(accountFacade, categoryFacade, operationFacade);
            importer.TestParseAndPopulate("");
            Assert.Empty(accountFacade.GetAccounts());
            Assert.Empty(categoryFacade.GetCategories());
            Assert.Empty(operationFacade.GetOperations());
        }
    }

    #endregion

    #region Program and AppRunner Tests

    public class ProgramAppRunnerTests
    {
        public class FiniteStringReader : TextReader
        {
            private readonly Queue<string> _lines;

            public FiniteStringReader(IEnumerable<string> lines)
            {
                _lines = new Queue<string>(lines);
            }

            public override string ReadLine()
            {
                if (_lines.Count > 0)
                    return _lines.Dequeue();

                return "0";
            }
        }

        [Fact]
        public void ProgramMain_ExitImmediately_CoversMainAndAppRunner()
        {
            var inputReader = new FiniteStringReader(new string[] { "0" });
            var output = new StringWriter();

            var originalIn = Console.In;
            var originalOut = Console.Out;

            try
            {
                Console.SetIn(inputReader);
                Console.SetOut(output);

                Program.Main(Array.Empty<string>());

                Console.Out.Flush();
                string consoleOutput = output.ToString();

                Assert.Contains("Выберите пункт", consoleOutput);
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void ProgramMain_CreateAccountAndExit_CoversPartOfMenu()
        {
            var inputLines = new[] { "1", "TestAccount", "100", "0" };
            var inputReader = new FiniteStringReader(inputLines);
            var output = new StringWriter();

            var originalIn = Console.In;
            var originalOut = Console.Out;

            try
            {
                Console.SetIn(inputReader);
                Console.SetOut(output);

                Program.Main(Array.Empty<string>());

                Console.Out.Flush();
                string consoleOutput = output.ToString();

                Assert.Contains("МЕНЮ", consoleOutput);
                Assert.Contains("Создан счёт:", consoleOutput);
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void AppRunner_Run_FullMenuSequence_CoversAllBranches()
        {
            string[] inputLines = new[]
            {
                "1",           // Создать счёт
                "Acc1",        // Название счёта
                "100",         // Баланс
                "4",           // Создать категорию
                "Cat1",        // Название категории
                "1",           // Тип: Income
                "6",           // Добавить операцию
                "1",           // Выбор счёта (первая строка)
                "1",           // Выбор категории (первая строка)
                "50",          // Сумма операции
                "OpDesc",      // Описание операции
                "8",           // Удалить счёт
                "1",           // Выбор счёта для удаления
                "9",           // Удалить категорию
                "1",           // Выбор категории для удаления
                "10",          // Удалить операцию
                "1",           // Выбор операции для удаления
                "I",           // Импорт из JSON (файл demo.json)
                "C",           // Импорт из CSV (файл demo.csv)
                "E",           // Экспорт в JSON
                "W",           // Экспорт в CSV
                "A",           // Показать аналитику
                "0"            // Выход
            };

            var inputReader = new FiniteStringReader(inputLines);
            var output = new StringWriter();

            var originalIn = Console.In;
            var originalOut = Console.Out;

            try
            {
                Console.SetIn(inputReader);
                Console.SetOut(output);

                var serviceProvider = DependencyInjection.ConfigureServices();
                var appRunner = serviceProvider.GetRequiredService<AppRunner>();
                appRunner.Run();

                Console.Out.Flush();
                string consoleOutput = output.ToString();

                Assert.Contains("Выберите пункт", consoleOutput);

                Assert.Contains("Создан счёт:", consoleOutput);
                Assert.Contains("Создана категория:", consoleOutput);
                Assert.Contains("Добавлена операция:", consoleOutput);

                Assert.Contains("Счёт удалён.", consoleOutput);
                Assert.Contains("Категория удалена.", consoleOutput);
                Assert.Contains("Операция удалена.", consoleOutput);

                Assert.Contains("Импорт из файла", consoleOutput);
                Assert.Contains("JSON также записан в файл", consoleOutput);
                Assert.Contains("CSV-файл сохранён", consoleOutput);

                Assert.Contains("Всего доходов:", consoleOutput);
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }
    }

    #endregion
}
