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
using Telerik.Sitefinity.Versioning;

namespace DF2023.ImportRelatedData
{
    public partial class Countries : System.Web.UI.Page
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

        protected void ImportCountries_Click(object sender, EventArgs e)
        {
            var countries = JsonSerializer.Deserialize<List<Country>>(CountryListAsJson.Countries);
            foreach (var country in countries)
            {
                string flag_path = Server.MapPath($"~/ImportRelatedData/flags-svg/{country.alpha2_code}.svg");
                Literal1.Text += $"{flag_path} -- {country.english_name} ({country.alpha2_code}) <br/><br/>";

                country.flag_svg = ReadSvgFromFile(flag_path, country);

                CreateCountry(country);
            }
        }

        private void CreateCountry(Country country)
        {
            // Set the provider name for the DynamicModuleManager here. All available providers are listed in
            // Administration -> Settings -> Advanced -> DynamicModules -> Providers
            var providerName = String.Empty;

            // Set a transaction name and get the version manager
            var transactionName = Guid.NewGuid().ToString();
            var versionManager = VersionManager.GetManager(null, transactionName);

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName, transactionName);
            Type countryType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Countries.Country");
            DynamicContent countryItem = dynamicModuleManager.CreateDataItem(countryType);

            // This is how values for the properties are set
            countryItem.SetValue("Title", country.english_name);
            countryItem.SetValue("TitleAr", country.arabic_name);
            countryItem.SetValue("Alpha2", country.alpha2_code);
            //countryItem.SetValue("IsArabic", true);
            countryItem.SetValue("Alpha3", country.alpha3_code);
            countryItem.SetValue("CountryCode", country.phone_code);
            countryItem.SetValue("FlagSVG", country.flag_svg);

            // Get related item manager
            //LibrariesManager flagManager = LibrariesManager.GetManager();
            //var flagItem = flagManager.GetImages().FirstOrDefault(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);
            //if (flagItem != null)
            //{
            //    // This is how we relate an item
            //    countryItem.CreateRelation(flagItem, "Flag");
            //}

            countryItem.SetString("UrlName", $"{country.english_name}-{Guid.NewGuid().ToString()}");
            countryItem.SetValue("Owner", SecurityManager.GetCurrentUserId());
            countryItem.SetValue("PublicationDate", DateTime.UtcNow);

            countryItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");

            // Create a version and commit the transaction in order changes to be persisted to data store

            TransactionManager.CommitTransaction(transactionName);
        }

        public static string ReadSvgFromFile(string filePath, Country cou)
        {
            var cc = cou.english_name;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"SVG file not found at: {filePath}");
            }

            string svgContent = File.ReadAllText(filePath);
            return svgContent;
        }
    }
}