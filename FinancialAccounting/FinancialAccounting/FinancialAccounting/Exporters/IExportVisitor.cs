using FinancialAccounting.Domain;

namespace FinancialAccounting.Exporters
{
    public interface IExportVisitor
    {
        void Visit(BankAccount account);
        void Visit(Category category);
        void Visit(Operation operation);
    }
}
