using ApprovalDemo.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ApprovalDemo.Permissions;

public class ApprovalDemoPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ordersGroup = context.AddGroup(ApprovalDemoPermissions.Orders.Default);
        ordersGroup.AddPermission(ApprovalDemoPermissions.Orders.Modify, L("Modify Order"));
        ordersGroup.AddPermission(ApprovalDemoPermissions.Orders.Prepare, L("Prepare Order"));
        ordersGroup.AddPermission(ApprovalDemoPermissions.Orders.Ship, L("Ship Order"));
        ordersGroup.AddPermission(ApprovalDemoPermissions.Orders.Receive, L("Receive Order"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ApprovalDemoResource>(name);
    }
}
