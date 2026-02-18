export interface Market {
  id: number;
  name: string;
  region: string;
  currency: string;
  createdAt: string;
}

export interface MarketDetails extends Market {
  dealersCount: number;
  carsCount: number;
}

export interface DealerBrief {
  id: number;
  name: string;
  rating: number;
  contactInfo: string | null;
  address: string | null;
}

export interface CarsCount {
  totalCount: number;
  availableCount: number;
}
