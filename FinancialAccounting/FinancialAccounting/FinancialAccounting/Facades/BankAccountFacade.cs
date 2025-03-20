using FinancialAccounting.Domain;
using FinancialAccounting.Factories;
using FinancialAccounting.Proxies;

namespace FinancialAccounting.Facades
{
    public class BankAccountFacade
    {
        private readonly IBankAccountFactory _factory;
        private readonly IBankAccountRepository _repository;

        public BankAccountFacade(IBankAccountFactory factory, IBankAccountRepository repository)
        {
            _factory = factory;
            _repository = repository;
        }

        public BankAccount CreateAccount(string name, decimal initialBalance)
        {
            BankAccount account = _factory.Create(name, initialBalance);
            _repository.Add(account);
            return account;
        }

        public BankAccount ImportAccount(string id, string name, decimal balance)
        {
            BankAccount account = _factory.Create(id, name, balance);
            _repository.Add(account);

            return account;
        }

        public IEnumerable<BankAccount> GetAccounts() => _repository.GetAll();

        public BankAccount GetById(Guid id) => _repository.Get(id);

        public void DeleteAccount(Guid accountId) => _repository.Delete(accountId);
    }
}
