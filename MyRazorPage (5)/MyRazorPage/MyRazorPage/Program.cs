using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using MyRazorPage.Models;
using MyRazorPage.SignalR;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


//Bổ sung 1 service làm việc với các pages vào container Kestrel
builder.Services.AddRazorPages();

//add DBcontext
builder.Services.AddDbContext<PRN221_DBContext>();
//add SignalR
builder.Services.AddSignalR();
//add session
builder.Services.AddSession();
//add cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Signin";
        options.AccessDeniedPath = "/404Page";
    });

var app = builder.Build();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<HubServer>("/hub");
app.UseSession();
app.Run();
