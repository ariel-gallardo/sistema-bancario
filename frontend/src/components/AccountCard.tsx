import { Box, LinearProgress, Paper, Stack, Typography } from '@mui/material';
import CreditScoreRoundedIcon from '@mui/icons-material/CreditScoreRounded';
import TrendingUpRoundedIcon from '@mui/icons-material/TrendingUpRounded';
import BoltRoundedIcon from '@mui/icons-material/BoltRounded';
import type { AccountSnapshot, AccountSummaryResponse } from '../types/api';
import MetricBadge from './MetricBadge';
import AccountSelector from './AccountSelector';

interface AccountCardProps {
  isLoading: boolean;
  hasData: boolean;
  accountSubtitle: string;
  selectedAccount?: AccountSnapshot;
  account?: AccountSummaryResponse;
  availableAccounts: AccountSnapshot[];
  activeAccountId: string | null;
  favoriteAccountId: string | null;
  onSelectAccount: (accountId: string) => void;
  onToggleFavorite: (accountId: string) => void;
  formatCurrency: (value: number, currency: string) => string;
  formatDate: (value: string, format?: string) => string;
}

const AccountCard = ({
  isLoading,
  hasData,
  accountSubtitle,
  selectedAccount,
  account,
  availableAccounts,
  activeAccountId,
  favoriteAccountId,
  onSelectAccount,
  onToggleFavorite,
  formatCurrency,
  formatDate,
}: AccountCardProps) => {
  const hydratedAccount = account?.cuentaId === activeAccountId ? account : undefined;

  return (
    <Paper elevation={0} className="account-paper">
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Stack spacing={1}>
          <Typography variant="h5">Saldo cuenta seleccionada</Typography>
          <Typography variant="body2" color="text.secondary">
            {accountSubtitle}
            {hydratedAccount
              ? ` · Actualizado ${formatDate(hydratedAccount.ultimaActualizacion)}`
              : ' · Actualización no disponible'}
          </Typography>
        </Stack>
        <CreditScoreRoundedIcon fontSize="large" color="primary" />
      </Stack>
      <Box mt={3}>
        {isLoading ? (
          <LinearProgress />
        ) : hasData ? (
          <>
            <Typography variant="h2" className="saldo-principal">
              {selectedAccount ? formatCurrency(selectedAccount.saldoActual, selectedAccount.moneda) : '--'}
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} mt={3}>
              <MetricBadge
                icon={<TrendingUpRoundedIcon />}
                label="Saldo disponible"
                value={
                  hydratedAccount
                    ? formatCurrency(hydratedAccount.saldoDisponible, hydratedAccount.moneda)
                    : 'No disponible'
                }
              />
              <MetricBadge
                icon={<BoltRoundedIcon />}
                label={hydratedAccount ? 'Banco' : 'Moneda'}
                value={hydratedAccount ? hydratedAccount.banco : selectedAccount?.moneda ?? '--'}
              />
            </Stack>
            {availableAccounts.length > 1 && (
              <AccountSelector
                accounts={availableAccounts}
                activeAccountId={activeAccountId}
                favoriteAccountId={favoriteAccountId}
                onSelectAccount={onSelectAccount}
                onToggleFavorite={onToggleFavorite}
                formatCurrency={formatCurrency}
              />
            )}
          </>
        ) : (
          <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            Iniciá sesión para consultar tu saldo en tiempo real.
          </Typography>
        )}
      </Box>
    </Paper>
  );
};

export default AccountCard;
