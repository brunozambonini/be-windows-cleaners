# Windows Cleaners Backend API

A comprehensive REST API for image management with user authentication and access control, built with ASP.NET Core 9.0.

## 🏗️ Architecture Overview

This project follows a **Clean Architecture** pattern with clear separation of concerns:

```
├── Controllers/          # API endpoints and HTTP handling
├── Services/            # Business logic layer
├── Repository/          # Data access layer
├── Models/              # Domain entities and DTOs
├── Data/                # Database context and configuration
├── Validators/          # Input validation logic
└── Migrations/          # Database schema versioning
```

### Architecture Benefits

- **Separation of Concerns**: Each layer has a specific responsibility
- **Testability**: Business logic is isolated and easily testable
- **Maintainability**: Changes in one layer don't affect others
- **Scalability**: Easy to add new features without breaking existing code

## 🛠️ Technology Stack

### Core Framework
- **ASP.NET Core 9.0** - Modern, cross-platform web framework
  - *Why*: Provides high performance, built-in dependency injection, and excellent tooling support
  - *Benefits*: Cross-platform compatibility, async/await support, and comprehensive middleware pipeline

### Database & ORM
- **Entity Framework Core 9.0** - Object-Relational Mapping framework
  - *Why*: Simplifies database operations with LINQ queries and automatic SQL generation
  - *Benefits*: Code-first approach, migration support, and change tracking
- **SQL Server LocalDB** - Local development database
  - *Why*: Lightweight, easy setup, and compatible with production SQL Server
  - *Benefits*: No installation required, automatic database creation

### API Documentation
- **Swagger/OpenAPI** - API documentation and testing
  - *Why*: Automatic API documentation generation and interactive testing interface
  - *Benefits*: Self-documenting APIs, client code generation, and easy testing

### Additional Tools
- **Entity Framework Tools** - Database migration management
  - *Why*: Enables code-first migrations and database schema versioning
  - *Benefits*: Version control for database changes, rollback capabilities

## 📋 API Endpoints

### Image Controller (`/api/image`)

#### `GET /api/image`
- **Purpose**: Retrieve images from the system
- **Parameters**: 
  - `userId` (optional): Filter images by specific user
- **Response**: Array of image objects with metadata
- **Use Cases**: Display image galleries, user-specific image lists

#### `GET /api/image/{id}`
- **Purpose**: Get a specific image by its unique identifier
- **Parameters**: `id` (path parameter) - Image ID
- **Response**: Single image object with full details
- **Use Cases**: Image detail views, image editing forms

#### `POST /api/image/upload`
- **Purpose**: Upload new images to the system
- **Content-Type**: `multipart/form-data`
- **Body**: 
  - `title`: Image title/description
  - `imageFile`: Binary image file
  - `userId`: Owner of the image
- **Response**: Created image object with generated ID
- **Use Cases**: Image upload forms, bulk image imports

#### `DELETE /api/image/{id}`
- **Purpose**: Remove an image from the system
- **Parameters**: 
  - `id` (path parameter) - Image ID to delete
  - `userId` (query parameter) - User ID for ownership verification
- **Response**: 204 No Content on success
- **Access Control**: Only image owner can delete
- **Use Cases**: Image management, cleanup operations

#### `DELETE /api/image/reset-db`
- **Purpose**: Remove all images from the database (development/testing)
- **Response**: Count of deleted images
- **Use Cases**: Database reset, testing scenarios

### User Controller (`/api/user`)

#### `GET /api/user`
- **Purpose**: Retrieve all users in the system
- **Response**: Array of user objects (without sensitive data)
- **Use Cases**: User management, user selection interfaces

#### `GET /api/user/{id}`
- **Purpose**: Get a specific user by ID
- **Parameters**: `id` (path parameter) - User ID
- **Response**: Single user object (without sensitive data)
- **Use Cases**: User profile views, user detail pages

