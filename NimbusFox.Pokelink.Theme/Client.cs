using System.Diagnostics;
using System.Net;
using System.Text;
using NetCoreServer;
using Pokelink.Core.Proto.V2;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

public class Client : TcpClient {
    private static readonly byte[] EomArray = "<EOM>"u8.ToArray();

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

            Base? message = null;

            try {
                message = Base.Parser.ParseFrom(data);
            } catch (Exception ex) {
                if (Debugger.IsAttached) {
                    Console.Error.WriteLine(ex);
                }

                message = null;
            }

            if (message != null) {
                switch (message.Channel) {
                    case "client:party:updated":
                        V2_Party? party = null;
                        try {
                            party = V2_Party.Parser.ParseFrom(data);
                        } catch {
                            // ignore
                        }

                        if (party != null) {
                            Party.Update(party);
                        }

                        break;
                    case "client:settings:updated":
                        var settings = Settings.Parser.ParseFrom(data);
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
