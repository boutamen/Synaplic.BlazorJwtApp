# Synaplic.BlazorJwtApp

Synaplic.BlazorJwtApp is a **Blazor WebAssembly Hosted** application implementing **JWT authentication** with **role-based claims permissions**. This project demonstrates **secure authentication, authorization, and API access control** using .NET 8.

---

## 🚀 Features

- ✅ **JWT Authentication**: Users log in with JWT tokens.
- ✅ **Role-Based Claims Authorization**: Permissions are dynamically enforced using claims-based policies.
- ✅ **Blazor WebAssembly Client**: A modern, interactive frontend.
- ✅ **ASP.NET Core API**: Secure backend with authentication and role management.
- ✅ **Token Management**: Supports access and refresh tokens.
- ✅ **LocalStorage Integration**: Stores authentication tokens persistently.
- ✅ **Automatic Token Injection**: `AuthHeaderHandler` ensures secure API requests.
- ✅ **Database Seed**: Auto-creates roles and users with predefined permissions.

---

## 📁 Project Structure

### **Client (Blazor WebAssembly)**

- `Program.cs` → Configures **HttpClient**, **JWT authentication**, and **role-based policies**.
- `AuthService.cs` → Handles **token storage and refresh logic**.
- `AuthHeaderHandler.cs` → Automatically appends JWT tokens to API requests.
- `ApiAuthenticationStateProvider.cs` → Manages **authentication state**.
- `Permissions.cs` → Defines **role-based permissions** using constants.

### **Server (ASP.NET Core API)**

- `Program.cs` → Configures **authentication, identity roles, and authorization policies**.
- `TokenService.cs` → Handles **JWT token generation and validation**.
- `AuthController.cs` → Provides **login, refresh token, and authentication endpoints**.
- `WeatherForecastController.cs` → Example **API protected with role-based policies**.
- `ApplicationDbContext.cs` → **Entity Framework Core database context**.

---

## 🔧 Setup & Installation

### **1️⃣ Clone the Repository**

```sh
git clone https://github.com/boutamen/Synaplic.BlazorJwtApp.git
cd Synaplic.BlazorJwtApp
```

### **2️⃣ Configure the API (Server Side)**

- Open `appsettings.json` in the **Server** project.
- Set up **SQL Server** connection string.
- Configure JWT settings (`Issuer`, `Audience`, `Key`).

### **3️⃣ Apply Database Migrations Optional**

```sh
dotnet ef database update --project Server
```

### **4️⃣ Run the Application**

```sh
dotnet run --project Server
```

The **Client** will start automatically.

---

## 🔑 Authentication & Role-Based Authorization

This application enforces **fine-grained permissions** using **claims-based authorization**.

### **User Roles & Permissions**

| Role    | Assigned Permissions                           |
| ------- | --------------------------------------------- |
| `Admin` | Full access, can manage users and settings.   |
| `Basic` | Limited access, can only fetch weather data. |

### **Default Users & Passwords**

| Username           | Role    | Password    |
|-------------------|--------|------------|
| `admin@synaplic.com` | Admin  | `Password+25` |
| `user@synaplic.com`  | Basic  | `Password+25` |

### **Policy-Based Authorization Example**

The `WeatherForecastController` enforces **claim-based policies**:

```csharp
[HttpGet("admin-weather")]
[Authorize(Policy = Permissions.GetAdminWeather)]
public IEnumerable<WeatherForecast> GetAsAdmin() { ... }

[HttpGet("basic-weather")]
[Authorize(Policy = Permissions.GetBasicWeather)]
public IEnumerable<WeatherForecast> GetAsBasic() { ... }
```

---

## 📜 Technologies Used

- **Blazor WebAssembly** – Interactive frontend.
- **ASP.NET Core Web API** – Secure backend services.
- **Entity Framework Core** – Database integration.
- **JWT Authentication** – Secure login and token management.
- **Blazored.LocalStorage** – Persistent token storage.

---

## 📌 Contribution & License

✅ **Contributions are welcome!** Open an issue or create a pull request.
📜 **License**: MIT

