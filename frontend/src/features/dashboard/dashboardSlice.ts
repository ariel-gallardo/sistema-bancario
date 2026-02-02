import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import axios from 'axios';
import { apiConfig } from '../../config/api';
import type { RootState } from '../../store';
import type { AccountSummaryResponse, TarjetaMovimientosResponse } from '../../types/api';

interface DashboardState {
  account?: AccountSummaryResponse;
  card?: TarjetaMovimientosResponse;
  favoriteAccountId: string | null;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error?: string;
}

const initialState: DashboardState = {
  status: 'idle',
  favoriteAccountId: null,
};

const buildAuthHeaders = (token: string) => ({
  Authorization: `Bearer ${token}`,
});

const getDashboardErrorMessage = (error: unknown, fallback = 'No pudimos cargar tus datos') => {
  let message = fallback;
  if (axios.isAxiosError(error)) {
    if (typeof error.response?.data?.message === 'string') {
      message = error.response.data.message;
    } else if (error.response?.status === 401) {
      message = 'Tu sesión expiró, volvé a iniciar sesión';
    }
  }
  return message;
};

const resolveFavoriteAccountId = (summary?: AccountSummaryResponse) => {
  if (!summary) {
    return null;
  }
  if (summary.esFavorita) {
    return summary.cuentaId;
  }
  return summary.otrasCuentas.find((account) => account.esFavorita)?.cuentaId ?? null;
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
    const headers = buildAuthHeaders(token);
    const [accountResponse, cardResponse] = await Promise.all([
      axios.get<AccountSummaryResponse>(`${apiConfig.cuentas}/api/cuentas/principal`, { headers }),
      axios.get<TarjetaMovimientosResponse>(`${apiConfig.tarjetas}/api/tarjetas/principal/movimientos?take=5`, {
        headers,
      }),
    ]);

    return { account: accountResponse.data, card: cardResponse.data };
  } catch (error) {
    return rejectWithValue(getDashboardErrorMessage(error));
  }
});

export const fetchAccountById = createAsyncThunk<
  AccountSummaryResponse,
  string,
  { state: RootState; rejectValue: string }
>('dashboard/fetchById', async (accountId, { getState, rejectWithValue }) => {
  const token = getState().auth.token;
  if (!token) {
    return rejectWithValue('Token no disponible');
  }

  try {
    const headers = buildAuthHeaders(token);
    const response = await axios.get<AccountSummaryResponse>(`${apiConfig.cuentas}/api/cuentas/${accountId}`, { headers });
    return response.data;
  } catch (error) {
    return rejectWithValue(getDashboardErrorMessage(error));
  }
});

export const updateFavoriteAccount = createAsyncThunk<
  void,
  string | null,
  { state: RootState; rejectValue: string }
>('dashboard/updateFavorite', async (accountId, { getState, rejectWithValue }) => {
  const token = getState().auth.token;
  if (!token) {
    return rejectWithValue('Token no disponible');
  }

  try {
    const headers = buildAuthHeaders(token);
    if (accountId) {
      await axios.put(`${apiConfig.cuentas}/api/cuentas/${accountId}/favorita`, null, { headers });
    } else {
      await axios.delete(`${apiConfig.cuentas}/api/cuentas/favorita`, { headers });
    }
  } catch (error) {
    return rejectWithValue(getDashboardErrorMessage(error));
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
        state.favoriteAccountId = resolveFavoriteAccountId(action.payload.account);
        state.error = undefined;
      })
      .addCase(fetchDashboardData.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.account = undefined;
        state.card = undefined;
        state.favoriteAccountId = null;
      })
      .addCase(fetchAccountById.pending, (state) => {
        state.status = 'loading';
        state.error = undefined;
      })
      .addCase(fetchAccountById.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.account = action.payload;
        state.favoriteAccountId = resolveFavoriteAccountId(action.payload);
      })
      .addCase(fetchAccountById.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
      })
      .addCase(updateFavoriteAccount.pending, (state) => {
        state.error = undefined;
      })
      .addCase(updateFavoriteAccount.fulfilled, (state, action) => {
        const nextFavoriteId = action.meta.arg;
        state.favoriteAccountId = nextFavoriteId;

        if (!state.account) {
          return;
        }

        const isCurrentFavorite = Boolean(nextFavoriteId && state.account.cuentaId === nextFavoriteId);
        state.account.esFavorita = isCurrentFavorite;
        if (!nextFavoriteId) {
          state.account.esFavorita = false;
        }

        state.account.otrasCuentas = state.account.otrasCuentas.map((cuenta) => ({
          ...cuenta,
          esFavorita: nextFavoriteId ? cuenta.cuentaId === nextFavoriteId : false,
        }));
      })
      .addCase(updateFavoriteAccount.rejected, (state, action) => {
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
      });
  },
});

export const { resetDashboard } = dashboardSlice.actions;
export default dashboardSlice.reducer;
