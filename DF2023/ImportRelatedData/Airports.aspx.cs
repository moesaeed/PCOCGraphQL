using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Web;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.ImportRelatedData
{
    public partial class Airports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var mgr = RoleManager.GetManager("AppRoles");

            var userId = SecurityManager.GetCurrentUserId();

            if (!mgr.IsUserInRole(userId, "Administrators"))
            {
                HttpContext.Current.Response.Redirect("/");
            }
        }

        protected void ImportAirports_Click(object sender, EventArgs e)
        {
            string jsonFilePath = Server.MapPath($"~/ImportRelatedData/airports.json");
            string airportsJson = File.ReadAllText(jsonFilePath);

            var airports = JsonSerializer.Deserialize<List<Airport>>(airportsJson);
            foreach (var airport in airports)
            {
                CreateAirport(airport);
            }
        }

        public void CreateAirport(Airport airport)
        {
            // Set the provider name for the DynamicModuleManager here. All available providers are listed in
            // Administration -> Settings -> Advanced -> DynamicModules -> Providers
            var providerName = String.Empty;

            // Set a transaction name and get the version manager
            var transactionName = Guid.NewGuid().ToString();

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName, transactionName);
            Type airportType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Airports.Airport");
            DynamicContent airportItem = dynamicModuleManager.CreateDataItem(airportType);

            // This is how values for the properties are set
            airportItem.SetValue("Title", airport.name);
            airportItem.SetValue("City", airport.city);
            airportItem.SetValue("Country", airport.country);
            //airportItem.SetValue("TitleAr", "Some TitleAr");
            airportItem.SetValue("IataCode", airport.iata_code);

            airportItem.SetString("UrlName", Guid.NewGuid().ToString());
            airportItem.SetValue("Owner", SecurityManager.GetCurrentUserId());
            airportItem.SetValue("PublicationDate", DateTime.UtcNow);

            airportItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");

            // Create a version and commit the transaction in order changes to be persisted to data store
            //versionManager.CreateVersion(airportItem, false);
            TransactionManager.CommitTransaction(transactionName);
        }
    }
}