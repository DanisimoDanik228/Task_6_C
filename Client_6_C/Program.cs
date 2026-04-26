var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
       .AddRazorRuntimeCompilation();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run("http://localhost:5111");