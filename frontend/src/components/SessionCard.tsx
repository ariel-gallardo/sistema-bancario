import { Avatar, Box, Chip, Paper, Stack, Typography } from '@mui/material';
import ShieldRoundedIcon from '@mui/icons-material/ShieldRounded';
import BoltRoundedIcon from '@mui/icons-material/BoltRounded';

interface SessionCardProps {
  profile: { nombre: string; email: string };
  accountAlias: string;
}

const SessionCard = ({ profile, accountAlias }: SessionCardProps) => (
  <Paper elevation={0} className="session-paper">
    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} alignItems="center">
      <Avatar sx={{ bgcolor: '#37b7c3', width: 56, height: 56 }}>{profile.nombre[0]}</Avatar>
      <Box flex={1}>
        <Typography variant="h6">{profile.nombre}</Typography>
        <Typography variant="body2" color="text.secondary">
          {profile.email}
        </Typography>
      </Box>
      <Stack direction="row" spacing={1}>
        <Chip icon={<ShieldRoundedIcon />} label="Sesión protegida" color="success" variant="outlined" />
        <Chip icon={<BoltRoundedIcon />} label={accountAlias} variant="outlined" />
      </Stack>
    </Stack>
  </Paper>
);

export default SessionCard;
