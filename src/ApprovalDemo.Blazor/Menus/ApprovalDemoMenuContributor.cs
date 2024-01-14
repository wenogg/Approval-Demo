using System.Threading.Tasks;
using ApprovalDemo.Localization;
using ApprovalDemo.MultiTenancy;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;

namespace ApprovalDemo.Blazor.Menus;

public class ApprovalDemoMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<ApprovalDemoResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                ApprovalDemoMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );

        _ = context.Menu.AddItem(
            new ApplicationMenuItem("Approval Items", "Approval Items", icon: "fa fa-book")
                .AddItem(new ApplicationMenuItem("ApprovalDemo.ApprovalItems", "Items", url: "/approval-items"))
        );

        _ = context.Menu.AddItem(
            new ApplicationMenuItem("Orders", "Orders", icon: "fa fa-book")
                .AddItem(new ApplicationMenuItem("ApprovalDemo.Orders", "Orders", url: "/orders"))
        );

        return Task.CompletedTask;
    }
}
