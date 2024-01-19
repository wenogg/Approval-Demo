using System;
using System.IO;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ApprovalDemo.Blazor.Menus;
using ApprovalDemo.EntityFrameworkCore;
using ApprovalDemo.Localization;
using ApprovalDemo.MultiTenancy;
using Elsa;
using Elsa.Api.Client.HttpMessageHandlers;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Elsa.Studio.Core.BlazorServer.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Login.HttpMessageHandlers;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Workflows.Extensions;
using FastEndpoints;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Components.Server.LeptonXLiteTheme;
using Volo.Abp.AspNetCore.Components.Server.LeptonXLiteTheme.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Blazor.Server;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement.Blazor.Server;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Blazor.Server;
using Volo.Abp.OpenIddict;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace ApprovalDemo.Blazor;

[DependsOn(
    typeof(ApprovalDemoApplicationModule),
    typeof(ApprovalDemoEntityFrameworkCoreModule),
    typeof(ApprovalDemoHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreComponentsServerLeptonXLiteThemeModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpIdentityBlazorServerModule),
    typeof(AbpTenantManagementBlazorServerModule),
    typeof(AbpSettingManagementBlazorServerModule)
   )]
public class ApprovalDemoBlazorModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            //https://github.com/abpframework/abp/pull/9299
            mvcBuilder.AddControllersAsServices();
            mvcBuilder.AddViewComponentsAsServices();
        });

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(ApprovalDemoResource),
                typeof(ApprovalDemoDomainModule).Assembly,
                typeof(ApprovalDemoDomainSharedModule).Assembly,
                typeof(ApprovalDemoApplicationModule).Assembly,
                typeof(ApprovalDemoApplicationContractsModule).Assembly,
                typeof(ApprovalDemoBlazorModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("ApprovalDemo");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", "500da7ea-09a3-4961-a559-9bc632424b0f");
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureSwaggerServices(context.Services);
        ConfigureAutoApiControllers();
        ConfigureBlazorise(context);
        ConfigureRouter();
        ConfigureMenu();
        ConfigureElsa(context, configuration);

        context.Services.AddRazorPages();
        context.Services.AddServerSideBlazor(options =>
        {
            // Register the root components.
            options.RootComponents.RegisterCustomElsaStudioElements();
        });
        context.Services.AddDashboardModule();
        context.Services.AddWorkflowsModule();
    }

    private void ConfigureElsa(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddElsa(elsa =>
        {
            var connection = configuration.GetConnectionString("Default");
            // Configure Management layer to use EF Core.
            elsa.UseWorkflowManagement(management =>
            {

                management.UseEntityFrameworkCore(opt =>
                {
                    opt.RunMigrations = true;
                    opt.UseSqlServer(connection!);
                });
            });

            // Configure Runtime layer to use EF Core.
            elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore( opt =>
            {
                opt.RunMigrations = true;
                opt.UseSqlServer(connection!);
            }));

            // Default Identity features for authentication/authorization.
            // elsa.UseIdentity(identity =>
            // {
            //     identity.TokenOptions = options => options.SigningKey = "sufficiently-large-secret-signing-key"; // This key needs to be at least 256 bits long.
            //     identity.UseAdminUserProvider();
            // });

            // Configure ASP.NET authentication/authorization.
            //elsa.UseDefaultAuthentication(auth => auth.UseAdminApiKey());

            EndpointSecurityOptions.DisableSecurity();
            // Expose Elsa API endpoints.
            elsa
                .UseHttp()
                .AddFastEndpointsAssembly<Program>()
                .UseWorkflowsApi();

            // Setup a SignalR hub for real-time updates from the server.
            elsa.UseRealTimeWorkflows();

            // Enable C# workflow expressions
            elsa.UseCSharp();

            // Enable HTTP activities.
            elsa.UseHttp();

            // Use timer activities.
            elsa.UseScheduling();

            // Register custom activities from the application, if any.
            elsa.AddActivitiesFrom<Program>();

            // Register custom workflows from the application, if any.
            elsa.AddWorkflowsFrom<Program>();

            elsa.Services.AddCore();
            elsa.Services.AddShell(options => options.DisableAuthorization = true);
            elsa.Services.AddRemoteBackend(
                elsaClient => elsaClient.AuthenticationHandler = typeof(ApiKeyHttpMessageHandler),
                options => configuration.GetSection("Backend").Bind(options));

        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });

    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            // MVC UI
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            //BLAZOR UI
            options.StyleBundles.Configure(
                BlazorLeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/blazor-global-styles.css");
                    //You can remove the following line if you don't use Blazor CSS isolation for components
                    bundle.AddFiles("/ApprovalDemo.Blazor.styles.css");
                }
            );
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ApprovalDemoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}ApprovalDemo.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<ApprovalDemoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}ApprovalDemo.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<ApprovalDemoApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}ApprovalDemo.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<ApprovalDemoApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}ApprovalDemo.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<ApprovalDemoBlazorModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "ApprovalDemo API", Version = "v1" });
                options.DocInclusionPredicate((_, _) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        context.Services
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
    }

    private void ConfigureMenu()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new ApprovalDemoMenuContributor());
        });
    }

    private void ConfigureRouter()
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(ApprovalDemoBlazorModule).Assembly;
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(ApprovalDemoApplicationModule).Assembly);
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ApprovalDemoBlazorModule>();
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();

        app.UseAbpRequestLocalization();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();



        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


        app.UseWorkflows(); // Use Elsa middleware to handle HTTP requests mapped to HTTP Endpoint activities.
        app.UseWorkflowsSignalRHubs(); // Optional SignalR integration. Elsa Studio uses SignalR to receive real-time updates from the server.
        app.UseWorkflowsApi();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ApprovalDemo API");
        });

        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
