using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest_Plugins.Helpers
{
    internal static class AppSettings
    {
        public static string GetEnv(string envName)
        {
            string _application = ConfigurationManager.AppSettings.Get(envName);
            if (string.IsNullOrEmpty(_application))
                throw new Exception($"Variavel:{envName} não encontrada");

            return _application;
        }
    }
}
