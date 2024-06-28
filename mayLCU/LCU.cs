using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using WebSocketSharp;



namespace mayLCU {
    public class LCU {
        public bool IsConnected { get; private set; } = false;
        public string Target { get => _target.ProcessName.Substring(_target.ProcessName.Length - 2, 2) == "Ux" ? _target.ProcessName.Substring(0, _target.ProcessName.Length - 2) : _target.ProcessName; } //removing "Ux" from processName
        private Target _target;
        private HttpClient httpClient;
        private Process hookedProcess;
        private string credentialsAuth;
        private string credentialsPort;

        #region CONSTRUCTORS
        public static LCU HookRiotClient() {
            return new LCU(mayLCU.Target.RiotClient());
        }
        public static LCU HookLeagueClient() {
            return new LCU(mayLCU.Target.LeagueClient());
        }
        public static LCU HookLeagueStore(LCU leagueClient) {
                var accountId = leagueClient.RequestDynamic("/lol-summoner/v1/current-summoner").accountId;
                var token = leagueClient.RequestDynamic("/lol-rso-auth/v1/authorization/access-token").token;
                var storeUrl = leagueClient.Request("/lol-store/v1/getStoreUrl").Replace("\"", "");

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(storeUrl);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
                //if (forPurchase) httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) LeagueOfLegendsClient/11.16.390.1945 (CEF 74) Safari/537.36");
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LeagueOfLegendsClient/11.17.393.0607 (CEF 91)");

                return new LCU(httpClient);
        }

        private LCU(Target target) {
            this._target = target;

            Assure(InitHttpClient);
            Assure(Hook);
            Assure(Connect);
        }
        private LCU(HttpClient httpClient) {
            _target = mayLCU.Target.LeagueStore();
            this.httpClient = httpClient;
            credentialsAuth = httpClient.DefaultRequestHeaders.Where(x => x.Key == "Bearer").Select(x => x.Value).First().ToString();
            credentialsPort = httpClient.BaseAddress.ToString();
            IsConnected = true;
        }
        #endregion
        #region INIT LCU
        private bool Hook() {
            var process = Process.GetProcessesByName(_target.ProcessName).FirstOrDefault();
            return Hook(process);
        }
        private bool Hook(Process process) {
            if (process == null) return false;
            using (var mos = new ManagementObjectSearcher(new ObjectQuery($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))) {
                using (var moc = mos.Get()) {
                    var commandLine = (string)moc.OfType<ManagementObject>().First()["CommandLine"];
                    //try {
                        credentialsAuth = _target.AuthTokenRegex.Match(commandLine).Groups[1].Value;
                        credentialsPort = _target.PortRegex.Match(commandLine).Groups[1].Value;
                        hookedProcess = process;
                        return credentialsAuth != null && credentialsPort != null;
                    //} catch (Exception e) {
                        //throw new InvalidOperationException($"Error while trying to get the status for {_target.ProcessName}:\n{e}\n\n(CommandLine = {commandLine})");
                    //}
                }
            }
        }
        public bool UpdateCredentials(LCU otherLCU) {
            return Hook(otherLCU.hookedProcess);
        }
        private bool InitHttpClient() {
            try {
                httpClient = new HttpClient(new HttpClientHandler() {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                });
            } catch {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                httpClient = new HttpClient(new HttpClientHandler() {
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                });
            }
            return true;
        }
        private bool Connect() {
            if (httpClient == null) return false;

            var byteArray = Encoding.ASCII.GetBytes("riot:" + credentialsAuth);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            IsConnected = true;
            return true;
        }
        #endregion
        #region REQUESTS STRING, DYNAMIC, ASYNC
        public async Task<string> RequestAsync(string uri) {
            return await RequestAsync(RequestMethod.GET, uri);
        }
        public async Task<string> RequestAsync(RequestMethod requestMethod, string uri, string payload = "") {
            if (!IsConnected) return "ERR: Not Connected to LCU";

            if (uri[0] != '/') uri = '/' + uri;
            var fullUri = $"https://127.0.0.1:{credentialsPort}{uri}";
            var httpMethod = new HttpMethod(requestMethod.ToString());
            var httpPayload = new StringContent(payload, Encoding.UTF8, "application/json");
            var httpRequest = new HttpRequestMessage(httpMethod, fullUri) {
                Content = httpPayload
            };
            var httpResponse = await httpClient.SendAsync(httpRequest);
            return await httpResponse.Content.ReadAsStringAsync();
        }

        public async Task<dynamic> RequestDynamicAsync(string uri) {
            return JsonConvert.DeserializeObject<dynamic>(await RequestAsync(uri));
        }
        public async Task<dynamic> RequestDynamicAsync(RequestMethod requestMethod, string uri, string payload = "") {
            return JsonConvert.DeserializeObject<dynamic>(await RequestAsync(uri));
        }


        public string Request(string uri) {
            return RequestAsync(uri).GetAwaiter().GetResult();
        }
        public string Request(RequestMethod requestMethod, string uri, string payload = "") {
            return RequestAsync(requestMethod, uri, payload).GetAwaiter().GetResult();
        }

        public dynamic RequestDynamic(string uri) {
            return RequestDynamicAsync(uri).GetAwaiter().GetResult();
        }
        public dynamic RequestDynamic(RequestMethod requestMethod, string uri, string payload = "") {
            return RequestDynamicAsync(requestMethod, uri, payload).GetAwaiter().GetResult();
        }
        #endregion

        private void Assure(Func<bool> function) {
            while (!function()) { }
            return;
        }
    }
}
