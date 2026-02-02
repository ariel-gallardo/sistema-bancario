import React, { type FormEvent, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Avatar,
  Box,
  Button,
  Checkbox,
  Chip,
  Container,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  InputAdornment,
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
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';
import VisibilityOffRoundedIcon from '@mui/icons-material/VisibilityOffRounded';
import SecurityRoundedIcon from '@mui/icons-material/SecurityRounded';
import LockRoundedIcon from '@mui/icons-material/LockRounded';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import './App.css';
import { useAppDispatch, useAppSelector } from './hooks';
import { login, logout } from './features/auth/authSlice';
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
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    dispatch(login(formState));
  };

  const handleLogout = () => {
    dispatch(logout());
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
      
      {!auth.token ? (
        <Container maxWidth="xl" className="login-container">
          <Grid container className="login-grid">
            <Grid item xs={12} md={5} className="login-left-panel">
              <Box className="security-panel">
                <Box className="robot-icon">
                  <SecurityRoundedIcon sx={{ fontSize: 120, color: '#37b7c3' }} />
                </Box>
                <Typography variant="h4" className="security-title" gutterBottom>
                  TE AYUDAMOS
                </Typography>
                <Typography variant="h4" className="security-title">
                  A PREVENIR ESTAFAS
                </Typography>
                
                <Stack spacing={2} mt={4} className="security-tips">
                  <Box className="security-tip">
                    <Typography variant="body1">
                      • No estamos realizando actualizaciones ni te vamos a pedir instalar un programa en tu computadora.
                    </Typography>
                  </Box>
                  <Box className="security-tip">
                    <Typography variant="body1">
                      • Si ves un mensaje solicitando una acción urgente y tu pantalla se congela, sospechá.
                    </Typography>
                  </Box>
                  <Box className="security-tip">
                    <Typography variant="body1">
                      • Solo validá identidad cuando la operación lo requiera.
                    </Typography>
                  </Box>
                  <Box className="security-tip">
                    <Typography variant="body1">
                      • Evitá compartir tu usuario y clave a terceros.
                    </Typography>
                  </Box>
                </Stack>

                <Box mt={5} className="security-footer">
                  <Typography variant="body2" className="security-contact">
                    Si tenés alguna sospecha de fraude comunicate al <strong>11-6842-3330</strong> las 24 hs.
                  </Typography>
                </Box>
              </Box>
            </Grid>

            <Grid item xs={12} md={7} className="login-right-panel">
              <Paper elevation={0} className="login-form-paper">
                <Box className="logo-header">
                  <LockRoundedIcon sx={{ fontSize: 48, color: '#37b7c3', mb: 2 }} />
                  <Typography variant="h4" gutterBottom className="login-title">
                    Iniciar sesión en Online Banking Personas
                  </Typography>
                </Box>

                <Stack
                  component="form"
                  spacing={3}
                  onSubmit={handleSubmit}
                  className="login-form"
                >
                  <TextField
                    required
                    label="Tu número de documento"
                    type="email"
                    value={formState.email}
                    onChange={(event) => setFormState((prev) => ({ ...prev, email: event.target.value }))}
                    fullWidth
                    variant="standard"
                    className="login-input"
                  />

                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={rememberMe}
                        onChange={(e) => setRememberMe(e.target.checked)}
                        size="small"
                      />
                    }
                    label="Recordar mi número de documento"
                  />

                  <TextField
                    required
                    label="Tu nombre de usuario"
                    type="text"
                    value={formState.email}
                    onChange={(event) => setFormState((prev) => ({ ...prev, email: event.target.value }))}
                    fullWidth
                    variant="standard"
                    className="login-input"
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <VisibilityRoundedIcon color="action" />
                        </InputAdornment>
                      ),
                    }}
                  />

                  <TextField
                    required
                    label="Tu clave"
                    type={showPassword ? 'text' : 'password'}
                    value={formState.password}
                    onChange={(event) => setFormState((prev) => ({ ...prev, password: event.target.value }))}
                    fullWidth
                    variant="standard"
                    className="login-input"
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowPassword(!showPassword)}
                            edge="end"
                            size="small"
                          >
                            {showPassword ? <VisibilityOffRoundedIcon /> : <VisibilityRoundedIcon />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                  />

                  <Box className="keyboard-button">
                    <Button
                      variant="outlined"
                      size="small"
                      startIcon={<LockRoundedIcon />}
                    >
                      Teclado virtual
                    </Button>
                  </Box>

                  <Button
                    size="large"
                    type="submit"
                    variant="contained"
                    disabled={auth.status === 'loading'}
                    className="login-button"
                    fullWidth
                  >
                    {auth.status === 'loading' ? 'Validando...' : 'Ingresar'}
                  </Button>

                  {auth.error && (
                    <Alert severity="error">
                      {auth.error}
                    </Alert>
                  )}

                  <Divider />

                  <Box className="login-help">
                    <Button variant="text" color="error" size="small">
                      Cambiar usuario y/o clave
                    </Button>
                    <Button variant="outlined" color="error" size="small">
                      ¿No tenés usuario y clave? Registrate
                    </Button>
                  </Box>

                  <Box className="demo-credentials">
                    <Typography variant="caption" color="text.secondary" gutterBottom>
                      Cuenta Gratuita Universal. Com *A* 6876.
                    </Typography>
                    <Stack direction="row" spacing={1} mt={1} flexWrap="wrap">
                      <Chip label="demo@aurorabank.com" size="small" variant="outlined" />
                      <Chip label="B4nco$123" size="small" variant="outlined" />
                    </Stack>
                  </Box>
                </Stack>
              </Paper>
            </Grid>
          </Grid>
        </Container>
      ) : (
        <Container maxWidth="lg">
          <Stack spacing={4} className="dashboard-container">
            <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
              <Box>
                <Typography variant="h3" gutterBottom className="greeting-title">
                  {greeting}
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Seguimiento en tiempo real de tu cuenta principal y los últimos consumos de tarjeta.
                </Typography>
              </Box>
              <Stack direction="row" spacing={2} alignItems="center">
                <Chip color="secondary" label="Negocio financiero" variant="filled" />
                <Tooltip title="Cerrar sesión">
                  <IconButton onClick={handleLogout} color="error">
                    <LogoutRoundedIcon />
                  </IconButton>
                </Tooltip>
              </Stack>
            </Stack>

            {auth.profile && (
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
                    <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
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
                  <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                    Iniciá sesión para ver tus movimientos
                  </Typography>
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
      )}
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
