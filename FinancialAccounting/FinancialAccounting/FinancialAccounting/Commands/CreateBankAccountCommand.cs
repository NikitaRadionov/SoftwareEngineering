using FinancialAccounting.Domain;
using FinancialAccounting.Facades;

namespace FinancialAccounting.Commands
{
    public class CreateBankAccountCommand : ICommand
    {
        private readonly BankAccountFacade _bankAccountFacade;
        private readonly string _accountName;
        private readonly decimal _initialBalance;

        public CreateBankAccountCommand(BankAccountFacade facade, string name, decimal initialBalance)
        {
            _bankAccountFacade = facade;
            _accountName = name;
            _initialBalance = initialBalance;
        }

        public string Name => "CreateBankAccount";

        public void Execute()
        {
            BankAccount account = _bankAccountFacade.CreateAccount(_accountName, _initialBalance);
            Console.WriteLine($"Создан счёт: {account}");
        }
    }
}
