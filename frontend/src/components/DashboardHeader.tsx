import { Box, Chip, IconButton, Stack, Tooltip, Typography } from '@mui/material';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';

interface DashboardHeaderProps {
  greeting: string;
  onLogout: () => void;
}

const DashboardHeader = ({ greeting, onLogout }: DashboardHeaderProps) => (
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
        <IconButton onClick={onLogout} color="error">
          <LogoutRoundedIcon />
        </IconButton>
      </Tooltip>
    </Stack>
  </Stack>
);

export default DashboardHeader;
