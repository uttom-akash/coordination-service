using System.Threading.Tasks;
using LeaderElection.Contracts;

namespace LeaderElection.Zookeeeper;

public interface IZooKeeperReadClient
{
    Task<Leader> GetLeader();

    Task<bool> AmILeader();

    Task<string> GetLeaderUrl();
}