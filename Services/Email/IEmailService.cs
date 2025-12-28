using System.Threading.Tasks;

namespace Nass.Services.Email
{
    public interface IEmailService<T>
    {
        Task SendAsync(string to, string subject, string body);
        Task SendAgency(string to, string subject, string body,string bcc);
      
    }
}
