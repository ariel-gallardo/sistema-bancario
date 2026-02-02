import { type FormEvent } from 'react';
import {
  Alert,
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
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';
import VisibilityOffRoundedIcon from '@mui/icons-material/VisibilityOffRounded';
import SecurityRoundedIcon from '@mui/icons-material/SecurityRounded';
import LockRoundedIcon from '@mui/icons-material/LockRounded';

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
}: LoginViewProps) => (
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
              <Button variant="outlined" size="small" startIcon={<LockRoundedIcon />}>
                Teclado virtual
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
);

export default LoginView;
