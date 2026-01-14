public class SmsService : ISmsService
{
    public Task SendAsync(string phoneNumber, string message)
    {
        // 🔹 TEMP: log instead of real SMS
        Console.WriteLine($"SMS sent to {phoneNumber}");
        Console.WriteLine($"Message: {message}");

        // Later you can plug Twilio / Firebase / AWS SNS
        return Task.CompletedTask;
    }
}
