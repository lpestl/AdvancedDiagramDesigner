using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DiagramDesigner.Controls;

namespace DiagramDesigner.Functionality
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl, ISelectable, IGroupable
    {
        #region ID

        private Guid id;

        public Guid ID
        {
            get { return id; }
        }

        #endregion

        // UPD: Name property
        #region Caption

        public string Caption { get; set; } = string.Empty;

        #endregion

        // UPD: DateTime created property
        #region DateTimeCreated property

        public DateTime DateTimeCreated { get; set; }

        #endregion
        
        public bool NoDelete { get; set; }

        #region ParentID

        public Guid ParentID
        {
            get { return (Guid) GetValue(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }

        public static readonly DependencyProperty ParentIDProperty =
            DependencyProperty.Register("ParentID", typeof(Guid), typeof(DesignerItem));

        #endregion

        #region IsGroup

        public bool IsGroup
        {
            get { return (bool) GetValue(IsGroupProperty); }
            set { SetValue(IsGroupProperty, value); }
        }

        public static readonly DependencyProperty IsGroupProperty =
            DependencyProperty.Register("IsGroup", typeof(bool), typeof(DesignerItem));

        #endregion

        #region IsSelected Property

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected",
                typeof(bool),
                typeof(DesignerItem),
                new FrameworkPropertyMetadata(false));

        #endregion

        #region DragThumbTemplate Property

        // can be used to replace the default template for the DragThumb
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate) element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(DragThumbTemplateProperty, value);
        }

        #endregion

        #region ConnectorDecoratorTemplate Property

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
            DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate),
                typeof(DesignerItem));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        {
            return (ControlTemplate) element.GetValue(ConnectorDecoratorTemplateProperty);
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(ConnectorDecoratorTemplateProperty, value);
        }

        #endregion

        #region IsDragConnectionOver

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver
        {
            get { return (bool) GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }

        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver",
                typeof(bool),
                typeof(DesignerItem),
                new FrameworkPropertyMetadata(false));

        #endregion

        static DesignerItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem(Guid id)
        {
            this.id = id;
            this.Loaded += new RoutedEventHandler(DesignerItem_Loaded);
        }

        public DesignerItem()
            : this(Guid.NewGuid())
        {
        }


        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            Functionality.DesignerCanvas designer = VisualTreeHelper.GetParent(this) as Functionality.DesignerCanvas;

            // update selection
            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (this.IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
                    }
                else if (!this.IsSelected)
                {
                    designer.SelectionService.SelectItem(this);
                }

                Focus();
            }

            e.Handled = false;
        }

        void DesignerItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.Template != null)
            {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null)
                {
                    // Fix unhandled exception of type 'System.ArgumentOutOfRangeException' occurred in PresentationFramework.dll
                    if (VisualTreeHelper.GetChildrenCount(contentPresenter) > 0)
                    {
                        UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                        if (contentVisual != null)
                        {
                            DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
                            if (thumb != null)
                            {
                                ControlTemplate template =
                                    DesignerItem.GetDragThumbTemplate(contentVisual) as ControlTemplate;
                                if (template != null)
                                    thumb.Template = template;
                            }
                        }
                    }
                }
            }
        }

        // UPD: Double click to add block name
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var shapeContent = GetShape();
            ClearContent();
            
            // Creat text box for inputing name
            var nameEditBox = new TextBox
            {
                Text = string.IsNullOrEmpty(Caption) ? string.Empty : Caption,
                Margin = new Thickness(5),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center, 
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
            };

            // Creat grid with figure (Shape.Path) and nameTextBox
            var grid = new Grid {Children = { shapeContent, nameEditBox}};

            this.Content = grid;
            
            // Bind to LostFocus eventHandler
            nameEditBox.LostFocus += NameEditBoxOnLostFocus;

            // Activate keybord cursor
            Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new Action(delegate () {
                    nameEditBox.Focus();         // Set Logical Focus
                    Keyboard.Focus(nameEditBox); // Set Keyboard Focus
                    nameEditBox.SelectAll();
                }));
        }

        // Find Shape.Path in self content
        private System.Windows.Shapes.Shape GetShape()
        {
            // We check the block content for an empty figure (without name)
            var shape = this.Content as System.Windows.Shapes.Shape;

            // if empty figure not found
            if (shape == null)
            {
                // We check the block content for an figure (Shape.Path)
                var oldGrid = this.Content as Grid;
                // if content not path and not grid, then return
                if (oldGrid == null)
                    return null;

                // Looking for a figure (Shape.Path)
                foreach (var oldGridChild in oldGrid.Children)
                {
                    if (oldGridChild is System.Windows.Shapes.Shape tmpShape)
                    {
                        shape = tmpShape;
                        break;
                    }
                }
            }

            return shape;
        }

        private void NameEditBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var shape = GetShape();
            ClearContent();

            var inputBox = sender as TextBox;
            if (inputBox == null)
            {
                this.Content = shape;
                Caption = string.Empty;
                return;
            }

            // UnBind to LostFocus eventHandler
            inputBox.LostFocus -= NameEditBoxOnLostFocus;

            Caption = inputBox.Text;
            // Creat TextBlock with Name text
            var nameTextBlock = new TextBlock
            {
                Margin = new Thickness(6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap, 
                TextTrimming = TextTrimming.CharacterEllipsis,
                Text = Caption,
            };

            // Creat grid with figure (Shape.Path) and nameTextBlock
            var grid = new Grid { Children = { shape, nameTextBlock } };

            this.Content = grid;
        }

        // Clear content
        private void ClearContent()
        {
            (this.Content as Grid)?.Children.Clear();
            this.Content = null;
        }
    }
}
