export interface UserSearchRequest {
  brandId?: number;
  modelId?: number;
  generation?: string;
  yearFrom?: number;
  yearTo?: number;
  maxPrice?: number;
  maxMileage?: number;
  regions?: string;
}

export interface UserSearchResponse {
  id: number;
  userId: number;
  brandId: number | null;
  modelId: number | null;
  brandName: string | null;
  modelName: string | null;
  generation: string | null;
  yearFrom: number | null;
  yearTo: number | null;
  maxPrice: number | null;
  maxMileage: number | null;
  regions: string | null;
  status: string;
  createdAt: string;
}

export interface UserSearchStatus {
  id: number;
  status: string;
  queues: QueueStatusItem[];
}

export interface QueueStatusItem {
  id: number;
  status: string;
  lastParsedAt: string | null;
  nextParseAt: string | null;
  priority: number;
}

export type SearchStatusType = 'Active' | 'Paused' | 'Completed' | 'Cancelled';
