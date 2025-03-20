using FinancialAccounting.Domain;

namespace FinancialAccounting.Facades
{
    public class AnalyticsFacade
    {
        private readonly OperationFacade _operationFacade;
        private readonly BankAccountFacade _accountFacade;

        public AnalyticsFacade(OperationFacade operationFacade, BankAccountFacade accountFacade)
        {
            _operationFacade = operationFacade;
            _accountFacade = accountFacade;
        }

        public decimal GetTotalByType(OperationType type)
        {
            decimal total = 0;

            foreach (Operation operation in _operationFacade.GetOperations())
            {
                if (operation.Type == type)
                {
                    total += operation.Amount;
                }
            }

            return total;
        }

        public decimal GetTotalBalance()
        {
            decimal sum = 0;

            foreach (var account in _accountFacade.GetAccounts())
            {
                sum += account.Balance;
            }

            return sum;
        }
    }
}
