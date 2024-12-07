using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionTaskTracker.Model.FileSystem;

namespace VersionTaskTracker.Model.Configuration
{
    public class VTTInstanceConfig : IStorable<VTTInstanceConfig>
    {
        [JsonIgnore]
        private IDirectoryInfo _workingDir;
        [JsonIgnore]
        private VTTConfig _config;
        public VTTInstanceConfig(VTTConfig config,IDirectoryInfo workingDir) { 
            this._config = config;
            _workingDir = workingDir;
        }

        public string? VTTInstanceDirName { get; set; } = ".vtt";
        public string? VTTInstanceTasksDatabaseName { get; set; } = "tasks.db";
        public string? VTTInstanceIgnoreFile { get; set; } = ".vttignore";
        public string? VTTInstanceMetadataFile { get; set; } = "metadata.json";
        public string? VTTInstanceVersion { get; set; } = string.Empty;

        public void Load()
        {
            string VTTInstanceConfigPath = Path.Combine(this._workingDir.FullName, this._config.VTTInstanceConfigPath);

            VTTInstanceConfig iconfig = JObject.Parse(File.ReadAllText(VTTInstanceConfigPath)).ToObject<VTTInstanceConfig>()!;

            VTTInstanceDirName = iconfig.VTTInstanceDirName;
            VTTInstanceTasksDatabaseName = iconfig.VTTInstanceTasksDatabaseName;
            VTTInstanceIgnoreFile = iconfig.VTTInstanceIgnoreFile;
            VTTInstanceMetadataFile = iconfig.VTTInstanceMetadataFile;
            VTTInstanceVersion = iconfig.VTTInstanceVersion;
        }

        public void Save()
        {
            string VTTInstanceConfigPath = Path.Combine(this._workingDir.FullName, this._config.VTTInstanceConfigPath);

            File.WriteAllText(VTTInstanceConfigPath, JObject.FromObject(this).ToString());
        }
    }
}
