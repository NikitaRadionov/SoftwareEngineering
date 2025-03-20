using FinancialAccounting.Commands;
using FinancialAccounting.Domain;
using FinancialAccounting.Exporters;
using FinancialAccounting.Facades;
using FinancialAccounting.Factories;
using FinancialAccounting.Proxies;
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
        private readonly FinancialFactory _factory = new FinancialFactory();
        private readonly InMemoryBankAccountRepository _baseRepository = new InMemoryBankAccountRepository();
        private readonly BankAccountRepositoryProxy _proxy;
        private readonly BankAccountFacade _accountFacade;
        private readonly CategoryFacade _categoryFacade;
        private readonly OperationFacade _operationFacade;
        private readonly ExporterService _exporterService;

        public ImporterExporterTests()
        {
            _proxy = new BankAccountRepositoryProxy(_baseRepository);
            _accountFacade = new BankAccountFacade(_factory, _proxy);
            _categoryFacade = new CategoryFacade(_factory);
            _operationFacade = new OperationFacade(_factory, _accountFacade, _categoryFacade);
            _exporterService = new ExporterService();
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
    }

    #endregion
}