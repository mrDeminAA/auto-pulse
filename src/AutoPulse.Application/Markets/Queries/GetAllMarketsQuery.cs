using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Markets.DTOs;
using MediatR;

namespace AutoPulse.Application.Markets.Queries;

/// <summary>
/// Query: Получить все рынки
/// </summary>
public record GetAllMarketsQuery : IRequest<IReadOnlyList<MarketDto>>;

/// <summary>
/// Handler для GetAllMarketsQuery
/// </summary>
public class GetAllMarketsHandler : IRequestHandler<GetAllMarketsQuery, IReadOnlyList<MarketDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllMarketsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MarketDto>> Handle(GetAllMarketsQuery request, CancellationToken ct)
    {
        var markets = await _unitOfWork.Markets.GetAllAsync(ct);
        return _mapper.Map<IReadOnlyList<MarketDto>>(markets);
    }
}
