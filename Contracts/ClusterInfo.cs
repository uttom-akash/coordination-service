using System;
using System.Collections.Generic;

namespace LeaderElection.Contracts
{
    public class ClusterInfo
    {
        public Leader Leader  { get; set; }

        public List<Follower> Followers { get; set; }

        public int ReplicationFactor { get; set; }

        public int QuoramFactor => (ReplicationFactor + 1) / 2;

        public long LastMutationId { get; set; }

        public static ClusterInfo Create(NodeInfo nodeInfo)
        {
            return new ClusterInfo
            {
                Leader = new Leader
                {
                    Address = nodeInfo.Address,
                    Znode = nodeInfo.Znode
                },
                ReplicationFactor = 3,
                LastMutationId = 1
            };
        }

        public void UpdateLeader(NodeInfo nodeInfo)
        {
            Leader = new Leader
            {
                Address = nodeInfo.Address,
                Znode = nodeInfo.Znode
            };
        }
    }

    public class Leader
    {
        public string Address { get; set; }

        public Znode Znode { get; set; }
    }

    public class Follower
    {
        public string Address { get; set; }

        public bool IsSynced { get; set; }

        public Znode Znode { get; set; }
    }
}
