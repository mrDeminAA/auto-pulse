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
