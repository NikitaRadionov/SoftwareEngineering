namespace FinancialAccounting.Exporters
{
    public interface IExportable
    {
        void Accept(IExportVisitor visitor);
    }
}
