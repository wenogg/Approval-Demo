using Volo.Abp.Settings;

namespace ApprovalDemo.Settings;

public class ApprovalDemoSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(ApprovalDemoSettings.MySetting1));
    }
}
