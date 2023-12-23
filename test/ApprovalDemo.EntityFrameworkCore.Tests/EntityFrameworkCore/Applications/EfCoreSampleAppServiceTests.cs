using ApprovalDemo.Samples;
using Xunit;

namespace ApprovalDemo.EntityFrameworkCore.Applications;

[Collection(ApprovalDemoTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ApprovalDemoEntityFrameworkCoreTestModule>
{

}
