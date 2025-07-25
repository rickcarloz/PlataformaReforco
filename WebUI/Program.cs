using WebUI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<AuthFilter>(); // Registra o filtro de autoriza��o


// Adicionar suporte ao HttpClient para chamadas � API
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7174/"); // URL da sua API
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


// Adicionar suporte � sess�o
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Define o tempo de expira��o da sess�o
    options.Cookie.HttpOnly = true; // Garante que os cookies de sess�o s� possam ser acessados via HTTP
    options.Cookie.IsEssential = true; // Necess�rio para uso da sess�o
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

app.UseSession(); // Habilitar sess�o

app.UseAuthorization();

app.MapRazorPages();

app.Run();
