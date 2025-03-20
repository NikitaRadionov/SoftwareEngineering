using FinancialAccounting.Exporters;

namespace FinancialAccounting.Domain
{
    public class BankAccount : IExportable
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public decimal Balance { get; private set; }

        public BankAccount(Guid id, string name, decimal initialBalance)
        {
            Id = id;
            Name = name;
            Balance = initialBalance;
        }

        public void IncreaseBalance(decimal amount)
        {
            Balance += amount;
        }

        public void DecreaseBalance(decimal amount)
        {
            Balance -= amount;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} (Balance: {Balance:F2})";
        }

        public void Accept(IExportVisitor visitor) => visitor.Visit(this);
    }
}
