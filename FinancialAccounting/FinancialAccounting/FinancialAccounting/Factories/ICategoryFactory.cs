using FinancialAccounting.Domain;

namespace FinancialAccounting.Factories
{
    public interface ICategoryFactory
    {
        Category Create(OperationType type, string name);
        Category Create(string id, OperationType type, string name);
    }
}
