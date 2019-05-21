using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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

            var fullStoragePath = appDir.FullName.LastIndexOf('\\') != appDir.FullName.Length - 1
                ? $"{appDir.FullName}\\{storagePath_}\\"
                : $"{appDir.FullName}{storagePath_}\\";
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
                    string toolBoxKey = $"{dictionaryEntry.Value}_Toolbox";
                    var toolBox = CreateToolbox(toolboxSettings);
                    
                    foreach (var itemsSetting in toolboxSettings.ItemsSettings)
                    {
                        var newItem = CreateNewToolboxItem(toolboxSettings, itemsSetting);

                        if (!itemsSetting.Invisible)
                            toolBox.Items.Add(newItem);
                        else
                            newResources.Add($"{itemsSetting.DisplayName.Replace(" ", "")}_HiddenItem", newItem);
                    }

                    newResources.Add(toolBoxKey, toolBox);
                    keys.Add(toolBoxKey);
                }
            }

            Application.Current.Resources.MergedDictionaries.Add(newResources);

            foreach (var key in keys)
            {
                foreach (var mergedDictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (mergedDictionary.Contains(key))
                    {
                        var toolboxExpander = new Expander
                            {Header = ((mergedDictionary[key] as Toolbox)?.Tag as string), IsExpanded = true};
                        toolboxExpander.Content = (mergedDictionary[key] as Toolbox);

                        toolboxesHandle_.Children.Add(toolboxExpander);
                    }
                }
            }
        }

        private ContentControl CreateNewToolboxItem(ToolboxSettings toolboxSettings, ToolboxItemSettings itemsSetting)
        {
            var newItem = new ContentControl { Tag = itemsSetting, ToolTip = itemsSetting.DisplayName };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var newGrid = new Grid();
            var newPath = new Path { Style = itemsSetting.PathStyle/*, ToolTip = itemsSetting.DisplayName*/ };

            newGrid.Children.Add(newPath);

            if (toolboxSettings.ToolboxGridType == ToolboxGrid.Grid)
            {
                newItem.Content = newGrid;
            }
            else
            {
                newGrid.Width = 25;
                stackPanel.Children.Add(newGrid);
                stackPanel.Children.Add(new TextBlock
                {
                    Text = itemsSetting.DisplayName,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                });
                newItem.Content = stackPanel;
            }

            if (itemsSetting.PathStyle_DragThumb != null)
            {
                var controlTemplate = new ControlTemplate();

                var pathTemplate = new FrameworkElementFactory(typeof(Path));
                pathTemplate.SetValue(Path.StyleProperty, itemsSetting.PathStyle_DragThumb);

                controlTemplate.VisualTree = pathTemplate;

                DesignerItem.SetDragThumbTemplate(newItem, controlTemplate);
            }

            if (itemsSetting.Container != null)
            {
                var bindingLeft = new Binding("ActualWidth");
                bindingLeft.Source = newGrid;

                var bindingTop = new Binding("ActualHeight");
                bindingTop.Source = newGrid;

                var multiBinding = new MultiBinding();
                multiBinding.Bindings.Add(bindingLeft);
                multiBinding.Bindings.Add(bindingTop);
                multiBinding.Converter = new RelativeMarginToMarginConverter(itemsSetting.Container.RelativeMargin);
                
                var button = new Button {
                    Content = new TextBlock
                    {
                        Text = Properties.Resources.ClickToOpen,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Visibility = Visibility.Collapsed
                    }
                };

                button.SetBinding(TextBlock.MarginProperty, multiBinding);
                newGrid.Children.Add(button);
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
                        connectorTemplate.SetValue(Connector.MaxInConnectionsProperty,
                            connectorsSetting.MaxInConnections);
                        connectorTemplate.SetValue(Connector.MaxOutConnectionsProperty,
                            connectorsSetting.MaxOutConnections);

                        relPanelTemplate.AppendChild(connectorTemplate);

                        if (!string.IsNullOrEmpty(connectorsSetting.Caption))
                        {
                            var newCaption = new TextBlock { Text = connectorsSetting.Caption, IsHitTestVisible = false, Tag = connectorsSetting };

                            if (connectorsSetting.Orientation == ConnectorOrientation.Left)
                                newCaption.HorizontalAlignment = HorizontalAlignment.Right;

                            if (connectorsSetting.Orientation == ConnectorOrientation.Top)
                                newCaption.VerticalAlignment = VerticalAlignment.Bottom;

                            var bindingLeft = new Binding("ActualWidth");
                            bindingLeft.Source = newGrid;

                            var bindingTop = new Binding("ActualHeight");
                            bindingTop.Source = newGrid;

                            var multiBinding = new MultiBinding();
                            multiBinding.Bindings.Add(bindingLeft);
                            multiBinding.Bindings.Add(bindingTop);
                            multiBinding.Converter = new SizeToMarginConverter(connectorsSetting.RelativePosition, connectorsSetting.Orientation);

                            newCaption.SetBinding(TextBlock.MarginProperty, multiBinding);
                            newGrid.Children.Add(newCaption);
                        }
                    }
                }

                controlTemplate.VisualTree = relPanelTemplate;

                DesignerItem.SetConnectorDecoratorTemplate(newItem, controlTemplate);
            }

            return newItem;
        }

        private Toolbox CreateToolbox(ToolboxSettings toolboxSettings)
        {
            var toolBox = new Toolbox { Tag = toolboxSettings.Name };

            switch (toolboxSettings.ToolboxGridType)
            {
                case ToolboxGrid.List:
                    toolBox.ItemSize = new Size(250, 40);
                    break;
                case ToolboxGrid.Grid:
                    toolBox.ItemSize = new Size(60, 60);
                    break;
            }

            return toolBox;
        }

        private List<DictionaryEntry> GetToolboxesSettings(ResourceDictionary newResources)
        {
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
                        $"{e.Source} - {e.Message}"), Properties.Resources.ErrorImport, MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

    public class RelativeMarginToMarginConverter : IMultiValueConverter
    {
        private Thickness relativeMargin_;

        public RelativeMarginToMarginConverter(Thickness relativeMargin)
        {
            relativeMargin_ = relativeMargin;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var width = System.Convert.ToDouble(values[0]);
            var height = System.Convert.ToDouble(values[1]);

            return new Thickness(width * relativeMargin_.Left,
                height * relativeMargin_.Top,
                width * relativeMargin_.Right,
                height * relativeMargin_.Bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SizeToMarginConverter : IMultiValueConverter
    {
        private Point relativePosition_;
        private ConnectorOrientation orientation_;

        public SizeToMarginConverter(Point relativePosition, ConnectorOrientation orientation)
        {
            relativePosition_ = relativePosition;
            orientation_ = orientation;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values[0] is double) && (values[1] is double))
            {
                var orientstionOffset = new Thickness(1,1,-1,-1);
                var sizeOffset = new Point(0,0);

                if (orientation_ == ConnectorOrientation.Left)
                {
                    orientstionOffset.Left = -1;
                    orientstionOffset.Right = 1;
                    sizeOffset.X = 1;
                }

                if (orientation_ == ConnectorOrientation.Top)
                {
                    orientstionOffset.Top = -1;
                    orientstionOffset.Bottom = 1;
                    sizeOffset.Y = 1;
                }

                var margin = new Thickness(System.Convert.ToDouble(values[0]) * orientstionOffset.Left * relativePosition_.X - System.Convert.ToDouble(values[0]) * sizeOffset.X,
                    System.Convert.ToDouble(values[1]) * orientstionOffset.Top * relativePosition_.Y - System.Convert.ToDouble(values[1]) * sizeOffset.Y,
                    System.Convert.ToDouble(values[0]) * orientstionOffset.Right * relativePosition_.X + System.Convert.ToDouble(values[0]) * sizeOffset.X,
                    System.Convert.ToDouble(values[1]) * orientstionOffset.Bottom * relativePosition_.Y + System.Convert.ToDouble(values[1]) * sizeOffset.Y);
                return margin;
            }

            return new Thickness(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}