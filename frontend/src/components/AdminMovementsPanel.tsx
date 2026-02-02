import { type ChangeEvent, useMemo, useState } from 'react';
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
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { apiConfig } from '../config/api';
import { fetchDashboardData } from '../features/dashboard/dashboardSlice';
import { useAppDispatch, useAppSelector } from '../hooks';

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
  const profile = useAppSelector((state) => state.auth.profile);

  const [formState, setFormState] = useState<MovimientoFormState>(defaultFormState);
  const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
  const [error, setError] = useState<string | undefined>(undefined);

  const tarjetaId = profile?.tarjetaPrincipalId ?? null;

  const categories = useMemo(
    () =>
      ['Servicios', 'Retail', 'Tecnología', 'Gastronomía', 'Viajes', 'Supermercado', 'Salud', 'Educación', 'Otros'] as const,
    [],
  );

  const handleChange = (field: keyof MovimientoFormState) => (event: ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setFormState((prev) => ({ ...prev, [field]: value }));
  };

  const resetForm = () => {
    setFormState(defaultFormState());
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!token || !tarjetaId) {
      setError('No encontramos una tarjeta principal para registrar movimientos.');
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
        `${apiConfig.tarjetas}/api/tarjetas/${tarjetaId}/movimientos`,
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
    !tarjetaId ||
    status === 'loading';

  return (
    <Container maxWidth="md" className="admin-panel-container">
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

        <Paper elevation={3} sx={{ p: 4, borderRadius: 4 }}>
          <Stack spacing={3} component="form" onSubmit={handleSubmit}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Chip color="primary" label="Administración" icon={<AddCardRoundedIcon />} />
              {tarjetaId && <Chip label={`Tarjeta principal: ${tarjetaId.slice(0, 8)}...`} variant="outlined" />}
            </Stack>

            <Divider />

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
