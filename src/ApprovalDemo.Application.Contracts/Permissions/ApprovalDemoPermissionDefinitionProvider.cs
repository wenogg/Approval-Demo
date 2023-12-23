using ApprovalDemo.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ApprovalDemo.Permissions;

public class ApprovalDemoPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ApprovalDemoPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ApprovalDemoPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ApprovalDemoResource>(name);
    }
}
