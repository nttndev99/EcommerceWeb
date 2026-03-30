# Basic
cd <-- den muc
cd.. <-- lui 1 muc
dotnet run
dotnet clean
dotnet restore
dotnet build
dotnet --version 
dir *.sln <----- kiem tra cau truc solution

ctrl + . : quick fix
ctrl + ` : open terminal
ctrl + p : search file
# DotNet
Web API:            dotnet new webapi -n MyFirstApi
MVC:                dotnet new mvc -n MyMvcApp
Razor Pages:        dotnet new webapp -n MyRazorApp
Lib:                dotnet new classlib -n MyApp.Common
Solution:           dotnet new sln -n MyApp
                    dotnet sln add MyApp.Common 
Reference:          dotnet add MyApp.Web reference MyApp.Common
Reference Remove:   dotnet remove MyApp.Web reference MyApp.Common
Open:               cd MyFirstApi
                    code .
Update global tool: dotnet tool update --global dotnet-ef
Check:              dotnet ef --version
# DotNet Architecture
dotnet sln add Ecommerce.API/Ecommerce.API.csproj

dotnet add Ecommerce.API reference Ecommerce.Application 
(API phu thuoc Application)
(Solution Application nam trong solution API)



# ORM + Identity
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.1
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  --version 10.0.1
dotnet add package Microsoft.EntityFrameworkCore.Tools  --version 10.0.1
dotnet add package Microsoft.EntityFrameworkCore.Design  --version 10.0.1
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.1

# Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
# Migrations Architecture
dotnet ef migrations add InitialCreate --project Ecommerce.Infrastructure --startup-project Ecommerce.MVC
dotnet ef migrations update InitialCreate --project Ecommerce.Infrastructure --startup-project Ecommerce.MVC

------------------------------------------------
# Git Basic
git init
git clone
git status
git add
git commit
git log

git remote
git push
git pull
git fetch

git branch
git checkout
git switch
git merge

Merge conflict
Resolve conflict
Commit lại

git rebase
git stash
git reset
git revert
git cherry-pick

# Workflow
main (production)
develop (integration)
feature/*
bugfix/*
hotfix/*

## Clone 
git clone https://github.com/your-repo.git
## Create Branch feature
git checkout -b feature/upload-csv
## Code + Commit
git add .
git commit -m "feat: upload CSV and parse data"
## Push
git push origin feature/upload-csv
## Note
feat: thêm chức năng
fix: sửa bug
refactor: cải thiện code
docs: cập nhật tài liệu
style: format code

------------------------------------------------
# API Package
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
# Swagger
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.OpenApi <--- dung cho AddSecurityRequirement

- Program.cs

// Add services
builder.Services.AddControllers();

// Swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Ecommerce API",
        Version = "v1",
        Description = "Clean Architecture API"
    });
    // JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// redirect root → swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});
app.Run();

------------------------------------------------
# SQL Server in VS code
Extension: Ctrl + shift + x : SQL Server (mssql)
Create (SQL file) -> Ctrl + Shift + P -> Input information SQL server -> connect
SQL file -> Ctrl + Shift + E -> query run

