using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DiagramDesigner.Functionality
{
    public class ToolboxImporter
    {
        private StackPanel toolboxesHandle_;
        private static string storagePath_ = "ExternalToolboxes";
        public ToolboxImporter(StackPanel toolboxesHandle)
        {
            toolboxesHandle_ = toolboxesHandle;
        }

        public void Scan()
        {
            var xamls = GetExternalXamlFromStorage();
        }

        private FileInfo[] GetExternalXamlFromStorage()
        {
            var appDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            var fullStoragePath = appDir.FullName.LastIndexOf('\\') != appDir.FullName.Length - 1 ? $"{appDir.FullName}\\{storagePath_}\\" : $"{appDir.FullName}{storagePath_}\\";
            if (!Directory.Exists(fullStoragePath))
                Directory.CreateDirectory(fullStoragePath);

            var storageDir = new DirectoryInfo(fullStoragePath);

            return storageDir.GetFiles("*.xaml", SearchOption.TopDirectoryOnly);
        }
    }
}
