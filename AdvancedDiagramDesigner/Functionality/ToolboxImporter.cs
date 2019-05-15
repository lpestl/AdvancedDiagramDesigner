using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using DiagramDesigner.Controls;
using ToolboxDesigner.Core;
using Path = System.Windows.Shapes.Path;

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

            var dictionaryEntries = GetToolboxesSettings(newResources);

            if (!dictionaryEntries.Any())
                return;

            var keys = new List<string>();
            foreach (var dictionaryEntry in dictionaryEntries)
            {
                if (dictionaryEntry.Value is ToolboxSettings toolboxSettings)
                {
                    string toolBoxKey = $"{dictionaryEntry.Key}_Toolbox";

                    var toolBox = new Toolbox { ItemSize = new Size(60, 60), Tag = toolboxSettings.Name};

                    foreach (var itemsSetting in toolboxSettings.ItemsSettings)
                    {
                        var newItem = new Path {Style = itemsSetting.PathStyle, ToolTip = itemsSetting.DisplayName};

                        if (itemsSetting.PathStyle_DragThumb != null)
                        {
                            var controlTemplate = new ControlTemplate();

                            var pathTemplate = new FrameworkElementFactory(typeof(Path));
                            pathTemplate.SetValue(Path.StyleProperty, itemsSetting.PathStyle_DragThumb);

                            controlTemplate.VisualTree = pathTemplate;

                            DesignerItem.SetDragThumbTemplate(newItem, controlTemplate);
                        }

                        if (itemsSetting.ConnectorsSettings != null)
                        {
                            var controlTemplate = new ControlTemplate();

                            var relPanelTemplate = new FrameworkElementFactory(typeof(RelativePositionPanel));
                            relPanelTemplate.SetValue(RelativePositionPanel.MarginProperty, new Thickness(-4));

                            if (itemsSetting.ConnectorsSettings.Any())
                            {
                                foreach (var connectorsSetting in itemsSetting.ConnectorsSettings)
                                {
                                    var connectorTemplate = new FrameworkElementFactory(typeof(Connector));

                                    connectorTemplate.SetValue(Connector.NameProperty, connectorsSetting.Name);
                                    connectorTemplate.SetValue(Connector.OrientationProperty,
                                        connectorsSetting.Orientation);
                                    connectorTemplate.SetValue(RelativePositionPanel.RelativePositionProperty,
                                        connectorsSetting.RelativePosition);

                                    relPanelTemplate.AppendChild(connectorTemplate);
                                }
                            }

                            controlTemplate.VisualTree = relPanelTemplate;

                            DesignerItem.SetConnectorDecoratorTemplate(newItem, controlTemplate);
                        }

                        toolBox.Items.Add(newItem);
                    }

                    newResources.Add(toolBoxKey, toolBox);
                    keys.Add(toolBoxKey);
                    //var toolboxExpander = new Expander { Header = toolboxSettings.Name, IsExpanded = true};
                    //toolboxExpander.Content = toolBox;

                    //toolboxesHandle_.Children.Add(toolboxExpander);
                }
            }

            Application.Current.Resources.MergedDictionaries.Add(newResources);

            foreach (var key in keys)
            {
                foreach (var mergedDictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (mergedDictionary.Contains(key))
                    {
                        var toolboxExpander = new Expander { Header = ((mergedDictionary[key] as Toolbox).Tag as string), IsExpanded = true };
                        toolboxExpander.Content = (mergedDictionary[key] as Toolbox);

                        toolboxesHandle_.Children.Add(toolboxExpander);
                    }
                }
            }
        }

        private List<DictionaryEntry> GetToolboxesSettings(ResourceDictionary newResources)
        {
            //var toolBox = new Toolbox { ItemSize = new Size(60, 40) };
            
            var toolboxesSettings = new List<DictionaryEntry>();
            foreach (var resourceEntry in newResources)
                if ((resourceEntry is DictionaryEntry dicEntry) && (dicEntry.Value is ToolboxSettings toolboxSettings))
                    toolboxesSettings.Add(dicEntry);

            return toolboxesSettings;
        }

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
