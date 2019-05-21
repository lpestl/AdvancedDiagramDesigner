using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DiagramDesigner.Annotations;
using DiagramDesigner.Functionality;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace DiagramDesigner.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open_Execute));

            InitializeComponent();

            var toolboxImporter = new ToolboxImporter(ToolboxesHandle);
            toolboxImporter.Scan();

            New_Executed(this, null);
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

            DesignersTabControl.SelectedIndex = DesignersTabControl.Items.Count - 1;

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
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DesignersTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectedTabIndex = (sender as TabControl).SelectedIndex;
        }
    }
}
