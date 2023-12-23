using Volo.Abp.Modularity;

namespace ApprovalDemo;

[DependsOn(
    typeof(ApprovalDemoApplicationModule),
    typeof(ApprovalDemoDomainTestModule)
)]
public class ApprovalDemoApplicationTestModule : AbpModule
{

}
