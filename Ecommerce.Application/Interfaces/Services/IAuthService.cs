using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
        Task<Result> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task LogoutAsync();
        Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
        Task<Result> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken ct = default);
        Task<Result> ResendConfirmationEmailAsync(string email, CancellationToken ct = default);
    }
}