# Модель TodoItem
```c#
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
```

# Program
```c#
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
```

# ConsoleClient
```c#
using System.Net.Http.Json;

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};
var client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7097") };

try
{
    // GET
    Console.WriteLine("1. Получаем все задачи...");
    var todos = await client.GetFromJsonAsync<List<TodoItem>>("api/todos");
    Console.WriteLine($"   Задач в системе: {todos!.Count}");

    foreach (var todo in todos!)
    {
        Console.WriteLine($"   - {todo.Id}: {todo.Title} (Выполнено: {todo.IsCompleted})");
    }
    Console.WriteLine();

    // POST 
    Console.WriteLine("2. Создаем новую задачу...");
    var response = await client.PostAsJsonAsync("api/todos", new { Title = "Задача от консоли", Description = "Тест клиента" });
    Console.WriteLine($"   Статус: {response.StatusCode}");
    Console.WriteLine();

    // GET
    Console.WriteLine("3. Проверяем задачи после создания...");
    todos = await client.GetFromJsonAsync<List<TodoItem>>("api/todos");
    Console.WriteLine($"   Теперь задач: {todos!.Count}");
    Console.WriteLine();

    // DELETE
    if (todos.Count > 0)
    {
        var lastId = todos.Last().Id;
        Console.WriteLine($"4. Удаляем задачу с id={lastId}...");
        var del = await client.DeleteAsync($"api/todos/{lastId}");
        Console.WriteLine($"   Статус: {del.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}
```

Вывод 
```text 
1. Получаем все задачи...
   Задач в системе: 3
   - 1: Изучить HTTP (Выполнено: False)
   - 2: Написать первый API (Выполнено: False)
   - 3: Протестировать в Postman (Выполнено: False)

2. Создаем новую задачу...
   Статус: Created

3. Проверяем задачи после создания...
   Теперь задач: 4

4. Удаляем задачу с id=4...
   Статус: NoContent
```


