using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mayLCU {
    public class Target {
        public string ProcessName { get; init; }
        public Regex AuthTokenRegex { get; init; }
        public Regex PortRegex { get; init; }
        private Target(string processName, Regex authTokenRegex, Regex portRegex) => (ProcessName, AuthTokenRegex, PortRegex) = (processName, AuthTokenRegex, PortRegex);
        public static Target RiotClient() {
            return new Target("RiotClientUx", new Regex("--riotclient-auth-token=(.+?)[\\\"\\s]"), new Regex("--riotclient-app-port=(\\d+)"));
        }
        public static Target LeagueClient() {
            return new Target("LeagueClientUx", new Regex("--remoting-auth-token=(.+?)[\\\"\\s]"), new Regex("--app-port=(\\d+)"));
        }
    }
}
