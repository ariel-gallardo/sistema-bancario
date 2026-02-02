import { type ChangeEvent, type FormEvent, useCallback, useMemo, useState } from 'react';
import axios from 'axios';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Chip,
  ClickAwayListener,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  Grid,
  Grow,
  IconButton,
  InputAdornment,
  Paper,
  Popper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';
import VisibilityOffRoundedIcon from '@mui/icons-material/VisibilityOffRounded';
import SecurityRoundedIcon from '@mui/icons-material/SecurityRounded';
import LockRoundedIcon from '@mui/icons-material/LockRounded';
import KeyboardAltRoundedIcon from '@mui/icons-material/KeyboardAltRounded';
import BackspaceRoundedIcon from '@mui/icons-material/BackspaceRounded';
import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import { apiConfig, demoUsers, type DemoUserCredential } from '../config/api';

interface LoginViewProps {
  formState: { email: string; password: string };
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  showPassword: boolean;
  onToggleShowPassword: () => void;
  rememberMe: boolean;
  onRememberMeChange: (value: boolean) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  isLoading: boolean;
  error?: string;
}

interface RegisterFormState {
  nombreCompleto: string;
  documento: string;
  email: string;
  telefono: string;
  clave: string;
  confirmaClave: string;
}

interface RecoveryFormState {
  documento: string;
  email: string;
  nuevaClave: string;
  confirmaClave: string;
}

interface RegisterResponseApi {
  registroId: string;
  estado: string;
  mensaje: string;
}

interface PasswordRecoveryResponseApi {
  mensaje: string;
}

const extractErrorMessage = (error: unknown, fallback: string) => {
  if (axios.isAxiosError(error)) {
    if (typeof error.response?.data?.message === 'string') {
      return error.response.data.message;
    }
    if (typeof error.message === 'string' && error.message.length) {
      return error.message;
    }
  } else if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
};

const createRegisterFormState = (): RegisterFormState => ({
  nombreCompleto: '',
  documento: '',
  email: '',
  telefono: '',
  clave: '',
  confirmaClave: '',
});

const createRecoveryFormState = (): RecoveryFormState => ({
  documento: '',
  email: '',
  nuevaClave: '',
  confirmaClave: '',
});

