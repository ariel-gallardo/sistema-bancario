import {
  Alert,
  Avatar,
  Chip,
  Divider,
  LinearProgress,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import CreditCardRoundedIcon from '@mui/icons-material/CreditCardRounded';
import type { TarjetaMovimientosResponse } from '../types/api';

interface MovementsCardProps {
  isLoading: boolean;
  hasData: boolean;
  card?: TarjetaMovimientosResponse;
  movementAccentColors: string[];
  formatCurrency: (value: number, currency: string) => string;
  formatDate: (value: string, format?: string) => string;
  error?: string;
}

const MovementsCard = ({
  isLoading,
  hasData,
  card,
  movementAccentColors,
  formatCurrency,
  formatDate,
  error,
}: MovementsCardProps) => (
  <Paper elevation={0} className="movements-paper">
    <Stack direction="row" justifyContent="space-between" alignItems="center">
      <Typography variant="h5">Últimos movimientos</Typography>
      <Tooltip title="Se muestran los últimos cinco consumos de la tarjeta principal">
        <Chip label="Top 5" size="small" />
      </Tooltip>
    </Stack>
    <Divider sx={{ my: 2.5, opacity: 0.2 }} />
    {isLoading && <LinearProgress />}
    {!isLoading && hasData && card ? (
      <>
        <Stack direction="row" spacing={2} mb={2}>
          <Chip icon={<CreditCardRoundedIcon />} label={card.marca} variant="outlined" />
          <Chip label={card.numeroEnmascarado} variant="outlined" />
        </Stack>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          Disponible {formatCurrency(card.disponible, 'ARS')} · Pago mínimo {formatCurrency(card.pagoMinimo, 'ARS')}
        </Typography>
        <List>
          {card.movimientos.map((movimiento, index) => (
            <ListItem key={movimiento.movimientoId} disableGutters>
              <ListItemAvatar>
                <Avatar sx={{ bgcolor: movementAccentColors[index % movementAccentColors.length] }}>
                  {movimiento.comercio[0]}
                </Avatar>
              </ListItemAvatar>
              <ListItemText
                primary={
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="subtitle1">{movimiento.comercio}</Typography>
                    <Typography variant="subtitle1">-{formatCurrency(movimiento.importe, 'ARS')}</Typography>
                  </Stack>
                }
                secondary={
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="caption" color="text.secondary">
                      {movimiento.descripcion} · {movimiento.categoria}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
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
      !isLoading && (
        <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
          Iniciá sesión para ver tus movimientos
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

export default MovementsCard;
