using FinancialAccounting.Domain;
using FinancialAccounting.Facades;
using System.Text.Json;

namespace FinancialAccounting.Importers
{
    public class JsonImporter : BaseImporter
    {
        public JsonImporter(
            BankAccountFacade accountFacade,
            CategoryFacade categoryFacade,
            OperationFacade operationFacade)
            : base(accountFacade, categoryFacade, operationFacade)
        { }

        protected override void ParseAndPopulate(string content)
        {
            using JsonDocument document = JsonDocument.Parse(content);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine("Неверный формат JSON. Ожидается массив объектов.");
                return;
            }

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("type", out var typeProp) == false)
                    continue;

                string type = typeProp.GetString();

                if (string.IsNullOrEmpty(type))
                    continue;

                if (type.Equals("BankAccount", StringComparison.OrdinalIgnoreCase))
                {
                    if (element.TryGetProperty("id", out var idProperty) &&
                        element.TryGetProperty("name", out var nameProperty) &&
                        element.TryGetProperty("balance", out var balanceProperty))
                    {
                        string id = idProperty.GetString();
                        string name = nameProperty.GetString();
                        decimal balance = balanceProperty.GetDecimal();

                        AccountFacade.ImportAccount(id, name, balance);
                    }
                }
                else if (type.Equals("Category", StringComparison.OrdinalIgnoreCase))
                {
                    if (element.TryGetProperty("id", out var idProperty) &&
                        element.TryGetProperty("name", out var nameProperty) &&
                        element.TryGetProperty("opType", out var operationTypeProperty))
                    {
                        string id = idProperty.GetString();
                        string name = nameProperty.GetString();
                        string operationTypeText = operationTypeProperty.GetString();
                        OperationType operationType = operationTypeText.ToLower() == "income" ? OperationType.Income : OperationType.Expense;

                        CategoryFacade.ImportCategory(id, operationType, name);
                    }
                }
                else if (type.Equals("Operation", StringComparison.OrdinalIgnoreCase))
                {
                    if (element.TryGetProperty("id", out var idProperty) &&
                        element.TryGetProperty("opType", out var operationTypeProperty) &&
                        element.TryGetProperty("amount", out var amountProperty))
                    {
                        string id = idProperty.GetString();
                        string operationTypeText = operationTypeProperty.GetString();
                        OperationType operationType = operationTypeText?.ToLower() == "income" ? OperationType.Income : OperationType.Expense;
                        decimal amount = amountProperty.GetDecimal();

                        DateTime date = DateTime.Now;
                        string description = string.Empty;

                        BankAccount account = AccountFacade.GetAccounts().FirstOrDefault();
                        Category category = CategoryFacade.GetCategories().FirstOrDefault();

                        if (account == null || category == null)
                        {
                            Console.WriteLine("Невозможно импортировать операцию, т.к. отсутствует счёт или категория.");
                            continue;
                        }

                        OperationFacade.ImportOperation(id, operationType, account.Id, amount, date, description, category.Id);
                    }
                }
            }
        }
    }
}
