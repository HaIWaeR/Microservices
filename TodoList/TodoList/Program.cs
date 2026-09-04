using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "API для управления задачами"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Бд
var todos = new List<TodoItem.TodoItem>
{
    new TodoItem.TodoItem { Id = 1, Title = "Изучить HTTP", Description = "GET, POST, PUT, DELETE" },
    new TodoItem.TodoItem { Id = 2, Title = "Написать первый API" },
    new TodoItem.TodoItem { Id = 3, Title = "Протестировать в Postman" }
};
int nextId = 4;

// Эндпоинты

// GET
app.MapGet("/api/todos", (bool? completed) =>
{
    List<TodoItem.TodoItem> result = completed.HasValue
        ? todos.Where(t => t.IsCompleted == completed.Value).ToList()
        : todos;
    return Results.Ok(result);
});

// GET
app.MapGet("/api/todos/{id:int}", (int id) =>
{
    TodoItem.TodoItem? todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.NotFound(new { message = $"Задача с id={id} не найдена" });
    return Results.Ok(todo);
});

// POST
app.MapPost("/api/todos", (TodoItem.TodoItem newTodo) =>
{
    if (string.IsNullOrWhiteSpace(newTodo.Title))
        return Results.BadRequest(new { message = "Поле Title обязательно" });

    newTodo.Id = nextId++;
    newTodo.CreatedAt = DateTime.UtcNow;
    newTodo.IsCompleted = false;
    todos.Add(newTodo);

    return Results.Created($"/api/todos/{newTodo.Id}", newTodo);
});

// PUT
app.MapPut("/api/todos/{id:int}", (int id, TodoItem.TodoItem updated) =>
{
    TodoItem.TodoItem? existing = todos.FirstOrDefault(t => t.Id == id);
    if (existing is null)
        return Results.NotFound(new { message = $"Задача с id={id} не найдена" });

    existing.Title = updated.Title ?? existing.Title;
    existing.Description = updated.Description;
    existing.IsCompleted = updated.IsCompleted;

    return Results.Ok(existing);
});

// DELETE
app.MapDelete("/api/todos/{id:int}", (int id) =>
{
    TodoItem.TodoItem? todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.NotFound(new { message = $"Задача с id={id} не найдена" });

    todos.Remove(todo);
    return Results.NoContent();
});

// PATCH 
app.MapPatch("/api/todos/{id:int}/complete", (int id) =>
{
    TodoItem.TodoItem? todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.NotFound(new { message = $"Задача с id={id} не найдена" });

    todo.IsCompleted = true;
    return Results.Ok(todo);
});

app.Run();