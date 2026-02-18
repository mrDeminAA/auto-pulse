using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Dealers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Application.Dealers.Queries;

/// <summary>
/// Query: Получить статистику по дилерам
/// </summary>
public record GetDealerStatsQuery : IRequest<DealerStatsSummaryDto>;

/// <summary>
/// Handler для GetDealerStatsQuery
/// </summary>
public class GetDealerStatsHandler : IRequestHandler<GetDealerStatsQuery, DealerStatsSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDealerStatsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DealerStatsSummaryDto> Handle(GetDealerStatsQuery request, CancellationToken ct)
    {
        var totalDealers = await _unitOfWork.Dealers.CountAsync(ct: ct);
        
        var allDealers = await _unitOfWork.Dealers.GetAllAsync(ct);
        var averageRating = allDealers.Any() 
            ? allDealers.Average(d => d.Rating) 
            : 0;

        var topRated = allDealers
            .OrderByDescending(d => d.Rating)
            .Take(10)
            .ToList();
        var topRatedDtos = _mapper.Map<List<DealerBriefDto>>(topRated);

        var byMarket = await _unitOfWork.Markets.GetAllAsync(ct);
        var marketStats = new List<MarketDealerStatDto>();
        foreach (var market in byMarket)
        {
            var count = await _unitOfWork.Dealers.CountAsync(d => d.MarketId == market.Id, ct);
            marketStats.Add(new MarketDealerStatDto(market.Name, count));
        }

        return new DealerStatsSummaryDto(
            totalDealers,
            (decimal)averageRating,
            topRatedDtos,
            marketStats
        );
    }
}
