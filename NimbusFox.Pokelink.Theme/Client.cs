using System.Diagnostics;
using System.Net;
using System.Text;
using NetCoreServer;
using Pokelink.Core.Proto.V2;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

/// <summary>
/// A TCP client that connects to the Pokelink Core server to receive real-time game data.
/// Handles the handshake, message framing (EOM splitting), and Protobuf deserialization.
/// </summary>
public class Client : TcpClient {
    /// <summary>
    /// The End-Of-Message delimiter used to separate distinct Protobuf messages in the stream.
    /// </summary>
    private static readonly byte[] EomArray = "<EOM>"u8.ToArray();

    /// <inheritdoc />
    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="address">The IP address or hostname of the Pokelink server.</param>
    /// <param name="port">The port number to connect to.</param>
    public Client(string address, int port) : base(address, port) { }

    /// <inheritdoc />
    /// <summary>
    /// Called when the client successfully connects to the server.
    /// Sends the initial JSON handshake to identify as a WebSource requesting Protobuf V2 data.
    /// </summary>
    protected override void OnConnected() {
        base.OnConnected();
        ReceiveAsync();
        Send(
            "{\"handshake\": { \"version\": 2, \"client\": \"WebSource\", \"dataType\": \"Protobuf\", \"gzip\": false }}");
    }

    /// <inheritdoc />
    /// <summary>
    /// Called when data is received from the server.
    /// Handles splitting the stream based on the &lt;EOM&gt; delimiter and parsing the resulting payloads.
    /// </summary>
    /// <param name="buffer">The buffer containing the received data.</param>
    /// <param name="offset">The offset in the buffer where data starts.</param>
    /// <param name="size">The number of bytes received.</param>
    protected override void OnReceived(byte[] buffer, long offset, long size) {
        // Search for the EOM marker to handle message framing
        var eomIndex = buffer.IndexOf(EomArray);

        while (eomIndex != -1) {
            // Extract the individual message payload
            var data = buffer[..eomIndex];

            Base? message;

            try {
                // Attempt to parse the generic base message to determine the channel/type
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
                        // Handle updates to the Pokemon party
                        V2_Party? party = null;
                        try {
                            party = V2_Party.Parser.ParseFrom(data);
                        } catch {
                            // ignore parsing failures for party data
                        }

                        if (party != null) {
                            Party.Update(party);
                        }

                        break;
                    case "client:settings:updated":
                        // Handle updates to global settings (e.g., sprite string templates)
                        var settings = Settings.Parser.ParseFrom(data);
                        if (settings.Data.HasSpriteTemplate) {
                            SpriteTemplate.RegisterTemplate(settings.Data.SpriteTemplate);
                        }

                        break;
                }
            }

            // Check if there is more data in the buffer after the current EOM
            if (eomIndex + 5 >= buffer.Length) {
                break;
            }

            // Advance the buffer to the next message
            buffer = buffer[(eomIndex + 5)..];
            eomIndex = buffer.IndexOf(EomArray);
        }
    }
}
