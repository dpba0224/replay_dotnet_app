# RePlay — Toy Trading Web Application Project Plan

> **Version:** 1.0 (MVP)
> **Author:** Solo Developer
> **Project Type:** Portfolio / Learning Project
> **Target Timeline:** 1 Month (4 Sprints × ~7 days each)
> **Last Updated:** February 7, 2026

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Comparative Analysis](#2-comparative-analysis)
3. [Unique Value Proposition](#3-unique-value-proposition)
4. [Tech Stack](#4-tech-stack)
5. [System Architecture](#5-system-architecture)
6. [Feature Specifications](#6-feature-specifications)
7. [Database Design](#7-database-design)
8. [API Design](#8-api-design)
9. [Cloud & Hosting Recommendations](#9-cloud--hosting-recommendations)
10. [Sprint Plan](#10-sprint-plan)
11. [Non-Functional Requirements](#11-non-functional-requirements)
12. [Future Enhancements (Post-MVP)](#12-future-enhancements-post-mvp)
13. [Risk Assessment](#13-risk-assessment)
14. [Appendix: References & Resources](#14-appendix-references--resources)

---

## 1. Executive Summary

**RePlay** is a centralized toy trading web application where the platform owns and manages a shared inventory of toys. Users can browse the collection, trade toys they have for new ones, or purchase toys using real money (via Stripe) if they have nothing to trade. The platform targets general users — from parents looking to refresh their children's toy collection to adult collectors interested in Funko Pops, LEGO sets, and vintage items.

### How It Works (The Core Loop)

1. **Admin stocks the inventory** — Platform administrators add toys with photos, descriptions, and condition ratings.
2. **Users browse and request** — Registered users search, filter, and find toys they want.
3. **Trade or Buy** — Users can either trade a toy they currently have (1-for-1 swap against the platform) or pay with Stripe if they have no toys to offer.
4. **Return & Approve** — When users want to swap again, they return their current toy. The original owner (admin) inspects and approves the return based on condition.
5. **Cycle continues** — Returned toys go back into inventory for the next user.

### Key Goals

- Demonstrate full-stack development skills with a modern tech stack (Angular + ASP.NET Core + PostgreSQL).
- Implement industry-standard security (JWT, email validation, ASP.NET Core Identity).
- Build a mobile-responsive interface that works across devices.
- Showcase RESTful API design best practices.
- Containerize the entire application with Docker for easy deployment.

---

## 2. Comparative Analysis

The following table compares RePlay against existing toy trading/swapping platforms to identify gaps and opportunities.

| Feature | **RePlay** (Ours) | **ToyTrader App** | **ToySwap** | **Whirli** | **ToyCycle** |
|---|---|---|---|---|---|
| **Platform Model** | Centralized library (platform owns toys) | Peer-to-peer marketplace | Peer-to-peer swap | Centralized subscription library | Peer-to-peer (school-based) |
| **Payment System** | Trade OR Stripe (real money) | In-app coins ("Zennies") | Points-based | Monthly subscription | Free swapping |
| **Target Audience** | General (kids' toys + collectibles) | Parents with young children | Kids and parents | Parents (ages 0–8) | Kids via school communities |
| **Toy Condition Rating** | ✅ Yes (5-tier system) | ❌ No formal system | ❌ Basic description only | ✅ Internal quality check | ❌ No |
| **User Reputation/Ratings** | ✅ Yes | ❌ No | ❌ No | N/A (centralized) | ❌ No |
| **In-App Messaging** | ✅ Yes | ✅ In-app chat | ✅ Chat room after match | ❌ No (centralized) | ✅ With parental approval |
| **Web App (Mobile Responsive)** | ✅ Yes | ❌ Mobile app only | ✅ Web app | ✅ Website + app | ✅ FlutterFlow app |
| **Search & Filter** | ✅ Advanced (category, condition, age group) | ✅ Basic categories | ✅ Categories/subcategories | ✅ Age-based browsing | ❌ Limited |
| **Admin Panel** | ✅ Full dashboard | ❌ No | ✅ Listing approval | N/A (internal) | ❌ No |
| **Docker Containerized** | ✅ Yes | Unknown | ❌ No | Unknown | ❌ No |
| **Open Source / Portfolio** | ✅ Yes | ❌ Commercial | ❌ Commercial | ❌ Commercial | ❌ Commercial |

### Key Takeaways from Competitors

- **ToyTrader** (toytraderapp.com) is the most established. Their "Zennies" currency system is clever but limits users to their ecosystem. RePlay differentiates by offering real-money purchasing as a flexible alternative.
- **Whirli** (whirli.com) is the closest to RePlay's centralized model but uses a paid subscription. RePlay is free to use — users only pay when they choose to buy instead of trade.
- **ToySwap** (toyswap.app) has a clean swap/give/get model but lacks quality control. RePlay's condition rating and return approval system fills this gap.
- **ToyCycle** focuses heavily on child safety with parental controls. While not RePlay's primary focus for MVP, this is worth considering for future versions.

---

## 3. Unique Value Proposition

What makes **RePlay** stand out from existing platforms:

### 3.1 Hybrid Trade + Purchase Model
Unlike platforms that force you into only trading (ToySwap) or only buying with virtual currency (ToyTrader), RePlay gives users genuine flexibility. If you have a toy to trade, great — swap it for free. If you do not, you can buy with real money. This removes the biggest barrier that stops new users from joining trading platforms: "But I don't have anything to trade yet."

### 3.2 Quality-Controlled Centralized Inventory
Because the platform owns and manages all toys (like a library), every item is condition-rated, inspected on return, and maintained to a standard. This solves the #1 complaint on peer-to-peer platforms: receiving toys in worse condition than advertised.

### 3.3 Trust Through Transparency
The combination of condition ratings (5-tier system), user reputation scores, and admin-approved returns creates multiple layers of trust. Most competitors rely on just one (or none) of these.

### 3.4 Collector-Friendly
Unlike Whirli (ages 0–8) and ToyTrader (kids' toys only), RePlay serves adult collectors too. LEGO sets, Funko Pops, vintage toys — the platform accommodates all categories.

### 3.5 Fully Open-Source Portfolio Piece
As an open-source project with Docker containerization, RePlay is fully reproducible. Other developers can clone, deploy, and learn from it — something no commercial competitor offers.

### 3.6 Additional Differentiating Features to Consider (Post-MVP)

- **Toy Wishlist with Smart Notifications** — Users flag toys they want; get notified when available. No competitor does this well.
- **Trade History Timeline** — Visual timeline of a toy's journey through different users. Tells a "story" for each toy.
- **Seasonal Collections & Themed Events** — Curated toy bundles for holidays (Christmas, Halloween). Drives engagement.
- **Sustainability Dashboard** — Track how many toys were saved from landfills, how many trades happened. Gamifies the environmental impact.
- **QR Code Toy Tracking** — Each toy gets a QR code label. Scan to see its history, condition, and availability. Adds a physical-digital bridge.

---

## 4. Tech Stack

| Layer | Technology | Why This Choice |
|---|---|---|
| **Frontend** | Angular 17+ | Component-based SPA framework with strong TypeScript support. Great for building complex, interactive UIs. |
| **Build Tool** | Vite | Much faster than Webpack (Angular's default). Hot module replacement makes development smoother. |
| **CSS Framework** | Tailwind CSS | Utility-first CSS that makes mobile-responsive design easier. No need to write custom CSS for most layouts. |
| **Backend** | ASP.NET Core 8 | High-performance, cross-platform API framework. Excellent for building RESTful APIs with built-in dependency injection. |
| **Authentication** | ASP.NET Core Identity + JWT | Identity handles user management (registration, passwords, roles). JWT tokens secure the API endpoints. |
| **ORM** | Entity Framework Core | Maps C# objects to database tables automatically. Migrations make schema changes trackable. |
| **Database** | PostgreSQL | Powerful open-source relational database. Excellent for structured data with relationships (users → trades → toys). |
| **Payment** | Stripe | Industry-standard payment processing with excellent developer docs and a generous test mode for development. |
| **Email** | MailKit + Free SMTP Provider | MailKit is the recommended .NET library for sending emails. See Section 9 for free email provider options. |
| **Containerization** | Docker + Docker Compose | Packages the entire app (frontend, backend, database) into containers. "Works on my machine" becomes "works everywhere." |
| **Version Control** | Git + GitHub | Standard for portfolio projects. GitHub Actions can handle CI/CD. |
| **API Documentation** | Swagger / OpenAPI | Auto-generates interactive API documentation from your code. Essential for portfolio projects. |

---

## 5. System Architecture

### 5.1 High-Level Architecture Diagram (Text Representation)

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT (Browser)                         │
│                   Angular 17+ / Vite / Tailwind                 │
│                     (Mobile Responsive SPA)                     │
└─────────────────────┬───────────────────────────────────────────┘
                      │ HTTPS (REST API calls)
                      │ JWT Bearer Token in headers
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                     API GATEWAY / BACKEND                       │
│                       ASP.NET Core 8                            │
│  ┌──────────┐  ┌───────────┐  ┌──────────┐  ┌──────────────┐  │
│  │ Auth     │  │ Toy       │  │ Trade    │  │ Admin        │  │
│  │ Controller│ │ Controller│  │Controller│  │ Controller   │  │
│  └────┬─────┘  └─────┬─────┘  └────┬─────┘  └──────┬───────┘  │
│       │              │              │               │          │
│  ┌────▼──────────────▼──────────────▼───────────────▼───────┐  │
│  │                   Service Layer                          │  │
│  │  (Business Logic, Validation, Trade Matching)            │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                           │                                    │
│  ┌────────────────────────▼─────────────────────────────────┐  │
│  │              Entity Framework Core (ORM)                 │  │
│  └────────────────────────┬─────────────────────────────────┘  │
└───────────────────────────┼─────────────────────────────────────┘
                            │
              ┌─────────────▼──────────────┐
              │        PostgreSQL           │
              │   (Users, Toys, Trades,     │
              │    Messages, Ratings)       │
              └────────────────────────────┘

External Services:
  ├── Stripe API (Payment Processing)
  ├── SMTP Provider (Email Validation & Notifications)
  └── Local File Storage → /uploads/images/ (Toy Photos)
```

### 5.2 Docker Compose Services

```
docker-compose.yml
├── replay-frontend    (Angular app served via Nginx)
├── replay-backend     (ASP.NET Core API)
├── replay-db          (PostgreSQL 16)
└── replay-pgadmin     (Optional: Database admin UI for development)
```

### 5.3 Project Folder Structure

```
RePlay/
├── docker-compose.yml
├── .env.example
├── README.md
│
├── replay-api/                          # ASP.NET Core Backend
│   ├── RePlay.API/                      # Web API project
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── ToyController.cs
│   │   │   ├── TradeController.cs
│   │   │   ├── MessageController.cs
│   │   │   ├── RatingController.cs
│   │   │   └── AdminController.cs
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Middleware/                   # JWT, Error Handling, CORS
│   │   ├── Extensions/                  # Service registration helpers
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── RePlay.Domain/                   # Domain models (entities)
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Toy.cs
│   │   │   ├── Trade.cs
│   │   │   ├── ToyReturn.cs
│   │   │   ├── Message.cs
│   │   │   ├── Rating.cs
│   │   │   └── TransactionHistory.cs
│   │   └── Enums/
│   │       ├── ToyCondition.cs
│   │       ├── TradeStatus.cs
│   │       └── ToyCategory.cs
│   │
│   ├── RePlay.Application/             # Business logic / services
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── ToyService.cs
│   │   │   ├── TradeService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── MessageService.cs
│   │   │   ├── RatingService.cs
│   │   │   ├── ImageService.cs
│   │   │   └── StripePaymentService.cs
│   │   └── Validators/
│   │
│   ├── RePlay.Infrastructure/           # Data access, external services
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   └── Configurations/              # EF Core Fluent API configs
│   │
│   ├── RePlay.API.sln
│   └── Dockerfile
│
├── replay-ui/                           # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                    # Guards, interceptors, services
│   │   │   │   ├── guards/
│   │   │   │   │   ├── auth.guard.ts
│   │   │   │   │   └── admin.guard.ts
│   │   │   │   ├── interceptors/
│   │   │   │   │   └── jwt.interceptor.ts
│   │   │   │   └── services/
│   │   │   │       ├── auth.service.ts
│   │   │   │       ├── toy.service.ts
│   │   │   │       ├── trade.service.ts
│   │   │   │       └── message.service.ts
│   │   │   ├── shared/                  # Reusable components, pipes
│   │   │   ├── features/                # Feature modules
│   │   │   │   ├── auth/
│   │   │   │   │   ├── login/
│   │   │   │   │   ├── register/
│   │   │   │   │   └── email-verify/
│   │   │   │   ├── toys/
│   │   │   │   │   ├── toy-list/
│   │   │   │   │   ├── toy-detail/
│   │   │   │   │   └── toy-form/
│   │   │   │   ├── trades/
│   │   │   │   │   ├── trade-request/
│   │   │   │   │   ├── trade-history/
│   │   │   │   │   └── trade-return/
│   │   │   │   ├── messages/
│   │   │   │   ├── profile/
│   │   │   │   └── admin/
│   │   │   │       ├── dashboard/
│   │   │   │       ├── user-management/
│   │   │   │       ├── toy-inventory/
│   │   │   │       └── trade-monitor/
│   │   │   └── app.routes.ts
│   │   ├── assets/
│   │   ├── environments/
│   │   └── styles.css                   # Tailwind imports
│   ├── tailwind.config.js
│   ├── vite.config.ts
│   ├── package.json
│   └── Dockerfile
│
└── docs/
    ├── API.md
    ├── DATABASE_SCHEMA.md
    └── DEPLOYMENT.md
```

---

## 6. Feature Specifications

### 6.1 User Registration & Authentication

**Why it matters:** This is the foundation of the entire app. Without secure authentication, nothing else works. JWT tokens are the industry standard for SPA (Single Page Application) authentication because the frontend and backend are separate applications.

| Feature | Details |
|---|---|
| **Registration** | Email, full name, password (min 8 chars, 1 uppercase, 1 number, 1 special char). Uses ASP.NET Core Identity for password hashing (bcrypt). |
| **Email Validation** | On registration, system sends a 6-digit code (or unique token link) to the user's email. Account stays inactive until validated. Code expires after 15 minutes. |
| **Login** | Email + password → returns JWT access token (expires 1 hour) + refresh token (expires 7 days). |
| **JWT Security** | Access token sent in `Authorization: Bearer <token>` header. Refresh token stored in HTTP-only cookie (prevents XSS attacks). |
| **Role-Based Access** | Two roles: `User` (default) and `Admin`. Admin role assigned manually in the database or via a seeded admin account. |
| **Password Reset** | Email-based password reset flow with secure token. |

**Email Service Provider Recommendations (Free Tier):**

| Provider | Free Tier | Best For |
|---|---|---|
| **Brevo (formerly Sendinblue)** | 300 emails/day | Best free tier for MVP. No credit card needed. |
| **Mailgun** | 1,000 emails/month (first 3 months free) | Good API, but limited free period. |
| **Mailtrap** | 1,000 emails/month | Excellent for testing/development with an email sandbox. |
| **Gmail SMTP** | 500 emails/day | Quick to set up for development but not for production. |

**Recommendation:** Use **Brevo** for production email delivery (300 free emails/day is plenty for a portfolio MVP) and **Mailtrap** during development (so you do not accidentally send real emails to test accounts).

### 6.2 Toy Management (Admin-Driven)

**Why it matters:** Since RePlay uses a centralized library model, the admin is the "librarian." Toy management is the admin's primary workflow.

| Feature | Details |
|---|---|
| **Create Toy** | Admin fills a form: name, description, category, age group, condition rating, price (for buy option), and uploads up to 5 images. |
| **Update Toy** | Admin can edit any field. Changes are reflected immediately in the public listing. |
| **Archive Toy (Soft Delete)** | Admin hides a toy from the public catalog. The toy remains in the database with `IsArchived = true`. Can be restored anytime. |
| **Restore Toy** | Admin un-archives a toy, making it visible in the catalog again. |
| **Share Toy** | Generate a shareable link for a specific toy (public URL that works even for non-logged-in users). |

**Toy Condition Rating System (5-Tier):**

| Rating | Label | Description |
|---|---|---|
| 5 | Mint / Like New | Unused or barely used. Original packaging may be included. |
| 4 | Excellent | Very light use. No visible wear or damage. |
| 3 | Good | Normal use. Minor cosmetic wear but fully functional. |
| 2 | Fair | Noticeable wear, minor scratches or scuffs. Fully functional. |
| 1 | Acceptable | Heavy wear, may have minor damage. Still functional for play. |

**Toy Categories:**

| Category | Example Items |
|---|---|
| Action Figures & Collectibles | Funko Pops, Marvel figures, Star Wars |
| Building Sets | LEGO, Mega Bloks, K'Nex |
| Board Games & Puzzles | Monopoly, Settlers of Catan, jigsaw puzzles |
| Dolls & Plush | Barbie, American Girl, Squishmallows |
| Vehicles & RC | Hot Wheels, Matchbox, remote control cars |
| Educational & STEM | Science kits, coding robots, telescope sets |
| Outdoor & Sports | Nerf, bikes, scooters |
| Vintage & Retro | Classic toys, retro consoles, vintage collectibles |

### 6.3 Toy Trading

**Why it matters:** This is the core feature — the reason the app exists. The trading system must be simple enough that users understand it instantly but robust enough to prevent abuse.

**Trading Flow:**

```
User browses catalog
        │
        ▼
User finds a toy they want
        │
        ▼
  ┌─────────────────────────────────┐
  │ Does the user have a toy to     │
  │ trade back to the platform?     │
  ├──────────┬──────────────────────┤
  │   YES    │         NO           │
  │          │                      │
  ▼          ▼                      │
Trade Flow   Buy Flow               │
(Free)       (Stripe Payment)       │
  │          │                      │
  ▼          ▼                      │
Platform     Payment confirmed      │
receives     via Stripe             │
user's toy   │                      │
  │          │                      │
  ▼          ▼                      │
Toy marked as "Traded" / "Sold"
        │
        ▼
New toy shipped/assigned to user
        │
        ▼
Trade recorded in Transaction History
```

**Availability Check:** Before a trade or purchase is confirmed, the system verifies:
- The requested toy exists and is not archived.
- The toy status is `Available` (not already `Traded`, `Sold`, or `PendingReturn`).
- The user's account is active (email validated, not suspended).

### 6.4 Toy Return & Approval

**Why it matters:** Returns keep the inventory flowing. Without returns, toys leave the platform permanently and the library runs dry.

| Step | Who | Action |
|---|---|---|
| 1. Request Return | User | User initiates return from their "My Toys" dashboard. Selects the toy and optionally adds a note about condition. |
| 2. Ship/Deliver | User | User sends the toy back (logistics handled outside the app for MVP). |
| 3. Inspect & Approve | Admin | Admin receives the toy, inspects its condition, and updates the condition rating if needed. |
| 4. Approve or Reject | Admin | If approved: toy goes back to `Available` in inventory. If rejected: admin contacts user via in-app message to resolve. |
| 5. Confirmation | System | User receives email notification of approval/rejection. Transaction history updated. |

### 6.5 Toy Selling (Stripe Integration)

**Why it matters:** This removes the cold-start problem. New users who join without any toys to trade can still participate by purchasing. This is critical for onboarding.

| Feature | Details |
|---|---|
| **When It's Used** | Only when the user does NOT have a toy to trade. The option appears as "Buy This Toy" alongside "Trade for This Toy." |
| **Price Display** | Each toy has a price set by the admin (in the toy listing form). |
| **Payment Flow** | Stripe Checkout Session → User redirected to Stripe's hosted payment page → On success, redirected back to RePlay with confirmation. |
| **Stripe Test Mode** | During development, use Stripe test keys. No real money changes hands. Card number `4242 4242 4242 4242` simulates a successful payment. |
| **Webhook** | Stripe sends a webhook to confirm payment. Backend listens on `/api/stripe/webhook` and updates trade status to `Completed`. |

### 6.6 In-App Messaging

**Why it matters:** Users need a way to ask admins about toys or communicate about trade/return issues without leaving the platform.

| Feature | Details |
|---|---|
| **User-to-Admin Chat** | Users can message the platform admin regarding a specific toy or trade. |
| **Trade-Linked Conversations** | Each trade/return can have an associated conversation thread. |
| **Notification** | Unread message count shown in the navbar badge. Email notification sent for new messages (using standard email). |
| **MVP Scope** | Simple threaded messaging (not real-time WebSocket for MVP). Messages load on page refresh or manual refresh button. |

### 6.7 User Reputation & Rating System

**Why it matters:** Ratings build community trust. Even in a centralized model, rating users who return toys in good (or poor) condition helps admins prioritize good actors.

| Feature | Details |
|---|---|
| **Who Gets Rated** | Users are rated by the admin after each return (based on toy condition upon return). |
| **Rating Scale** | 1–5 stars. |
| **Reputation Score** | Average of all ratings. Displayed on user profile. |
| **Impact** | For MVP: informational only. Future: users with low ratings could be restricted from trading. |

### 6.8 Search & Filter

| Filter | Options |
|---|---|
| **Text Search** | Search by toy name, description (full-text search). |
| **Category** | Dropdown/multi-select from toy categories. |
| **Condition** | Filter by minimum condition (e.g., "Good or better"). |
| **Age Group** | 0–2, 3–5, 6–8, 9–12, 13+, All Ages. |
| **Availability** | Available, Traded, All. |
| **Price Range** | Min–Max slider (for buy option). |
| **Sort By** | Newest, Price Low→High, Price High→Low, Condition, Most Popular. |

### 6.9 Transaction History Dashboard

| Feature | Details |
|---|---|
| **User Dashboard** | Shows all the user's trades (completed, pending, rejected), purchases, and returns in a timeline view. |
| **Admin Dashboard** | Shows all platform-wide transactions with filters by date, user, status, and type (trade/purchase/return). |
| **Export** | CSV export for admin (future enhancement). |

### 6.10 Admin Panel

| Module | Features |
|---|---|
| **Dashboard** | Overview stats: total users, active toys, pending trades, pending returns, revenue (from Stripe). |
| **User Management** | View all users, activate/deactivate accounts, view user ratings and trade history. |
| **Toy Inventory** | CRUD operations on toys, bulk archive, condition re-rating, image management. |
| **Trade Monitoring** | View all trades, filter by status, manually resolve disputes. |
| **Return Approval** | Queue of pending returns. Inspect, approve, or reject with notes. |

---

## 7. Database Design

### 7.1 Entity Relationship Overview

```
Users ──────────< Trades >────────── Toys
  │                  │                  │
  │                  │                  │
  ├──< Messages      ├──< ToyReturns   ├──< ToyImages
  │                  │                  │
  └──< Ratings       └──< Transactions └──< ToyHistory
```

### 7.2 Core Entities

**Users** (extends ASP.NET Core Identity `IdentityUser`)

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key (from Identity) |
| FullName | string(100) | Required |
| Email | string(256) | Unique, from Identity |
| DateOfBirth | DateTime? | Optional |
| ProfileImageUrl | string? | Local file path |
| ReputationScore | decimal(3,2) | Calculated average, default 0 |
| TotalTradesCompleted | int | Counter, default 0 |
| IsActive | bool | Controlled by admin |
| CreatedAt | DateTime | UTC timestamp |
| UpdatedAt | DateTime | UTC timestamp |

**Toys**

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| Name | string(200) | Required |
| Description | string(2000) | Required |
| Category | enum (ToyCategory) | See categories list above |
| AgeGroup | string(20) | e.g., "3-5", "13+", "All Ages" |
| Condition | enum (ToyCondition) | 1–5 rating |
| Price | decimal(10,2) | For Stripe purchase option |
| Status | enum (ToyStatus) | Available, Traded, Sold, PendingReturn, Archived |
| IsArchived | bool | Soft delete flag |
| ShareableSlug | string(50) | URL-friendly slug for public sharing |
| CreatedByAdminId | GUID | FK → Users |
| CurrentHolderId | GUID? | FK → Users (who currently has it) |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime | UTC |

**ToyImages**

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| ToyId | GUID | FK → Toys |
| ImagePath | string(500) | Local file path (e.g., `/uploads/images/toy-abc-1.jpg`) |
| DisplayOrder | int | 1 = primary image |
| CreatedAt | DateTime | UTC |

**Trades**

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| RequestedToyId | GUID | FK → Toys (what the user wants) |
| OfferedToyId | GUID? | FK → Toys (what the user trades back, null if buying) |
| UserId | GUID | FK → Users |
| TradeType | enum | Trade, Purchase |
| Status | enum (TradeStatus) | Pending, Approved, Completed, Rejected, Cancelled |
| StripePaymentIntentId | string? | Only for purchases |
| AmountPaid | decimal? | Only for purchases |
| Notes | string(500)? | Optional notes from user |
| CreatedAt | DateTime | UTC |
| CompletedAt | DateTime? | UTC |

**ToyReturns**

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| ToyId | GUID | FK → Toys |
| ReturnedByUserId | GUID | FK → Users |
| ApprovedByAdminId | GUID? | FK → Users |
| Status | enum | Pending, Approved, Rejected |
| ConditionOnReturn | enum (ToyCondition) | Admin rates condition on inspection |
| UserNotes | string(500)? | User's note when initiating return |
| AdminNotes | string(500)? | Admin's note on approval/rejection |
| CreatedAt | DateTime | UTC |
| ResolvedAt | DateTime? | UTC |

**Messages**

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| SenderId | GUID | FK → Users |
| ReceiverId | GUID | FK → Users |
| TradeId | GUID? | FK → Trades (optional, links message to a trade) |
| Content | string(2000) | Message text |
| IsRead | bool | Default false |
| CreatedAt | DateTime | UTC |

**Ratings**

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| RatedUserId | GUID | FK → Users (the user being rated) |
| RatedByAdminId | GUID | FK → Users (admin who gave the rating) |
| ToyReturnId | GUID | FK → ToyReturns (linked to specific return) |
| Score | int | 1–5 |
| Comment | string(500)? | Optional |
| CreatedAt | DateTime | UTC |

**TransactionHistory** (Denormalized read-optimized table for the dashboard)

| Column | Type | Notes |
|---|---|---|
| Id | GUID | Primary key |
| UserId | GUID | FK → Users |
| Type | enum | Trade, Purchase, Return, ReturnApproved, ReturnRejected |
| ToyId | GUID | FK → Toys |
| RelatedTradeId | GUID? | FK → Trades |
| Description | string(500) | Human-readable summary |
| AmountPaid | decimal? | Only for purchases |
| CreatedAt | DateTime | UTC |

---

## 8. API Design

### 8.1 RESTful API Endpoints

All endpoints are prefixed with `/api/v1/`. Authentication required unless marked 🔓 (public).

**Auth Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/auth/register` | Register new user | 🔓 Public |
| POST | `/auth/verify-email` | Validate email with code | 🔓 Public |
| POST | `/auth/login` | Login, returns JWT + refresh token | 🔓 Public |
| POST | `/auth/refresh-token` | Refresh expired access token | 🔓 Public |
| POST | `/auth/forgot-password` | Send password reset email | 🔓 Public |
| POST | `/auth/reset-password` | Reset password with token | 🔓 Public |

**Toy Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/toys` | List all available toys (with pagination, search, filter) | 🔓 Public |
| GET | `/toys/{id}` | Get toy details | 🔓 Public |
| GET | `/toys/share/{slug}` | Get toy by shareable slug | 🔓 Public |
| POST | `/toys` | Create new toy (admin only) | Admin |
| PUT | `/toys/{id}` | Update toy (admin only) | Admin |
| PATCH | `/toys/{id}/archive` | Archive toy (admin only) | Admin |
| PATCH | `/toys/{id}/restore` | Restore archived toy (admin only) | Admin |
| POST | `/toys/{id}/images` | Upload toy images (admin only) | Admin |
| DELETE | `/toys/{id}/images/{imageId}` | Remove toy image (admin only) | Admin |

**Trade Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/trades` | Create trade request (trade or buy) | User |
| GET | `/trades` | Get user's trades (or all for admin) | User/Admin |
| GET | `/trades/{id}` | Get trade details | User/Admin |
| PATCH | `/trades/{id}/approve` | Approve a trade (admin) | Admin |
| PATCH | `/trades/{id}/cancel` | Cancel a trade | User |

**Return Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/returns` | Initiate toy return | User |
| GET | `/returns` | Get returns (user sees theirs, admin sees all) | User/Admin |
| PATCH | `/returns/{id}/approve` | Approve return with condition rating | Admin |
| PATCH | `/returns/{id}/reject` | Reject return with notes | Admin |

**Message Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/messages` | Send a message | User |
| GET | `/messages/conversations` | Get conversation list | User |
| GET | `/messages/conversations/{userId}` | Get messages with specific user | User |
| PATCH | `/messages/{id}/read` | Mark message as read | User |

**Rating Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/ratings` | Rate a user (admin only, linked to return) | Admin |
| GET | `/ratings/user/{userId}` | Get user's ratings | User/Admin |

**Stripe Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/payments/create-checkout` | Create Stripe checkout session | User |
| POST | `/payments/webhook` | Stripe webhook handler | 🔓 Stripe |

**Admin Endpoints**

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/admin/dashboard` | Get dashboard stats | Admin |
| GET | `/admin/users` | List all users (with pagination) | Admin |
| PATCH | `/admin/users/{id}/activate` | Activate user | Admin |
| PATCH | `/admin/users/{id}/deactivate` | Deactivate user | Admin |
| GET | `/admin/transactions` | Get all transactions (with filters) | Admin |

### 8.2 API Standards & Best Practices

- **Versioning:** URL-based (`/api/v1/`). Allows future versions without breaking existing clients.
- **Pagination:** All list endpoints return paginated responses with `pageNumber`, `pageSize`, `totalCount`, `totalPages`.
- **Error Responses:** Consistent format:
  ```json
  {
    "status": 400,
    "title": "Validation Error",
    "errors": {
      "Name": ["Toy name is required."],
      "Price": ["Price must be greater than 0."]
    }
  }
  ```
- **HTTP Status Codes:**
  - `200` Success
  - `201` Created
  - `400` Bad Request (validation errors)
  - `401` Unauthorized (missing/invalid JWT)
  - `403` Forbidden (insufficient role)
  - `404` Not Found
  - `409` Conflict (e.g., toy already traded)
  - `500` Internal Server Error
- **CORS:** Configure to allow only the Angular frontend origin.
- **Rate Limiting:** Basic rate limiting on auth endpoints to prevent brute-force attacks.

---

## 9. Cloud & Hosting Recommendations

Since this is a portfolio project, free tiers are ideal. Here are the best options:

### 9.1 Recommended Setup (Best Free Tier Combination)

| Component | Provider | Free Tier Details | Why |
|---|---|---|---|
| **Backend (ASP.NET)** | **Render** | 750 free instance-hours/month, auto-deploy from GitHub | Supports Docker, no credit card needed, simple setup |
| **Frontend (Angular)** | **Render** (Static Site) | Unlimited free static site hosting, 100GB bandwidth | Perfect for Angular SPA, global CDN |
| **Database** | **Neon** | 0.5 GB storage, autoscaling, serverless PostgreSQL | Always-free tier, no expiry, excellent for portfolio projects |
| **Email** | **Brevo** | 300 emails/day | Best free email API for MVP |
| **Payments** | **Stripe** | Free test mode, 2.9% + $0.30 per live transaction | Industry standard, excellent docs |
| **Domain** | **Freenom / GitHub Pages** | Free subdomain or `.render.com` subdomain | Not essential for MVP |

### 9.2 Alternative Options

| Component | Alternative 1 | Alternative 2 |
|---|---|---|
| **Backend** | **Azure App Service** (F1 free tier: 60 min/day compute, 1GB storage) | **Google Cloud Run** (2M requests/month free) |
| **Frontend** | **Vercel** (unlimited static, 100GB bandwidth) | **Netlify** (100GB bandwidth, 300 build min/month) |
| **Database** | **Supabase** (500MB, 50K monthly active users) | **Aiven** (1 CPU, 1GB RAM, 5GB storage, always free) |
| **Email** | **Mailtrap** (1,000 emails/month, great for testing) | **Gmail SMTP** (500/day, dev only) |

### 9.3 Why Render is the Top Pick

Render stands out for portfolio projects because it offers a permanent free tier (no credit card required), supports Docker natively, auto-deploys from GitHub (push code → app updates automatically), and hosts both backend and frontend on the same platform. This simplifies deployment enormously compared to managing separate services across multiple providers.

**Important limitation:** Render's free web services sleep after 15 minutes of inactivity. This means the first request after inactivity takes ~30 seconds to "wake up." This is acceptable for a portfolio project but would need a paid tier ($7/month) for production use.

---

## 10. Sprint Plan

**Timeline:** 4 weeks (28 days), solo developer
**Working assumption:** ~6-8 hours/day, 5-6 days/week

### Sprint 1: Foundation (Days 1–7)

**Goal:** Project setup, authentication system, and basic toy CRUD.

| Day | Tasks | Deliverables |
|---|---|---|
| 1 | Project scaffolding: Initialize Angular app with Vite + Tailwind. Initialize ASP.NET Core solution with Clean Architecture layers. Setup Docker Compose with PostgreSQL. | Running development environment with hot reload. |
| 2 | Database design: Create EF Core entity models, configure `AppDbContext`, run initial migration. Seed admin user. | Database schema created in PostgreSQL. |
| 3 | Auth backend: Implement registration, email validation (with Brevo/Mailtrap), login with JWT, refresh token. Use ASP.NET Core Identity. | Working auth API endpoints (test with Swagger). |
| 4 | Auth frontend: Build login, registration, and email verification pages in Angular. Setup JWT interceptor and auth guard. | Users can register, verify email, and log in via the UI. |
| 5 | Toy management backend: Implement Toy CRUD endpoints (create, read, update, archive, restore). Image upload (local storage). | Working toy API endpoints with Swagger docs. |
| 6 | Toy management frontend: Build admin toy form (create/edit), toy list page with grid/card layout, toy detail page. | Admin can manage toys via the UI. |
| 7 | Testing & review: Write unit tests for auth and toy services. Fix bugs. Ensure mobile responsiveness on existing pages. | Stable Sprint 1 build. |

**Sprint 1 Milestone:** ✅ Users can register, verify email, login. Admin can create and manage toys.

### Sprint 2: Core Trading System (Days 8–14)

**Goal:** Trading flow, purchasing with Stripe, and search/filter.

| Day | Tasks | Deliverables |
|---|---|---|
| 8 | Trade backend: Implement trade request endpoint with availability checks. Handle both trade and purchase `TradeType`. | Trade API with validation logic. |
| 9 | Stripe integration: Setup Stripe test account. Implement checkout session creation and webhook listener. | Users can pay via Stripe (test mode). |
| 10 | Trade frontend: Build "Trade for this toy" and "Buy this toy" UI flows. Stripe redirect integration. Trade confirmation page. | End-to-end trade/purchase flow working. |
| 11 | Search & filter: Implement backend query with filters (category, condition, age, price, text search). Build frontend filter sidebar/toolbar. | Users can search and filter the toy catalog. |
| 12 | Return system backend: Implement return request, admin approval/rejection with condition re-rating. | Return API endpoints working. |
| 13 | Return system frontend: "My Toys" page with return button. Admin return approval queue with condition form. | Return flow working end-to-end. |
| 14 | Testing & review: Integration tests for trade and return flows. Bug fixes. Mobile responsiveness check. | Stable Sprint 2 build. |

**Sprint 2 Milestone:** ✅ Complete trade cycle works: browse → trade/buy → return → approve.

### Sprint 3: Community Features (Days 15–21)

**Goal:** Messaging, ratings, transaction history, admin dashboard.

| Day | Tasks | Deliverables |
|---|---|---|
| 15 | Messaging backend: Implement message send, conversation list, message thread, mark-as-read. | Messaging API endpoints. |
| 16 | Messaging frontend: Build inbox, conversation view, compose message UI. Unread badge in navbar. | Users can send and receive messages. |
| 17 | Rating system: Backend endpoints for admin to rate users after returns. Frontend: star rating component on return approval form. Calculate reputation score. | Admin can rate users; scores display on profiles. |
| 18 | Transaction history: Backend endpoint with filters. Frontend: timeline/table view for users. Enhanced admin version. | Users and admins see full transaction history. |
| 19 | Admin dashboard: Build overview page with stats (total users, toys, trades, revenue). User management page (activate/deactivate). | Admin panel MVP complete. |
| 20 | User profile page: Show user's trades, ratings, reputation score. Edit profile (name, profile image). | User profile feature complete. |
| 21 | Testing & review: End-to-end testing of all features. Fix bugs. Polish UI. | Stable Sprint 3 build. |

**Sprint 3 Milestone:** ✅ Community features live. Admin dashboard operational.

### Sprint 4: Polish, Deployment & Documentation (Days 22–28)

**Goal:** Mobile responsiveness, Docker, deployment, documentation.

| Day | Tasks | Deliverables |
|---|---|---|
| 22 | Mobile responsiveness: Test every page on mobile breakpoints. Fix layouts with Tailwind responsive classes (`sm:`, `md:`, `lg:`). | Fully responsive on mobile, tablet, desktop. |
| 23 | Email notifications: Implement email triggers for: account verification, trade approved, return approved/rejected, new message received. | Email notifications working. |
| 24 | Docker finalization: Create production Dockerfiles (multi-stage builds for smaller images). Test `docker-compose up` from clean state. | One-command deployment with Docker. |
| 25 | Security hardening: CORS configuration, rate limiting, input validation review, SQL injection prevention check (EF Core handles this, but verify). | Security checklist complete. |
| 26 | Deploy to Render: Setup Render services (static site + web service + Neon DB). Configure environment variables. Test live. | App live on the internet. |
| 27 | Documentation: Write README.md (with screenshots), API documentation, database schema docs, deployment guide. | Comprehensive documentation. |
| 28 | Final testing & polish: Full user journey test on live deployment. Fix any remaining issues. Record a demo video or GIF for README. | Project complete and portfolio-ready. |

**Sprint 4 Milestone:** ✅ App deployed, documented, and portfolio-ready.

---

## 11. Non-Functional Requirements

### 11.1 Security

| Requirement | Implementation |
|---|---|
| Password hashing | ASP.NET Core Identity uses bcrypt by default. |
| JWT tokens | Short-lived access tokens (1 hour) + HTTP-only cookie refresh tokens (7 days). |
| CORS | Whitelist only the Angular frontend domain. |
| Input validation | Server-side validation using FluentValidation or Data Annotations. |
| SQL injection prevention | Entity Framework Core parameterizes all queries automatically. |
| XSS prevention | Angular sanitizes output by default. Server validates all input. |
| File upload security | Validate file types (jpg, png, webp only), max size 5MB, rename files to GUIDs. |
| HTTPS | Enforced by Render (free SSL certificate included). |
| Rate limiting | ASP.NET Core rate limiting middleware on auth endpoints (e.g., 5 login attempts per minute). |

### 11.2 Performance

| Requirement | Target |
|---|---|
| API response time | < 500ms for standard endpoints |
| Page load time | < 3 seconds on 3G connection |
| Image optimization | Compress uploaded images, serve WebP where possible |
| Database queries | Indexed on frequently queried columns (Status, Category, IsArchived) |
| Pagination | Max 20 items per page to limit payload size |

### 11.3 Mobile Responsiveness

| Breakpoint | Target |
|---|---|
| Mobile | 320px – 767px |
| Tablet | 768px – 1023px |
| Desktop | 1024px+ |

Tailwind CSS responsive prefixes (`sm:`, `md:`, `lg:`, `xl:`) will be used consistently. The layout strategy is mobile-first (design for mobile, then enhance for larger screens).

### 11.4 Accessibility (Basic)

| Requirement | Implementation |
|---|---|
| Semantic HTML | Use proper `<nav>`, `<main>`, `<article>`, `<button>` elements |
| Alt text | All toy images have descriptive alt text |
| Keyboard navigation | All interactive elements reachable via Tab |
| Color contrast | Minimum 4.5:1 ratio (WCAG AA) |

---

## 12. Future Enhancements (Post-MVP)

These features are intentionally excluded from the MVP to meet the 1-month deadline, but are documented here for future development.

| Priority | Feature | Description |
|---|---|---|
| High | Cloud image storage | Migrate from local storage to Azure Blob Storage or Cloudinary. |
| High | Real-time notifications | Add SignalR (WebSocket) for instant trade/message notifications. |
| High | Wishlist system | Users flag toys they want; get notified when available. |
| Medium | CI/CD pipeline | GitHub Actions for automated testing and deployment on every push. |
| Medium | Toy history timeline | Visual timeline showing a toy's journey through different users. |
| Medium | Advanced analytics | Admin charts showing trade volume, popular categories, user growth. |
| Medium | PWA support | Convert to Progressive Web App for install-to-home-screen on mobile. |
| Low | Multi-language support | i18n for Angular and backend error messages. |
| Low | Sustainability dashboard | Track environmental impact (toys saved from landfill, CO2 saved). |
| Low | QR code tracking | Each toy gets a QR code; scan to see history and condition. |
| Low | OAuth login | Google/Facebook login via ASP.NET Core Identity external providers. |

---

## 13. Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Scope creep (adding features mid-sprint) | High | High | Stick strictly to sprint plan. Log new ideas in "Future Enhancements" backlog. |
| Stripe integration complexity | Medium | Medium | Use Stripe Checkout (hosted page) instead of custom payment form. Simplest integration path. |
| Docker configuration issues | Medium | Low | Use official Docker images for PostgreSQL and .NET. Test `docker-compose up` early (Day 1). |
| Free tier limitations (Render sleep) | Low | Low | Acceptable for portfolio. Document the limitation in README. |
| Time underestimation | High | High | Prioritize core features (auth, toys, trading) first. Messaging and ratings are secondary. |
| Database migration errors | Medium | Medium | Use EF Core migrations carefully. Always backup before major changes. |

---

## 14. Appendix: References & Resources

### 14.1 Similar Platforms (Research Sources)

| Platform | URL | Notes |
|---|---|---|
| ToyTrader App | https://www.toytraderapp.com/ | Peer-to-peer toy swap with in-app currency "Zennies" |
| ToySwap | https://toyswap.app/ | Swap/give/get model with point system |
| Whirli | https://whirli.com/ | UK-based toy subscription library (centralized model) |
| ToyCycle | https://www.lowcode.agency/case-studies/toycycle | Child-safety-focused toy trading (school communities) |
| The Toyary | https://thetoyary.com/ | Subscription toy library with delivery |

### 14.2 Technical Documentation

| Resource | URL |
|---|---|
| ASP.NET Core Documentation | https://learn.microsoft.com/en-us/aspnet/core/ |
| Angular Documentation | https://angular.dev/ |
| Entity Framework Core | https://learn.microsoft.com/en-us/ef/core/ |
| Tailwind CSS | https://tailwindcss.com/docs |
| Stripe .NET SDK | https://stripe.com/docs/api?lang=dotnet |
| Vite + Angular | https://angular.dev/tools/cli/build-system-migration |
| Docker for .NET | https://learn.microsoft.com/en-us/dotnet/core/docker/introduction |
| PostgreSQL | https://www.postgresql.org/docs/ |
| MailKit (Email) | https://github.com/jstedfast/MailKit |
| JWT in ASP.NET Core | https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt |

### 14.3 Hosting & Deployment

| Service | URL | Free Tier |
|---|---|---|
| Render | https://render.com/ | 750 hrs/month web service + free static sites |
| Neon (PostgreSQL) | https://neon.tech/ | 0.5GB always-free serverless PostgreSQL |
| Brevo (Email) | https://www.brevo.com/ | 300 emails/day |
| Mailtrap (Dev Email) | https://mailtrap.io/ | 1,000 emails/month sandbox |
| Stripe | https://stripe.com/ | Free test mode |

### 14.4 Learning Resources

| Topic | Resource | URL |
|---|---|---|
| Clean Architecture in .NET | Jason Taylor's Template | https://github.com/jasontaylordev/CleanArchitecture |
| Angular Best Practices | Angular Style Guide | https://angular.dev/style-guide |
| RESTful API Design | Microsoft REST Guidelines | https://github.com/microsoft/api-guidelines |
| JWT Authentication Guide | Auth0 Blog | https://auth0.com/blog/securing-asp-net-core-with-jwt/ |
| Stripe Integration (.NET) | Stripe Docs | https://docs.stripe.com/checkout/quickstart?lang=dotnet |

---

> **Ready to build? Start with Sprint 1, Day 1. Set up your development environment, initialize both projects, and get Docker Compose running. The rest follows from there. Good luck with RePlay! 🎮**
