# CafeManagement

> A full-featured cafe chain management system built on ASP.NET Core 8 MVC

[![Demo](https://img.shields.io/badge/▶%20Watch%20Demo-YouTube-red?style=for-the-badge&logo=youtube)](https://youtu.be/Eg-ARTI8crY)

---

## Overview

**CafeManagement** is a web-based management application for cafe businesses, built with the **MVC** pattern and a clean **Service Layer** architecture. The system covers all real-world operations — from point-of-sale ordering, inventory tracking, and staff scheduling, to payroll calculation and revenue reporting.

---

## Features

### Point of Sale (POS)
- Intuitive order interface with real-time item and topping selection
- Billing, loyalty point redemption, and receipt generation

### Menu Management
- Full CRUD for categories, food/drink items, and toppings
- Recipe management — link ingredients to each menu item

### Inventory
- Track raw material stock levels and suppliers
- Low-stock alerts and import/export logging

### Staff Management
- Role-based access control (Admin / Staff)
- Manage job positions, work shifts, and schedules
- Shift handover records with incident notes

### Payroll
- Automatic salary calculation based on hours worked and position coefficient
- Monthly payroll history per employee

### Reports & Dashboard
- Daily and monthly revenue and order statistics
- Visual charts on the dashboard overview page

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Frontend | Razor Views, Bootstrap, jQuery |
| Authentication | ASP.NET Core Identity |
| Architecture | Service Layer Pattern |

---

## Project Structure

```
CafeManagement/
├── Controllers/        # MVC Controllers per business domain
├── Services/           # Business logic decoupled from Controllers
├── Models/
│   ├── Domain/         # Entity classes mapped to database tables
│   └── ViewModels/     # View-specific models per screen
├── Views/              # Razor Views + Shared Layout
├── Data/
│   └── AppDbContext.cs # EF Core DbContext
└── wwwroot/            # Static assets (CSS, JS, images)

CafeManagement_Schema.sql    # Database creation script
CafeManagement_SeedData.sql  # Sample data seed script
```

---

## Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/AndrewNguyenITVN/cafe-management-system-dotnet.git
cd CafeManagement

# 2. Run the two SQL scripts in SSMS (in order)
#    CafeManagement_Schema.sql → CafeManagement_SeedData.sql

# 3. Start the application (F5 or dotnet run)
```

Navigate to `http://localhost:7190` and sign in with the default account:

| Email | Password |
|---|---|
| `admin@cafe.com` | `Admin@123` |

---

## Demo

[![Watch the full demo on YouTube](https://img.shields.io/badge/▶%20Full%20Demo-YouTube-red?style=flat-square&logo=youtube)](https://youtu.be/Eg-ARTI8crY)

---

## About

This project was developed as a hands-on ASP.NET Core MVC exercise in a team environment, applying a Git feature-branch workflow with modules divided across team members.
