using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JoplinAsustorMediator.Controllers
{
    public class ProxyController : Controller
    {
        private readonly HttpProxyOptions _httpOptions = HttpProxyOptionsBuilder.Instance.WithShouldAddForwardedHeaders(false)
            .WithHttpClientName("CustomHttpClient")
            .WithIntercept(async c =>
            {
                var isRealJoplinRequest = c.Request.Path != string.Empty && c.Request.Path != "/";
                if (isRealJoplinRequest) return false; // do not intercept

                c.Response.StatusCode = (int)HttpStatusCode.OK;
                await c.Response.WriteAsync("I'm up and running!");

                return true;
            })
            .WithAfterReceive(async (_, e) =>
            {
                if (e.StatusCode == HttpStatusCode.Forbidden)
                {
                    e.StatusCode = HttpStatusCode.NotFound;
                    e.Content = new StringContent((await e.Content.ReadAsStringAsync())
                                                  .Replace("403", "404")
                                                  .Replace("Forbidden", "NotFound"));
                }

                if (e.StatusCode == HttpStatusCode.MultiStatus)
                {
                    e.Content = new StringContent((await e.Content.ReadAsStringAsync())
                                                  .Replace("<D:href>", "<D:href>/joplin"));
                }
            })
            .Build();

        private readonly string _joplinUrl;

        public ProxyController(IOptionsMonitor<AppSettings> settings) => _joplinUrl = settings.CurrentValue.JoplinUrl;

        [Route("{**rest}")]
        public Task ProxyCatchAll(string rest)
        {
            var queryString = Request.QueryString.Value;
            return this.HttpProxyAsync($"{_joplinUrl}/{rest}{queryString}", _httpOptions);
        }
    }
}