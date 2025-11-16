# Euro Note Explorer

## Description

Euro Note Explorer is a web application that provides insights into Euro banknote circulation statistics. The application fetches data from the Bank of Finland (BoF) Open API and presents banknote quantities and values for user-selected time periods, along with real-time currency conversions.

### Features

- **Banknote Statistics**: View the count and total value of Euro banknotes in circulation by denomination
- **Date Range Filtering**: Select custom time periods to analyze historical banknote data
- **Currency Conversions**: Automatic conversion of banknote values to multiple currencies using current exchange rates
- **Redis Caching**: Optional Redis integration for improved performance, with automatic fallback to in-memory caching
- **Modern UI**: Clean, responsive Blazor Server interface

### Technology Stack

- **Backend**: ASP.NET Core Web API
- **Frontend**: Blazor Server
- **Caching**: Redis (optional) or In-Memory
- **Data Source**: Bank of Finland (BoF) Open API
- **Containerization**: Docker & Docker Compose

## Usage Instructions

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose
- Alternatively, for local development: [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Running with Docker Compose (Recommended)

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd euro-notes-explorer
   ```

2. Start all services:
   ```bash
   docker-compose up -d
   ```

3. Access the application:
   - **Web UI**: http://localhost:7000
   - **API**: http://localhost:7001
   - **API Swagger**: http://localhost:7001/swagger
   - **Redis Explorer**: http://localhost:7002

4. Stop the services:
   ```bash
   docker-compose down
   ```

### Running Locally (Development)

#### API

```bash
cd src/api/EuroNoteExplorer.Api
dotnet run
```

The API will be available at http://localhost:7001

#### UI

```bash
cd src/ui/EuroNoteExplorer.UI
dotnet run
```

The UI will be available at http://localhost:7000

### Configuration

#### Environment Variables

**API (EuroNoteExplorer.Api)**:
- `BoFOpenAPIBaseUrl`: Base URL for Bank of Finland Open API (default: configured in appsettings.json)
- `RedisConnectionString`: Redis connection string (optional, e.g., `localhost:6379`)

**UI (EuroNoteExplorer.UI)**:
- `EuroNoteAPIBaseUrl`: Base URL for the Euro Note Explorer API (default: `http://localhost:7001`)

#### Docker Compose Configuration

The docker-compose.yml file includes the following services:

- **api**: Euro Note Explorer API (port 7001)
- **ui**: Euro Note Explorer UI (port 7000)
- **redis**: Redis cache (port 6379)
- **redis-explorer**: RedisInsight for Redis database management (port 7002)

### Using the Application

1. Open the web UI at http://localhost:7000
2. Navigate to the "Eurosetelit" (Banknotes) page
3. Select a date range using the start and end date pickers
4. Click "Hae tiedot" (Fetch Data) to retrieve banknote statistics
5. View the results showing:
   - Banknote denominations (5€, 10€, 20€, 50€, 100€, 200€, 500€)
   - Quantity of notes in circulation
   - Total value in Euros
   - Converted values in other currencies

### Development

#### Run tests

```
dotnet test
```

#### Project Structure

```
euro-notes-explorer/
├── src/
│   ├── api/
│   │   ├── EuroNoteExplorer.Api/              # ASP.NET Core API
│   │   └── EuroNoteExplorer.Api.Tests/        # Test project for the API
│   ├── ui/
│   │   └── EuroNoteExplorer.UI/               # Blazor Server UI
│   └── shared/
│       └── EuroNoteExplorer.Shared/           # Shared DTOs and utilities
├── docker-compose.yml
└── README.md

```