const LoginView = ({
  formState,
  onEmailChange,
  onPasswordChange,
  showPassword,
  onToggleShowPassword,
  rememberMe,
  onRememberMeChange,
  onSubmit,
  isLoading,
  error,
}: LoginViewProps) => {
  const [keyboardAnchorEl, setKeyboardAnchorEl] = useState<HTMLElement | null>(null);
  const isKeyboardOpen = Boolean(keyboardAnchorEl);
  const [showDemoSuggestions, setShowDemoSuggestions] = useState(true);

  const qwertyRows = useMemo(
    () => [
      { keys: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'], offset: 0 },
      { keys: ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'], offset: 12 },
      { keys: ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'ñ'], offset: 24 },
      { keys: ['z', 'x', 'c', 'v', 'b', 'n', 'm'], offset: 48 },
    ],
    [],
  );

  const symbolKeys = useMemo(() => ['@', '.', '-', '_', '#', '¡', '!', '$'], []);

  const adminUsers = useMemo(() => demoUsers.filter((user) => user.role === 'admin'), []);
  const clientUsers = useMemo(() => demoUsers.filter((user) => user.role === 'cliente'), []);

  const renderCredential = useCallback(
    (user: DemoUserCredential) => (
      <Stack
        key={user.email}
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1}
        alignItems={{ xs: 'flex-start', sm: 'center' }}
        flexWrap="wrap"
      >
        <Chip
          label={`${user.name} · ${user.description}`}
          size="small"
          color={user.role === 'admin' ? 'primary' : 'info'}
        />
        <Chip label={user.email} size="small" variant="outlined" />
        <Chip label={user.password} size="small" color="success" variant="outlined" />
      </Stack>
    ),
    [],
  );

  const [registerDialogOpen, setRegisterDialogOpen] = useState(false);
  const [registerForm, setRegisterForm] = useState<RegisterFormState>(() => createRegisterFormState());
  const [registerStatus, setRegisterStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
  const [registerError, setRegisterError] = useState<string>();
  const [registerMessage, setRegisterMessage] = useState<string>();

  const [recoveryDialogOpen, setRecoveryDialogOpen] = useState(false);
  const [recoveryForm, setRecoveryForm] = useState<RecoveryFormState>(() => createRecoveryFormState());
  const [recoveryStatus, setRecoveryStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
  const [recoveryError, setRecoveryError] = useState<string>();
  const [recoveryMessage, setRecoveryMessage] = useState<string>();

  const handleVirtualKeyPress = useCallback(
    (value: string) => {
      if (value === 'BACKSPACE') {
        onPasswordChange(formState.password.slice(0, -1));
        return;
      }

      if (value === 'CLEAR') {
        onPasswordChange('');
        return;
      }

      if (value === 'SPACE') {
        onPasswordChange(`${formState.password} `);
        return;
      }

      onPasswordChange(formState.password + value);
    },
    [formState.password, onPasswordChange],
  );

  const handleKeyboardToggle = (event: React.MouseEvent<HTMLButtonElement>) => {
    setKeyboardAnchorEl((prev) => (prev && prev === event.currentTarget ? null : event.currentTarget));
  };

  const closeKeyboard = () => {
    setKeyboardAnchorEl(null);
  };

  const handleRegisterFieldChange = (field: keyof RegisterFormState) => (
    event: ChangeEvent<HTMLInputElement>,
  ) => {
    const value = event.target.value;
    setRegisterForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleRecoveryFieldChange = (field: keyof RecoveryFormState) => (
    event: ChangeEvent<HTMLInputElement>,
  ) => {
    const value = event.target.value;
    setRecoveryForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleRegisterSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setRegisterStatus('loading');
    setRegisterError(undefined);
    setRegisterMessage(undefined);

    try {
      const payload = {
        nombreCompleto: registerForm.nombreCompleto.trim(),
        documento: registerForm.documento.trim(),
        email: registerForm.email.trim(),
        telefono: registerForm.telefono.trim(),
        clave: registerForm.clave,
      };

      const response = await axios.post<RegisterResponseApi>(`${apiConfig.clientes}/api/auth/register`, payload);
      setRegisterStatus('success');
      setRegisterMessage(response.data?.mensaje ?? 'Recibimos tu solicitud. Te avisaremos cuando esté lista.');
    } catch (submissionError) {
      setRegisterStatus('error');
      setRegisterError(extractErrorMessage(submissionError, 'No pudimos registrar tu solicitud.'));
    }
  };

  const handleRecoverySubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setRecoveryStatus('loading');
    setRecoveryError(undefined);
    setRecoveryMessage(undefined);

    try {
      const payload = {
        documento: recoveryForm.documento.trim(),
        email: recoveryForm.email.trim(),
        nuevaClave: recoveryForm.nuevaClave,
      };

      const response = await axios.post<PasswordRecoveryResponseApi>(`${apiConfig.clientes}/api/auth/password`, payload);
      setRecoveryStatus('success');
      setRecoveryMessage(response.data?.mensaje ?? 'Actualizamos tu clave. Podés volver a iniciar sesión.');
    } catch (submissionError) {
      setRecoveryStatus('error');
      setRecoveryError(extractErrorMessage(submissionError, 'No pudimos actualizar tu clave.'));
    }
  };

  const openRegisterDialog = () => {
    setRegisterForm(createRegisterFormState());
    setRegisterStatus('idle');
    setRegisterError(undefined);
    setRegisterMessage(undefined);
    setRegisterDialogOpen(true);
  };

  const closeRegisterDialog = () => {
    setRegisterDialogOpen(false);
  };

  const openRecoveryDialog = () => {
    setRecoveryForm(createRecoveryFormState());
    setRecoveryStatus('idle');
    setRecoveryError(undefined);
    setRecoveryMessage(undefined);
    setRecoveryDialogOpen(true);
  };

  const closeRecoveryDialog = () => {
    setRecoveryDialogOpen(false);
  };

  const isRegisterValid = useMemo(() => {
    const { nombreCompleto, documento, email, telefono, clave, confirmaClave } = registerForm;
    return (
      nombreCompleto.trim().length > 4 &&
      documento.trim().length >= 7 &&
      email.trim().includes('@') &&
      telefono.trim().length >= 6 &&
      clave.length >= 6 &&
      clave === confirmaClave
    );
  }, [registerForm]);

  const isRecoveryValid = useMemo(() => {
    const { documento, email, nuevaClave, confirmaClave } = recoveryForm;
    return (
      documento.trim().length >= 7 &&
      email.trim().includes('@') &&
      nuevaClave.length >= 6 &&
      nuevaClave === confirmaClave
    );
  }, [recoveryForm]);

  const toggleDemoSuggestions = () => setShowDemoSuggestions((prev) => !prev);

  return (
    <Container maxWidth="xl" className="login-container">
      {showDemoSuggestions ? (
        <Box className="admin-demo-banner">
          <Stack spacing={1.5}>
            <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={1.5}>
              <Stack spacing={0.5}>
                <Typography variant="caption" color="text.secondary">
                  Usuarios demo disponibles
                </Typography>
                <Typography variant="body2" fontWeight={600}>
                  Elegí un perfil administrador para acceder al panel o probá un cliente minorista.
                </Typography>
              </Stack>
              <IconButton size="small" onClick={toggleDemoSuggestions} className="admin-demo-close" aria-label="Ocultar sugerencias">
                <CloseRoundedIcon fontSize="small" />
              </IconButton>
            </Stack>

            <Stack spacing={1}>
              <Typography variant="overline" color="primary">
                Administradores
              </Typography>
              {adminUsers.map(renderCredential)}
            </Stack>

            <Divider flexItem />

            <Stack spacing={1}>
              <Typography variant="overline" color="text.secondary">
                Clientes demo
              </Typography>
              {clientUsers.map(renderCredential)}
            </Stack>

            <Typography variant="caption" color="text.secondary">
              Después de iniciar sesión como admin vas a ver el botón "Panel admin" arriba a la derecha.
              También podés ir directo a /admin o /admin/movimientos.
            </Typography>
          </Stack>
        </Box>
      ) : (
        <Button
          variant="contained"
          size="small"
          className="admin-demo-reopen"
          onClick={toggleDemoSuggestions}
        >
          Mostrar sugerencias demo
        </Button>
      )}

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
              <Typography variant="body1">• Solo validá identidad cuando la operación lo requiera.</Typography>
            </Box>
            <Box className="security-tip">
              <Typography variant="body1">• Evitá compartir tu usuario y clave a terceros.</Typography>
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

          <Stack component="form" spacing={3} onSubmit={onSubmit} className="login-form">
            <TextField
              required
              label="Tu número de documento"
              type="email"
              value={formState.email}
              onChange={(event) => onEmailChange(event.target.value)}
              fullWidth
              variant="standard"
              className="login-input"
            />

            <FormControlLabel
              control={
                <Checkbox
                  checked={rememberMe}
                  onChange={(e) => onRememberMeChange(e.target.checked)}
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
              onChange={(event) => onEmailChange(event.target.value)}
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
              onChange={(event) => onPasswordChange(event.target.value)}
              fullWidth
              variant="standard"
              className="login-input"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={onToggleShowPassword} edge="end" size="small">
                      {showPassword ? <VisibilityOffRoundedIcon /> : <VisibilityRoundedIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <Box className="keyboard-button">
              <Button
                variant={isKeyboardOpen ? 'contained' : 'outlined'}
                color={isKeyboardOpen ? 'primary' : 'inherit'}
                size="small"
                startIcon={<KeyboardAltRoundedIcon />}
                onClick={handleKeyboardToggle}
                type="button"
              >
                {isKeyboardOpen ? 'Ocultar teclado' : 'Teclado virtual'}
              </Button>
            </Box>

            <Button
              size="large"
              type="submit"
              variant="contained"
              disabled={isLoading}
              className="login-button"
              fullWidth
            >
              {isLoading ? 'Validando...' : 'Ingresar'}
            </Button>

            {error && <Alert severity="error">{error}</Alert>}

            <Divider />

            <Box className="login-help">
              <Button variant="text" color="error" size="small" onClick={openRecoveryDialog}>
                Cambiar usuario y/o clave
              </Button>
              <Button variant="outlined" color="error" size="small" onClick={openRegisterDialog}>
                ¿No tenés usuario y clave? Registrate
              </Button>
            </Box>

            <Box className="demo-credentials">
              <Typography variant="caption" color="text.secondary" gutterBottom>
                Usuario demo administrador habilitado para el panel.
              </Typography>
              <Stack direction="row" spacing={1} mt={1} flexWrap="wrap">
                <Chip label="demo@aurorabank.com" size="small" variant="outlined" color="primary" />
                <Chip label="B4nco$123" size="small" variant="outlined" color="success" />
                <Chip label="Rol: Admin" size="small" variant="filled" color="secondary" />
              </Stack>
            </Box>
          </Stack>
        </Paper>
      </Grid>
    </Grid>
      <Popper
        open={isKeyboardOpen}
        anchorEl={keyboardAnchorEl}
        placement="top-end"
        transition
        className="virtual-keyboard-popper"
      >
        {({ TransitionProps }) => (
          <Grow {...TransitionProps} style={{ transformOrigin: 'right bottom' }}>
            <ClickAwayListener onClickAway={closeKeyboard}>
              <Paper elevation={8} className="virtual-keyboard-panel">
                <Stack spacing={2}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="subtitle2" color="text.secondary">
                      Teclado seguro (QWERTY)
                    </Typography>
                    <IconButton size="small" onClick={closeKeyboard} aria-label="Cerrar teclado virtual">
                      <CloseRoundedIcon fontSize="small" />
                    </IconButton>
                  </Stack>

                  {qwertyRows.map((row) => (
                    <Stack
                      key={row.keys.join('-')}
                      direction="row"
                      spacing={1}
                      justifyContent="center"
                      className="virtual-keyboard-row"
                      sx={{ pl: `${row.offset}px` }}
                    >
                      {row.keys.map((keyValue) => (
                        <Button
                          key={keyValue}
                          variant="contained"
                          color="inherit"
                          size="small"
                          onClick={() => handleVirtualKeyPress(keyValue)}
                          type="button"
                          className="virtual-keyboard-key"
                        >
                          {keyValue.toUpperCase()}
                        </Button>
                      ))}
                    </Stack>
                  ))}

                  <Stack direction="row" spacing={1} justifyContent="center" flexWrap="wrap">
                    {symbolKeys.map((symbol) => (
                      <Button
                        key={symbol}
                        variant="outlined"
                        color="primary"
                        size="small"
                        onClick={() => handleVirtualKeyPress(symbol)}
                        type="button"
                        className="virtual-keyboard-key"
                      >
                        {symbol}
                      </Button>
                    ))}
                  </Stack>

                  <Stack direction="row" spacing={1} justifyContent="space-between" alignItems="center">
                    <Button
                      variant="text"
                      color="inherit"
                      size="small"
                      onClick={() => handleVirtualKeyPress('CLEAR')}
                      type="button"
                    >
                      Limpiar
                    </Button>
                    <Button
                      variant="contained"
                      color="primary"
                      size="small"
                      onClick={() => handleVirtualKeyPress('SPACE')}
                      type="button"
                      className="virtual-keyboard-space"
                    >
                      Espacio
                    </Button>
                    <Button
                      variant="outlined"
                      color="primary"
                      size="small"
                      onClick={() => handleVirtualKeyPress('BACKSPACE')}
                      type="button"
                    >
                      <BackspaceRoundedIcon fontSize="small" />
                    </Button>
                  </Stack>
                </Stack>
              </Paper>
            </ClickAwayListener>
          </Grow>
        )}
      </Popper>

      <Dialog open={registerDialogOpen} onClose={closeRegisterDialog} maxWidth="xs" fullWidth>
        <Box component="form" onSubmit={handleRegisterSubmit} noValidate>
          <DialogTitle>Solicitar registro</DialogTitle>
          <DialogContent dividers>
            <Stack spacing={2}>
              <Typography variant="body2" color="text.secondary">
                Completá tus datos para recibir el alta del canal. Te enviaremos un correo con los próximos pasos.
              </Typography>
              <TextField
                label="Nombre completo"
                value={registerForm.nombreCompleto}
                onChange={handleRegisterFieldChange('nombreCompleto')}
                required
                fullWidth
              />
              <TextField
                label="Número de documento"
                value={registerForm.documento}
                onChange={handleRegisterFieldChange('documento')}
                required
                fullWidth
              />
              <TextField
                type="email"
                label="Correo de contacto"
                value={registerForm.email}
                onChange={handleRegisterFieldChange('email')}
                required
                fullWidth
              />
              <TextField
                label="Teléfono"
                value={registerForm.telefono}
                onChange={handleRegisterFieldChange('telefono')}
                required
                fullWidth
              />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  label="Clave deseada"
                  type="password"
                  value={registerForm.clave}
                  onChange={handleRegisterFieldChange('clave')}
                  required
                  fullWidth
                />
                <TextField
                  label="Confirmar clave"
                  type="password"
                  value={registerForm.confirmaClave}
                  onChange={handleRegisterFieldChange('confirmaClave')}
                  required
                  fullWidth
                />
              </Stack>
              {registerStatus === 'success' && (
                <Alert severity="success">{registerMessage ?? 'Recibimos tu solicitud. Te avisaremos cuando el alta esté lista.'}</Alert>
              )}
              {registerStatus === 'error' && registerError && <Alert severity="error">{registerError}</Alert>}
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={closeRegisterDialog}>Cerrar</Button>
            <Button
              type="submit"
              disabled={!isRegisterValid || registerStatus === 'loading'}
              variant="contained"
            >
              {registerStatus === 'loading' ? 'Enviando...' : 'Solicitar alta'}
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <Dialog open={recoveryDialogOpen} onClose={closeRecoveryDialog} maxWidth="xs" fullWidth>
        <Box component="form" onSubmit={handleRecoverySubmit} noValidate>
          <DialogTitle>Recuperar usuario / clave</DialogTitle>
          <DialogContent dividers>
            <Stack spacing={2}>
              <Typography variant="body2" color="text.secondary">
                Confirmá tus datos y te enviaremos instrucciones para actualizar el acceso.
              </Typography>
              <TextField
                label="Número de documento"
                value={recoveryForm.documento}
                onChange={handleRecoveryFieldChange('documento')}
                required
                fullWidth
              />
              <TextField
                type="email"
                label="Correo asociado"
                value={recoveryForm.email}
                onChange={handleRecoveryFieldChange('email')}
                required
                fullWidth
              />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  label="Nueva clave"
                  type="password"
                  value={recoveryForm.nuevaClave}
                  onChange={handleRecoveryFieldChange('nuevaClave')}
                  required
                  fullWidth
                />
                <TextField
                  label="Confirmar nueva clave"
                  type="password"
                  value={recoveryForm.confirmaClave}
                  onChange={handleRecoveryFieldChange('confirmaClave')}
                  required
                  fullWidth
                />
              </Stack>
              {recoveryStatus === 'success' && (
                <Alert severity="success">
                  {recoveryMessage ?? 'Enviamos un correo con los pasos finales para restablecer tus credenciales.'}
                </Alert>
              )}
              {recoveryStatus === 'error' && recoveryError && <Alert severity="error">{recoveryError}</Alert>}
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={closeRecoveryDialog}>Cerrar</Button>
            <Button
              type="submit"
              disabled={!isRecoveryValid || recoveryStatus === 'loading'}
              variant="contained"
            >
              {recoveryStatus === 'loading' ? 'Validando...' : 'Actualizar clave'}
            </Button>
          </DialogActions>
        </Box>
      </Dialog>
  </Container>
  );
};

export default LoginView;
