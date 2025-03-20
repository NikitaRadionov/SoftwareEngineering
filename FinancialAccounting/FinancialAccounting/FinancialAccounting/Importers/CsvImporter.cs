using FinancialAccounting.Domain;
using FinancialAccounting.Facades;

namespace FinancialAccounting.Importers
{
    public class CsvImporter : BaseImporter
    {
        public CsvImporter(
            BankAccountFacade accountFacade,
            CategoryFacade categoryFacade,
            OperationFacade operationFacade)
            : base(accountFacade, categoryFacade, operationFacade)
        { }

        protected override void ParseAndPopulate(string content)
        {
            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] columns = line.Split(';');

                if (columns.Length == 0)
                    continue;

                if (columns[0].Equals("Account", StringComparison.OrdinalIgnoreCase) && columns.Length >= 4)
                {
                    string id = columns[1];
                    string name = columns[2];

                    if (decimal.TryParse(columns[3], out var balance))
                    {
                        AccountFacade.ImportAccount(id, name, balance);
                    }
                }
                else if (columns[0].Equals("Category", StringComparison.OrdinalIgnoreCase) && columns.Length >= 4)
                {
                    string id = columns[1];
                    string typeText = columns[2];
                    string name = columns[3];
                    OperationType type = typeText.ToLower() == "income" ? OperationType.Income : OperationType.Expense;

                    CategoryFacade.ImportCategory(id, type, name);
                }
                else if (columns[0].Equals("Operation", StringComparison.OrdinalIgnoreCase) && columns.Length >= 8)
                {
                    string id = columns[1];
                    string typeText = columns[2];
                    OperationType type = typeText.ToLower() == "income" ? OperationType.Income : OperationType.Expense;

                    if (decimal.TryParse(columns[3], out var amount) == false)
                        continue;

                    if (DateTime.TryParse(columns[4], out var date) == false)
                        continue;

                    string accountIdText = columns[5];
                    string categoryIdText = columns[6];
                    string description = columns[7];

                    if (Guid.TryParse(accountIdText, out Guid accountId) == false)
                        continue;

                    if (Guid.TryParse(categoryIdText, out Guid categoryId) == false)
                        continue;

                    OperationFacade.ImportOperation(id, type, accountId, amount, date, description, categoryId);
                }
            }
        }
    }
}
