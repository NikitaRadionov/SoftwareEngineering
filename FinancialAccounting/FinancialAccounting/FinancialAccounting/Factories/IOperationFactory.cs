using FinancialAccounting.Domain;

namespace FinancialAccounting.Factories
{
    public interface IOperationFactory
    {
        Operation Create(
            OperationType type,
            BankAccount bankAccount,
            decimal amount,
            DateTime date,
            string description,
            Category category);

        Operation Create(string id,
            OperationType type,
            BankAccount bankAccount,
            decimal amount,
            DateTime date,
            string description,
            Category category);
    }
}
