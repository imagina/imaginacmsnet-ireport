namespace Ireport.Config
{
    public static class Configs
    {

        private static string configs = @"{
            'exportable':{
                
                'reports':{
                    'moduleName':'report',
                    'fileName':'report',
                    'repositoryName':'ReportsApiRepository',
                    'formats':['csv', 'pdf', 'xlsx'],
                    'apiRoute': '/ireport/v1/reports/export',
                    'allowCreation': true,
                }
            }
        }";

        public static string GetConfigs()
        {
            return configs;
        }
    }
}
