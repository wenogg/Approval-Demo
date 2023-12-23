using ApprovalDemo.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ApprovalDemo.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class ApprovalDemoController : AbpControllerBase
{
    protected ApprovalDemoController()
    {
        LocalizationResource = typeof(ApprovalDemoResource);
    }
}
