export interface LoginResponse {
  clienteId: string;
  cuentaPrincipalId: string;
  tarjetaPrincipalId: string;
  nombre: string;
  email: string;
  token: string;
  expiraUtc: string;
  esAdministrador: boolean;
}

export interface AccountSnapshot {
  cuentaId: string;
  alias: string;
  moneda: string;
  saldoActual: number;
  esPrincipal: boolean;
  esFavorita: boolean;
}

export interface AccountSummaryResponse {
  cuentaId: string;
  alias: string;
  banco: string;
  numero: string;
  moneda: string;
  saldoActual: number;
  saldoDisponible: number;
  ultimaActualizacion: string;
  esPrincipal: boolean;
  esFavorita: boolean;
  otrasCuentas: AccountSnapshot[];
}

export interface MovimientoTarjetaResponse {
  movimientoId: string;
  comercio: string;
  descripcion: string;
  categoria: string;
  importe: number;
  fecha: string;
}

export interface TarjetaMovimientosResponse {
  tarjetaId: string;
  marca: string;
  numeroEnmascarado: string;
  limite: number;
  saldoUtilizado: number;
  disponible: number;
  pagoMinimo: number;
  fechaCierre: string;
  fechaVencimiento: string;
  movimientos: MovimientoTarjetaResponse[];
}

export interface AdminOverviewResponse {
  totalClientes: number;
  solicitudesPendientes: number;
  solicitudesRevisadas: number;
  ultimaSolicitudUtc: string | null;
}

export interface ClienteRegistroSummary {
  registroId: string;
  nombreCompleto: string;
  documento: string;
  email: string;
  telefono: string;
  estado: string;
  creadoEnUtc: string;
}
