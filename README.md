# SmartShop

SmartShop is a complete E-Commerce web application developed using ASP.NET Core MVC. The project provides a modern online shopping experience with a clean architecture, secure authentication, and a responsive user interface.

## Overview

The application allows customers to browse products, view product details, and manage their shopping cart, while administrators can manage products and categories through a dedicated dashboard.

## Architecture

The project follows MVC architecture:

- Models: Database entities and business data
- Views: Razor Views for UI
- Controllers: Handle requests and application flow
- Entity Framework Core: Database communication
- Identity: Authentication and Authorization
## Features

### Customer Features

* User registration and login
* Browse products
* View product details
* Add products to the shopping cart
* Remove products from the shopping cart
* Responsive user interface

### Admin Features

* Add new products
* Edit existing products
* Delete products
* Manage product categories
* Full CRUD operations
* Admin dashboard

## Technologies Used

### Backend

* ASP.NET Core MVC
* C#
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* LINQ

### Frontend

* HTML5
* CSS3
* Bootstrap 5
* JavaScript
* jQuery
* Razor Views

### Development Tools

* Visual Studio 2022
* Git
* GitHub

## Security

* ASP.NET Identity Authentication
* Role-Based Authorization
* Password Hashing
* Model Validation
* Secure database access using Entity Framework Core

## Project Structure

```text
SmartShop
│
├── Controllers
├── Models
├── Views
├── Data
├── Migrations
├── wwwroot
├── ViewModels
└── Program.cs
```

## Installation

1. Clone the repository.

```bash
git clone https://github.com/a7medyasser-tech/SmartShop.git
```

2. Open the solution in Visual Studio 2022.

3. Configure the SQL Server connection string in `appsettings.json`.

4. Run the following command to apply the database migrations:

```powershell
Update-Database
```

5. Run the project.

## Author

Ahmed Yasser

Backend Developer specializing in ASP.NET Core MVC, C#, Entity Framework Core, and SQL Server.
