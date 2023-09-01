namespace LeaderElection.Contracts
{
    public static class ZookeeperConfig
    {
        public static string ElectionBasePath = "/election";

        public static string ElectionConnectedNodes = $"{ElectionBasePath}/connected";

        public static string ElectionZnodePrefix = $"{ElectionConnectedNodes}/node-";

        public static string ElectionClusterInfoZnode = $"{ElectionBasePath}/cluster-Info";

        public static string ElectionLeaderInfoZnode = $"{ElectionBasePath}/leader-Info";

        public static string URL = "127.0.0.1:32771";
    }
}
