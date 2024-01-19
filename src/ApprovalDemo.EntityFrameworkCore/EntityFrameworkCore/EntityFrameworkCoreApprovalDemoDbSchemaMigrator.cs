using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ApprovalDemo.Data;
using Volo.Abp.DependencyInjection;

namespace ApprovalDemo.EntityFrameworkCore;

public class EntityFrameworkCoreApprovalDemoDbSchemaMigrator(IServiceProvider serviceProvider)
    : IApprovalDemoDbSchemaMigrator, ITransientDependency
{
    public async Task MigrateAsync()
    {
        /* We intentionally resolve the ApprovalDemoDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await serviceProvider
            .GetRequiredService<ApprovalDemoDbContext>()
            .Database
            .MigrateAsync();
    }
}
