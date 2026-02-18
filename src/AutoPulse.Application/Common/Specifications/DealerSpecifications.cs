using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Domain;

namespace AutoPulse.Application.Common.Specifications;

/// <summary>
/// Спецификация для фильтрации дилеров по рынку и рейтингу
/// </summary>
public class DealersByMarketSpecification : Specification<Dealer>
{
    public DealersByMarketSpecification(int marketId, decimal? minRating = null, int? page = null, int? pageSize = null)
    {
        AddCriteria(d => d.MarketId == marketId);
        
        if (minRating.HasValue)
        {
            AddCriteria(d => d.Rating >= minRating.Value);
        }

        AddInclude(d => d.Market);

        if (page.HasValue && pageSize.HasValue)
        {
            AddPaging(pageSize.Value, (page.Value - 1) * pageSize.Value);
        }
    }
}

/// <summary>
/// Спецификация для поиска дилеров по названию
/// </summary>
public class DealerSearchSpecification : Specification<Dealer>
{
    public DealerSearchSpecification(string searchTerm, int? page = null, int? pageSize = null)
    {
        AddCriteria(d => d.Name.Contains(searchTerm));
        AddInclude(d => d.Market);

        if (page.HasValue && pageSize.HasValue)
        {
            AddPaging(pageSize.Value, (page.Value - 1) * pageSize.Value);
        }
    }
}

/// <summary>
/// Спецификация для фильтрации автомобилей по рынку
/// </summary>
public class CarsByMarketSpecification : Specification<Car>
{
    public CarsByMarketSpecification(int marketId, bool? isAvailable = null)
    {
        AddCriteria(c => c.MarketId == marketId);
        
        if (isAvailable.HasValue)
        {
            AddCriteria(c => c.IsAvailable == isAvailable.Value);
        }

        AddInclude(c => c.Brand);
        AddInclude(c => c.Model);
        AddInclude(c => c.Dealer);
    }
}

/// <summary>
/// Спецификация для фильтрации автомобилей по дилеру
/// </summary>
public class CarsByDealerSpecification : Specification<Car>
{
    public CarsByDealerSpecification(int dealerId, int page = 1, int pageSize = 20)
    {
        AddCriteria(c => c.DealerId == dealerId);
        AddInclude(c => c.Brand);
        AddInclude(c => c.Model);
        AddPaging(pageSize, (page - 1) * pageSize);
    }
}
