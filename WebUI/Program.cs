using WebUI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<AuthFilter>(); // Registra o filtro de autorização


// Adicionar suporte ao HttpClient para chamadas à API
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7174/"); // URL da sua API
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


// Adicionar suporte à sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Define o tempo de expiração da sessão
    options.Cookie.HttpOnly = true; // Garante que os cookies de sessão só possam ser acessados via HTTP
    options.Cookie.IsEssential = true; // Necessário para uso da sessão
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Habilitar sessão

app.UseAuthorization();

app.MapRazorPages();

app.Run();
