using FinancialAccounting.Domain;
using FinancialAccounting.Factories;

namespace FinancialAccounting.Facades
{
    public class OperationFacade
    {
        private readonly List<Operation> _operations = new List<Operation>();
        private readonly IOperationFactory _operationFactory;
        private readonly BankAccountFacade _accountFacade;
        private readonly CategoryFacade _categoryFacade;

        public OperationFacade(
            IOperationFactory factory,
            BankAccountFacade accountFacade,
            CategoryFacade categoryFacade)
        {
            _operationFactory = factory;
            _accountFacade = accountFacade;
            _categoryFacade = categoryFacade;
        }

        public Operation CreateOperation(OperationType type,
            Guid accountId, decimal amount, DateTime date, string description, Guid categoryId)
        {
            BankAccount account = _accountFacade.GetById(accountId);
            Category category = _categoryFacade.GetById(categoryId);

            if (account == null)
                throw new ArgumentException("Счёт не найден!");

            if (category == null)
                throw new ArgumentException("Категория не найдена!");

            Operation opertation = _operationFactory.Create(type, account, amount, date, description, category);
            _operations.Add(opertation);

            if (type == OperationType.Income)
            {
                account.IncreaseBalance(amount);
            }
            else
            {
                account.DecreaseBalance(amount);
            }

            return opertation;
        }

        public Operation ImportOperation(string id, OperationType type,
        Guid accountId, decimal amount, DateTime date, string description, Guid categoryId)
        {
            BankAccount account = _accountFacade.GetById(accountId);
            Category category = _categoryFacade.GetById(categoryId);

            if (account == null || category == null)
                throw new Exception("Счёт или категория не найдены при импорте операции.");

            Operation operation = _operationFactory.Create(id, type, account, amount, date, description, category);
            _operations.Add(operation);

            if (type == OperationType.Income)
            {
                account.IncreaseBalance(amount);
            }
            else
            {
                account.DecreaseBalance(amount);
            }

            return operation;
        }

        public IEnumerable<Operation> GetOperations() => _operations.AsReadOnly();

        public Operation GetById(Guid operationId) => _operations.FirstOrDefault(o => o.Id == operationId);

        public void DeleteOperation(Guid operationId)
        {
            _operations.RemoveAll(operation => operation.Id == operationId);
        }
    }
}
