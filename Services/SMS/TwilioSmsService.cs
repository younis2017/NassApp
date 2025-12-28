using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
namespace Nass.Services.SMS
{
    public class TwilioSmsService : Controller
    {
        private readonly IConfiguration _config;

        public TwilioSmsService(IConfiguration config)
        {
            _config = config;

            TwilioClient.Init(
                _config["Twilio:AccountSid"],
                _config["Twilio:AuthToken"]
            );
        }

        public void SendSms(string to, string message)
        {
            MessageResource.Create(
                to: new PhoneNumber(to),
                from: new PhoneNumber(_config["Twilio:FromNumber"]),
                body: message
            );
        }
    }
}
