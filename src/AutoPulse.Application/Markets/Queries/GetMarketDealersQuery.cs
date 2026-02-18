using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Common.Specifications;
using AutoPulse.Application.Dealers.DTOs;
using AutoPulse.Application.Markets.DTOs;
using MediatR;

namespace AutoPulse.Application.Markets.Queries;

/// <summary>
/// Query: Получить дилеров рынка
/// </summary>
public record GetMarketDealersQuery(int MarketId) : IRequest<IReadOnlyList<DealerBriefDto>>;

/// <summary>
/// Handler для GetMarketDealersQuery
/// </summary>
public class GetMarketDealersHandler : IRequestHandler<GetMarketDealersQuery, IReadOnlyList<DealerBriefDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMarketDealersHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DealerBriefDto>> Handle(GetMarketDealersQuery request, CancellationToken ct)
    {
        // Проверяем существование рынка
        var marketExists = await _unitOfWork.Markets.ExistsAsync(m => m.Id == request.MarketId, ct);
        if (!marketExists) return Array.Empty<DealerBriefDto>();

        var specification = new DealersByMarketSpecification(request.MarketId);
        var dealers = await _unitOfWork.Dealers.FindBySpecificationAsync(specification, ct);
        return _mapper.Map<IReadOnlyList<DealerBriefDto>>(dealers);
    }
}
