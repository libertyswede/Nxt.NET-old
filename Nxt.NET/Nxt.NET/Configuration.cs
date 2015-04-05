using System;
using System.Configuration;
using NLog;

namespace Nxt.NET
{
    public interface IConfiguration
    {
        bool IsTestnet { get; }
        int MaxNumberOfConnectedPublicPeers { get; }
        bool ForceValidate { get; }
        int TaskSleepInterval { get; }
    }

    class Configuration : IConfiguration
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool? _isTestnet;
        public bool IsTestnet
        {
            get
            {
                if (!_isTestnet.HasValue)
                    _isTestnet = GetBool("isTestnet");
                return _isTestnet.Value;
            }
        }

        private int? _maxNumberOfConnectedPublicPeers;
        public int MaxNumberOfConnectedPublicPeers
        {
            get
            {
                if (!_maxNumberOfConnectedPublicPeers.HasValue)
                    _maxNumberOfConnectedPublicPeers = GetInt("maxNumberOfConnectedPublicPeers");
                return _maxNumberOfConnectedPublicPeers.Value;
            }
        }

        private bool? _forceValidate;
        public bool ForceValidate
        {
            get
            {
                if (!_forceValidate.HasValue)
                    _forceValidate = GetBool("forceValidate");
                return _forceValidate.Value;
            }
        }

        public int TaskSleepInterval { get { return 5000; } }

        private static int GetInt(string key)
        {
            int value;
            var success = Int32.TryParse(ConfigurationManager.AppSettings[key], out value);
            Log(key, success, value);
            return value;
        }

        private static bool GetBool(string key)
        {
            bool value;
            var success = Boolean.TryParse(ConfigurationManager.AppSettings[key], out value);
            Log(key, success, value);
            return value;
        }

        private static void Log<T>(string key, bool success, T value)
        {
            if (success)
                Logger.Info(string.Format("{0} = \"{1}\"", key, value));
            else
                Logger.Warn(string.Format("{0} not defined, assuming {1}", key, default(T)));
        }
    }
}
