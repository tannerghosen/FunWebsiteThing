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
                if (context.Request.IsWebSocketRequest) // if the request is a websocket
                {
                    var websocketcontext = await context.AcceptWebSocketAsync(null); // accept it
                    // What we are encoding below before sending is our Status, which if blank, we just send an empty string, else we send status as it likely changed
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

                        if (message.Contains(AccessPassword))
                        {
                            int index = message.IndexOf(AccessPassword); // get the index of where AccessPassword starts
                            index = index - 1; // as we include the space in the removal, the index is 1 backwards from where AccessPassword starts
                            message = message.Remove(index, AccessPassword.Length + 1); // remove from the index to the end of the accesspassword length's + 1.
                            if (message == "clear")
                            {
                                Status = String.Empty;
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
