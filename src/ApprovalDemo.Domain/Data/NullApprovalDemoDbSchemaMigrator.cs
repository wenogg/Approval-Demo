using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ApprovalDemo.Data;

/* This is used if database provider does't define
 * IApprovalDemoDbSchemaMigrator implementation.
 */
public class NullApprovalDemoDbSchemaMigrator : IApprovalDemoDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
