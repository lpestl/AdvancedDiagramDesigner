using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DiagramDesigner.Annotations;
using DiagramDesigner.Functionality;
using DiagramDesigner.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace DiagramDesigner.Views
{
    public partial class MainWindow : Window
    {
        private readonly List<CultureInfo> culturesList = new List<CultureInfo>
        {
            new CultureInfo("ru-RU"),
            new CultureInfo("en-US"),
        };

        public MainWindow()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open_Execute));

            InitializeComponent();

            for (var i = 0; i < culturesList.Count; i++)
            {
                LanguagesComboBox.Items.Add(culturesList[i].DisplayName);
                if (Equals(culturesList[i], CultureInfo.CurrentUICulture))
                {
                    LanguagesComboBox.SelectedIndex = i;
                }
            }
            LanguagesComboBox.SelectionChanged += LanguagesComboBoxOnSelectionChanged;

            var toolboxImporter = new ToolboxImporter(ToolboxesHandle);
            toolboxImporter.Scan();

            ItemPropertyGrid.PropertyValueChanged += ItemPropertyGridOnPropertyValueChanged;

            New_Executed(this, null);
        }

        private void LanguagesComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var answer = MessageBox.Show(Properties.Resources.RestartAppMessage, Properties.Resources.RestartApp,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            Thread.CurrentThread.CurrentCulture = culturesList[LanguagesComboBox.SelectedIndex];
            Thread.CurrentThread.CurrentUICulture = culturesList[LanguagesComboBox.SelectedIndex];

            XElement settings = new XElement("language", culturesList[LanguagesComboBox.SelectedIndex].Name);
            settings.Save("language.xml");

            if (answer == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        #region Application Common Commands

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var designer = AddNewTab();
            designer.New_Executed(sender, e);
        }

        private void Open_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var designer = AddNewTab();
            designer.Open_Executed(sender, e);
        }

        #endregion

        #region Designer`s tabs contol

        public DesignerCanvas AddNewTab()
        {
            var header = new TabItemHeader(Properties.Resources.NewDiagram);
            header.CloseMouseUpEventHandler += CloseDiagram_MouseUp;

            var content = new DiagramControl();
            content.CaptionChangedEventHandler += ContentOnCaptionChangedEventHandler;

            var index = DesignersTabControl.Items.Add(new TabItem
            {
                Header = header,
                Content = content
            });
            SetVisibilityTabItemHeaders(DesignersTabControl.Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed);

            DesignersTabControl.SelectedIndex = index;

            return content.Designer;
        }

        private void CloseTab(int index)
        {
            if ((DesignersTabControl.Items[index] is TabItem tabItem) &&
                (tabItem.Content is DiagramControl diagramControl) &&
                (diagramControl.Designer.Children.Count > 0))
            {
                switch (MessageBox.Show(Properties.Resources.SaveAfterCloseMessage, Properties.Resources.SaveAfterCloseTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        diagramControl.Designer.Save_Executed(this, null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return;
                }
            }

            // Note: This fix binding exception ( https://stackoverflow.com/questions/14419248/cannot-find-source-for-binding )
            var tabitem = DesignersTabControl.Items[index] as TabItem;
            tabitem.Template = null;

            DesignersTabControl.Items.RemoveAt(index);

            DesignersTabControl.SelectedIndex = index != 0 ? --index : 0;

            SetVisibilityTabItemHeaders(DesignersTabControl.Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
        }

        #endregion

        #region Tab Headers managment

        private void ContentOnCaptionChangedEventHandler(object sender, CaptionChangedEventArgs e)
        {
            foreach (var item in DesignersTabControl.Items)
            {
                if ((item is TabItem tabItem) && (tabItem.Content == sender))
                {
                    if (tabItem.Header is TabItemHeader tabItemHeader)
                        tabItemHeader.HeaderTextBlock.Text = e.NewCaption;
                }
            }
        }

        private void SetVisibilityTabItemHeaders(Visibility visibility)
        {
            Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, visibility));
            DesignersTabControl.ItemContainerStyle = s;
        }

        private void CloseDiagram_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseTab(DesignersTabControl.SelectedIndex);
        }

        #endregion
        
        private void DesignersTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((e.RemovedItems.Count != 0) && (e.RemovedItems[0] is TabItem tabItem) && (tabItem.Content is DiagramControl olddiagramControl))
                olddiagramControl.Designer.SelectionService.CurrentSelection.CollectionChanged -= CurrentSelectionOnCollectionChanged;

            if ((e.AddedItems.Count != 0) && (e.AddedItems[0] is TabItem tabNewItem) && (tabNewItem.Content is DiagramControl newdiagramControl))
            {
                ItemPropertyGrid.SelectedObject = newdiagramControl.Designer.SelectionService.GetSelectedDesignItem()?.PropertiesHandler;
                newdiagramControl.Designer.SelectionService.CurrentSelection.CollectionChanged +=
                    CurrentSelectionOnCollectionChanged;
            }
        }

        private void CurrentSelectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var selectionService =
                ((DesignersTabControl.Items[DesignersTabControl.SelectedIndex] as TabItem)?.Content as DiagramControl)?.Designer.SelectionService;

            if (selectionService != null)
                ItemPropertyGrid.SelectedObject = selectionService.GetSelectedDesignItem()?.PropertiesHandler;
        }
        
        private void ItemPropertyGridOnPropertyValueChanged(object sender, PropertyValueChangedEventArgs propertyValueChangedEventArgs)
        {
            var selectionService =
                ((DesignersTabControl.Items[DesignersTabControl.SelectedIndex] as TabItem)?.Content as DiagramControl)?.Designer.SelectionService;

            if (selectionService != null)
            {
                var selectedDesItem = selectionService.GetSelectedDesignItem();
                selectedDesItem.PropertyValueChanged(sender, propertyValueChangedEventArgs);
            }
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ApplicationCommands.Delete.Execute(e, ((DesignersTabControl.Items[DesignersTabControl.SelectedIndex] as TabItem)?.Content as DiagramControl)?.Designer);
            }
        }
    }
}