#### `POST /api/user`
- **Purpose**: Create a new user account
- **Body**: User registration data (name, email, password, type)
- **Response**: Created user object (without password)
- **Use Cases**: User registration, account creation

#### `PUT /api/user/{id}`
- **Purpose**: Update an existing user's information
- **Parameters**: `id` (path parameter) - User ID to update
- **Body**: Updated user data
- **Response**: Updated user object
- **Use Cases**: Profile editing, user management

#### `DELETE /api/user/{id}`
- **Purpose**: Remove a user account
- **Parameters**: `id` (path parameter) - User ID to delete
- **Response**: 204 No Content on success
- **Use Cases**: Account deletion, user management

## 🔐 Current Access Control

The system currently implements basic access control:

- **Image Ownership**: Users can only delete images they uploaded
- **User Validation**: All operations validate user existence
- **Parameter Validation**: Input validation on all endpoints

## 🚀 Future Improvements

### JWT Authentication System

#### Overview
Implement a comprehensive JWT (JSON Web Token) based authentication system to enhance security and user experience.

#### Proposed Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Frontend      │    │     Backend      │    │    Database     │
│                 │    │                  │    │                 │
│ 1. Login Form   │───▶│ 2. Auth Service  │───▶│ 3. User Table   │
│                 │    │                  │    │                 │
│ 4. Store Token  │◀───│ 5. Generate JWT  │    │                 │
│                 │    │                  │    │                 │
│ 6. API Calls    │───▶│ 7. Validate JWT  │    │                 │
│ (with token)    │    │                  │    │                 │
│                 │    │ 8. Authorize     │    │                 │
│ 9. User Data    │◀───│ 9. Return Data   │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

#### New Endpoints

##### `POST /api/auth/login`
- **Purpose**: Authenticate user and return JWT token
- **Body**: 
  ```json
  {
    "email": "user@example.com",
    "password": "userpassword"
  }
  ```
- **Response**: 
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": 1,
      "name": "John Doe",
      "email": "user@example.com",
      "type": "Customer"
    }
  }
  ```

#### Enhanced Security Features

1. **Token-Based Authentication**
   - JWT tokens with configurable expiration
   - Refresh token mechanism for seamless user experience
   - Token blacklisting for secure logout

2. **Automatic User Context**
   - Extract user ID from JWT token
   - Automatic filtering of user-specific data
   - No need to pass `userId` parameters

3. **Enhanced Access Control**
   - Role-based permissions (Admin, Customer, Employee)
   - Resource-level authorization
   - Audit logging for security events

#### Implementation Benefits

- **Improved Security**: No sensitive data in URLs or query parameters
- **Better UX**: Automatic user context, no manual user ID entry
- **Scalability**: Stateless authentication, easy horizontal scaling
- **Standards Compliance**: Industry-standard JWT implementation
- **Audit Trail**: Complete logging of user actions

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd be-windows-cleaners
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - API: `http://localhost:8000`
   - Swagger UI: `http://localhost:8000/swagger`

### Configuration

Update `appsettings.json` for your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WindowsCleanersDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-should-be-at-least-32-characters-long-for-production",
    "ExpirationHours": 24
  }
}
```

## 📊 Database Schema

### Users Table
- `Id` (int, Primary Key)
- `Name` (nvarchar(100), Required)
- `Email` (nvarchar(255), Required, Unique)
- `Password` (nvarchar(255), Required)
- `Type` (enum: Customer, Employee, Admin)
- `CreatedAt` (datetime2, Required)
- `UpdatedAt` (datetime2, Required)

### Images Table
- `Id` (int, Primary Key)
- `Title` (nvarchar(200), Required)
- `ImageData` (nvarchar(max), Required) - Base64 encoded
- `Created_At` (datetime2, Required)
- `UserId` (int, Foreign Key to Users.Id)

## 🧪 Testing

Use the provided HTTP files for API testing:
- `api-examples.http` - Image management endpoints
- `user-api-examples.http` - User management endpoints

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📞 Support

For support and questions, please open an issue in the repository or contact the development team.
