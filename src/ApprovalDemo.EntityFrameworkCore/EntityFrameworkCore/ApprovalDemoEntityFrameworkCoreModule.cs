using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace ApprovalDemo.EntityFrameworkCore;

[DependsOn(
    typeof(ApprovalDemoDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class ApprovalDemoEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ApprovalDemoEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ApprovalDemoDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        ConfigureElsaMigrationContext(context);

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also ApprovalDemoMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });

    }

    /// <summary>
    /// Configures the Elsa for the DbMigrator to use while running miggrations.
    /// </summary>
    /// <param name="context"></param>
    private static void ConfigureElsaMigrationContext(ServiceConfigurationContext context)
    {
        // var configuration = context.Services.GetSingletonInstance<IConfiguration>();
        // var migrationsAssemblyMarker = typeof(SqlServerElsaContextFactory);
        // var connectionString = configuration.GetConnectionString("Default");
        //
        // context.Services.AddDbContext<ElsaContext>(options =>
        // {
        //    options
        //         .UseSqlServer(connectionString: connectionString,
        //             sqlOptions => sqlOptions
        //                 .MigrationsAssembly(migrationsAssemblyMarker.Assembly.GetName().Name)
        //                 .MigrationsHistoryTable(ElsaContext.MigrationsHistoryTable, ElsaContext.ElsaSchema));
        // });
    }
}
