using FinancialAccounting.Domain;

namespace FinancialAccounting.Proxies
{
    public interface IBankAccountRepository
    {
        BankAccount Get(Guid id);
        IEnumerable<BankAccount> GetAll();
        void Add(BankAccount account);
        void Delete(Guid id);
    }
}
