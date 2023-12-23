using Volo.Abp.Modularity;

namespace ApprovalDemo;

[DependsOn(
    typeof(ApprovalDemoDomainModule),
    typeof(ApprovalDemoTestBaseModule)
)]
public class ApprovalDemoDomainTestModule : AbpModule
{

}
