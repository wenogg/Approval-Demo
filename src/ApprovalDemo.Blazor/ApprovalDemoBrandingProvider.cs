using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace ApprovalDemo.Blazor;

[Dependency(ReplaceServices = true)]
public class ApprovalDemoBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "ApprovalDemo";
}
