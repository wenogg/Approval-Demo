using ApprovalDemo.Localization;
using Volo.Abp.AspNetCore.Components;

namespace ApprovalDemo.Blazor;

public abstract class ApprovalDemoComponentBase : AbpComponentBase
{
    protected ApprovalDemoComponentBase()
    {
        LocalizationResource = typeof(ApprovalDemoResource);
    }
}
