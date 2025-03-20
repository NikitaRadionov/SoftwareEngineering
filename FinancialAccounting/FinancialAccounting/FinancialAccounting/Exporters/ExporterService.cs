using FinancialAccounting.Domain;

namespace FinancialAccounting.Exporters
{
    public class ExporterService
    {
        public string ExportAllToJson(
            IEnumerable<BankAccount> accounts,
            IEnumerable<Category> categories,
            IEnumerable<Operation> operations)
        {
            var visitor = new JsonExportVisitor();

            foreach (var account in accounts)
            {
                account.Accept(visitor);
            }

            foreach (var category in categories)
            {
                category.Accept(visitor);
            }

            foreach (var operation in operations)
            {
                operation.Accept(visitor);
            }

            return visitor.GetResult();
        }

        public string ExportAllToCsv(
            IEnumerable<BankAccount> accounts,
            IEnumerable<Category> categories,
            IEnumerable<Operation> operations)
        {
            var visitor = new CsvExportVisitor();

            foreach (var account in accounts)
            {
                account.Accept(visitor);
            }

            foreach (var category in categories)
            {
                category.Accept(visitor);
            }

            foreach (var operation in operations)
            {
                operation.Accept(visitor);
            }

            return visitor.GetResult();
        }
    }
}
