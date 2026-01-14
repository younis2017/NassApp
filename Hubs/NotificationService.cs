using Microsoft.AspNetCore.SignalR;
using Nass.Domain.Entities; // instead of Nass.Models or Nass.Data
using Nass.Hubs;
using System.Threading.Tasks;

namespace Nass.Helpers
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// Broadcasts a new transaction to all agencies (blind order)
        /// </summary>
        public async Task BroadcastNewTransactionAsync(Transaction transaction)
        {
            await _hub.Clients
                .Group("Agencies") // all agencies listen here
                .SendAsync("ReceiveNotification", new
                {
                    transactionId = transaction.Trans_id,
                    title = "New Transaction",
                    message = transaction.Trans_description,
                    createdAt = transaction.Trans_date,
                    category = transaction.Trans_categories
                });
        }

        /// <summary>
        /// Notify agencies that a transaction was claimed (optional)
        /// </summary>
        public async Task BroadcastTransactionClaimedAsync(int transactionId)
        {
            await _hub.Clients
                .Group("Agencies")
                .SendAsync("TransactionClaimed", transactionId);
        }
    }
}
