using FinancialAccounting.Domain;

namespace FinancialAccounting.Factories
{
    public class FinancialFactory : IBankAccountFactory, ICategoryFactory, IOperationFactory
    {
        public BankAccount Create(string name, decimal initialBalance)
        {
            if (initialBalance < 0)
                throw new ArgumentException("Начальный баланс не может быть отрицательным.");

            return new BankAccount(Guid.NewGuid(), name, initialBalance);
        }

        public BankAccount Create(string id, string name, decimal initialBalance)
        {
            return new BankAccount(Guid.Parse(id), name, initialBalance);
        }

        public Category Create(OperationType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название категории не может быть пустым.");

            return new Category(Guid.NewGuid(), type, name);
        }

        public Category Create(string id, OperationType type, string name)
        {
            return new Category(Guid.Parse(id), type, name);
        }

        public Operation Create(
            OperationType type,
            BankAccount bankAccount,
            decimal amount,
            DateTime date,
            string description,
            Category category)
        {
            if (amount <= 0)
                throw new ArgumentException("Сумма операции должна быть > 0.");

            if (bankAccount == null)
                throw new ArgumentNullException(nameof(bankAccount));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            return new Operation(Guid.NewGuid(), type, bankAccount.Id, amount, date, description, category.Id);
        }

        public Operation Create(
            string id,
            OperationType type,
            BankAccount bankAccount,
            decimal amount,
            DateTime date,
            string description,
            Category category)
        {
            return new Operation(Guid.Parse(id), type, bankAccount.Id, amount, date, description, category.Id);
        }
    }
}
