using ApprovalDemo.Samples;
using Xunit;

namespace ApprovalDemo.EntityFrameworkCore.Domains;

[Collection(ApprovalDemoTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ApprovalDemoEntityFrameworkCoreTestModule>
{

}
