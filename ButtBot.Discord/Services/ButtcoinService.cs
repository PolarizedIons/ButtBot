using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using ButtBot.Library.Models;
using ButtBot.Library.Models.Database;
using ButtBot.Library.Requests;
using Discord;
using RMQCommandService.Models;
using RMQCommandService.RabbitMq;

namespace ButtBot.Discord.Services
{
    public class ButtcoinService : ISingletonDiService
    {
        private readonly RMQBus _bus;

        public ButtcoinService(RMQBus bus)
        {
            _bus = bus;
        }
        
        public async Task<ButtcoinAccount> GetOrCreateAccount(IGuildUser user, bool activateAccount)
        {
            if (activateAccount)
            {
                // Activate account will automagically create an account
                return await _bus.Send<ButtcoinAccount>(new ActivateAccountRequest
                {
                    UserId = user.Id.ToString(),
                });
            }
            
            return  await _bus.Send<ButtcoinAccount>(new GetOrCreateAccountRequest
            {
                UserId = user.Id.ToString(),
            });
        }

        public async Task<ButtcoinAccount> ActivateAccount(IUser user)
        {
            return await _bus.Send<ButtcoinAccount>(new ActivateAccountRequest
            {
                UserId = user.Id.ToString(),
            });
        }

        public async Task<(ButtcoinAccount, ButtcoinAccount)> Transfer(IUser fromUser, IUser toUser, ulong amount, string reason)
        {
            return await _bus.Send<(ButtcoinAccount, ButtcoinAccount)>(new TransferRequest
            {
                FromUserId = fromUser.Id.ToString(),
                ToUserId = toUser.Id.ToString(),
                Amount = amount,
                Reason = reason,
            });
        }

        public async Task MineCoin(string userId, bool isBruteForce)
        {
            await _bus.Send<EmptyResponse>(new MineCoinRequest
            {
                UserId = userId,
                IsBruteForce = isBruteForce,
                Platform = BotPlatform.Discord,
            });
        }
    }
}
