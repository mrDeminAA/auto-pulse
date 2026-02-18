using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Common.Specifications;
using AutoPulse.Application.Dealers.DTOs;
using MediatR;

namespace AutoPulse.Application.Dealers.Queries;

/// <summary>
/// Query: Получить дилеров по рынку
/// </summary>
public record GetDealersByMarketQuery(int MarketId) : IRequest<List<DealerDto>>;

/// <summary>
/// Handler для GetDealersByMarketQuery
/// </summary>
public class GetDealersByMarketHandler : IRequestHandler<GetDealersByMarketQuery, List<DealerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDealersByMarketHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<DealerDto>> Handle(GetDealersByMarketQuery request, CancellationToken ct)
    {
        var specification = new DealersByMarketSpecification(request.MarketId);
        var dealers = await _unitOfWork.Dealers.FindBySpecificationAsync(specification, ct);
        return _mapper.Map<List<DealerDto>>(dealers);
    }
}
