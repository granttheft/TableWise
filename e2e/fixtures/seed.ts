// DbSeeder.cs ve SeedIds.cs'ten alınan sabitler
export const SEED = {
  admin: {
    email: 'ahmet@demo-restoran.com',
    password: 'Demo123!',
  },
  superAdmin: {
    email: 'admin@tablewise.com.tr',
    password: 'Admin123!',
  },
  tenant: {
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    name: 'Demo Restoran A.Ş.',
    slug: 'demo-restoran',
  },
  venue: {
    id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    name: 'Ana Salon',
  },
  tables: {
    table1: { id: 'e0000001-0000-0000-0000-000000000001', name: 'Masa 1', capacity: 2 },
    table2: { id: 'e0000002-0000-0000-0000-000000000002', name: 'Masa 2', capacity: 2 },
    table3: { id: 'e0000003-0000-0000-0000-000000000003', name: 'Masa 3', capacity: 4 },
    table4: { id: 'e0000004-0000-0000-0000-000000000004', name: 'Masa 4', capacity: 6 },
    table5: { id: 'e0000005-0000-0000-0000-000000000005', name: 'Masa 5', capacity: 8 },
  },
  customers: {
    mehmet: { id: 'e0000050-0000-0000-0000-000000000050', name: 'Mehmet Demir', tier: 'VIP' },
    ayse: { id: 'e0000051-0000-0000-0000-000000000051', name: 'Ayşe Kaya', tier: 'Regular' },
    burak: { id: 'e0000056-0000-0000-0000-000000000056', name: 'Burak Koç', tier: 'Blacklisted' },
  },
}
