# AuctionPlatformGe

A real-time online auction platform built with **ASP.NET Core (.NET 10)** using Clean Architecture + CQRS, and an **Angular 21** frontend with NgRx. Bids are pushed to every viewer live over SignalR, auctions close automatically, and bid credits are purchased through the Payze payment gateway.

## Features

- **Live bidding** — SignalR hub (`BidHub`) broadcasts new bids to everyone watching an auction
- **Auto-bids** — set a maximum and the system bids for you until it is reached
- **Buy Now** and **bid packages** — paid through the Payze gateway, confirmed via webhook
- **Automatic auction ending** — a background service closes expired auctions and settles winners
- **Auth** — JWT + BCrypt, plus phone verification with one-time codes (OTP)
- **Wallets** — per-user balance with a transaction history
- **Image upload** for auction listings
- **Admin panel** — stats, user roles, activating and removing auctions
- Global exception middleware, request validation (FluentValidation pipeline behavior), Serilog logging, health checks

## Tech Stack

| Layer | Technologies |
|---|---|
| API | ASP.NET Core (.NET 10), SignalR, Swagger, Serilog |
| Application | MediatR (CQRS), FluentValidation, AutoMapper |
| Infrastructure | EF Core + SQL Server, Repository + Unit of Work, JWT, BCrypt, Payze |
| Frontend | Angular 21, NgRx (store/effects), SignalR client, SCSS |
| Tests | xUnit + Moq (backend), Vitest (frontend) |
| DevOps | Docker, docker-compose, Nginx, GitHub Actions CI |

## Project Structure

```
AuctionPlatformGe.API/             Controllers, SignalR hub, background service, middleware
AuctionPlatformGe.Application/     CQRS commands/queries, DTOs, validators, interfaces
AuctionPlatformGe.Core/            Domain entities (Auction, Bid, AutoBid, Wallet, ...)
AuctionPlatformGe.Infrastructure/  DbContext, migrations, repositories, JWT, payment gateway
AuctionPlatformGe.Tests/           Unit tests (bid placement rules)
auction-client/                    Angular frontend
```

## Getting Started

### Run with Docker

```bash
cp .env.example .env      # fill in values
docker compose up --build
```

The app is served at `http://localhost`.

### Run locally

```bash
# Backend
dotnet ef database update --project AuctionPlatformGe.Infrastructure --startup-project AuctionPlatformGe.API
dotnet run --project AuctionPlatformGe.API

# Frontend
cd auction-client
npm install
npm start          # http://localhost:4200
```

Set `Jwt:Key` and the `Payze` keys with user secrets or environment variables. Test mode is on by default.

## API Overview

| Area | Endpoints |
|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/phone/send-otp`, `POST /api/auth/phone/verify-otp` |
| Auctions | `GET /api/auctions`, `GET /api/auctions/{id}`, `GET /api/auctions/mine`, `POST /api/auctions` |
| Bids | `GET /api/bids/history`, `GET/POST/DELETE /api/auto-bids/{auctionId}` |
| Payments | `GET /api/payments/packages`, `POST /api/payments/initiate`, `POST /api/payments/buy-now`, `POST /api/payments/webhook/payze` |
| Admin | `GET /api/admin/stats`, `GET /api/admin/users`, `PATCH /api/admin/users/{id}/role`, `GET /api/admin/auctions` |
