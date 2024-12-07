using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionTaskTracker.Model.Configuration;

namespace VersionTaskTracker.Services
{
    public class VTTEnvironment
    {
        public const string CONFIG_FILE_NAME = "config.json";

        private VTTConfig _config;
        public VTTConfig Config {  get { return _config; } }

        private VTTEnvironment(VTTConfig config)
        {
            _config = config;
        }

        public static VTTEnvironment Setup(string path)
        {
            VTTEnvironment? env = LoadEnvironment(path);

            if (env == null) return InitializeNewEnvironment(path);
            else return env;
        }

        private static VTTEnvironment InitializeNewEnvironment(string path)
        {
            VTTEnvironment env = new VTTEnvironment(new VTTConfig());

            SaveEnvironment(path,env);

            return env;
        }

        private static VTTEnvironment? LoadEnvironment(string path)
        {
            string configPath = Path.Combine(path, CONFIG_FILE_NAME);

            if (!File.Exists(configPath)) return null;

            VTTConfig config = JObject.Parse(File.ReadAllText(configPath)).ToObject<VTTConfig>()!;

            return new VTTEnvironment(config);
        }

        private static void SaveEnvironment(string path,VTTEnvironment env)
        {
            string configPath = Path.Combine(path, CONFIG_FILE_NAME);

            File.WriteAllText(configPath,JObject.FromObject(env._config).ToString());
        }
    }
}
