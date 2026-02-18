export interface DealerBrief {
  id: number;
  name: string;
  rating: number;
  contactInfo: string | null;
  address: string | null;
}

export interface Dealer {
  id: number;
  name: string;
  rating: number;
  contactInfo: string | null;
  address: string | null;
  marketId: number;
  marketName: string;
  marketRegion: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface DealerDetails extends Dealer {
  marketCurrency: string;
  carsCount: number;
  availableCarsCount: number;
}

export interface CarBrief {
  id: number;
  brandId: number;
  modelId: number;
  year: number;
  price: number;
  currency: string;
  isAvailable: boolean;
  brandName: string;
  modelName: string;
}

export interface CarDto {
  id: number;
  brandId: number;
  modelId: number;
  marketId: number;
  dealerId: number | null;
  dataSourceId: number | null;
  year: number;
  price: number;
  currency: string;
  vin: string | null;
  mileage: number;
  transmission: string | null;
  engine: string | null;
  fuelType: string | null;
  color: string | null;
  location: string | null;
  country: string | null;
  sourceUrl: string;
  imageUrl: string | null;
  isAvailable: boolean;
  createdAt: string;
  brandName: string;
  modelName: string;
  marketName: string;
  dealerName: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface DealerStats {
  totalDealers: number;
  averageRating: number;
  topRated: DealerBrief[];
  byMarket: MarketStat[];
}

export interface MarketStat {
  market: string;
  count: number;
}
