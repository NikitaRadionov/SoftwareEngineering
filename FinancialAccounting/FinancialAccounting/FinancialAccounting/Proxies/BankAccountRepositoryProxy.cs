using FinancialAccounting.Domain;

namespace FinancialAccounting.Proxies
{
    public class BankAccountRepositoryProxy : IBankAccountRepository
    {
        private readonly IBankAccountRepository _baseRepository;
        private readonly Dictionary<Guid, BankAccount> _cache = new Dictionary<Guid, BankAccount>();

        public BankAccountRepositoryProxy(IBankAccountRepository baseRepository)
        {
            _baseRepository = baseRepository;

            foreach (BankAccount account in baseRepository.GetAll())
            {
                _cache[account.Id] = account;
            }
        }

        public BankAccount Get(Guid id)
        {
            if (_cache.TryGetValue(id, out BankAccount foundAccount))
                return foundAccount;

            BankAccount fromBaseBankAccount = _baseRepository.Get(id);

            if (fromBaseBankAccount != null)
                _cache[fromBaseBankAccount.Id] = fromBaseBankAccount;

            return fromBaseBankAccount;
        }

        public IEnumerable<BankAccount> GetAll()
        {
            return _cache.Values;
        }

        public void Add(BankAccount account)
        {
            _cache[account.Id] = account;
            _baseRepository.Add(account);
        }

        public void Delete(Guid id)
        {
            _cache.Remove(id);
            _baseRepository.Delete(id);
        }
    }
}
