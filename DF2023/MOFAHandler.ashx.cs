using DF2023.Core;
using DF2023.GraphQL;
using DF2023.GraphQL.Handlers;
using GraphQL;
using GraphQL.Transport;
using GraphQLParser.AST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using Telerik.Microsoft.Practices.Unity.Utility;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Events;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Personalization.Impl.Web.Services.ViewModel;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Configuration;
using Telerik.Sitefinity.Web.Events;
using Telerik.Web.UI;

namespace DF2023
{
    //## The EchoHandler class
    // The most low-level way to utilize the new WebSocket features in ASP.NET is to implement your own IHttpHandler.
    public class MOFAHandler : IHttpHandler
    {
        public delegate void EventToBroadcast(JObject eventData);

        public static event EventToBroadcast OnEventToBroadcast;

        public static List<MOFAHandler> instances = new List<MOFAHandler>();

        public WebSocket Socket = null;

        private UserViewModel _loggedUser = null;

        public UserViewModel LoggedInUser
        {
            get
            {
                return this._loggedUser;
            }
            private set
            {
                this._loggedUser = value;
            }
        }

        public HttpContextBase SavedContext { get; set; }

        public string OperationId
        {
            get; set;
        }

        /// <summary>
        /// The Websocket key.
        /// </summary>
        public string Key
        {
            get; set;
        }

        public static StringBuilder InternalLog
        {
            get; set;
        }

        public MOFAHandler()
        {
            InternalLog = new StringBuilder();
        }

        public HttpContext InitialContext { get; set; }

        public static void Content_Action(IDynamicContentEvent @event, DynamicContent dynamicContentItem)
        {
            IDataItem masterItem = null;
            if (dynamicContentItem.Status != ContentLifecycleStatus.Live)
            {
                return;
            }

            try
            {
                var manager = DynamicModuleManager.GetManager();
                manager.Provider.SuppressSecurityChecks = true;
                masterItem = DynamicModuleManager.GetManager().Lifecycle.GetMaster(dynamicContentItem);

                ObservableFactory.PublishUpdate(dynamicContentItem.GetType().FullName,
                    dynamicContentItem,
                    ((DynamicContentEventBase)(@event)).Action);

                manager.Provider.SuppressSecurityChecks = false;
            }
            catch (Exception ex)
            {
                Log.Write(new Exception("[WEBSOCKETS] Error trying to stream change.", ex));
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest && LoadUser(context))
            {
                InternalLog = InternalLog.AppendLine("Connection received");
                this.InitialContext = context;
                context.AcceptWebSocketRequest(HandleWebSocket, new AspNetWebSocketOptions
                {
                    SubProtocol = "graphql-transport-ws"
                });
                InternalLog = InternalLog.AppendLine("Socket accepted, protocol is graphql-transport-ws");
            }
            else
            {
                InternalLog = InternalLog.AppendLine("Sending 400 error.");
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Not Authorized.";
            }
        }

