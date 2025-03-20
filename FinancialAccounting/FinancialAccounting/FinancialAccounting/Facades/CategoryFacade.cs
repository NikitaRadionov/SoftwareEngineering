using FinancialAccounting.Domain;
using FinancialAccounting.Factories;

namespace FinancialAccounting.Facades
{
    public class CategoryFacade
    {
        private readonly List<Category> _categories = new List<Category>();
        private readonly ICategoryFactory _factory;

        public CategoryFacade(ICategoryFactory factory)
        {
            _factory = factory;
        }

        public Category CreateCategory(OperationType type, string name)
        {
            Category category = _factory.Create(type, name);
            _categories.Add(category);

            return category;
        }

        public Category ImportCategory(string id, OperationType type, string name)
        {
            Category category = _factory.Create(id, type, name);
            _categories.Add(category);

            return category;
        }

        public IEnumerable<Category> GetCategories() => _categories.AsReadOnly();

        public Category GetById(Guid id) => _categories.FirstOrDefault(category => category.Id == id);

        public void DeleteCategory(Guid categoryId) => _categories.RemoveAll(category => category.Id == categoryId);
    }
}
