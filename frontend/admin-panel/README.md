# Tablewise Admin Panel

React-based admin panel for Tablewise reservation management platform.

## Tech Stack

- **Framework:** React 18 + TypeScript + Vite
- **Styling:** Tailwind CSS + shadcn/ui
- **State Management:** Zustand (auth), React Query (server state)
- **Forms:** React Hook Form + Zod validation
- **Routing:** React Router DOM v6
- **HTTP Client:** Axios
- **Notifications:** Sonner

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Copy environment variables
cp .env.example .env

# Update .env with your API URL
# VITE_API_URL=http://localhost:5000
```

### Development

```bash
# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

The app will be available at `http://localhost:3000`

## Project Structure

```
src/
├── components/
│   ├── ui/              # shadcn/ui components
│   ├── layout/          # Layout components (AppLayout, Sidebar, TopBar)
│   └── common/          # Shared components
├── features/
│   ├── auth/            # Authentication pages
│   ├── dashboard/       # Dashboard page
│   ├── tables/          # Tables management
│   ├── rules/           # Rules management
│   ├── reservations/    # Reservations list
│   ├── customers/       # Customer CRM
│   ├── staff/           # Staff management
│   ├── settings/        # Settings page
│   └── subscription/    # Subscription management
├── hooks/               # Custom hooks
├── lib/                 # Utilities (API, auth helpers)
├── stores/              # Zustand stores
├── App.tsx
├── router.tsx
└── main.tsx
```

## Features

### Authentication
- Login with email/password
- Registration with 14-day trial
- Forgot password flow
- Email verification
- JWT token refresh

### Dashboard
- Real-time statistics
- Recent reservations
- Table status overview

### Reservations
- View all reservations
- Create manual reservation
- Filter and search
- Export data

### Tables
- Visual table layout
- Drag & drop arrangement
- Table status management

### Rules
- Custom reservation rules
- Rule templates
- Test rule engine
- Priority management

### Customers
- Customer database
- Reservation history
- Notes and tags
- VIP management

### Staff
- Invite staff members
- Role-based permissions
- Activity log

### Settings
- Venue configuration
- Business hours
- Notification preferences
- Integration settings

## API Integration

The app communicates with the Tablewise API:

- **Base URL:** `VITE_API_URL` (default: http://localhost:5000)
- **Authentication:** JWT Bearer token
- **Idempotency:** Automatic `Idempotency-Key` header on POST requests
- **Error Handling:** 
  - 401: Automatic token refresh
  - 403: Plan limit exceeded notifications
  - 5xx: System error toasts
  - Network: Connection error toasts

## Responsive Design

The admin panel is fully responsive:

- **Mobile:** Sidebar collapses to icon-only view
- **Tablet:** Optimized layout with responsive grids
- **Desktop:** Full sidebar and multi-column layouts

## Dark Mode

Dark mode is enabled by default and uses Tailwind's dark mode utilities.

## State Management

### Auth State (Zustand + Persist)
- User information
- Access & refresh tokens
- Persisted to localStorage

### UI State (Zustand)
- Sidebar collapsed state
- Modal states

### Server State (React Query)
- API data caching
- Automatic refetch
- Optimistic updates

## Environment Variables

```env
VITE_API_URL=http://localhost:5000
VITE_BOOKING_BASE_URL=http://localhost:5173/rezervasyon
```

## License

Proprietary - All rights reserved
