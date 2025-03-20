using FinancialAccounting.Domain;

namespace FinancialAccounting.Proxies
{
    public class InMemoryBankAccountRepository : IBankAccountRepository
    {
        private readonly List<BankAccount> _items = new List<BankAccount>();

        public BankAccount Get(Guid id) => _items.FirstOrDefault(account => account.Id == id);
        public IEnumerable<BankAccount> GetAll() => _items.AsReadOnly();

        public void Add(BankAccount account)
        {
            _items.Add(account);
        }

        public void Delete(Guid id)
        {
            _items.RemoveAll(account => account.Id == id);
        }
    }
}
