using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ButtBot.Library.Requests;
using Microsoft.Extensions.Configuration;
using RMQCommandService.Models;

namespace ButtBot.Core.Services
{
    public class ConfigurationService : IConsumer<ButtWordsRequest, List<string>>
    {
        private readonly IConfiguration _config;

        public ConfigurationService(IConfiguration config)
        {
            _config = config;
        }
        
        public Task<List<string>> HandleCommand(ButtWordsRequest command)
        {
            return Task.FromResult(_config["Words"].Split(",").Select(x => x.Trim()).ToList());
        }
    }
}
