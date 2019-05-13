using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DiagramDesigner.Functionality;

namespace DiagramDesigner.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));

            InitializeComponent();

            AddNewTab();
        }

        private void AddNewTab()
        {
            var header = new TabItemHeader(Properties.Resources.NewDiagram);
            header.CloseMouseUpEventHandler += CloseDiagram_MouseUp;

            DesignersTabControl.Items.Add(new TabItem
            {
                Header = header,
                Content = new DiagramControl()
            });
            SetVisibilityTabItemHeaders(DesignersTabControl.Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
        }

        private void CloseTab(int index)
        {
            // Note: This fix binding exception ( https://stackoverflow.com/questions/14419248/cannot-find-source-for-binding )
            var tabitem = DesignersTabControl.Items[index] as TabItem;
            tabitem.Template = null;

            DesignersTabControl.Items.RemoveAt(index);

            DesignersTabControl.SelectedIndex = index != 0 ? --index : 0;

            SetVisibilityTabItemHeaders(DesignersTabControl.Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddNewTab();

            e.Handled = true;
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
    }
}
