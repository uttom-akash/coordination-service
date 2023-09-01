using LeaderElection.Database;
using LeaderElection.Zookeeeper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeaderElection.Controllers;

[ApiController]
[Route("[controller]/[Action]")]
public class DbOperationController : ControllerBase
{
    private readonly ILogger<DbOperationController> _logger;
    private IZooKeeperReadClient _client;
    private readonly IKvService _kvService;
    private readonly HttpClient _httpClient;

    public DbOperationController(
        ILogger<DbOperationController> logger,
        IZooKeeperReadClient client,
        IKvService kvService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _client = client;
        _kvService = kvService;
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost]
    public async Task<string> Set([FromForm] string key, [FromForm] string value)
    {
        if (await _client.AmILeader())
        {
            return await _kvService.Set(key, value);
        }

        var uri = await _client.GetLeaderUrl();

        var targetUrl = uri + "DbOperation/Set";

        var postData = HttpContext.Request.Form.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();

        var response = await _httpClient.PostAsync(targetUrl, new FormUrlEncodedContent(postData));

        return await response.Content.ReadAsStringAsync();
    }

    [HttpGet]
    public async Task<string> Get(string key)
    {

        return await _kvService.Get(key);
    }

    [HttpGet]
    public async Task<Dictionary<string,string>> Pull()
    {

        return await _kvService.Pull();
    }
}
