using DF2023.Core;
using DF2023.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules.Events;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Security.Sanitizers;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.Events;

namespace DF2023
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Bootstrapper.Bootstrapped += OnBootstrapped;
            ObjectFactory.Initialized += this.ObjectFactory_Initialized;
        }

        private void OnBootstrapped(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(RegisterGraphQLMutationsController);
            EventHub.Subscribe<IDynamicContentCreatedEvent>(evt => DynamicContentCreatedEventHandler(evt));
            EventHub.Subscribe<IDynamicContentUpdatedEvent>(evt => DynamicContentUpdatedEventHandler(evt));
            EventHub.Subscribe<IDynamicContentDeletingEvent>(evt => DynamicContentDeletingEventHandler(evt));

            EventHub.Subscribe<ILogoutCompletedEvent>(evt => LogoutEvent(evt));
        }

        private void DynamicContentCreatedEventHandler(IDynamicContentCreatedEvent eventInfo)
        {

            MOFAHandler.Content_Action(eventInfo, eventInfo.Item);
        }

        private void DynamicContentDeletingEventHandler(IDynamicContentDeletingEvent eventInfo)
        {
            MOFAHandler.Content_Action(eventInfo, eventInfo.Item);
        }

        private void DynamicContentUpdatedEventHandler(IDynamicContentUpdatedEvent eventInfo)
        {
            MOFAHandler.Content_Action(eventInfo, eventInfo.Item);
        }

        private void LogoutEvent(ILogoutCompletedEvent evt)
        {
            if (evt != null)
            {
                MOFAHandler.HandleLogout(evt);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Bootstrapper.IsReady && !SystemManager.Initializing)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["token"]))
                {
                    string token = HttpContext.Current.Request.QueryString["token"];

                    HttpContext.Current.Request.Headers.Add("Authorization", $"Bearer {token}");
                }

                this.AddCorsRules();
            }

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE");
                // The following line solves the error message
                // If any http headers are shown in preflight error in browser console add them below
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", $"Content-Type, Accept, Pragma, Cache-Control, Authorization, ExternalApiRequest");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }

        private void AddCorsRules()
        {
            var allowOriginUrls = new List<string>()
            {
                "http://fcal.sitefinityapps.com",
                "https://localhost",
                "http://localhost:20090" ,
                "ecfltter.mofa.gov.qa",
                "ec",
                "http://ec",
                "https://ecfltter.mofa.gov.qa",
                "https://2fa.mofa.gov.qa"
            };

            this.Response.Headers.Remove("Access-Control-Allow-Origin");
            this.Response.AddHeader("Access-Control-Allow-Origin", "*");
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }

        private void ObjectFactory_Initialized(object sender, ExecutedEventArgs e)
        {
            if (e.CommandName == "RegisterCommonTypes")
            {
                ObjectFactory.Container.RegisterType<IHtmlSanitizer>(new ContainerControlledLifetimeManager());
            }
        }

        public static void RegisterGraphQLMutationsController(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
            name: "AppleAppSiteAssociation",
            routeTemplate: ".well-known/apple-app-site-association",
            defaults: new { controller = "FidoLogin", action = "AppleAppSiteAssociation" }
        );



            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "graphqllayer/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            config.EnsureInitialized();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data"));
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Routes.MapHttpRoute(
            "DefaultApiCP",
            "apicustom/{controller}/{action}/{id}",
            new { id = RouteParameter.Optional });
        }
    }
}