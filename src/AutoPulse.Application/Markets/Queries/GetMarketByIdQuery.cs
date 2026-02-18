using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Markets.DTOs;
using MediatR;

namespace AutoPulse.Application.Markets.Queries;

/// <summary>
/// Query: Получить рынок по ID
/// </summary>
public record GetMarketByIdQuery(int Id) : IRequest<MarketDetailsDto?>;

/// <summary>
/// Handler для GetMarketByIdQuery
/// </summary>
public class GetMarketByIdHandler : IRequestHandler<GetMarketByIdQuery, MarketDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMarketByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MarketDetailsDto?> Handle(GetMarketByIdQuery request, CancellationToken ct)
    {
        var market = await _unitOfWork.Markets.GetByIdAsync(request.Id, ct);
        if (market is null) return null;

        var dealersCount = await _unitOfWork.Dealers.CountAsync(d => d.MarketId == market.Id, ct);
        var carsCount = await _unitOfWork.Cars.CountAsync(c => c.MarketId == market.Id, ct);

        var dto = _mapper.Map<MarketDetailsDto>(market);
        return dto with { DealersCount = dealersCount, CarsCount = carsCount };
    }
}
