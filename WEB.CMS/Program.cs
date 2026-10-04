using Entities.ConfigModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Repositories.IRepositories;
using Repositories.Repositories;
using WEB.CMS.Customize;
using WEB.CMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024 * 1024 * 100; // Limit upload size to 100MB
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.AccessDeniedPath = new PathString("/Account/RedirectLogin");
    options.LoginPath = new PathString("/Account/RedirectLogin");
    options.ReturnUrlParameter = "url";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // nếu dùng ExpireTimeSpan thì  SlidingExpiration phải set là false. Như vậy cho dù tương tác hay k tương tác thì đều timeout theo thời gian đã set
    options.SlidingExpiration = true; //được sử dụng để thiết lập thời gian sống của cookie dựa trên thời gian cuối cùng mà người dùng đã tương tác với ứng dụng . Nếu người dùng tiếp tục tương tác với ứng dụng trước khi cookie hết hạn, thời gian sống của cookie sẽ được gia hạn thêm.

    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = "Net.Security.Cookie",
        Path = "/",
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.SameAsRequest
    };

});
ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config

builder.Services.Configure<DataBaseConfig>(configuration.GetSection("DataBaseConfig"));
builder.Services.Configure<DomainConfig>(configuration.GetSection("DomainConfig"));
// Register services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
builder.Services.AddSingleton<IVehicleInspectionRepository, VehicleInspectionRepository>();
builder.Services.AddSingleton<IRoleRepository, RoleRepository>();
builder.Services.AddSingleton<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddSingleton<IMenuRepository, MenuRepository>();
builder.Services.AddSingleton<IInspectionRepository, InspectionRepository>();
builder.Services.AddSingleton<RedisConn>();
builder.Services.AddHostedService<RedisSubscriberService>();
builder.Services.AddSingleton<ManagementUser>();
// Add SignalR với cấu hình KeepAlive
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // client timeout sau 60s
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);     // server ping client mỗi 15s
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    //app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
// app.UseAntiXssMiddleware();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Hub
app.UseWebSockets();
app.MapHub<CarHub>("/CarHub");

app.Run();
