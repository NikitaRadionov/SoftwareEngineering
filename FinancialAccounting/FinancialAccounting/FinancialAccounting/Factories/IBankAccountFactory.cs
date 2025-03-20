using FinancialAccounting.Domain;

namespace FinancialAccounting.Factories
{
    public interface IBankAccountFactory
    {
        BankAccount Create(string name, decimal initialBalance);
        BankAccount Create(string id, string name, decimal initialBalance);
    }
}
