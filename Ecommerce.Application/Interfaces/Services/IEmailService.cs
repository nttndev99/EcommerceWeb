using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string fullName, string userId, string token);
        Task SendPasswordResetAsync(string toEmail, string fullName, string userId, string token);
    }
}