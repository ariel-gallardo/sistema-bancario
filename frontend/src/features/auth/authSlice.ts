import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit';
import axios from 'axios';
import { apiConfig } from '../../config/api';
import type { LoginResponse } from '../../types/api';

export interface LoginPayload {
  email: string;
  password: string;
}

interface AuthProfile {
  clienteId: string;
  cuentaPrincipalId: string;
  tarjetaPrincipalId: string;
  nombre: string;
  email: string;
  expiraUtc: string;
}

interface AuthState {
  token: string | null;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error?: string;
  profile?: AuthProfile;
}

const initialState: AuthState = {
  token: null,
  status: 'idle',
};

export const login = createAsyncThunk<
  LoginResponse,
  LoginPayload,
  { rejectValue: string }
>('auth/login', async (payload, { rejectWithValue }) => {
  try {
    const response = await axios.post<LoginResponse>(`${apiConfig.clientes}/api/auth/login`, payload);
    return response.data;
  } catch (error) {
    let message = 'No pudimos iniciar sesión';
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        message = 'Credenciales inválidas';
      } else if (typeof error.response?.data?.message === 'string') {
        message = error.response.data.message;
      }
    }
    return rejectWithValue(message);
  }
});

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.status = 'loading';
        state.error = undefined;
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<LoginResponse>) => {
        state.status = 'succeeded';
        state.token = action.payload.token;
        state.profile = {
          clienteId: action.payload.clienteId,
          cuentaPrincipalId: action.payload.cuentaPrincipalId,
          tarjetaPrincipalId: action.payload.tarjetaPrincipalId,
          nombre: action.payload.nombre,
          email: action.payload.email,
          expiraUtc: action.payload.expiraUtc,
        };
      })
      .addCase(login.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.token = null;
        state.profile = undefined;
      });
  },
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;
