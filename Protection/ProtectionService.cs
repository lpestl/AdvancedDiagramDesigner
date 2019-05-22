using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Protection
{
    public static class ProtectionService
    {
        public static bool TrialTimeIsValid()
        {
            bool trialEnded = false;

            try
            {
                RegistryKey currentUserKey = Registry.CurrentUser;
                RegistryKey diagramDesignerKey = currentUserKey.CreateSubKey("DiagramDesigner");

                if (DateTime.Now > new DateTime(2019, 5, 25))
                {
                    diagramDesignerKey.SetValue("TrialEnded", true);
                    trialEnded = true;
                }
                else
                {
                    var value = diagramDesignerKey.GetValue("TrialEnded");
                    if ((value != null) && (bool.Parse((string) value) == true))
                    {
                        trialEnded = true;
                    }
                }
            }
            finally
            {
                if (trialEnded)
                {
                    MessageBox.Show("Закончился период демонстрационной версии. Пожалуйста обратитесь к разработчику.",
                        "Обратитесь к разработчику.", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }

            return !trialEnded;
        }
    }
}
