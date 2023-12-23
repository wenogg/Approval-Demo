using ApprovalDemo.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace ApprovalDemo.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ApprovalDemoEntityFrameworkCoreModule),
    typeof(ApprovalDemoApplicationContractsModule)
    )]
public class ApprovalDemoDbMigratorModule : AbpModule
{
}
