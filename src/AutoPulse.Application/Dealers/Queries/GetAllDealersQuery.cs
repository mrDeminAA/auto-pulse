using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Common.Specifications;
using AutoPulse.Application.Dealers.DTOs;
using MediatR;

namespace AutoPulse.Application.Dealers.Queries;

/// <summary>
/// Query: Получить всех дилеров с пагинацией и фильтрами
/// </summary>
public record GetAllDealersQuery(
    int? MarketId = null,
    decimal? MinRating = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<DealerDto>>;

/// <summary>
/// Handler для GetAllDealersQuery
/// </summary>
public class GetAllDealersHandler : IRequestHandler<GetAllDealersQuery, PagedResult<DealerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDealersHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<DealerDto>> Handle(GetAllDealersQuery request, CancellationToken ct)
    {
        var specification = new DealersByMarketSpecification(
            request.MarketId ?? 0,
            request.MinRating,
            request.Page,
            request.PageSize
        );

        var dealers = await _unitOfWork.Dealers.FindBySpecificationAsync(specification, ct);
        var totalCount = await _unitOfWork.Dealers.CountBySpecificationAsync(specification, ct);

        var items = _mapper.Map<List<DealerDto>>(dealers);
        return new PagedResult<DealerDto>(items, totalCount, request.Page, request.PageSize);
    }
}
