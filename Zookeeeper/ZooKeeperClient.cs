using LeaderElection.Contracts;
using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeaderElection.Database;

namespace LeaderElection.Zookeeeper;

public class ZooKeeperClient : IZooKeeperClient
{
    private IZookeeperClient _client;

    private readonly NodeInfo _nodeInfo;

    private readonly IKvService _kvService;

    public ZooKeeperClient(NodeInfo nodeInfo, IKvService kvService)
    {
        ConnectZookeeper();

        _nodeInfo = nodeInfo;
        _kvService = kvService;
    }

    private void ConnectZookeeper()
    {
        _client = new ZookeeperClient(new ZookeeperClientOptions(ZookeeperConfig.URL)
        {
            BasePath = "/",
            ConnectionTimeout = TimeSpan.FromSeconds(5), 
            SessionTimeout = TimeSpan.FromSeconds(10),
            OperatingTimeout = TimeSpan.FromSeconds(10), 
            ReadOnly = false,
            SessionId = 0, 
            SessionPasswd = null,
            EnableEphemeralNodeRestore = true
        });
    }

    public async Task ParticipateElection()
    {
        _nodeInfo.Znode.ZnodePath = await CreateElectionNode();

        var previousNode = await PreviousNode(_nodeInfo.Znode.ZnodeName);

        if (previousNode is null)
        {

            Console.WriteLine("leader");

            await CreateOrUpdateClusterInfo();
        }
        else
        {
            Console.WriteLine("follower");

            await _client.SubscribeDataChange($"{ZookeeperConfig.ElectionConnectedNodes}/{previousNode}", PreviousLeaderDownListener());
        }

        await _client.SubscribeDataChange(ZookeeperConfig.ElectionClusterInfoZnode, ElectionCusterInfoChangeListener());

        _nodeInfo.ClusterInfo = await GetClusterInfo();
    }

    private NodeDataChangeHandler PreviousLeaderDownListener()
    {
        return async (ct, args) =>
        {
            if (args.Type == Watcher.Event.EventType.NodeDeleted)
            {
                
                Console.WriteLine($"previous node info: ${args.Path}");
                
                if (args.Path == _nodeInfo.ClusterInfo.Leader.Znode.ZnodePath)
                {
                    await UpdateNewLeader();

                    Console.WriteLine("New Leader: " + _nodeInfo.Znode.ZnodeName);
                }
                else
                {
                    var previousNode = await PreviousNode(_nodeInfo.Znode.ZnodeName);
                    await _client.SubscribeDataChange($"{ZookeeperConfig.ElectionConnectedNodes}/{previousNode}", PreviousLeaderDownListener());

                    Console.WriteLine("previoud node changed: " + previousNode);
                }
            }
        };
    }

    private NodeDataChangeHandler ElectionCusterInfoChangeListener()
    {
        return async (ct, args) =>
        {
            var clusterInfo = ConvertBytesToObject(args.CurrentData);

            if (_nodeInfo.ClusterInfo.LastMutationId < clusterInfo.LastMutationId)
            {
                await _kvService.Pull();
            }

            _nodeInfo.ClusterInfo = clusterInfo;
        };
    }

    private async Task<string> CreateElectionNode()
    {
        return await _client
            .CreateEphemeralAsync(
                ZookeeperConfig.ElectionZnodePrefix,
                Encoding.ASCII.GetBytes(_nodeInfo.Address),
                isSequential: true);
    }

    private async Task CreateOrUpdateClusterInfo()
    {
        var doesExist = await _client.ExistsAsync(ZookeeperConfig.ElectionClusterInfoZnode);
        if (!doesExist)
        {
            _nodeInfo.ClusterInfo = ClusterInfo.Create(_nodeInfo);

            await _client
                .CreatePersistentAsync(
                    ZookeeperConfig.ElectionClusterInfoZnode,
                    GetBytes(_nodeInfo.ClusterInfo));
        }
        else
        {
            await UpdateNewLeader();
        }
    }

    public async Task<string> PreviousNode(string currentNode)
    {
        var nodes = (await _client
                .GetChildrenAsync(ZookeeperConfig.ElectionConnectedNodes))
            .OrderBy(x => x)
            .ToList();

        string previous = null;

        foreach (var node in nodes)
        {
            if (node == currentNode)
                return previous;

            previous = node;
        }

        return null;
    }

    private async Task UpdateNewLeader()
    {
        _nodeInfo.ClusterInfo.Leader.Address = _nodeInfo.Address;
        _nodeInfo.ClusterInfo.Leader.Znode = _nodeInfo.Znode;
        await UpdateClusterInfo(_nodeInfo.ClusterInfo);
    }

    private async Task UpdateClusterInfo(ClusterInfo clusterInfo)
    {
        await _client.SetDataAsync(ZookeeperConfig.ElectionClusterInfoZnode,
            GetBytes(clusterInfo));
    }

    private async Task<ClusterInfo> GetClusterInfo()
    {
        var data = await _client.GetDataAsync(ZookeeperConfig.ElectionClusterInfoZnode);

        return ConvertBytesToObject(data);
    }

    private static ClusterInfo ConvertBytesToObject(IEnumerable<byte> data)
    {
        return JsonSerializer.Deserialize<ClusterInfo>(Encoding.ASCII.GetString(data.ToArray()));
    }

    private byte[] GetBytes(object obj)
    {
        return Encoding.ASCII.GetBytes(JsonSerializer.Serialize(obj));
    }
}