using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeaderElection.Contracts;

namespace LeaderElection.Database
{
    public class KvService : IKvService
    {
        private Dictionary<string, string> _dict;

        private readonly HttpClient _httpClient;

        private readonly NodeInfo _nodeInfo;

        public KvService(HttpClient httpClient, NodeInfo nodeInfo)
        {
            _httpClient = httpClient;
            _nodeInfo = nodeInfo;
            _dict = new Dictionary<string, string>();
        }

        public async Task<string> Set(string key, string value)
        {
            if(_dict.ContainsKey(key))
            {
                _dict.Remove(key);
            }

            _dict.Add(key, value);
        
            return $"{key} : {_dict.GetValueOrDefault(key)}";
        }
        public async Task<string> Get(string key)
        {
            return $"{key} : {_dict.GetValueOrDefault(key)}";
        }

        public async Task<Dictionary<string, string>> Pull()
        {
            var targetUrl = _nodeInfo.ClusterInfo.Leader.Address + "DbOperation/Pull";

            var response = await _httpClient.GetAsync(targetUrl);

            _dict = JsonSerializer.Deserialize<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());

            return _dict;
        }
    }

    public interface IKvService
    {
        Task<string> Set(string key, string value);

        Task<string> Get(string key);

        Task<Dictionary<string, string>> Pull();
    }
}