        private bool LoadUser(HttpContext context)
        {
            try
            {
                var userIdentity = ClaimsManager.GetCurrentIdentity();
                var user = UserManager.GetManager(userIdentity.MembershipProvider).GetUser(userIdentity.UserId);
                Guard.ArgumentNotNull(user, nameof(user));

                this.LoggedInUser = new UserViewModel(user);
                InternalLog = InternalLog.AppendLine("User loaded.");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task HandleWebSocket(WebSocketContext wsContext)
        {
            const int maxMessageSize = 32768;
            byte[] receiveBuffer = new byte[maxMessageSize];
            Socket = wsContext.WebSocket;

            InternalLog = InternalLog.AppendLine("Handling socket.");

            this.Key = wsContext.Headers["Sec-WebSocket-Key"];

            InternalLog = InternalLog.AppendLine($"Key is {this.Key}");

            //OnEventToBroadcast += MOFAHandler_OnEventToBroadcast;

            InternalLog = InternalLog.AppendLine("Cleaning existing instances.");
            instances.RemoveAll(itm => itm.Key == this.Key);

            InternalLog = InternalLog.AppendLine("Adding the client.");
            instances.Add(this);

            // While the WebSocket connection remains open we run a simple loop that receives messages and then sends them back.
            while (Socket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    InternalLog = InternalLog = InternalLog.AppendLine($"Received data {receiveResult.Count}");

                    int count = receiveResult.Count;
                    while (!receiveResult.EndOfMessage)
                    {
                        if (count >= maxMessageSize)
                        {
                            string closeMessage = string.Format("Maximum message size: {0} bytes.", maxMessageSize);
                            InternalLog = InternalLog.AppendLine($"Closing socket with {closeMessage}.");
                            await Socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, closeMessage, CancellationToken.None);
                            return;
                        }

                        InternalLog = InternalLog.AppendLine($"Reading ...");

                        receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, count, maxMessageSize - count), CancellationToken.None);

                        InternalLog = InternalLog.AppendLine($"Received successfully {receiveResult}.");

                        count += receiveResult.Count;
                    }

                    var receivedString = Encoding.UTF8.GetString(receiveBuffer, 0, count);

                    InternalLog = InternalLog.AppendLine($"String is: {receivedString}.");

                    if (!string.IsNullOrWhiteSpace(receivedString))
                    {
                        InternalLog = InternalLog.AppendLine($"Parsing JObject: {receivedString}.");
                        var message = JObject.Parse(receivedString);

                        InternalLog = InternalLog.AppendLine($"Mesage is: {message}.");

                        string operationId = "";

                        switch (message["type"].Value<string>())
                        {
                            case "connection_init":
                                InternalLog = InternalLog.AppendLine($"Sending ack for connection_init.");
                                await ServerAck();
                                break;

                            case "connection_terminate":
                                InternalLog = InternalLog.AppendLine($"Client is terminating ...");
                                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested termination", CancellationToken.None);
                                break;

                            case "start":
                                InternalLog = InternalLog.AppendLine($"Start command ...");

                                operationId = message["id"].Value<string>();
                                var payload = message["payload"].ToObject<GraphQLRequest>();

                                string startQuery = message["payload"]["query"].Value<string>();
                                OperationId = operationId;

                                var docStart = GraphQLParser.Parser.Parse(startQuery);

                                await ServerAck();
                                break;

                            case "subscribe":

                                InternalLog = InternalLog.AppendLine($"Start command ...");

                                operationId = message["id"].Value<string>();
                                var payloadSubscribe = message["payload"].ToString();

                                var options = new ExecutionOptions
                                {
                                    Query = message["payload"]["query"]?.ToString(),
                                    Schema = MySchemaBuilder.GeneratedSchema,
                                };

                                var doc = GraphQLParser.Parser.Parse(message["payload"]["query"]?.ToString());

                                var definition = ((GraphQLOperationDefinition)(doc.Definitions.FirstOrDefault())).SelectionSet;
                                var selections = definition.Selections;

                                var executer = new DocumentExecuter();
                                var executionResult = await executer
                                    .ExecuteAsync(options)
                                    .ConfigureAwait(false);

                                SubscribeAllFieldsInSelection(message, operationId, selections);

                                break;

                            case "stop":
                                var stopOperationId = message["id"].Value<string>();
                                // ...
                                break;

                            case "complete":
                                var completeOperationId = message["id"].Value<string>();
                                break;

                            default:
                                // Unrecognized message type.
                                //Log.Write(new Exception($"[WEBSOCKETS] UNKNOWN DATA:'{receivedString}'"));
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(new Exception($"[WEBSOCKETS]\n{PrintHeaders(InitialContext.Request.Headers)}\nLog so far: {InternalLog.ToString()}", ex), ConfigurationPolicy.ErrorLog);
                }
            }
        }

