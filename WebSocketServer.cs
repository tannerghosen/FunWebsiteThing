using System.Net.WebSockets;
using System.Net;
using System.Text;
using FunWebsiteThing.Controllers.Classes;

namespace FunWebsiteThing
{
    public static class WebSocketServer
    {
        public static string Status = String.Empty;

        private static string AccessPassword = String.Empty;

        public static async Task Start()
        {
            AccessPassword = Password.GeneratePassword(); // generate a password needed to actually send new updates to the websocket server (can be get'd)
            // Start Server
            var hl = new HttpListener();
            hl.Prefixes.Add("http://"+Globals.DomainName+":5000/");
            hl.Start();
            Console.WriteLine("WebSocket Server Started!");

            while (2 + 2 == 4) // while as long the program runs
            {
                var context = await hl.GetContextAsync();
                if (context.Request.IsWebSocketRequest) // if the request is to the websocket
                {
                    var websocketcontext = await context.AcceptWebSocketAsync(null); // accept the request and make a websocket
                    websocketcontext.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes((Status == "" ? "" : Status))), WebSocketMessageType.Text, true, CancellationToken.None); // this is sent to anyone listening into this websocket
                    await HandleWebSocket(websocketcontext.WebSocket); // this method is always called, but it handles the websocket request further if it has more to offer, i.e. an encoded message
                }
                else // if not, invalid request
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        // Handles the websocket request further
        private static async Task HandleWebSocket(WebSocket ws)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024]); // create a buffer, this will contain the data we send to the websocket
                while (ws.State == WebSocketState.Open) // while the ws is open
                {
                    var result = await ws.ReceiveAsync(buffer, CancellationToken.None); // result is the data in the websocket, i.e. the encoded message
                    var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count); // this is the encoded message decoded
                    if (message.Contains(AccessPassword)) // did the client provide the access password?
                    {
                        // Good, that means they're allowed to change this.
                        int index = message.IndexOf(AccessPassword) - 1; // get the index of where AccessPassword starts - 1 (we include the space) in the message
                        message = message.Remove(index, AccessPassword.Length + 1); // remove from the index to the end of the accesspassword length's + 1. (again, we include the space)
                        if (message == "clear")  // if the message is clear, reset it back to an empty string
                        {
                            Status = String.Empty;
                        }
                        else // update status
                        {
                            Status = message;
                        }
                        await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes((Status == "" ? "" : Status))), WebSocketMessageType.Text, true, CancellationToken.None);  // update everyone listening's Status
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static string GetAccessPassword()
        {
            return AccessPassword;
        }
    }
}
