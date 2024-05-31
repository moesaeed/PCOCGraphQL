using DF2023.GraphQL;
using DF2023.Mvc.Models;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using GraphQL.Validation.Complexity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Telerik.Sitefinity.GraphQL;
using Telerik.Sitefinity.Security.Claims;

namespace DF2023.Mvc.Controllers
{
    public class GraphQLMutationController : GraphQLController
    {
        private readonly IDocumentExecuter executer;
        private readonly IGraphQLSerializer serializer;

        public GraphQLMutationController()
        {
            executer = new DocumentExecuter();
            serializer = new GraphQLSerializer(indent: true);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Mutation(HttpRequestMessage request, GraphQLQuery query)
        {
            //bool result = false;
            ////HandleUnauthenticated(out result);
            //if (!result)
            //    return Forbidden(request);

            ISchema s = MySchemaBuilder.GeneratedSchema;

            Exception originalException = null;

            string queryToExecute = query.Query;
            Inputs variables = serializer.ReadNode<Inputs>(query.Variables);
            int maxDepth = 100;
            DataLoaderDocumentListener listener = new DataLoaderDocumentListener(new DataLoaderContextAccessor());

            ExecutionResult executionResult = await executer.ExecuteAsync(delegate (ExecutionOptions _)
            {
                _.Schema = s;
                _.Query = queryToExecute;
                _.OperationName = query.OperationName;
                _.Variables = variables;
                _.ComplexityConfiguration = new ComplexityConfiguration
                {
                    MaxDepth = maxDepth
                };
                _.Listeners.Add(listener);
                _.UnhandledExceptionDelegate = delegate (UnhandledExceptionContext ctx)
                {
                    if (ctx.OriginalException != null)
                    {
                        originalException = ctx.OriginalException;
                    }
                    return Task.CompletedTask;
                };
            }).ConfigureAwait(continueOnCapturedContext: false);

            HttpStatusCode httpResult = (executionResult.Executed ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            using (MemoryStream stream = new MemoryStream())
            {
                await serializer.WriteAsync(stream, executionResult);
                stream.Position = 0L;

                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = await reader.ReadToEndAsync();
                    if (originalException != null)
                    {
                        JObject originalExecutionResult = JsonConvert.DeserializeObject<JObject>(content);
                        if (originalException is NoStackTraceException)
                        {
                            content = JsonConvert.SerializeObject(originalException.Message);
                        }
                        else
                        {
                            originalExecutionResult.Add("OriginalException", $"{originalException.Message} Stack Trace: {originalException.StackTrace}");
                            content = JsonConvert.SerializeObject(originalExecutionResult);
                        }
                    }
                    HttpResponseMessage httpResponseMessage = request.CreateResponse(httpResult);
                    httpResponseMessage.Content = new StringContent(content, Encoding.UTF8, "application/graphql+json");
                    return httpResponseMessage;
                }
            }
        }

        private void HandleUnauthenticated(out bool result)
        {
            result = ClaimsManager.GetCurrentIdentity().IsAuthenticated;
            if (!result)
            {
                HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
                var response = Forbidden(httpRequestMessage);
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.StatusCode = (int)response.StatusCode;
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }
        }

        private HttpResponseMessage Forbidden(HttpRequestMessage request)
        {
            HttpStatusCode httpResult = HttpStatusCode.Forbidden;
            HttpResponseMessage httpResponseMessage = request.CreateResponse(httpResult);
            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new
            {
                Message = "You are not authorized."
            }),
                Encoding.UTF8,
                "application/graphql+json")
            {
            };
            return httpResponseMessage;
        }
    }
}