        private void SubscribeAllFieldsInSelection(JObject message, string operationId, List<ASTNode> selections)
        {
            foreach (GraphQLField selection in selections)
            {
                var type = GetTypeNameDynamically(selection.Name.StringValue);

                if (!string.IsNullOrWhiteSpace(type))
                {
                    var subscription = ObservableFactory.GetObservable(null, type).Subscribe(async args =>
                    {
                        var updatedDataJson = "";
                        var argsDictionary = args as Dictionary<string, object>;

                        var dynamicContent = (DynamicContent)argsDictionary["Item"];
                        var action = argsDictionary["Action"];

                        string permissionsSetName = "General";
                        SecurityConfig secConfig = Config.Get<SecurityConfig>();

                        // get the permissions set
                        Permission generalPermSet = secConfig.Permissions[permissionsSetName];
                        int actionsMask = generalPermSet.Actions["View"].Value;
                        Guid principalId = LoggedInUser.Id;
                        List<Guid> principals = new List<Guid> { principalId };

                        var identity = ClaimsManager.GetCurrentIdentity();

                        // Everyone is static role so a constant value is OK,
                        // We may want to improve this code later though to follow
                        // best practices and get the role properly.
                        Guid EveryoneRoleId = new Guid("FD58B53E-F894-42F9-BC74-E6013D95A246");
                        principals.Add(EveryoneRoleId);

                        principals.AddRange(LoggedInUser.Roles.Select(r => r.Id));

                        var isGranted = dynamicContent.IsGranted(permissionsSetName, principals.ToArray(), actionsMask);

                        if (isGranted)
                        {
                            var docQuery = GraphQLParser.Parser.Parse(message["payload"]["query"]?.ToString());

                            var fields = ((GraphQLField)((GraphQLExecutableDefinition)docQuery.Definitions.FirstOrDefault()).SelectionSet.Selections.FirstOrDefault()).SelectionSet.Selections;
                            Dictionary<string, object> values = new Dictionary<string, object>();
                            var subscriptionName = selection.Alias != null ? selection.Alias.Name.StringValue : selection.Name.StringValue;

                            foreach (var field in fields)
                            {
                                var gqField = (GraphQLField)field;

                                if (!gqField.Name.StringValue.StartsWith("__"))
                                {
                                    var sfName = gqField.Name.StringValue[0].ToString().ToUpper() + gqField.Name.StringValue.Substring(1);
                                    if (dynamicContent.DoesFieldExist(sfName))
                                    {
                                        var value = dynamicContent.GetValue(sfName);
                                        if (value?.GetType().FullName == typeof(Lstring).FullName)
                                        {
                                            values.Add(gqField.Name.StringValue, value.ToString());
                                        }
                                        else
                                        {
                                            values.Add(gqField.Name.StringValue, value);
                                        }
                                    }
                                    else if (sfName == "CreatedAt")
                                    {
                                        if (dynamicContent != null)
                                        {
                                            var createdAt = dynamicContent.DateCreated;
                                            values.Add(gqField.Name.StringValue, createdAt);
                                        }
                                    }
                                    else if (sfName == "Username")
                                    {
                                        var profile = Core.Extensions.UserExtensions.GetUserProfile(dynamicContent);
                                        if (profile != null)
                                        {
                                            values.Add(gqField.Name.StringValue, profile.Nickname);
                                        }
                                    }
                                    else if (sfName == "Avatar")
                                    {
                                        var avatar =Core.Extensions.UserExtensions.GetUserAvatarURL(dynamicContent);
                                        values.Add(gqField.Name.StringValue, avatar);
                                    }
                                    else if (sfName == "UserId")
                                    {
                                        values.Add(gqField.Name.StringValue, dynamicContent.Owner);
                                    }
                                }
                            }

                            values.Add("__typename", subscriptionName);
                            values.Add("action", action);

                            var ackMessage = new
                            {
                                type = "next",
                                id = operationId,
                                payload = new
                                {
                                    data = new Dictionary<string, object>
                                    {
                                        [subscriptionName] = values
                                    },
                                    errors = new object[0]
                                }
                            };

                            updatedDataJson = JsonConvert.SerializeObject(ackMessage);
                            try
                            {
                                await SendMessage(updatedDataJson);
                            }
                            catch
                            {
                                Log.Write("Error sending message to client");
                            }
                        }
                    });
                    if (selection.SelectionSet?.Selections != null)
                    {
                        foreach (GraphQLField field in selection.SelectionSet?.Selections)
                        {
                            if (field.SelectionSet?.Selections != null)
                            {
                                SubscribeAllFieldsInSelection(message, operationId, selection.SelectionSet?.Selections);
                            }
                        }
                    }
                }
            }
        }

