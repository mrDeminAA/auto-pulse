using AutoMapper;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Common.Specifications;
using AutoPulse.Application.Dealers.DTOs;
using MediatR;

namespace AutoPulse.Application.Dealers.Queries;

/// <summary>
/// Query: Получить автомобили дилера
/// </summary>
public record GetDealerCarsQuery(
    int DealerId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CarBriefDto>?>;

/// <summary>
/// Handler для GetDealerCarsQuery
/// </summary>
public class GetDealerCarsHandler : IRequestHandler<GetDealerCarsQuery, PagedResult<CarBriefDto>?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDealerCarsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<CarBriefDto>?> Handle(GetDealerCarsQuery request, CancellationToken ct)
    {
        // Проверяем существование дилера
        var dealerExists = await _unitOfWork.Dealers.ExistsAsync(d => d.Id == request.DealerId, ct);
        if (!dealerExists) return null;

        var specification = new CarsByDealerSpecification(request.DealerId, request.Page, request.PageSize);
        var cars = await _unitOfWork.Cars.FindBySpecificationAsync(specification, ct);
        var totalCount = await _unitOfWork.Cars.CountAsync(c => c.DealerId == request.DealerId, ct);

        var items = _mapper.Map<List<CarBriefDto>>(cars);
        return new PagedResult<CarBriefDto>(items, totalCount, request.Page, request.PageSize);
    }
}
