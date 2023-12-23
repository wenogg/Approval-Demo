using System;
using System.Collections.Generic;
using System.Text;
using ApprovalDemo.Localization;
using Volo.Abp.Application.Services;

namespace ApprovalDemo;

/* Inherit your application services from this class.
 */
public abstract class ApprovalDemoAppService : ApplicationService
{
    protected ApprovalDemoAppService()
    {
        LocalizationResource = typeof(ApprovalDemoResource);
    }
}
