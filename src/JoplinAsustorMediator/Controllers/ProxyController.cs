using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JoplinAsustorMediator.Controllers
{
    public class ProxyController : Controller
    {
        private readonly HttpProxyOptions _httpOptions = HttpProxyOptionsBuilder.Instance
            .WithShouldAddForwardedHeaders(false)
            .WithAfterReceive(async (_, e) =>
            {
                if (e.StatusCode == HttpStatusCode.Forbidden) e.StatusCode = HttpStatusCode.NotFound;

                if (e.StatusCode == HttpStatusCode.MultiStatus)
                {
                    var s = await e.Content.ReadAsStringAsync();
                    e.Content = new StringContent(s.Replace("<D:href>", "<D:href>/joplin"));
                }
            }).Build();

        private readonly string _joplinUrl;

        public ProxyController(IOptionsMonitor<AppSettings> settings)
        {
            _joplinUrl = settings.CurrentValue.JoplinUrl;
        }

        [Route("{**rest}")]
        public Task ProxyCatchAll(string rest)
        {
            var queryString = Request.QueryString.Value;
            return this.HttpProxyAsync($"{_joplinUrl}/{rest}{queryString}", _httpOptions);
        }
    }
}