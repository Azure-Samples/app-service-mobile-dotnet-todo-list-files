using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Swagger;
using Microsoft.Azure.Mobile.Server.Tables.Config;
using MobileAppsFileSampleService.DataObjects;
using MobileAppsFileSampleService.Models;
using Owin;
using Swashbuckle.Application;

namespace MobileAppsFileSampleService
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            //config.EnableSystemDiagnosticsTracing();

            config.MapHttpAttributeRoutes();

            new MobileAppConfiguration()
                .MapApiControllers()
                .AddTables(                               // from the Tables package
                    new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework()             // from the Entity package
                    )
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new TodoItemInitializer());

            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer(null);

            //app.UseMobileAppAuthentication(config);
            app.UseWebApi(config);
            ConfigureSwagger(config);
        }

        public static void ConfigureSwagger(HttpConfiguration config)
        {
            // Use the custom ApiExplorer that applies constraints. This prevents
            // duplicate routes on /api and /tables from showing in the Swagger doc.
            config.Services.Replace(typeof(IApiExplorer), new MobileAppApiExplorer(config));
            config
               .EnableSwagger(c => {
                   c.SingleApiVersion("v1", "Azure Mobile todo list with images");

                   // Tells the Swagger doc that any MobileAppController needs a
                   // ZUMO-API-VERSION header with default 2.0.0
                   c.OperationFilter<MobileAppHeaderFilter>();

                   // Looks at attributes on properties to decide whether they are readOnly.
                   // Right now, this only applies to the DatabaseGeneratedAttribute.
                   c.SchemaFilter<MobileAppSchemaFilter>();

                   // 1. Adds an OAuth implicit flow description that points to App Service Auth with the specified provdier
                   // 2. Adds a Swashbuckle filter that applies this Oauth description to any Action with [Authorize]
                   //c.AppServiceAuthentication("https://{mysite}.azurewebsites.net/", "{provider}");
               })
               .EnableSwaggerUi(c => {
                   //c.EnableOAuth2Support("na", "na", "na");

                   // Replaces some javascript files with specific logic to:
                   // 1. Do the OAuth flow using the App Service Auth parameters
                   // 2. Parse the returned token
                   // 3. Apply the token to the X-ZUMO-AUTH header
                   c.MobileAppUi();
               });
        }
    }

    public class TodoItemInitializer : CreateDatabaseIfNotExists<MobileAppsFileSampleContext>
    {
        protected override void Seed(MobileAppsFileSampleContext context)
        {
            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Complete = false },
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Complete = false },
            };

            foreach (TodoItem todoItem in todoItems)
            {
                context.Set<TodoItem>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}

