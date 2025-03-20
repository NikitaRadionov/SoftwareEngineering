using FinancialAccounting.Domain;
using System.Text;

namespace FinancialAccounting.Exporters
{
    public class CsvExportVisitor : IExportVisitor
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public void Visit(BankAccount account)
        {
            _stringBuilder.AppendLine($"Account;{account.Id};{account.Name};{account.Balance}");
        }

        public void Visit(Category category)
        {
            _stringBuilder.AppendLine($"Category;{category.Id};{category.Type};{category.Name}");
        }

        public void Visit(Operation operation)
        {
            _stringBuilder.AppendLine($"Operation;{operation.Id};{operation.Type};{operation.Amount};{operation.Date:yyyy-MM-dd};{operation.BankAccountId};{operation.CategoryId};{operation.Description}");
        }

        public string GetResult()
        {
            return _stringBuilder.ToString();
        }
    }
}
