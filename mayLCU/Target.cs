using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mayLCU {
    internal class Target {
        internal string ProcessName { get; init; }
        internal Regex AuthTokenRegex { get; init; }
        internal Regex PortRegex { get; init; }
        private Target(string processName, Regex authTokenRegex, Regex portRegex) => (ProcessName, AuthTokenRegex, PortRegex) = (processName, authTokenRegex, portRegex);
        internal static Target LeagueStore() {
            return new Target("LeagueStore", null, null);
        }
        internal static Target RiotClient() {
            return new Target("LeagueClientUx", new Regex("--riotclient-auth-token=(.+?)[\\\"\\s]"), new Regex("--riotclient-app-port=(\\d+)"));
        }
        internal static Target LeagueClient() {
            return new Target("LeagueClientUx", new Regex("--remoting-auth-token=(.+?)[\\\"\\s]"), new Regex("--app-port=(\\d+)"));
        }
    }
}
