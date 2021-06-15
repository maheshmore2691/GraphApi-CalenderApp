
namespace Scheduler.WebClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Scheduler.WebClient.Models;

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfig _config;

        public HomeController(ILogger<HomeController> logger, IConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IActionResult Index()
        {
            var model = GetProviderViewModel();

            return View(model);
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            AuthenticationProperties props = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("Callback"),
                Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
            };
            return Challenge(props, provider);
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false); //"Identity.External"
            if (result?.Succeeded != true)
            {
                throw new AuthenticationException("External authentication error");
            }

            return Redirect("~/");
        }

        public IActionResult RegisterTenant()
        {
            return View();
        }

        [HttpPost]
        [ActionName("registertenant")]
        public IActionResult RegisterTenant(string tenantId)
        {
            var adminConsentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/adminconsent?client_id={_config.AzureConfigs.ClientId}&scope=https://graph.microsoft.com/Calendars.ReadWrite https://graph.microsoft.com/Mail.Send&redirect_uri=http://localhost:17794/";

            //var adminConsentUrl = $"https://login.microsoftonline.com/{tenantId}/adminconsent?client_id={_config.AzureConfigs.ClientId}";
            return Redirect(adminConsentUrl);
        }

        #region Private methods

        private ProviderViewModel GetProviderViewModel()
        {
            ProviderViewModel providerViewModel = new ProviderViewModel
            {
                ExternalProviders = new List<ExternalProvider>
                {
                    new ExternalProvider{ AuthenticationScheme = "aad", DisplayName = "Outlook" },
                    new ExternalProvider{ AuthenticationScheme = "aad", DisplayName = "Gmail" }
                }
            };

            return providerViewModel;
        }

        #endregion
    }
}
