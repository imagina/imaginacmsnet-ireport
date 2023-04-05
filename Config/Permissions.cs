namespace Ireport.Config
{
    public static class Permissions
    {

        private static string permissions = @"{
  'ireport.report-types': {
    'manage': 'ireport::report-types.manage',
    'index': 'ireport::report-types.list resource',
    'edit': 'ireport::report-types.edit resource',
    'create': 'ireport::report-types.create resource',
    'destroy': 'ireport::report-types.destroy resource',
    'restore': 'ireport::report-types.restore resource'
  },
  'ireport.reports': {
    'manage': 'ireport::reports.manage',
    'index': 'ireport::reports.list resource',
    'edit': 'ireport::reports.edit resource',
    'create': 'ireport::reports.create resource',
    'destroy': 'ireport::reports.destroy resource',
    'index-all': 'ireport::reports.list-all resource',
    'restore': 'ireport::reports.restore resource'
  },
  'ireport.folders': {
    'manage': 'ireport::folders.manage',
    'index': 'ireport::folders.list resource',
    'edit': 'ireport::folders.edit resource',
    'create': 'ireport::folders.create resource',
    'destroy': 'ireport::folders.destroy resource',
    'index-all': 'ireport::folders.list-all resource',
    'restore': 'ireport::folders.restore resource'
  }
}";

        public static string GetPermissions()
        {
            return permissions;
        }
    }
}
