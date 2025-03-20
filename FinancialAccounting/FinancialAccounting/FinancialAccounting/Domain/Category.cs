using FinancialAccounting.Exporters;

namespace FinancialAccounting.Domain
{
    public class Category : IExportable
    {
        public Guid Id { get; private set; }
        public OperationType Type { get; private set; }
        public string Name { get; private set; }

        public Category(Guid id, OperationType type, string name)
        {
            Id = id;
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} ({Type})";
        }

        public void Accept(IExportVisitor visitor) => visitor.Visit(this);
    }
}
