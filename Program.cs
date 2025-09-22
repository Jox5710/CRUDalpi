using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Middleware: Logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"{DateTime.Now}: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
});

// ✅ Enable Swagger/OpenAPI (شغال دايمًا)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();

// In-memory user store
var users = new List<User>();
var nextId = 1;

#region CRUD Endpoints

// GET /users: List all users
app.MapGet("/users", () => users);

// GET /users/{id}: Get user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// POST /users: Create a new user
app.MapPost("/users", (UserDto userDto) =>
{
    // Validation
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(userDto);
    if (!Validator.TryValidateObject(userDto, context, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    var user = new User
    {
        Id = nextId++,
        Name = userDto.Name,
        Email = userDto.Email
    };
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

// PUT /users/{id}: Update user
app.MapPut("/users/{id:int}", (int id, UserDto userDto) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    // Validation
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(userDto);
    if (!Validator.TryValidateObject(userDto, context, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    user.Name = userDto.Name;
    user.Email = userDto.Email;
    return Results.Ok(user);
});

// DELETE /users/{id}: Delete user
app.MapDelete("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();
    users.Remove(user);
    return Results.NoContent();
});

#endregion

#region Weather Endpoint (Demo)

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

#endregion

app.Run();

#region Models

// User model
record User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}

// DTO with validation
record UserDto
{
    [Required]
    public string Name { get; set; } = default!;
    [Required, EmailAddress]
    public string Email { get; set; } = default!;
}

// WeatherForecast model (Demo)
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

#endregion
