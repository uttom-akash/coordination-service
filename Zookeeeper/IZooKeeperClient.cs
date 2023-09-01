using System.Threading.Tasks;

namespace LeaderElection.Zookeeeper;

public interface IZooKeeperClient
{
    Task ParticipateElection();
}