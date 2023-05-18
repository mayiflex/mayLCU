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
using WebSocketSharp;

namespace mayLCU {
    public class LCU {
        public bool IsConnected { get; private set; } = false;
        public string Targeted { get => target.ProcessName.Substring(0, target.ProcessName.Length - 2); } //removing "Ux" from processName
        private readonly Target target;
        private HttpClient httpClient;
        private string credentialsAuth;
        private string credentialsPort;

        #region CONSTRUCTORS
        public static LCU HookRiotClient() {
            return new LCU(Target.RiotClient());
        }
        public static LCU HookLeagueClient() {
            return new LCU(Target.LeagueClient());
        }
        private LCU(Target target) {
            this.target = target;

            Assure(InitHttpClient);
            Assure(GetHook);
            Assure(Connect);
        }
        #endregion
        #region INIT LCU
        private bool GetHook() {
            var process = Process.GetProcessesByName(target.ProcessName).FirstOrDefault();
            if (process == null) return false;
            using (var mos = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id.ToString())) {
                using (var moc = mos.Get()) {
                    var commandLine = (string)moc.OfType<ManagementObject>().First()["CommandLine"];
                    try {
                        credentialsAuth = target.AuthTokenRegex.Match(commandLine).Groups[1].Value;
                        credentialsPort = target.PortRegex.Match(commandLine).Groups[1].Value;
                        return credentialsAuth != null && credentialsPort != null;
                    } catch (Exception e) {
                        throw new InvalidOperationException($"Error while trying to get the status for LeagueClientUx: {e.ToString()}\n\n(CommandLine = {commandLine})");
                    }
                }
            }
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
            var httpPayload = new StringContent(payload, Encoding.UTF8, "applicaiton/json");
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
            return JsonConvert.DeserializeObject<dynamic>(RequestDynamicAsync(uri).GetAwaiter().GetResult());
        }
        public dynamic RequestDynamic(RequestMethod requestMethod, string uri, string payload = "") {
            return JsonConvert.DeserializeObject<dynamic>(RequestDynamicAsync(requestMethod, uri, payload).GetAwaiter().GetResult());
        }
        #endregion


        private void Assure(Func<bool> function) {
            while (!function()) { }
            return;
        }
    }
}
