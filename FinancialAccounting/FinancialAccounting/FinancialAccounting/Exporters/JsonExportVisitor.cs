using FinancialAccounting.Domain;
using System.Text;

namespace FinancialAccounting.Exporters
{
    public class JsonExportVisitor : IExportVisitor
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public void Visit(BankAccount account)
        {
            _stringBuilder.AppendLine($"  {{ \"type\": \"BankAccount\", \"id\": \"{account.Id}\", \"name\": \"{account.Name}\", \"balance\": {account.Balance} }},");
        }

        public void Visit(Category category)
        {
            _stringBuilder.AppendLine($"  {{ \"type\": \"Category\", \"id\": \"{category.Id}\", \"name\": \"{category.Name}\", \"opType\": \"{category.Type}\" }},");
        }

        public void Visit(Operation operation)
        {
            _stringBuilder.AppendLine($"  {{ \"type\": \"Operation\", \"id\": \"{operation.Id}\", \"opType\": \"{operation.Type}\", \"amount\": {operation.Amount} }},");
        }

        public string GetResult()
        {
            string result = _stringBuilder.ToString().TrimEnd(',', '\r', '\n');

            return $"[\n{result}\n]";
        }
    }
}
