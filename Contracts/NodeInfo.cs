using System.Linq;

namespace LeaderElection.Contracts
{
    public class NodeInfo
    {
        public string Address { get; set; }

        public bool IsSynced { get; set; }

        public Znode Znode { get; set; }

        public ClusterInfo ClusterInfo { get; set; }

        public static NodeInfo Create(string address)
        {
            return new NodeInfo()
            {
                Address = address,
                Znode = new Znode()
            };
        }

        public static NodeInfo Create(string address, string znodePath)
        {
            return new NodeInfo()
            {
                Address = address,
                Znode = new Znode{
                    ZnodePath = znodePath
                }
            };
        }
    }

    public class Znode
    {
        public string ZnodePath { get; set; }

        public string ZnodeName => ZnodePath
            .Split("/").LastOrDefault();
    }
}
