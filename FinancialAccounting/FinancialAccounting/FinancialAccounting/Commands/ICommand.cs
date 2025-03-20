namespace FinancialAccounting.Commands
{
    public interface ICommand
    {
        string Name { get; }
        void Execute();
    }
}
