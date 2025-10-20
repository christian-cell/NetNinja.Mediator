# NetNinja.Mediator

[![NuGet](https://img.shields.io/nuget/v/NetNinja.Mediator.svg)](https://www.nuget.org/packages/NetNinja.Mediator/)
[![Downloads](https://img.shields.io/nuget/dt/NetNinja.Mediator.svg)](https://www.nuget.org/packages/NetNinja.Mediator/)

A simple and efficient tool to implement the Mediator pattern in .NET 8 applications. This package provides a lightweight implementation of the Mediator pattern that enables decoupling between components by sending requests and commands through a centralized mediator service.

## 🚀 Features

- ✅ Simple Mediator pattern implementation
- ✅ .NET 8 support
- ✅ Automatic dependency injection
- ✅ Automatic handler registration from assemblies
- ✅ Clean and easy-to-use interface
- ✅ Compatible with Microsoft.Extensions.DependencyInjection

## 📦 Installation

Install the package from NuGet Package Manager:

```bash
dotnet add package NetNinja.Mediator
```

Or via Package Manager Console:

```powershell
Install-Package NetNinja.Mediator
```

## 🛠️ Setup

### 1. Register the Mediator in Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using NetNinja.Mediator;

namespace Your.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            /*Dependency Injection*/
            builder.Services.AddApplicationServices();
            
            /*Injecting mediator*/
            builder.Services.AddNetNinjaMediator(
                typeof(UserQueryHandler).Assembly, 
                typeof(UserCommandHandler).Assembly
                );

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseCorsHere();
            app.MapControllers();

            app.Run();
        }
    }
}
```


## 📋 Basic Usage

### 1. Create a Request (Query or Command)

```csharp
using NetNinja.Mediator.Abstractions;

// Query Request
public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
    
    public GetUserByIdQuery(int userId)
    {
        UserId = userId;
    }
}

// Command Request
public class CreateUserCommand : IRequest<int>
{
    public string Name { get; set; }
    public string Email { get; set; }
    
    public CreateUserCommand(string name, string email)
    {
        Name = name;
        Email = email;
    }
}
```

### 2. Create Request Handlers

```csharp
using NetNinja.Mediator.Abstractions;

// Query Handler
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    
    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public UserDto Handle(GetUserByIdQuery request)
    {
        var user = _userRepository.GetById(request.UserId);
        
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}

// Command Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public int Handle(CreateUserCommand request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email
        };
        
        return _userRepository.Create(user);
    }
}
```

### 3. Use the Mediator in your Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using NetNinja.Mediator.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediatorService _mediator;
    
    public UsersController(IMediatorService mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetUser(int id)
    {
        var query = new GetUserByIdQuery(id);
        var result = _mediator.Send(query);
        
        return Ok(result);
    }
    
    [HttpPost]
    public ActionResult<int> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Name, request.Email);
        var userId = _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
    }
}
```

## 🏗️ Architecture

The NetNinja.Mediator package implements a simple Mediator pattern with the following components:

### Main Interfaces

- **`IMediatorService`**: Main interface for the mediator service
- **`IRequest<TResponse>`**: Base interface for all requests
- **`IRequestHandler<TRequest, TResponse>`**: Interface for handlers that process requests

### Workflow

1. Create an instance of `IRequest<TResponse>`
2. Send it to `IMediatorService` using the `Send<TResponse>()` method
3. The mediator automatically finds the appropriate `IRequestHandler`
4. The handler processes the request and returns the response

```
Controller → IMediatorService → IRequestHandler → Repository/Service → Response
```

## 🔧 Advanced Configuration

### Register Handlers from Multiple Assemblies

```csharp
builder.Services.AddNetNinjaMediator(
    typeof(UserQueryHandler).Assembly,        // Queries
    typeof(UserCommandHandler).Assembly,      // Commands
    typeof(ProductQueryHandler).Assembly,     // More queries
    typeof(OrderCommandHandler).Assembly      // More commands
);
```

### Use in Services (not just Controllers)

```csharp
public class UserService
{
    private readonly IMediatorService _mediator;
    
    public UserService(IMediatorService mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<UserDto> GetUserAsync(int id)
    {
        var query = new GetUserByIdQuery(id);
        return _mediator.Send(query);
    }
}
```

## 📁 Recommended Project Structure

```
YourProject/
├── Controllers/
│   └── UsersController.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateUserCommand.cs
│   │   └── UpdateUserCommand.cs
│   ├── Queries/
│   │   ├── GetUserByIdQuery.cs
│   │   └── GetUsersQuery.cs
│   ├── Handlers/
│   │   ├── CommandHandlers/
│   │   │   ├── CreateUserCommandHandler.cs
│   │   │   └── UpdateUserCommandHandler.cs
│   │   └── QueryHandlers/
│   │       ├── GetUserByIdQueryHandler.cs
│   │       └── GetUsersQueryHandler.cs
│   └── Models/
│       ├── DTOs/
│       │   ├── UserDto.cs
│       │   └── CreateUserRequest.cs
│       └── Responses/
│           └── ApiResponse.cs
├── Domain/
│   ├── Entities/
│   │   └── User.cs
│   └── Interfaces/
│       └── IUserRepository.cs
└── Infrastructure/
    └── Repositories/
        └── UserRepository.cs
```

### Alternative Structure (Separating Models)

```
YourProject/
├── Controllers/
│   └── UsersController.cs
├── Models/
│   ├── Commands/
│   │   ├── CreateUserCommand.cs
│   │   └── UpdateUserCommand.cs
│   ├── Queries/
│   │   ├── GetUserByIdQuery.cs
│   │   └── GetUsersQuery.cs
│   └── DTOs/
│       └── UserDto.cs
├── Application/
│   ├── Handlers/
│   │   ├── CommandHandlers/
│   │   │   ├── CreateUserCommandHandler.cs
│   │   │   └── UpdateUserCommandHandler.cs
│   │   └── QueryHandlers/
│   │       ├── GetUserByIdQueryHandler.cs
│   │       └── GetUsersQueryHandler.cs
│   └── Services/
│       └── UserService.cs
├── Domain/
│   └── Entities/
│       └── User.cs
└── Infrastructure/
    └── Repositories/
        └── UserRepository.cs
```

## ⚡ Mediator Pattern Benefits

- **Decoupling**: Controllers don't depend directly on business services
- **Reusability**: Handlers can be reused from different entry points
- **Testability**: Easy to mock and unit test
- **Organization**: Cleaner code organized by responsibilities
- **CQRS Ready**: Perfect for implementing Command Query Responsibility Segregation

## 🔗 Useful Links

- [GitHub Repository](https://github.com/christian-cell/NetNinja.Mediator)
- [NuGet Package](https://www.nuget.org/packages/NetNinja.Mediator/)

## 👥 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## ✨ Author

**Christian García Martín**

---

⭐ If this package helps you in your projects, don't forget to give it a star on GitHub!
