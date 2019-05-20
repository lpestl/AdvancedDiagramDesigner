using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using DiagramDesigner.Views;
using ToolboxDesigner.Core;

namespace DiagramDesigner.Functionality
{
    public partial class DesignerCanvas : Canvas, INotifyPropertyChanged
    {
        private Point? rubberbandSelectionStartPoint = null;

        private SelectionService selectionService;
        // UPD: Updated and readable style
        internal SelectionService SelectionService => selectionService ?? (selectionService = new SelectionService(this));
        
        public FileInfo DiagramXmlFileInfo { get; set; }

        // UPD: Caption for tabItem
        private string _caption = Properties.Resources.NewDiagram;
        public string Caption
        {
            get => _caption;
            set
            {
                if (_caption != value)
                {
                    _caption = value;
                    OnPropertyChanged("Caption");
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                // in case that this click is the start of a 
                // drag operation we cache the start point
                this.rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

                // if you click directly on the canvas all 
                // selected items are 'de-selected'
                SelectionService.ClearSelection();
                Focus();
                //e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this.rubberbandSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (this.rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, rubberbandSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            //e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (dragObject != null && !String.IsNullOrEmpty(dragObject.Xaml))
            {
                DesignerItem newItem = null;
                Object content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));

                if (content != null)
                {
                    if (content is ContentControl conCtrl)
                    {
                        if (conCtrl.Content is StackPanel stackPanel)
                        {
                            var gridobj = stackPanel.Children[0];
                            stackPanel.Children.Clear();
                            conCtrl.Content = null;

                            var bindingWidth = new Binding("ActualWidth");
                            bindingWidth.Source = conCtrl;

                            if (gridobj is Grid grid)
                                grid.SetBinding(Grid.WidthProperty, bindingWidth);

                            conCtrl.Content = gridobj;
                        }

                        // Restore binding on size
                        if (conCtrl.Content is Grid itemGrid)
                        {
                            foreach (var child in itemGrid.Children)
                            {
                                if (child is Button button)
                                {
                                    if (conCtrl.Tag is ToolboxItemSettings toolboxItemSettings)
                                    {
                                        var bindingLeft = new Binding("ActualWidth");
                                        bindingLeft.Source = itemGrid;

                                        var bindingTop = new Binding("ActualHeight");
                                        bindingTop.Source = itemGrid;

                                        var multiBinding = new MultiBinding();
                                        multiBinding.Bindings.Add(bindingLeft);
                                        multiBinding.Bindings.Add(bindingTop);
                                        multiBinding.Converter = new RelativeMarginToMarginConverter(toolboxItemSettings.Container.RelativeMargin);

                                        button.SetBinding(Button.MarginProperty, multiBinding);

                                        (button.Content as TextBlock).Visibility = Visibility.Visible;

                                        button.Click += ButtonOnClick;
                                    }
                                }

                                if (child is TextBlock conCapture)
                                {
                                    var bindingLeft = new Binding("ActualWidth");
                                    bindingLeft.Source = itemGrid;

                                    var bindingTop = new Binding("ActualHeight");
                                    bindingTop.Source = itemGrid;

                                    var multiBinding = new MultiBinding();
                                    multiBinding.Bindings.Add(bindingLeft);
                                    multiBinding.Bindings.Add(bindingTop);

                                    if (conCapture.Tag is ConnectorSettings conSettings)
                                        multiBinding.Converter = new SizeToMarginConverter(conSettings.RelativePosition,
                                            conSettings.Orientation);

                                    conCapture.SetBinding(TextBlock.MarginProperty, multiBinding);
                                }
                            }
                        }
                    }
                    
                    newItem = new DesignerItem { DateTimeCreated = DateTime.Now };
                    newItem.Content = content;

                    if ((content is ContentControl contentCtrl) &&
                        (contentCtrl.Tag is ToolboxItemSettings itemSettings))
                    {
                        newItem.NoDelete = itemSettings.NoDelete;
                        newItem.Proportional = itemSettings.Proportional;
                    }

                    Point position = e.GetPosition(this);

                    if (dragObject.DesiredSize.HasValue)
                    {
                        Size desiredSize = dragObject.DesiredSize.Value;
                        newItem.Width = desiredSize.Width;
                        newItem.Height = desiredSize.Height;

                        Functionality.DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                        Functionality.DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    }
                    else
                    {
                        Functionality.DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X));
                        Functionality.DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y));
                    }

                    Canvas.SetZIndex(newItem, this.Children.Count);
                    this.Children.Add(newItem);                    
                    SetConnectorDecoratorTemplate(newItem);

                    //update selection
                    this.SelectionService.SelectItem(newItem);
                    newItem.Focus();
                }

                e.Handled = true;
            }
        }

        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            DesignerItem container = FindParent<DesignerItem>(sender as Button);

            if (string.IsNullOrEmpty(container.Caption))
            {
                MessageBox.Show(Properties.Resources.ContainerCaptionIsClearMessage,
                    Properties.Resources.ContainerCaptionIsClearTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DiagramXmlFileInfo == null)
            {
                MessageBox.Show(Properties.Resources.SaveDiagramForAttachMessage,
                    Properties.Resources.SaveDiagramForAttachTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dir = DiagramXmlFileInfo.Directory;
            var files = dir.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
            var dependensDiagramXmlName = $"{container.Caption}.xml";

            FileInfo diagramFile = null;
            foreach (var fileInfo in files)
            {
                if (Path.GetFileName(fileInfo.FullName).ToLower().Equals(container.Caption.ToLower()))
                {
                    diagramFile = fileInfo;
                }
            }

            if (diagramFile == null)
            {
                var settingsDir = CheckSettingsDirectory();
                files = settingsDir.GetFiles();

                FileInfo defaultFileInfo = null;
                foreach (var fileInfo in files)
                    if (fileInfo.Name.Equals("default.xml"))
                        defaultFileInfo = fileInfo;

                var dirPath = dir.FullName[dir.FullName.Length - 1] == '\\'
                    ? dir.FullName
                    : $"{dir.FullName}\\";
                diagramFile = defaultFileInfo?.CopyTo($"{dirPath}{dependensDiagramXmlName}") ?? new FileInfo($"{dirPath}{dependensDiagramXmlName}");
                if (!diagramFile.Exists)
                {
                    XElement r = new XElement("Root");
                    r.Save(diagramFile.FullName);

                }
            }

            MainWindow mainWindow = FindParent<MainWindow>(this);

            var newDesigner = mainWindow.AddNewTab();
            var root = newDesigner.LoadSerializedDataFromFile(diagramFile.FullName);
            newDesigner.RestoreDiagramFromXElement(root);
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentWithoutType = VisualTreeHelper.GetParent(child);
            T parent = parentWithoutType as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentWithoutType);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();

            foreach (UIElement element in this.InternalChildren)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            // add margin 
            size.Width += 10;
            size.Height += 10;
            return size;
        }
        
        private void SetConnectorDecoratorTemplate(DesignerItem item)
        {
            if (item.ApplyTemplate() && item.Content is UIElement)
            {
                ControlTemplate template = DesignerItem.GetConnectorDecoratorTemplate(item.Content as UIElement);
                Control decorator = item.Template.FindName("PART_ConnectorDecorator", item) as Control;
                if (decorator != null && template != null)
                    decorator.Template = template;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
