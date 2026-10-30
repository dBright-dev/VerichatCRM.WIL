using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// HttpClient pointing to the API
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

builder.Services.AddScoped<IWebAuthService, WebAuthService>();
builder.Services.AddScoped<IClientApiService, ClientApiService>();
builder.Services.AddScoped<ISalesGoalApiService, SalesGoalApiService>();
builder.Services.AddScoped<IClientFieldApiService, ClientFieldApiService>();
builder.Services.AddScoped<IQuotationApiService, QuotationApiService>();
builder.Services.AddScoped<IPipelineApiService, PipelineApiService>();
builder.Services.AddScoped<ISupportApiService, SupportApiService>();
builder.Services.AddScoped<IProductApiService, ProductApiService>();
    builder.Services.AddScoped<IInquiryApiService, InquiryApiService>();
builder.Services.AddScoped<IWarehouseAlertApiService, WarehouseAlertApiService>();
builder.Services.AddScoped<IAdminApiService, AdminApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// No more 404s
app.UseStatusCodePagesWithReExecute("/Home/StatusPage", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
