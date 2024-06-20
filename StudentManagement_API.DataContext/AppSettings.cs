using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace DemoApiWithoutEF.DataContext
{
    public class AppSettings
    {
        public static string GetConnectionString(string settingName = "DefaultConnection")
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, optional: true, reloadOnChange: true);

            var root = configurationBuilder.Build();
            return root.GetConnectionString(settingName);
        }
    }
}
