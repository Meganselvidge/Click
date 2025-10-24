using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using myapp.Services;

var builder = WebApplication.CreateBuilder(args);

// ⬇️ ALL service registrations go here (before Build)
builder.Services.AddControllersWithViews();
// MongoDB client (singleton) + CounterService (Mongo-variant)
// MongoDB client (singleton) + CounterService (Mongo-variant)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    // Försök läsa i ordning: miljövariabeln MONGODB_URI,
    // sedan ConnectionStrings:Mongo, sedan Mongo:ConnectionString.
    var cs =
        Environment.GetEnvironmentVariable("MONGODB_URI")
        ?? cfg.GetConnectionString("Mongo")
        ?? cfg["Mongo:ConnectionString"]
        ?? cfg["Mongo:Uri"]
        ?? "mongodb://root:changeme@localhost:27017/admin?authSource=admin"; // fallback (lokalt)

    return new MongoClient(cs);
});

builder.Services.AddSingleton<CounterService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".MyApp.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🚫 Do NOT call builder.Build() above this line
var app = builder.Build();

// ⬇️ Middleware pipeline (after Build)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // session must be in the pipeline before endpoints

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();