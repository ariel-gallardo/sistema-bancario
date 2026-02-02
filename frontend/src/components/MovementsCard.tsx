import {
  Alert,
  Avatar,
  Box,
  Chip,
  Divider,
  LinearProgress,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  MenuItem,
  Paper,
  Stack,
  Tooltip,
  TextField,
  Typography,
} from '@mui/material';
import CreditCardRoundedIcon from '@mui/icons-material/CreditCardRounded';
import type { TarjetaMovimientosResponse, TarjetaResumenResponse } from '../types/api';
import { resolveCardBrand } from '../utils/cardBrand';

interface MovementsCardProps {
  isLoading: boolean;
  hasData: boolean;
  card?: TarjetaMovimientosResponse;
  cards: TarjetaResumenResponse[];
  selectedCardId: string | null;
  onSelectCard: (cardId: string) => void;
  cardAccountAlias?: string;
  movementAccentColors: string[];
  formatCurrency: (value: number, currency: string) => string;
  formatDate: (value: string, format?: string) => string;
  error?: string;
}

const MovementsCard = ({
  isLoading,
  hasData,
  card,
  cards,
  selectedCardId,
  onSelectCard,
  cardAccountAlias,
  movementAccentColors,
  formatCurrency,
  formatDate,
  error,
}: MovementsCardProps) => {
  const brandMeta = card ? resolveCardBrand(card.marca) : undefined;

  return (
    <Paper elevation={0} className="movements-paper">
      <Stack direction="row" justifyContent="space-between" alignItems="center" className="movements-header">
        <Typography variant="h4" className="movements-title">
          Últimos movimientos
        </Typography>
        <Tooltip title="Se muestran los últimos cinco consumos de la tarjeta seleccionada">
          <Chip label="Top 5" size="medium" color="secondary" variant="outlined" />
        </Tooltip>
      </Stack>
      <Divider sx={{ my: 2.5, opacity: 0.2 }} />
      {isLoading && <LinearProgress />}

      {!isLoading && cards.length === 0 && (
        <Alert severity="info" sx={{ my: 3 }}>
          No encontramos tarjetas asociadas a la cuenta seleccionada.
        </Alert>
      )}

      {!isLoading && hasData && card ? (
        <>
          {cards.length > 0 && (
            <Stack mb={2}>
              <TextField
                select
                size="medium"
                label="Tarjeta asociada"
                value={selectedCardId ?? card.tarjetaId}
                onChange={(event) => onSelectCard(event.target.value)}
                fullWidth
                className="card-selector-field"
              >
                {cards.map((item) => (
                  <MenuItem key={item.tarjetaId} value={item.tarjetaId}>
                    {item.marca} · {item.numeroEnmascarado}
                  </MenuItem>
                ))}
              </TextField>
            </Stack>
          )}

          {brandMeta && (
            <Stack
              direction={{ xs: 'column', sm: 'row' }}
              spacing={2}
              mb={3}
              alignItems="center"
              flexWrap="wrap"
              rowGap={2}
            >
              <Box className="card-brand-pill" sx={{ background: brandMeta.background }}>
                <img src={brandMeta.logo} alt={`${brandMeta.displayName} logo`} />
                <Stack spacing={0}>
                  <Typography variant="subtitle2" color="text.secondary">
                    {brandMeta.displayName}
                  </Typography>
                  <Typography variant="h6">{card.marca}</Typography>
                </Stack>
              </Box>
              <Chip icon={<CreditCardRoundedIcon />} label={card.numeroEnmascarado} variant="outlined" size="medium" />
              {cardAccountAlias && <Chip label={`Cuenta: ${cardAccountAlias}`} variant="outlined" size="medium" />}
            </Stack>
          )}

          <Typography variant="body2" color="text.secondary" gutterBottom>
            Disponible {formatCurrency(card.disponible, 'ARS')} · Pago mínimo {formatCurrency(card.pagoMinimo, 'ARS')}
          </Typography>

          <List>
            {card.movimientos.map((movimiento, index) => (
              <ListItem key={movimiento.movimientoId} disableGutters className="movement-item-row">
                <ListItemAvatar>
                  <Avatar sx={{ bgcolor: movementAccentColors[index % movementAccentColors.length] }}>
                    {movimiento.comercio[0]}
                  </Avatar>
                </ListItemAvatar>
                <ListItemText
                  primary={
                    <Stack direction="row" justifyContent="space-between" alignItems="center">
                      <Typography variant="h6">{movimiento.comercio}</Typography>
                      <Typography variant="h6">-{formatCurrency(movimiento.importe, 'ARS')}</Typography>
                    </Stack>
                  }
                  secondary={
                    <Stack direction="row" justifyContent="space-between">
                      <Typography variant="body2" color="text.secondary">
                        {movimiento.descripcion} · {movimiento.categoria}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
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
        !isLoading && cards.length > 0 && (
          <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            Seleccioná una tarjeta para consultar tus últimos movimientos.
          </Typography>
        )
      )}

      {error && (
        <Alert severity="warning" sx={{ mt: 2 }}>
          {error}
        </Alert>
      )}
    </Paper>
  );
};

export default MovementsCard;
