import { type FormEvent, useCallback, useEffect, useMemo, useState } from 'react';
import { Alert, Box, Button, Container, Grid, Stack, Typography } from '@mui/material';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import './App.css';
import { useAppDispatch, useAppSelector } from './hooks';
import { login, logout } from './features/auth/authSlice';
import { fetchAccountById, fetchCardMovements, fetchDashboardData, updateFavoriteAccount } from './features/dashboard/dashboardSlice';
import { demoCredentials } from './config/api';
import { formatCurrency, formatDate } from './utils/formatters';
import LoginView from './components/LoginView';
import DashboardHeader from './components/DashboardHeader';
import SessionCard from './components/SessionCard';
import AccountCard from './components/AccountCard';
import MovementsCard from './components/MovementsCard';
import AdminDashboard from './components/AdminDashboard';
import AdminMovementsPanel from './components/AdminMovementsPanel';
import type { AccountSnapshot } from './types/api';

const App = () => {
  const dispatch = useAppDispatch();
  const auth = useAppSelector((state) => state.auth);
  const dashboard = useAppSelector((state) => state.dashboard);
  const navigate = useNavigate();

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

  const isAdmin = auth.profile?.esAdministrador ?? false;

  const handleNavigateAdmin = useCallback(() => {
    navigate('/admin');
  }, [navigate]);

  const handleNavigateAdminMovements = useCallback(() => {
    navigate('/admin/movimientos');
  }, [navigate]);

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

  const handleSelectCardMovements = useCallback(
    (cardId: string) => {
      if (!cardId || cardId === dashboard.selectedCardId) {
        return;
      }
      void dispatch(fetchCardMovements(cardId));
    },
    [dashboard.selectedCardId, dispatch],
  );

  useEffect(() => {
    if (auth.token) {
      dispatch(fetchDashboardData());
    }
  }, [auth.token, dispatch]);

  const isLoadingDashboard = dashboard.status === 'loading';
  const isCardLoading = isLoadingDashboard || dashboard.cardStatus === 'loading';
  const hasAccountData = Boolean(dashboard.account);
  const hasCardData = Boolean(dashboard.card && dashboard.cards.length > 0);

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

  const cardAccountAlias = useMemo(() => {
    if (!dashboard.card) {
      return undefined;
    }
    const target = availableAccounts.find((account) => account.cuentaId === dashboard.card?.cuentaId);
    return target?.alias;
  }, [availableAccounts, dashboard.card]);

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
          path="/admin"
          element={
            auth.token && isAdmin ? (
              <AdminDashboard />
            ) : (
              <Navigate to={auth.token ? '/dashboard' : '/login'} replace />
            )
          }
        />
        <Route
          path="/admin/movimientos"
          element={
            auth.token && isAdmin ? (
              <AdminMovementsPanel />
            ) : (
              <Navigate to={auth.token ? '/dashboard' : '/login'} replace />
            )
          }
        />
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
                  <DashboardHeader
                    greeting={greeting}
                    onLogout={handleLogout}
                    isAdmin={isAdmin}
                    onNavigateAdmin={handleNavigateAdmin}
                  />

                  {isAdmin && (
                    <Alert
                      severity="info"
                      variant="outlined"
                      className="admin-panel-alert"
                      action={
                        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
                          <Button size="small" variant="contained" onClick={handleNavigateAdmin}>
                            Panel principal
                          </Button>
                          <Button size="small" variant="outlined" onClick={handleNavigateAdminMovements}>
                            Movimientos críticos
                          </Button>
                        </Stack>
                      }
                    >
                      <Stack spacing={0.5}>
                        <Typography variant="body2">
                          Sos administrador. El botón "Panel admin" vive arriba a la derecha y te lleva al tablero en /admin.
                        </Typography>
                        <Typography variant="body2">
                          También podés saltar directo al módulo de monitoreo en /admin/movimientos desde acá mismo.
                        </Typography>
                      </Stack>
                    </Alert>
                  )}

                  {auth.profile && <SessionCard profile={auth.profile} accountAlias={accountAlias} />}

                  <Grid container spacing={3}>
                    <Grid item xs={12} md={7}>
                      <AccountCard
                        isLoading={isLoadingDashboard}
                        hasData={hasAccountData}
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
                        isLoading={isCardLoading}
                        hasData={hasCardData}
                        card={dashboard.card}
                        cards={dashboard.cards}
                        selectedCardId={dashboard.selectedCardId}
                        onSelectCard={handleSelectCardMovements}
                        cardAccountAlias={cardAccountAlias}
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
