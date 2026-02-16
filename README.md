## Fraud Detection Microservice

A real-time fraud detection microservice that evaluates transaction risk by analyzing user behavior patterns. Orders are scored based on configurable risk signals like as ordering velocity, spending anomalies, and account age. Once evaluated, orders are automatically approved, declined, or flagged for manual analyst review.

### Architecture & Tech Stack

Built with Clean Architecture, project consists of three layers:

- **Core** — Domain entities, enums, and repository interfaces. No external dependencies.
- **Infrastructure** — EF Core implementations, PostgreSQL data access, and the fraud scoring engine.
- **API** — REST controllers, request/response DTOs, and Swagger documentation.

### Stack

- C# / .NET 10
- PostgreSQL 16
- Entity Framework Core
- Docker & Docker Compose
- Swagger UI (Swashbuckle)

### Setup

### Prerequisites

- Docker & Docker Compose

### Run

```bash
git clone https://github.com/your-username/fraud-detection.git
cd fraud-detection
cp .env.example .env
docker compose up --build
```

The API will be available at `http://localhost:5092/swagger`.
