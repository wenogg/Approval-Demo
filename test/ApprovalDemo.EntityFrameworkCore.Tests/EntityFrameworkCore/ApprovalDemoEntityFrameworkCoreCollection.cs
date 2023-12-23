using Xunit;

namespace ApprovalDemo.EntityFrameworkCore;

[CollectionDefinition(ApprovalDemoTestConsts.CollectionDefinitionName)]
public class ApprovalDemoEntityFrameworkCoreCollection : ICollectionFixture<ApprovalDemoEntityFrameworkCoreFixture>
{

}
