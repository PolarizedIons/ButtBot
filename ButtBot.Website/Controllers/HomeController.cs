using System.Diagnostics;
using System.Threading.Tasks;
using ButtBot.Website.Models;
using ButtBot.Website.Services;
using Microsoft.AspNetCore.Mvc;

namespace ButtBot.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly DiscordOAuthService _oAuthService;
        private readonly DiscordService _discordService;
        private readonly ButtcoinService _buttcoinService;

        public HomeController(DiscordOAuthService oAuthService, DiscordService discordService, ButtcoinService buttcoinService)
        {
            _oAuthService = oAuthService;
            _discordService = discordService;
            _buttcoinService = buttcoinService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Link()
        {
            return View();
        }

        [HttpPost]
        [Route("link")]
        public IActionResult LinkRedirect()
        {
            return Redirect(_oAuthService.GetOAuth2Url());
        }

        [Route("callback")]
        public async Task<IActionResult> DoLink([FromQuery] string code)
        {
            var userToken = await _oAuthService.ExchangeCodeForAccessToken(code);
            if (userToken == null)
            {
                return Unauthorized("The discord token has expired, or was never valid.");
            }

            var (userInfo, connections) = await _discordService.GetUserInfo(userToken);
            if (userInfo == null || connections == null)
            {
                return Problem("Could not fetch user information.");
            }

            var linkedConnections = await _buttcoinService.LinkConnections(userInfo, connections);

            return View(new LinkedModel {User = userInfo, Connections = linkedConnections});
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
