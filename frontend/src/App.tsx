import { type FormEvent, useCallback, useEffect, useMemo, useState } from 'react';
import { Box, Container, Grid, Stack } from '@mui/material';
import { Navigate, Route, Routes } from 'react-router-dom';
import './App.css';
import { useAppDispatch, useAppSelector } from './hooks';
import { login, logout } from './features/auth/authSlice';
import { fetchAccountById, fetchDashboardData, updateFavoriteAccount } from './features/dashboard/dashboardSlice';
import { demoCredentials } from './config/api';
import { formatCurrency, formatDate } from './utils/formatters';
import LoginView from './components/LoginView';
import DashboardHeader from './components/DashboardHeader';
import SessionCard from './components/SessionCard';
import AccountCard from './components/AccountCard';
import MovementsCard from './components/MovementsCard';
import type { AccountSnapshot } from './types/api';

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
  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(null);
  const favoriteAccountId = dashboard.favoriteAccountId;

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    dispatch(login(formState));
  };

  const handleLogout = () => {
    dispatch(logout());
  };

  const loadAccount = useCallback(
    (accountId: string) => {
      if (!accountId) {
        return;
      }

      if (selectedAccountId !== accountId) {
        setSelectedAccountId(accountId);
      }

      if (dashboard.account?.cuentaId !== accountId) {
        void dispatch(fetchAccountById(accountId));
      }
    },
    [dashboard.account?.cuentaId, dispatch, selectedAccountId],
  );

  const handleSelectAccount = (accountId: string) => {
    loadAccount(accountId);
  };

  const handleToggleFavorite = (accountId: string) => {
    const nextFavorite = favoriteAccountId === accountId ? null : accountId;
    void dispatch(updateFavoriteAccount(nextFavorite));
  };

  useEffect(() => {
    if (auth.token) {
      dispatch(fetchDashboardData());
    }
  }, [auth.token, dispatch]);

  const isLoadingDashboard = dashboard.status === 'loading';
  const hasData = Boolean(dashboard.account && dashboard.card);

  const accountAlias = dashboard.account?.alias ?? 'Cuenta sueldo';

  const availableAccounts = useMemo<AccountSnapshot[]>(() => {
    if (!dashboard.account) {
      return [];
    }

    const principal: AccountSnapshot = {
      cuentaId: dashboard.account.cuentaId,
      alias: dashboard.account.alias,
      moneda: dashboard.account.moneda,
      saldoActual: dashboard.account.saldoActual,
      esPrincipal: dashboard.account.esPrincipal,
      esFavorita: dashboard.account.esFavorita,
    };

    return [principal, ...dashboard.account.otrasCuentas];
  }, [dashboard.account]);

  useEffect(() => {
    if (!availableAccounts.length) {
      if (selectedAccountId !== null) {
        setSelectedAccountId(null);
      }
      return;
    }

    const isSelectionValid = selectedAccountId
      ? availableAccounts.some((account) => account.cuentaId === selectedAccountId)
      : false;

    if (!isSelectionValid) {
      loadAccount(availableAccounts[0].cuentaId);
    }
  }, [availableAccounts, loadAccount, selectedAccountId]);

  const activeAccountId = selectedAccountId ?? dashboard.account?.cuentaId ?? null;
  const selectedAccount = useMemo(
    () => availableAccounts.find((account) => account.cuentaId === activeAccountId),
    [availableAccounts, activeAccountId],
  );

  const selectedIsPrincipal = selectedAccount?.esPrincipal ?? false;
  const accountSubtitle = selectedIsPrincipal ? 'Cuenta principal activa' : 'Cuenta seleccionada';

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
      <Routes>
        <Route
          path="/"
          element={<Navigate to={auth.token ? '/dashboard' : '/login'} replace />}
        />
        <Route
          path="/login"
          element={
            auth.token ? (
              <Navigate to="/dashboard" replace />
            ) : (
              <LoginView
                formState={formState}
                onEmailChange={(value) => setFormState((prev) => ({ ...prev, email: value }))}
                onPasswordChange={(value) => setFormState((prev) => ({ ...prev, password: value }))}
                showPassword={showPassword}
                onToggleShowPassword={() => setShowPassword((prev) => !prev)}
                rememberMe={rememberMe}
                onRememberMeChange={setRememberMe}
                onSubmit={handleSubmit}
                isLoading={auth.status === 'loading'}
                error={auth.error}
              />
            )
          }
        />
        <Route
          path="/dashboard"
          element={
            auth.token ? (
              <Container maxWidth="lg">
                <Stack spacing={4} className="dashboard-container">
                  <DashboardHeader greeting={greeting} onLogout={handleLogout} />

                  {auth.profile && <SessionCard profile={auth.profile} accountAlias={accountAlias} />}

                  <Grid container spacing={3}>
                    <Grid item xs={12} md={7}>
                      <AccountCard
                        isLoading={isLoadingDashboard}
                        hasData={hasData}
                        accountSubtitle={accountSubtitle}
                        selectedAccount={selectedAccount}
                        account={dashboard.account}
                        availableAccounts={availableAccounts}
                        activeAccountId={activeAccountId}
                        favoriteAccountId={favoriteAccountId}
                        onSelectAccount={handleSelectAccount}
                        onToggleFavorite={handleToggleFavorite}
                        formatCurrency={formatCurrency}
                        formatDate={formatDate}
                      />
                    </Grid>

                    <Grid item xs={12} md={5}>
                      <MovementsCard
                        isLoading={isLoadingDashboard}
                        hasData={hasData}
                        card={dashboard.card}
                        movementAccentColors={movementAccentColors}
                        formatCurrency={formatCurrency}
                        formatDate={formatDate}
                        error={dashboard.error}
                      />
                    </Grid>
                  </Grid>
                </Stack>
              </Container>
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
        <Route path="*" element={<Navigate to={auth.token ? '/dashboard' : '/login'} replace />} />
      </Routes>
    </Box>
  );
};
export default App;
