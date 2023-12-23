using Volo.Abp.Modularity;

namespace ApprovalDemo;

/* Inherit from this class for your domain layer tests. */
public abstract class ApprovalDemoDomainTestBase<TStartupModule> : ApprovalDemoTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
