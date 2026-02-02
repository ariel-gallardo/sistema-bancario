import { Box, Button, Chip, IconButton, Stack, Tooltip, Typography } from '@mui/material';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded';

interface DashboardHeaderProps {
  greeting: string;
  onLogout: () => void;
  isAdmin?: boolean;
  onNavigateAdmin?: () => void;
}

const DashboardHeader = ({ greeting, onLogout, isAdmin = false, onNavigateAdmin }: DashboardHeaderProps) => (
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
      {isAdmin && onNavigateAdmin && (
        <Button
          variant="outlined"
          color="primary"
          startIcon={<AdminPanelSettingsRoundedIcon />}
          onClick={onNavigateAdmin}
        >
          Panel admin
        </Button>
      )}
      <Chip color="secondary" label="Negocio financiero" variant="filled" />
      <Tooltip title="Cerrar sesión">
        <IconButton onClick={onLogout} color="error">
          <LogoutRoundedIcon />
        </IconButton>
      </Tooltip>
    </Stack>
  </Stack>
);

export default DashboardHeader;
