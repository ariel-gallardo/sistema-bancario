import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import axios from 'axios';
import { apiConfig } from '../../config/api';
import type { RootState } from '../../store';
import type { AccountSummaryResponse, TarjetaMovimientosResponse, TarjetaResumenResponse } from '../../types/api';

interface DashboardState {
  account?: AccountSummaryResponse;
  card?: TarjetaMovimientosResponse;
  cards: TarjetaResumenResponse[];
  selectedCardId: string | null;
  favoriteAccountId: string | null;
  cardStatus: 'idle' | 'loading';
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error?: string;
}

const initialState: DashboardState = {
  status: 'idle',
  cardStatus: 'idle',
  favoriteAccountId: null,
  cards: [],
  selectedCardId: null,
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

const normalizeId = (value?: string | null) => (value && value.length > 0 ? value : null);

const selectCardId = (
  cards: TarjetaResumenResponse[],
  preferredId?: string | null,
  fallbackId?: string | null,
) => {
  if (cards.length === 0) {
    return null;
  }

  const normalizedPreferred = normalizeId(preferredId);
  if (normalizedPreferred && cards.some((card) => card.tarjetaId === normalizedPreferred)) {
    return normalizedPreferred;
  }

  const normalizedFallback = normalizeId(fallbackId);
  if (normalizedFallback && cards.some((card) => card.tarjetaId === normalizedFallback)) {
    return normalizedFallback;
  }

  const principal = cards.find((card) => card.esPrincipal);
  if (principal) {
    return principal.tarjetaId;
  }

  return cards[0].tarjetaId;
};

const requestCardMovements = async (tarjetaId: string, headers: Record<string, string>) => {
  const response = await axios.get<TarjetaMovimientosResponse>(
    `${apiConfig.tarjetas}/api/tarjetas/${tarjetaId}/movimientos?take=5`,
    { headers },
  );
  return response.data;
};

type DashboardPayload = {
  account: AccountSummaryResponse;
  cards: TarjetaResumenResponse[];
  card?: TarjetaMovimientosResponse;
  selectedCardId: string | null;
};

export const fetchDashboardData = createAsyncThunk<
  DashboardPayload,
  void,
  { state: RootState; rejectValue: string }
>('dashboard/fetch', async (_, { getState, rejectWithValue }) => {
  const token = getState().auth.token;
  if (!token) {
    return rejectWithValue('Token no disponible');
  }

  try {
    const headers = buildAuthHeaders(token);
    const accountResponse = await axios.get<AccountSummaryResponse>(`${apiConfig.cuentas}/api/cuentas/principal`, {
      headers,
    });
    const account = accountResponse.data;

    const cardsResponse = await axios.get<TarjetaResumenResponse[]>(
      `${apiConfig.tarjetas}/api/tarjetas/cuentas/${account.cuentaId}`,
      { headers },
    );
    const cards = cardsResponse.data;

    const preferredCardId = getState().dashboard.selectedCardId;
    const fallbackCardId = getState().auth.profile?.tarjetaPrincipalId ?? null;
    const selectedCardId = selectCardId(cards, preferredCardId, fallbackCardId);
    const card = selectedCardId ? await requestCardMovements(selectedCardId, headers) : undefined;

    return { account, cards, card, selectedCardId };
  } catch (error) {
    return rejectWithValue(getDashboardErrorMessage(error));
  }
});

export const fetchAccountById = createAsyncThunk<
  DashboardPayload,
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
    const account = response.data;

    const cardsResponse = await axios.get<TarjetaResumenResponse[]>(
      `${apiConfig.tarjetas}/api/tarjetas/cuentas/${accountId}`,
      { headers },
    );
    const cards = cardsResponse.data;

    const preferredCardId = getState().dashboard.selectedCardId;
    const fallbackCardId = getState().auth.profile?.tarjetaPrincipalId ?? null;
    const selectedCardId = selectCardId(cards, preferredCardId, fallbackCardId);
    const card = selectedCardId ? await requestCardMovements(selectedCardId, headers) : undefined;

    return { account, cards, card, selectedCardId };
  } catch (error) {
    return rejectWithValue(getDashboardErrorMessage(error));
  }
});

export const fetchCardMovements = createAsyncThunk<
  { card: TarjetaMovimientosResponse; selectedCardId: string },
  string,
  { state: RootState; rejectValue: string }
>('dashboard/fetchCardMovements', async (tarjetaId, { getState, rejectWithValue }) => {
  const token = getState().auth.token;
  if (!token) {
    return rejectWithValue('Token no disponible');
  }

  try {
    const headers = buildAuthHeaders(token);
    const card = await requestCardMovements(tarjetaId, headers);
    return { card, selectedCardId: tarjetaId };
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
        state.cardStatus = 'loading';
        state.error = undefined;
        state.card = undefined;
        state.cards = [];
        state.selectedCardId = null;
      })
      .addCase(fetchDashboardData.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.account = action.payload.account;
        state.cards = action.payload.cards;
        state.card = action.payload.card;
        state.selectedCardId = action.payload.selectedCardId;
        state.favoriteAccountId = resolveFavoriteAccountId(action.payload.account);
        state.cardStatus = 'idle';
        state.error = undefined;
      })
      .addCase(fetchDashboardData.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.account = undefined;
        state.card = undefined;
        state.cards = [];
        state.selectedCardId = null;
        state.favoriteAccountId = null;
        state.cardStatus = 'idle';
      })
      .addCase(fetchAccountById.pending, (state) => {
        state.status = 'loading';
        state.cardStatus = 'loading';
        state.error = undefined;
      })
      .addCase(fetchAccountById.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.account = action.payload.account;
        state.cards = action.payload.cards;
        state.card = action.payload.card;
        state.selectedCardId = action.payload.selectedCardId;
        state.favoriteAccountId = resolveFavoriteAccountId(action.payload.account);
        state.cardStatus = 'idle';
      })
      .addCase(fetchAccountById.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.cardStatus = 'idle';
      })
      .addCase(fetchCardMovements.pending, (state) => {
        state.cardStatus = 'loading';
        state.error = undefined;
      })
      .addCase(fetchCardMovements.fulfilled, (state, action) => {
        state.cardStatus = 'idle';
        state.card = action.payload.card;
        state.selectedCardId = action.payload.selectedCardId;
      })
      .addCase(fetchCardMovements.rejected, (state, action) => {
        state.cardStatus = 'idle';
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
