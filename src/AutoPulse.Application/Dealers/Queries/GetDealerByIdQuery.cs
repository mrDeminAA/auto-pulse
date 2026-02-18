using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Dealers.DTOs;
using MediatR;

namespace AutoPulse.Application.Dealers.Queries;

/// <summary>
/// Query: Получить дилера по ID
/// </summary>
public record GetDealerByIdQuery(int Id) : IRequest<DealerDetailsDto?>;

/// <summary>
/// Handler для GetDealerByIdQuery
/// </summary>
public class GetDealerByIdHandler : IRequestHandler<GetDealerByIdQuery, DealerDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDealerByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DealerDetailsDto?> Handle(GetDealerByIdQuery request, CancellationToken ct)
    {
        var dealer = await _unitOfWork.Dealers.GetByIdAsync(request.Id, ct);
        if (dealer is null) return null;

        var carsCount = await _unitOfWork.Cars.CountAsync(c => c.DealerId == dealer.Id, ct);
        var availableCarsCount = await _unitOfWork.Cars.CountAsync(c => c.DealerId == dealer.Id && c.IsAvailable, ct);

        var dto = _mapper.Map<DealerDetailsDto>(dealer);
        return dto with { CarsCount = carsCount, AvailableCarsCount = availableCarsCount };
    }
}
