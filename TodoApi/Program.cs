using Microsoft.EntityFrameworkCore;
using TodoApi;
using Swashbuckle.AspNetCore.SwaggerGen;
var builder = WebApplication.CreateBuilder(args);

// חיבור ל-DB דרך appsettings.json
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 41))));

// הגדרת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://todoapi-sl0z.onrender.com") // כאן תכתבי את ה-URL של ה-Client שלך, למשל אם זה React או Vue
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// בדיקה אם מסד הנתונים קיים, ואם לא – יצירה אוטומטית
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    db.Database.EnsureCreated();
}

// הוספת נתיבים (Routes) לטיפול במשימות
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item inputItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = inputItem.Name;
    item.IsComplete = inputItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// הפעלת השרת עם הגדרת CORS
app.UseCors("AllowSpecificOrigin"); // משתמשים בהגדרה "AllowSpecificOrigin"

// הפעלת Swagger
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/",()=>"helloruti&rivki");
app.Run();
