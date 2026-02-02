import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import axios from 'axios';
import { apiConfig } from '../../config/api';
import type { RootState } from '../../store';
import type { AccountSummaryResponse, TarjetaMovimientosResponse } from '../../types/api';

interface DashboardState {
  account?: AccountSummaryResponse;
  card?: TarjetaMovimientosResponse;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error?: string;
}

const initialState: DashboardState = {
  status: 'idle',
};

export const fetchDashboardData = createAsyncThunk<
  { account: AccountSummaryResponse; card: TarjetaMovimientosResponse },
  void,
  { state: RootState; rejectValue: string }
>('dashboard/fetch', async (_, { getState, rejectWithValue }) => {
  const token = getState().auth.token;
  if (!token) {
    return rejectWithValue('Token no disponible');
  }

  try {
    const headers = { Authorization: `Bearer ${token}` };
    const [accountResponse, cardResponse] = await Promise.all([
      axios.get<AccountSummaryResponse>(`${apiConfig.cuentas}/api/cuentas/principal`, { headers }),
      axios.get<TarjetaMovimientosResponse>(`${apiConfig.tarjetas}/api/tarjetas/principal/movimientos?take=5`, {
        headers,
      }),
    ]);

    return { account: accountResponse.data, card: cardResponse.data };
  } catch (error) {
    let message = 'No pudimos cargar tus datos';
    if (axios.isAxiosError(error)) {
      if (typeof error.response?.data?.message === 'string') {
        message = error.response.data.message;
      } else if (error.response?.status === 401) {
        message = 'Tu sesión expiró, volvé a iniciar sesión';
      }
    }
    return rejectWithValue(message);
  }
});

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState,
  reducers: {
    resetDashboard: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchDashboardData.pending, (state) => {
        state.status = 'loading';
        state.error = undefined;
      })
      .addCase(fetchDashboardData.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.account = action.payload.account;
        state.card = action.payload.card;
      })
      .addCase(fetchDashboardData.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.account = undefined;
        state.card = undefined;
      });
  },
});

export const { resetDashboard } = dashboardSlice.actions;
export default dashboardSlice.reducer;
