export interface RegisterRequest {
  email: string;
  password: string;
  name?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  token: string;
  expiresAt: string;
}

export interface UserDto {
  id: number;
  email: string;
  name: string | null;
  avatarUrl: string | null;
  telegramId: string | null;
  createdAt: string;
}

export interface UpdateProfileRequest {
  name?: string;
  avatarUrl?: string;
  telegramId?: string;
}
