using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace View
{
    public class Config
    {
        public static Config Instance { get; private set; }
        private static readonly string ExecutionPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

        private HashSet<string> _paths;
        public IEnumerable<string> Paths => _paths;

        public void RememberPath(string path)
        {
            _paths.Add(path);
            Save();
        }

        public void ForgetPath(string path)
        {
            _paths.Remove(path);
            Save();
        }

        private string _lastPath;

        public string LastPath
        {
            get => _lastPath;
            set
            {
                _lastPath = value;
                Save();
            }
        }

        public Config()
        {
            if (Instance is null) Instance = this;
            else return;
            
            _paths = new HashSet<string>();
            
            if(!File.Exists(ExecutionPath))
                Save();
            
            using var stream = new StreamReader(ExecutionPath);
            var json = JsonObject.Parse(stream.ReadToEnd());
            foreach (var node in json["paths"].AsArray())
            {
                _paths.Add(node.Deserialize<string>());
            }

            _lastPath = json["lastPath"].Deserialize<string>();
        }

        public void Save()
        {
            using var stream = new StreamWriter(ExecutionPath);
            var json = new JsonObject();
            var paths = new JsonArray();
            foreach (var path in _paths)
            {
                paths.Add(path);
            }

            json.Add(new KeyValuePair<string, JsonNode?>("paths", paths));
            json.Add(new KeyValuePair<string, JsonNode?>("lastPath", _lastPath));

            stream.Write(json.ToJsonString());
        }
    }
}