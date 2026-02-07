# RePlay - Toy Trading Web Application

A centralized toy trading platform where users can browse, trade, or purchase toys. Built with Angular 21 and ASP.NET Core 10.

## Tech Stack

- **Frontend**: Angular 21 with Tailwind CSS v4
- **Backend**: ASP.NET Core 10 with Clean Architecture
- **Database**: PostgreSQL 16
- **Authentication**: ASP.NET Core Identity with JWT
- **Containerization**: Docker & Docker Compose

## Project Structure

```
replay_dotnet/
├── replay-api/                 # ASP.NET Core Backend
│   ├── RePlay.API/             # Web API layer
│   ├── RePlay.Application/     # Business logic
│   ├── RePlay.Domain/          # Entities & enums
│   └── RePlay.Infrastructure/  # Data access
├── replay-ui/                  # Angular Frontend
│   └── src/app/
│       ├── core/               # Guards, interceptors, services
│       ├── features/           # Feature modules
│       └── shared/             # Shared components
├── docker-compose.yml
└── .env.example
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker & Docker Compose
- PostgreSQL 16 (or use Docker)

### Development Setup

1. **Clone the repository**

2. **Start PostgreSQL with Docker**
   ```bash
   docker-compose up -d replay-db
   ```

3. **Run the Backend**
   ```bash
   cd replay-api
   dotnet run --project RePlay.API
   ```
   API will be available at `http://localhost:5000`

4. **Run the Frontend**
   ```bash
   cd replay-ui
   npm install
   npm start
   ```
   App will be available at `http://localhost:4200`

### Docker Deployment

Run the entire stack with Docker Compose:
```bash
docker-compose up --build
```

Services:
- Frontend: http://localhost:4200
- Backend API: http://localhost:5000
- pgAdmin (dev): http://localhost:5050

### Default Admin Account

- Email: `admin@replay.com`
- Password: `Admin@123!`

## Features

- User registration with email verification
- JWT-based authentication
- Role-based access (User/Admin)
- Toy catalog with search & filters
- Trade or purchase toys
- Return approval workflow
- In-app messaging
- User ratings & reputation
- Admin dashboard

## API Documentation

Swagger UI available at `http://localhost:5000/openapi/v1.json` (development mode)

## License

MIT
