
using Idata.Data;
using Idata.Data.Entities.Ireport;
using Idata.Data.Entities.Isite;
using Ireport.Config;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Ireport.Data.Seeders
{
    public class IreportModuleSeeder
    {

        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<IdataContext>();

                context.Database.EnsureCreated();

                object values = new
                {
                    name = "Ireport",
                    alias = "Ireport",
                    permissions = Permissions.GetPermissions(),
                    settings = "",
                    status = true,
                    priority = 1,
                    enabled = true,
                    configs = Configs.GetConfigs(),
					cms_pages = "",
                    cms_sidebar = ""
                };

                //Query a ef

                //entidad.name = object.name;

                Module? module = context.Modules.Where(m => m.alias == "Ireport").FirstOrDefault();

                if (module == null)
                {
                    module = JsonConvert.DeserializeObject<Module>(JsonConvert.SerializeObject(values));
                    context.Modules.Add(module);
                    context.SaveChanges();
                    module = context.Modules.Where(m => m.alias == "Ireport").FirstOrDefault();
                    module.translations = new List<ModuleTranslation>() {
                        new ModuleTranslation()
                            {
                                locale = "en",
                                title = "Users"
                            }
                        };

                }
                else
                {
                    context.Entry(module).CurrentValues.SetValues(values);
                }



                context.SaveChanges();

            }
        }


    }
}
