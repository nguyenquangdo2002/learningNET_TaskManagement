using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagement.Data;
using TaskManagement.Services;
using TaskManagement.Middleware;
var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowReact", policy =>
//         policy.WithOrigins("http://localhost:5173")

//               .AllowAnyHeader()
//               .AllowAnyMethod());
// });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.SetIsOriginAllowed(origin =>
            origin.Contains("vercel.app") ||
            origin.Contains("localhost"))
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});
// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Cấu hình để SignalR có thể đọc token từ query string "access_token"
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && 
                    (path.StartsWithSegments("/hub/notifications") || path.StartsWithSegments("/hub/chat")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddOpenApi();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }


app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowReact");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TaskManagement.Hubs.NotificationHub>("/hub/notifications");
app.MapHub<TaskManagement.Hubs.ChatHub>("/hub/chat");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Đảm bảo DB tồn tại
    db.Database.EnsureCreated();

    // Fix migration history: nếu DB đã tạo bằng EnsureCreated() trước đó,
    // đánh dấu InitialCreate đã apply để Migrate() chỉ chạy migration mới
    try
    {
        db.Database.ExecuteSqlRaw(@"
            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            VALUES ('20260421004115_InitialCreate', '8.0.11')
            ON CONFLICT (""MigrationId"") DO NOTHING;
        ");
    }
    catch { /* __EFMigrationsHistory chưa tồn tại → bỏ qua, Migrate() sẽ tạo */ }

    // Apply tất cả migration pending
    db.Database.Migrate();
}

app.Run();
