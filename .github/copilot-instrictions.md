# ✨ Copilot Instructions

This project is a **marketplace platform** built using a **microservices architecture**. The backend stack is **ASP.NET + PostgreSQL + Entity Framework**, with **pgvector** for AI-powered product recommendations. Services communicate via **RabbitMQ**, and **Nginx** serves as the API Gateway with JWT-based authentication.

---

## ⚙️ Architecture

**Microservices:**

- `AuthService` – Registration & login (email/phone), JWT tokens.
- `UserService` – User profiles, roles, bans.
- `ProductService` – Product and category CRUD, embeddings.
- `CartOrderService` – Cart management, order flow, promo codes.
- `ReviewService` – Ratings and comments with moderation.
- `ModerationService` – Manual and automatic moderation logic.
- `RecommendationService` – Vector-based recommendations via `pgvector`.

**Common stack:**

- **API Gateway** – Nginx for routing, CORS, rate limiting.
- **Communication** – RabbitMQ for async event-based flows.
- **Logging** – Serilog with DB sink.
- **Monitoring** – Prometheus + Grafana.
- **CI/CD** – Docker, GitHub Actions.

---

## 🧑‍💼 User Roles

- **Buyer** – Can browse, purchase, and leave reviews.
- **Seller** – Can add and promote products.
- **Moderator** – Handles moderation queues.
- **Admin** – Can assign roles, manually ban users.

---

## 🧾 Database Overview

### Core Tables

- `Users` – Phone, Email, Role, StatusId, BlockedById, BanReason
- `Products` – Title, Description, CategoryId, SellerId, Embedding, PromotionId, StatusId
- `Categories` – Name, ParentCategoryId, StatusId
- `Orders` + `OrderItems`
- `CartItems`
- `Ratings` – Stars, Comment, IsApproved, StatusId
- `ModerationQueue` – EntityType, EntityId, Content, StatusId
- `Promotions`, `PromoCodes` – Time-limited discounts, per-seller
- Each entity uses a `StatusId` field for logical deletion or state transitions.

### Status Lookup Tables

- `UserStatuses` – Active, Blocked, AwaitingConfirmation, Deleted
- `ProductStatuses` – Active, Blocked, PendingModeration, Archived, Deleted
- Other entities follow the same logic.

---

## 🔄 Moderation Logic

- On product or comment creation → status = `PendingModeration`  
  → sent to `ModerationQueue` via **RabbitMQ**.
- Moderator or automated rules determine outcome.
- 5 rejected comments = **automatic ban**. Admin/moderator can also ban manually.

---

## 🧠 Recommendations (pgvector)

- Product embeddings are stored in a `pgvector` column.
- Similar products are calculated using cosine similarity:
  ```sql
  SELECT * FROM products
  ORDER BY embedding <=> '[current_vector]'
  LIMIT 10;


## Project structure

/services
  /auth-service
  /user-service
  /product-service
  ...
/gateway
/frontend
/docker-compose.yml
/copilot-instructions.md


🧩 Copilot Suggestions
Use clean architecture (Controllers → Services → Repositories).

Always include DTO validation (e.g., FluentValidation).

Log important events and errors using Serilog.

Respect entity state/status patterns (no hard deletes).

Keep everything scalable, readable, and future-proof.
