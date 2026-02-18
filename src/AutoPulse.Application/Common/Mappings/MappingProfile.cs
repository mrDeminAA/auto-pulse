using AutoMapper;
using AutoPulse.Domain;
using AutoPulse.Application.Markets.DTOs;
using AutoPulse.Application.Dealers.DTOs;

namespace AutoPulse.Application.Common.Mappings;

/// <summary>
/// Профиль маппинга для сущностей
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Market mappings
        CreateMap<Market, MarketDto>();
        CreateMap<Market, MarketDetailsDto>()
            .ForMember(d => d.DealersCount, opt => opt.Ignore())
            .ForMember(d => d.CarsCount, opt => opt.Ignore());

        // Dealer mappings
        CreateMap<Dealer, DealerDto>()
            .ForMember(d => d.MarketName, opt => opt.MapFrom(s => s.Market.Name))
            .ForMember(d => d.MarketRegion, opt => opt.MapFrom(s => s.Market.Region));
            
        CreateMap<Dealer, DealerDetailsDto>()
            .ForMember(d => d.MarketName, opt => opt.MapFrom(s => s.Market.Name))
            .ForMember(d => d.MarketRegion, opt => opt.MapFrom(s => s.Market.Region))
            .ForMember(d => d.MarketCurrency, opt => opt.MapFrom(s => s.Market.Currency))
            .ForMember(d => d.CarsCount, opt => opt.Ignore())
            .ForMember(d => d.AvailableCarsCount, opt => opt.Ignore());

        CreateMap<Dealer, DealerBriefDto>();

        // Car mappings
        CreateMap<Car, CarBriefDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.ModelName, opt => opt.MapFrom(s => s.Model.Name));

        // Market stat mappings
        CreateMap<Market, MarketDealerStatDto>()
            .ForMember(d => d.Market, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Count, opt => opt.Ignore());
    }
}
