const fallbackUrls = {
  clientes: 'http://localhost:65534',
  cuentas: 'http://localhost:65532',
  tarjetas: 'http://localhost:65515',
};

export const apiConfig = {
  clientes: import.meta.env.VITE_CLIENTES_URL ?? fallbackUrls.clientes,
  cuentas: import.meta.env.VITE_CUENTAS_URL ?? fallbackUrls.cuentas,
  tarjetas: import.meta.env.VITE_TARJETAS_URL ?? fallbackUrls.tarjetas,
};

export const demoCredentials = {
  email: 'demo@aurorabank.com',
  password: 'B4nco$123',
};
