using System.Net.WebSockets;
using System.Net;
using System.Text;
using FunWebsiteThing.Controllers.Classes;

namespace FunWebsiteThing
{
    public static class WebSocketManager
    {
        public static string Status = String.Empty;

        private static string AccessPassword = String.Empty;
        public static async Task Start()
        {
            AccessPassword = Password.GeneratePassword();
            // Start Server
            var hl = new HttpListener();
            hl.Prefixes.Add("http://localhost:5000/");
            hl.Start();
            Console.WriteLine("WebSocket Server Started!");

            while (2 + 2 == 4) // while as long the program runs
            {
                var context = await hl.GetContextAsync();
                if (context.Request.IsWebSocketRequest) // if the request is a websocket
                {
                    var websocketcontext = await context.AcceptWebSocketAsync(null); // accept it
                    websocketcontext.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes((Status == "" ? "" : Status))), WebSocketMessageType.Text, true, CancellationToken.None); // send the websocket a message
                    await HandleWebSocket(websocketcontext.WebSocket); // await responses from the socket again
                }
                else // if not, invalid request
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        private static async Task HandleWebSocket(WebSocket ws)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024]); // create a buffer, this will contain the data the websocket sends
                while (ws.State == WebSocketState.Open) // while the ws is open
                {
                    // Async receive data from WebSocket ws. The data from the websocket is in buffer
                    var result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close) // if the result of this message is to close, we handle it
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None); // close the websocket
                    }
                    else // else we interpret it as a message
                    {
                        var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                        //Console.WriteLine($"Message Received: {message}");
                        if (message.Contains(AccessPassword)) // if the request message contains the password
                        {
                            int index = message.IndexOf(AccessPassword)-1; // get the index of where AccessPassword starts - 1 because of the space
                            message = message.Remove(index, AccessPassword.Length + 1); // make the new message that, but removed
                            if (message == "clear")
                            {
                                Status = String.Empty;
                                message = "";
                            }
                            else
                            {
                                Status = message;
                            }
                        }
                        await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes((Status == "" ? "" : Status))), WebSocketMessageType.Text, true, CancellationToken.None); // send the websocket a message
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
