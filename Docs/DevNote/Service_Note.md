# Functions Structures (Auth, Identity, SendMail)

Domain
└── Entities/
    └── AppUser.cs        ← IdentityUser + thêm: FullName, Avatar, IsActive, CreatedAt

Application
├── Common/
│   └── Result.cs
├── DTOs/
│   ├── Auth/
│   │   ├── ConfirmEmailDto.cs
│   │   ├── RegisterDto.cs
│   │   ├── RegisterDto.cs
│   │   └── ForgotPasswordDto.cs
│   │   └── ResetPasswordDto.cs
│   └── Admin/.......
├── Interfaces/
│   ├── IAuthService.cs
│   ├── IEmailService.cs
│   ├── IIdentityService.cs
│   ├── IRolesService.cs
│   └── IUsersManagerService.cs
└── Services/
    ├── AuthService.cs
    ├── EmailService.cs
    ├── IdentityService.cs
    ├── RolesService.cs
    └── UsersManagerService.cs

Infrastructure
├── Persistence/
│   └── EcommerceDbContext.cs     ← : IdentityDbContext<AppUser>
├── Identity/
│   └── AppDbSeeder.cs            ← seed Admin role + admin user
└── InfrastructureServiceRegistration.cs

MVC(Admin)
├── Controllers/
│   ├── AuthController.cs         ← Login, Register, Logout, ForgotPassword, ResetPassword
│   ├── RolesController.cs        ← CRUD roles (Admin)
│   └── UsersController.cs        ← CRUD users (Admin)
├── Views/
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── ForgotPassword.cshtml
│   │   └── ResetPassword.cshtml
│   ├── Roles/
│   │   ├── Index.cshtml
│   │   └── Create.cshtml
│   └── Users/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       └── Detail.cshtml
└── Filters/
    └── ActionFilter.cs


## Thứ tự implement
```
1. MailSettings model + appsettings.json
2. AppUser entity
3. EcommerceDbContext : IdentityDbContext<AppUser>
4. Migration
5. EmailService  (test Mailtrap trước)
6. AuthService
7. IdentityService  (FindById, Update, Lock...)
8. RolesService
9. UsersManagerService
10. AppDbSeeder
11. Controllers + Views
```

# Auth + Email(Identity) + Roles-User(ADMIN)
appsettings.json / (MailTrap Test Email)
