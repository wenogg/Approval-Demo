using System.Threading.Tasks;

namespace ApprovalDemo.Data;

public interface IApprovalDemoDbSchemaMigrator
{
    Task MigrateAsync();
}
