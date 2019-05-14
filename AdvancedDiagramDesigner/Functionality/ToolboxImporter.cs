﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

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

        private FileInfo[] GetExternalXamlFromStorage()
        {
            var appDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            var fullStoragePath = appDir.FullName.LastIndexOf('\\') != appDir.FullName.Length - 1 ? $"{appDir.FullName}\\{storagePath_}\\" : $"{appDir.FullName}{storagePath_}\\";
            if (!Directory.Exists(fullStoragePath))
                Directory.CreateDirectory(fullStoragePath);

            var storageDir = new DirectoryInfo(fullStoragePath);

            return storageDir.GetFiles("*.xaml", SearchOption.TopDirectoryOnly);
        }
        
        private void AddCustomToolbox(FileInfo xamlFileInfo)
        {
            var newResources = AddResources(xamlFileInfo);

            if (newResources == null)
                return;

            //CreateToolboxDictionary(newResources);
        }

        //private void CreateToolboxDictionary(ResourceDictionary newResources)
        //{
        //    var toolBox = new Toolbox { ItemSize = new Size(60,40) };

        //    foreach (var resourcePair in newResources)
        //    {
                
        //    }
        //}

        private ResourceDictionary AddResources(FileInfo xamlFileInfo)
        {
            ResourceDictionary myResourceDictionary = null;

            try
            {
                Stream stream = File.OpenRead(xamlFileInfo.FullName);
                System.Windows.Markup.XamlReader reader = new System.Windows.Markup.XamlReader();
                myResourceDictionary = (ResourceDictionary) reader.LoadAsync(stream);
                //Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Properties.Resources.ErrorImportMessage, xamlFileInfo.FullName,
                    $"{e.Source} - {e.Message}"), Properties.Resources.ErrorImport, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return myResourceDictionary;
        }

        public void Scan()
        {
            var xamls = GetExternalXamlFromStorage();
            foreach (var xamlFileInfo in xamls)
            {
                AddCustomToolbox(xamlFileInfo);
            }
        }
    }
}