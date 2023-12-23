using Volo.Abp.Modularity;

namespace ApprovalDemo;

public abstract class ApprovalDemoApplicationTestBase<TStartupModule> : ApprovalDemoTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