        private string GetTypeNameDynamically(string typeFieldName)
        {
            try
            {
                var name = typeFieldName.Replace("Subscription", "");
                if (name.StartsWith("child"))
                {
                    name = name.Substring(5);
                }

                var typeFound = FieldHandlers.SitefinityMetaTypes.FirstOrDefault(t => t.ClassName.ToLower() == name.ToLower());
                return typeFound.FullTypeName;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task ServerAck()
        {
            await SendMessage(JsonConvert.SerializeObject(new { type = "connection_ack" }));
        }

        private string PrintHeaders(NameValueCollection headers)
        {
            string result = "";
            foreach (var header in headers)
            {
                result += $"{header?.ToString()} : {headers[header?.ToString()]}\n";
            }

            return result;
        }

        //public static async void MOFAHandler_OnEventToBroadcast(JObject eventData)
        //{
        //    foreach (var client in new List<MOFAHandler>(instances))
        //    {
        //        try
        //        {
        //            if (client != null && client.Socket != null && client.Socket.State == WebSocketState.Open)
        //            {
        //                try
        //                {
        //                    var ackMessage = new
        //                    {
        //                        type = "next",
        //                        id = client.OperationId,
        //                        payload = new
        //                        {
        //                            data = new
        //                            {
        //                                MeetingSubscription = new
        //                                {
        //                                    meetingSubscription = new
        //                                    {
        //                                        __typename = eventData["__typename"],
        //                                        operation = eventData["Action"],
        //                                        masterId = eventData["MasterId"],
        //                                        liveId = eventData["ItemId"]
        //                                    }
        //                                }
        //                            },
        //                            errors = new object[0]
        //                        }
        //                    };

        //                    await client.SendMessage(JsonConvert.SerializeObject(ackMessage));
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Write(new Exception($"[WEBSOCKETS] Error sending to client.\nLog so far:{InternalLog.ToString()}", ex));
        //                }
        //            }
        //            else
        //            {
        //                Log.Write(new Exception($"[WEBSOCKETs] - Client to write to may not be properly setup, hadve disconnected or is unstable.Log so far {InternalLog.ToString()}"));
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write(new Exception($"[WEBSOCKETs] Error.\nLog so far:{InternalLog.ToString()}", ex));
        //            instances.Remove(client);
        //        }
        //    }
        //}

        private async Task SendMessage(string message)
        {
            ArraySegment<byte> outputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            if (Socket != null && Socket.State == WebSocketState.Open)
            {
                InternalLog = InternalLog.AppendLine($"Sending message {message}");
                await Socket.SendAsync(outputBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public static async void HandleLogout(ILogoutCompletedEvent evt)
        {
            Guid userId = new Guid();
            if (evt != null && !string.IsNullOrWhiteSpace(evt.UserId) && Guid.TryParse(evt.UserId, out userId))
            {
                List<MOFAHandler> subscriptionsToRemove = new List<MOFAHandler>();
                foreach (var subscription in instances)
                {
                    if (subscription.LoggedInUser != null && subscription.LoggedInUser.Id == userId)
                    {
                        try
                        {
                            subscriptionsToRemove.Add(subscription);
                            CancellationToken token = new CancellationToken(false);
                            await subscription.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User logged out.", token);
                        }
                        catch (Exception ex)
                        {
                            // We try to signal the socket first.
                            // But even if this fails - we still want to proceed and remove the session.
                        }
                    }
                }

                foreach (var subscription in subscriptionsToRemove)
                {
                    instances.Remove(subscription);
                }
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }

    public class UserViewModel
    {
        public string Email { get; set; }
        public Guid Id { get; set; }

        public string ProviderName { get; set; }

        public List<RoleViewModel> Roles
        {
            get; set;
        }

        public UserViewModel(Telerik.Sitefinity.Security.Model.User user)
        {
            this.Roles = new List<RoleViewModel>();

            this.Email = user.Email;
            this.Id = user.Id;
            this.ProviderName = user.ProviderName;

            this.Roles.AddRange(RoleManager.GetManager().GetRolesForUser(user.Id).ToList().Select(r => new RoleViewModel()
            {
                Id = r.Id,
                Title = r.Name
            }));
        }
    }
}