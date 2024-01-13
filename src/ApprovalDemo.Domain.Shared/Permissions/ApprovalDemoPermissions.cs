namespace ApprovalDemo.Permissions;

public static class ApprovalDemoPermissions
{
    public const string GroupName = "ApprovalDemo";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    public static class Orders
    {
        public const string Default = GroupName + ".Orders";
        public const string Modify = Default + ".View";
        public const string Prepare = Default + ".Prepare";
        public const string Ship = Default + ".Ship";
        public const string Receive = Default + ".Receive";
    }
}
