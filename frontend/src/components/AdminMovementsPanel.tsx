import { type ChangeEvent, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  Container,
  Divider,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import ArrowBackRoundedIcon from '@mui/icons-material/ArrowBackRounded';
import AddCardRoundedIcon from '@mui/icons-material/AddCardRounded';
import CreditCardRoundedIcon from '@mui/icons-material/CreditCardRounded';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { apiConfig } from '../config/api';
import { fetchAccountById, fetchCardMovements, fetchDashboardData } from '../features/dashboard/dashboardSlice';
import { useAppDispatch, useAppSelector } from '../hooks';
import type { AccountSnapshot } from '../types/api';
import { resolveCardBrand } from '../utils/cardBrand';

interface MovimientoFormState {
  comercio: string;
  descripcion: string;
  categoria: string;
  importe: string;
  fecha: string;
}

const defaultFormState = (): MovimientoFormState => ({
  comercio: '',
  descripcion: '',
  categoria: '',
  importe: '',
  fecha: new Date().toISOString().split('T')[0],
});

const AdminMovementsPanel = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const token = useAppSelector((state) => state.auth.token);
  const dashboard = useAppSelector((state) => state.dashboard);

  const [formState, setFormState] = useState<MovimientoFormState>(defaultFormState);
  const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
  const [error, setError] = useState<string | undefined>(undefined);
  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(dashboard.account?.cuentaId ?? null);
  const [selectedTarjetaId, setSelectedTarjetaId] = useState<string | null>(dashboard.selectedCardId ?? null);

  const categories = useMemo(
    () =>
      ['Servicios', 'Retail', 'Tecnología', 'Gastronomía', 'Viajes', 'Supermercado', 'Salud', 'Educación', 'Otros'] as const,
    [],
  );

  useEffect(() => {
    if (token && !dashboard.account && dashboard.status === 'idle') {
      dispatch(fetchDashboardData());
    }
  }, [dashboard.account, dashboard.status, dispatch, token]);

  useEffect(() => {
    if (dashboard.account) {
      setSelectedAccountId(dashboard.account.cuentaId);
    }
  }, [dashboard.account?.cuentaId]);

  useEffect(() => {
    if (!dashboard.cards.length) {
      setSelectedTarjetaId(null);
      return;
    }
    setSelectedTarjetaId(dashboard.selectedCardId ?? dashboard.cards[0].tarjetaId);
  }, [dashboard.cards, dashboard.selectedCardId]);

  const availableAccounts = useMemo<AccountSnapshot[]>(() => {
    if (!dashboard.account) {
      return [];
    }

    const principal: AccountSnapshot = {
      cuentaId: dashboard.account.cuentaId,
      alias: dashboard.account.alias,
      moneda: dashboard.account.moneda,
      saldoActual: dashboard.account.saldoActual,
      esPrincipal: dashboard.account.esPrincipal,
      esFavorita: dashboard.account.esFavorita,
    };

    return [principal, ...dashboard.account.otrasCuentas];
  }, [dashboard.account]);

  const selectedCardSummary = useMemo(
    () => dashboard.cards.find((cardOption) => cardOption.tarjetaId === selectedTarjetaId),
    [dashboard.cards, selectedTarjetaId],
  );
  const selectedCardBrand = useMemo(
    () => resolveCardBrand(selectedCardSummary?.marca),
    [selectedCardSummary?.marca],
  );
  const selectedAccountAlias = useMemo(
    () => availableAccounts.find((account) => account.cuentaId === selectedAccountId)?.alias,
    [availableAccounts, selectedAccountId],
  );

  const handleAccountChange = (accountId: string) => {
    if (!accountId || accountId === selectedAccountId) {
      return;
    }
    setSelectedAccountId(accountId);
    void dispatch(fetchAccountById(accountId));
  };

  const handleCardChange = (cardId: string) => {
    setSelectedTarjetaId(cardId);
    if (cardId) {
      void dispatch(fetchCardMovements(cardId));
    }
  };

  const handleChange = (field: keyof MovimientoFormState) => (event: ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setFormState((prev) => ({ ...prev, [field]: value }));
  };

  const resetForm = () => {
    setFormState(defaultFormState());
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!token || !selectedTarjetaId) {
      setError('Seleccioná una tarjeta antes de registrar movimientos.');
      setStatus('error');
      return;
    }

    setStatus('loading');
    setError(undefined);

    try {
      const payload = {
        comercio: formState.comercio.trim(),
        descripcion: formState.descripcion.trim(),
        categoria: formState.categoria,
        importe: Number(formState.importe),
        fecha: formState.fecha ? new Date(formState.fecha).toISOString() : undefined,
      };

      await axios.post(
        `${apiConfig.tarjetas}/api/tarjetas/${selectedTarjetaId}/movimientos`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      setStatus('success');
      resetForm();
      void dispatch(fetchDashboardData());
    } catch (requestError) {
      setStatus('error');
      if (axios.isAxiosError(requestError)) {
        if (requestError.response?.status === 403) {
          setError('Necesitás permisos de administrador para ejecutar esta acción.');
        } else {
          setError(requestError.response?.data?.message ?? 'No pudimos registrar el movimiento.');
        }
      } else {
        setError('No pudimos registrar el movimiento.');
      }
    }
  };

  const isSubmitDisabled =
    !formState.comercio.trim() ||
    !formState.descripcion.trim() ||
    !formState.categoria ||
    !formState.importe ||
    Number.isNaN(Number(formState.importe)) ||
    !selectedTarjetaId ||
    status === 'loading';

  return (
    <Container maxWidth="lg" className="admin-panel-container">
      <Stack spacing={3} py={6}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography variant="h4" gutterBottom>
              Panel administrativo de movimientos
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Registrá cargos manuales para tarjetas seleccionadas sin exponer el teclado físico.
            </Typography>
          </Box>
          <Button
            variant="text"
            color="inherit"
            startIcon={<ArrowBackRoundedIcon />}
            onClick={() => navigate('/admin')}
          >
            Volver al dashboard
          </Button>
        </Stack>

        <Paper elevation={3} sx={{ p: { xs: 3, md: 5 }, borderRadius: 4 }}>
          <Stack spacing={3} component="form" onSubmit={handleSubmit}>
            <Stack spacing={2}>
              <Chip color="primary" label="Administración" icon={<AddCardRoundedIcon />} sx={{ alignSelf: 'flex-start' }} />
              {selectedCardSummary && (
                <Stack spacing={1.5}>
                  <Box className="card-brand-pill" sx={{ background: selectedCardBrand.background }}>
                    <img src={selectedCardBrand.logo} alt={`${selectedCardBrand.displayName} logo`} />
                    <Stack spacing={0}>
                      <Typography variant="subtitle2" color="text.secondary">
                        {selectedCardBrand.displayName}
                      </Typography>
                      <Typography variant="body1">
                        {selectedCardSummary.marca} · {selectedCardSummary.numeroEnmascarado}
                      </Typography>
                    </Stack>
                  </Box>
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} flexWrap="wrap">
                    <Chip
                      icon={<CreditCardRoundedIcon />}
                      label={selectedCardSummary.numeroEnmascarado}
                      variant="outlined"
                      size="medium"
                    />
                    {selectedAccountAlias && (
                      <Chip label={`Cuenta: ${selectedAccountAlias}`} variant="outlined" size="medium" />
                    )}
                  </Stack>
                </Stack>
              )}
            </Stack>

            <Divider />

            {availableAccounts.length > 0 && (
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  select
                  label="Cuenta"
                  value={selectedAccountId ?? ''}
                  onChange={(event) => handleAccountChange(event.target.value as string)}
                  fullWidth
                  helperText="Cuenta asociada a la tarjeta"
                >
                  {availableAccounts.map((account) => (
                    <MenuItem key={account.cuentaId} value={account.cuentaId}>
                      {account.alias} · {account.moneda}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  label="Tarjeta"
                  value={selectedTarjetaId ?? ''}
                  onChange={(event) => handleCardChange(event.target.value as string)}
                  fullWidth
                  disabled={!dashboard.cards.length}
                  helperText={dashboard.cards.length ? 'Seleccioná el plástico a debitar' : 'Sin tarjetas disponibles'}
                >
                  {dashboard.cards.map((cardOption) => (
                    <MenuItem key={cardOption.tarjetaId} value={cardOption.tarjetaId}>
                      {cardOption.marca} · {cardOption.numeroEnmascarado}
                    </MenuItem>
                  ))}
                </TextField>
              </Stack>
            )}

            <TextField
              label="Comercio"
              value={formState.comercio}
              onChange={handleChange('comercio')}
              required
              fullWidth
              helperText="Nombre del comercio o entidad que genera el cargo"
            />

            <TextField
              label="Descripción"
              value={formState.descripcion}
              onChange={handleChange('descripcion')}
              required
              fullWidth
              multiline
              rows={2}
              helperText="Breve explicación del movimiento"
            />

            <TextField
              select
              label="Categoría"
              value={formState.categoria}
              onChange={handleChange('categoria')}
              required
              fullWidth
            >
              {categories.map((category) => (
                <MenuItem key={category} value={category}>
                  {category}
                </MenuItem>
              ))}
            </TextField>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label="Importe"
                type="number"
                value={formState.importe}
                onChange={handleChange('importe')}
                required
                fullWidth
                inputProps={{ min: 1, step: '0.01' }}
              />
              <TextField
                label="Fecha del movimiento"
                type="date"
                value={formState.fecha}
                onChange={handleChange('fecha')}
                required
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
            </Stack>

            {status === 'success' && <Alert severity="success">Movimiento registrado con éxito.</Alert>}
            {status === 'error' && error && <Alert severity="error">{error}</Alert>}

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={resetForm} disabled={status === 'loading'}>
                Limpiar
              </Button>
              <Button type="submit" variant="contained" disabled={isSubmitDisabled}>
                {status === 'loading' ? 'Registrando...' : 'Registrar movimiento'}
              </Button>
            </Stack>
          </Stack>
        </Paper>
      </Stack>
    </Container>
  );
};

export default AdminMovementsPanel;
