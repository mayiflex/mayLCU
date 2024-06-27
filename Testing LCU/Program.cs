using mayLCU;

namespace Testing_LCU {
    internal class Program {
        static void Main(string[] args) {
            var lcu = LCU.HookLeagueClient();
            var test = lcu.Request(RequestMethod.GET, "/lol-chat/v1/me");
            Console.WriteLine(test);
        }
    }
}