using System.Threading.Tasks;
using LeaderElection.Contracts;
using LeaderElection.Zookeeeper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LeaderElection.Controllers;

[ApiController]
[Route("[controller]")]
public class ClusterInfoController : ControllerBase
{
    private readonly ILogger<ClusterInfoController> _logger;
    private IZooKeeperReadClient _client;

    public ClusterInfoController(ILogger<ClusterInfoController> logger, IZooKeeperReadClient client)
    {
        _logger = logger;
        _client = client;
    }


    [HttpGet(Name = "Leader")]
    public async Task<Leader> Get()
    {
        return await _client.GetLeader();
    }
}
