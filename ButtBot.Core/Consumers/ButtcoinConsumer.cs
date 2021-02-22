using System.Threading.Tasks;
using ButtBot.Core.Services;
using ButtBot.Library.Models.Database;
using ButtBot.Library.Requests;
using RMQCommandService.Models;

namespace ButtBot.Core.Consumers
{
    public class ButtcoinConsumer : IConsumer<GetOrCreateAccountRequest, ButtcoinAccount>,
        IConsumer<ActivateAccountRequest, ButtcoinAccount>,
        IConsumer<MineCoinRequest, EmptyResponse>,
        IConsumer<TransferRequest, (ButtcoinAccount, ButtcoinAccount)>
    {
        private readonly ButtcoinService _buttcoinService;

        public ButtcoinConsumer(ButtcoinService buttcoinService)
        {
            _buttcoinService = buttcoinService;
        }
        
        public async Task<ButtcoinAccount> HandleCommand(GetOrCreateAccountRequest command)
        {
            return await _buttcoinService.GetOrCreateAccount(command.UserId);
        }

        public async Task<ButtcoinAccount> HandleCommand(ActivateAccountRequest command)
        {
            return await _buttcoinService.ActivateAccount(command.UserId);
        }

        public async Task<EmptyResponse> HandleCommand(MineCoinRequest command)
        {
            await _buttcoinService.MineCoin(command.UserId, command.Platform, command.IsBruteForce);
            return new EmptyResponse();
        }

        public async Task<(ButtcoinAccount, ButtcoinAccount)> HandleCommand(TransferRequest command)
        {
            return await _buttcoinService.Transfer(command.FromUserId, command.ToUserId, command.Amount, command.Reason);
        }
    }
}
