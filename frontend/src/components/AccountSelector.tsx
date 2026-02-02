import { Box, Button, Chip, IconButton, Paper, Stack, Typography } from '@mui/material';
import StarRoundedIcon from '@mui/icons-material/StarRounded';
import StarBorderRoundedIcon from '@mui/icons-material/StarBorderRounded';
import type { AccountSnapshot } from '../types/api';

interface AccountSelectorProps {
  accounts: AccountSnapshot[];
  activeAccountId: string | null;
  favoriteAccountId: string | null;
  onSelectAccount: (accountId: string) => void;
  onToggleFavorite: (accountId: string) => void;
  formatCurrency: (value: number, currency: string) => string;
}

const AccountSelector = ({
  accounts,
  activeAccountId,
  favoriteAccountId,
  onSelectAccount,
  onToggleFavorite,
  formatCurrency,
}: AccountSelectorProps) => (
  <Box mt={4}>
    <Typography variant="subtitle2" gutterBottom>
      Elegí tu cuenta
    </Typography>
    <Stack spacing={1.5} className="account-selector">
      {accounts.map((cuenta) => {
        const isSelected = cuenta.cuentaId === activeAccountId;
        const isFavorite = cuenta.cuentaId === favoriteAccountId;

        return (
          <Paper
            key={cuenta.cuentaId}
            elevation={0}
            className={`account-item ${isSelected ? 'is-selected' : ''}`}
          >
            <Stack direction="row" spacing={2} alignItems="center">
              <Box flex={1}>
                <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap">
                  <Typography variant="subtitle1">{cuenta.alias}</Typography>
                  {cuenta.esPrincipal && <Chip label="Principal" size="small" />}
                  {isFavorite && <Chip label="Favorita" size="small" color="secondary" />}
                </Stack>
                <Typography variant="caption" color="text.secondary">
                  {formatCurrency(cuenta.saldoActual, cuenta.moneda)}
                </Typography>
              </Box>
              <Stack direction="row" spacing={1} alignItems="center">
                <IconButton
                  className="favorite-button"
                  color={isFavorite ? 'secondary' : 'default'}
                  onClick={() => onToggleFavorite(cuenta.cuentaId)}
                  aria-label="Marcar cuenta favorita"
                >
                  {isFavorite ? <StarRoundedIcon /> : <StarBorderRoundedIcon />}
                </IconButton>
                <Button
                  size="small"
                  variant={isSelected ? 'contained' : 'outlined'}
                  onClick={() => onSelectAccount(cuenta.cuentaId)}
                >
                  {isSelected ? 'Seleccionada' : 'Elegir'}
                </Button>
              </Stack>
            </Stack>
          </Paper>
        );
      })}
    </Stack>
  </Box>
);

export default AccountSelector;
