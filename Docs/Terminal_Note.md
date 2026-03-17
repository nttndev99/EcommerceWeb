# Basic
cd <-- den muc
cd.. <-- lui 1 muc
dotnet run
dotnet clean
dotnet build
dotnet --version 
dir *.sln <----- kiem tra cau truc solution

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


