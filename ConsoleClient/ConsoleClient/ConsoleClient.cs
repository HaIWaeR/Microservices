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