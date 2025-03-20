namespace FinancialAccounting.Commands
{
    public class TimingCommandDecorator : ICommand
    {
        private readonly ICommand _innerCommand;

        public TimingCommandDecorator(ICommand innerCommand)
        {
            _innerCommand = innerCommand;
        }

        public string Name => _innerCommand.Name;

        public void Execute()
        {
            DateTime startTime = DateTime.Now;
            _innerCommand.Execute();
            DateTime endTime = DateTime.Now;

            TimeSpan duration = endTime - startTime;

            Console.WriteLine($"Команда {Name} выполнена за {duration.TotalMilliseconds} мс.");
        }
    }
}
