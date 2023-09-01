using LeaderElection.Contracts;
using System.Threading.Tasks;

namespace LeaderElection.Zookeeeper;

public class ZooKeeperReadClient : IZooKeeperReadClient
{

    private readonly NodeInfo _nodeInfo;

    public ZooKeeperReadClient(NodeInfo nodeInfo)
    {
        _nodeInfo = nodeInfo;
    }

    public async Task<Leader> GetLeader()
    {
        return _nodeInfo.ClusterInfo.Leader;
    }

    public async Task<string> GetLeaderUrl()
    {
        return _nodeInfo.ClusterInfo.Leader.Address;
    }

    public async Task<bool> AmILeader()
    {
        return _nodeInfo.Znode.ZnodeName == _nodeInfo.ClusterInfo.Leader.Znode.ZnodeName;
    }
}