using System.Net;
using System.Text;
using NetCoreServer;
using Pokelink.Core.Proto.V2;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

public class Client : TcpClient {
    private static readonly byte[] EomArray = "<EOM>"u8.ToArray();

    private static readonly byte[] Nullarray = "\0\0\0\0"u8.ToArray();

    /// <inheritdoc />
    public Client(string address, int port) : base(address, port) { }

    /// <inheritdoc />
    protected override void OnConnected() {
        base.OnConnected();
        ReceiveAsync();
        Send(
            "{\"handshake\": { \"version\": 2, \"client\": \"WebSource\", \"dataType\": \"Protobuf\", \"gzip\": false }}");
    }

    /// <inheritdoc />
    protected override void OnReceived(byte[] buffer, long offset, long size) {
        var eomIndex = buffer.IndexOf(EomArray);

        while (eomIndex != -1) {
            var data = buffer[..eomIndex];

            var eop = data.IndexOf(Nullarray) + 1;

            Base? message;

            try {
                message = Base.Parser.ParseFrom(eop == 0 ? data : data[..eop]);
            } catch {
                // ignore
                if (eomIndex + 5 >= buffer.Length) {
                    break;
                }
                
                buffer = buffer[(eomIndex + 5)..];
                eomIndex = buffer.IndexOf(EomArray);
                continue;
            }

            if (message != null) {
                switch (message.Channel) {
                    case "client:party:updated":
                        Party.Update(V2_Party.Parser.ParseFrom(eop == 0 ? data : data[..eop]));
                        break;
                    case "client:settings:updated":
                        var settings = Settings.Parser.ParseFrom(eop == 0 ? data : data[..eop]);
                        if (settings.Data.HasSpriteTemplate) {
                            SpriteTemplate.RegisterTemplate(settings.Data.SpriteTemplate);
                        }

                        break;
                }
            }

            if (eomIndex + 5 >= buffer.Length) {
                break;
            }

            buffer = buffer[(eomIndex + 5)..];
            eomIndex = buffer.IndexOf(EomArray);
        }
    }
}
