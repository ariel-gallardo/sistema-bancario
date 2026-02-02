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

export type DemoUserRole = 'admin' | 'cliente';

export interface DemoUserCredential {
  role: DemoUserRole;
  name: string;
  email: string;
  password: string;
  description: string;
}

export const demoUsers: DemoUserCredential[] = [
  {
    role: 'admin',
    name: 'Ariel Gallardo',
    email: 'demo@aurorabank.com',
    password: 'B4nco$123',
    description: 'Admin general',
  },
  {
    role: 'admin',
    name: 'Luciana Ferraro',
    email: 'luciana@aurorabank.com',
    password: 'Admin#2024',
    description: 'Operaciones corporativas',
  },
  {
    role: 'cliente',
    name: 'Mateo Rivas',
    email: 'mateo@aurorabank.com',
    password: 'Cliente#1',
    description: 'PyME servicios',
  },
  {
    role: 'cliente',
    name: 'Valentina Duarte',
    email: 'valentina@aurorabank.com',
    password: 'Cliente#2',
    description: 'Estudio creativo',
  },
];

export const demoCredentials = demoUsers[0];
