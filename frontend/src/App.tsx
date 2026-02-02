import React, { type FormEvent, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Avatar,
  Box,
  Button,
  Chip,
  Container,
  Divider,
  Grid,
  LinearProgress,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import CreditScoreRoundedIcon from '@mui/icons-material/CreditScoreRounded';
import TrendingUpRoundedIcon from '@mui/icons-material/TrendingUpRounded';
import BoltRoundedIcon from '@mui/icons-material/BoltRounded';
import ShieldRoundedIcon from '@mui/icons-material/ShieldRounded';
import CreditCardRoundedIcon from '@mui/icons-material/CreditCardRounded';
import './App.css';
import { useAppDispatch, useAppSelector } from './hooks';
import { login } from './features/auth/authSlice';
import { fetchDashboardData } from './features/dashboard/dashboardSlice';
import { demoCredentials } from './config/api';
import { formatCurrency, formatDate } from './utils/formatters';

const App = () => {
  const dispatch = useAppDispatch();
  const auth = useAppSelector((state) => state.auth);
  const dashboard = useAppSelector((state) => state.dashboard);

  const [formState, setFormState] = useState({
    email: demoCredentials.email,
    password: demoCredentials.password,
  });

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    dispatch(login(formState));
  };

  useEffect(() => {
    if (auth.token) {
      dispatch(fetchDashboardData());
    }
  }, [auth.token, dispatch]);

  const isLoadingDashboard = dashboard.status === 'loading';
  const hasData = Boolean(dashboard.account && dashboard.card);

  const accountAlias = dashboard.account?.alias ?? 'Cuenta sueldo';

  const movementAccentColors = ['#37b7c3', '#ffb547', '#f368a5', '#62d2a2', '#b892ff'];

  const greeting = useMemo(() => {
    if (auth.profile?.nombre) {
      const firstName = auth.profile.nombre.split(' ')[0];
      return `Bienvenido, ${firstName}`;
    }
    return 'Home banking Aurora';
  }, [auth.profile?.nombre]);

  return (
    <Box className="app-shell">
      <div className="aurora-glow" aria-hidden />
      <Container maxWidth="lg">
        <Stack spacing={4}>
          <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
            <Box>
              <Typography variant="h3" gutterBottom>
                {greeting}
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Seguimiento en tiempo real de tu cuenta principal y los últimos consumos de tarjeta.
              </Typography>
            </Box>
            <Chip color="secondary" label="Negocio financiero" variant="filled" />
          </Stack>

          {!auth.token && (
            <Paper elevation={0} className="login-paper">
              <Stack
                component="form"
                spacing={2.5}
                direction={{ xs: 'column', sm: 'row' }}
                onSubmit={handleSubmit}
                alignItems={{ xs: 'stretch', sm: 'flex-end' }}
              >
                <TextField
                  required
                  label="Email corporativo"
                  type="email"
                  value={formState.email}
                  onChange={(event) => setFormState((prev) => ({ ...prev, email: event.target.value }))}
                  fullWidth
                />
                <TextField
                  required
                  label="Contraseña"
                  type="password"
                  value={formState.password}
                  onChange={(event) => setFormState((prev) => ({ ...prev, password: event.target.value }))}
                  fullWidth
                />
                <Button
                  size="large"
                  type="submit"
                  variant="contained"
                  disabled={auth.status === 'loading'}
                >
                  {auth.status === 'loading' ? 'Validando...' : 'Ingresar'}
                </Button>
              </Stack>
              <Stack direction="row" spacing={2} alignItems="center" mt={2}>
                <Chip label="demo@aurorabank.com" size="small" />
                <Chip label="B4nco$123" size="small" />
                <Typography variant="caption" color="text.secondary">
                  Usá las credenciales demo para explorar.
                </Typography>
              </Stack>
              {auth.error && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {auth.error}
                </Alert>
              )}
            </Paper>
          )}

          {auth.token && auth.profile && (
            <Paper elevation={0} className="session-paper">
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} alignItems="center">
                <Avatar sx={{ bgcolor: '#37b7c3', width: 56, height: 56 }}>
                  {auth.profile.nombre[0]}
                </Avatar>
                <Box flex={1}>
                  <Typography variant="h6">{auth.profile.nombre}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {auth.profile.email}
                  </Typography>
                </Box>
                <Stack direction="row" spacing={1}>
                  <Chip icon={<ShieldRoundedIcon />} label="Sesión protegida" color="success" variant="outlined" />
                  <Chip icon={<BoltRoundedIcon />} label={accountAlias} variant="outlined" />
                </Stack>
              </Stack>
            </Paper>
          )}

          <Grid container spacing={3}>
            <Grid item xs={12} md={7}>
              <Paper elevation={0} className="account-paper">
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Stack spacing={1}>
                    <Typography variant="h5">Saldo cuenta principal</Typography>
                    <Typography variant="body2" color="text.secondary">
                      Actualizado {dashboard.account ? formatDate(dashboard.account.ultimaActualizacion) : '...'}
                    </Typography>
                  </Stack>
                  <CreditScoreRoundedIcon fontSize="large" color="primary" />
                </Stack>
                <Box mt={3}>
                  {isLoadingDashboard ? (
                    <LinearProgress />
                  ) : hasData ? (
                    <>
                      <Typography variant="h2" className="saldo-principal">
                        {formatCurrency(dashboard.account!.saldoActual, dashboard.account!.moneda)}
                      </Typography>
                      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} mt={3}>
                        <MetricBadge
                          icon={<TrendingUpRoundedIcon />}
                          label="Saldo disponible"
                          value={formatCurrency(
                            dashboard.account!.saldoDisponible,
                            dashboard.account!.moneda,
                          )}
                        />
                        <MetricBadge
                          icon={<BoltRoundedIcon />}
                          label="Banco"
                          value={dashboard.account!.banco}
                        />
                      </Stack>
                      {dashboard.account!.otrasCuentas.length > 0 && (
                        <Box mt={4}>
                          <Typography variant="subtitle2" gutterBottom>
                            Otras cuentas
                          </Typography>
                          <Stack direction="row" spacing={1} flexWrap="wrap" gap={1}>
                            {dashboard.account!.otrasCuentas.map((cuenta) => (
                              <Chip
                                key={cuenta.cuentaId}
                                label={`${cuenta.alias} · ${formatCurrency(cuenta.saldoActual, cuenta.moneda)}`}
                                variant="outlined"
                              />
                            ))}
                          </Stack>
                        </Box>
                      )}
                    </>
                  ) : (
                    <Typography variant="body1" color="text.secondary">
                      Iniciá sesión para consultar tu saldo en tiempo real.
                    </Typography>
                  )}
                </Box>
              </Paper>
            </Grid>

            <Grid item xs={12} md={5}>
              <Paper elevation={0} className="movements-paper">
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="h5">Últimos movimientos</Typography>
                  <Tooltip title="Se muestran los últimos cinco consumos de la tarjeta principal">
                    <Chip label="Top 5" size="small" />
                  </Tooltip>
                </Stack>
                <Divider sx={{ my: 2.5, opacity: 0.2 }} />
                {isLoadingDashboard && <LinearProgress />}
                {!isLoadingDashboard && hasData ? (
                  <>
                    <Stack direction="row" spacing={2} mb={2}>
                      <Chip icon={<CreditCardRoundedIcon />} label={dashboard.card!.marca} variant="outlined" />
                      <Chip label={dashboard.card!.numeroEnmascarado} variant="outlined" />
                    </Stack>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Disponible {formatCurrency(dashboard.card!.disponible, 'ARS')} · Pago mínimo {formatCurrency(dashboard.card!.pagoMinimo, 'ARS')}
                    </Typography>
                    <List>
                      {dashboard.card!.movimientos.map((movimiento, index) => (
                        <ListItem key={movimiento.movimientoId} disableGutters>
                          <ListItemAvatar>
                            <Avatar sx={{ bgcolor: movementAccentColors[index % movementAccentColors.length] }}>
                              {movimiento.comercio[0]}
                            </Avatar>
                          </ListItemAvatar>
                          <ListItemText
                            primary={
                              <Stack direction="row" justifyContent="space-between" alignItems="center">
                                <Typography variant="subtitle1">{movimiento.comercio}</Typography>
                                <Typography variant="subtitle1">
                                  -{formatCurrency(movimiento.importe, 'ARS')}
                                </Typography>
                              </Stack>
                            }
                            secondary={
                              <Stack direction="row" justifyContent="space-between">
                                <Typography variant="caption" color="text.secondary">
                                  {movimiento.descripcion} · {movimiento.categoria}
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                  {formatDate(movimiento.fecha, 'DD MMM')}
                                </Typography>
                              </Stack>
                            }
                          />
                        </ListItem>
                      ))}
                    </List>
                  </>
                ) : (
                  !isLoadingDashboard && (
                    <Typography variant="body1" color="text.secondary">
                      Los movimientos se habilitan luego de iniciar sesión.
                    </Typography>
                  )
                )}
                {dashboard.error && (
                  <Alert severity="warning" sx={{ mt: 2 }}>
                    {dashboard.error}
                  </Alert>
                )}
              </Paper>
            </Grid>
          </Grid>
        </Stack>
      </Container>
    </Box>
  );
};

interface MetricBadgeProps {
  icon: React.ReactNode;
  label: string;
  value: string;
}

const MetricBadge = ({ icon, label, value }: MetricBadgeProps) => (
  <Paper elevation={0} className="metric-badge">
    <Stack direction="row" spacing={1.5} alignItems="center">
      <Avatar variant="rounded" className="metric-icon">
        {icon}
      </Avatar>
      <Box>
        <Typography variant="caption" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="subtitle1">{value}</Typography>
      </Box>
    </Stack>
  </Paper>
);

export default App;
