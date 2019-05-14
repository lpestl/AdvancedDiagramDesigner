using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace ToolboxDesigner.Core
{
    //[ContentProperty(nameof(ItemsSettings))]
    public class ToolboxSettings //: DependencyObject
    {
        //    public static readonly DependencyProperty NameProperty =
        //        DependencyProperty.Register("Name", typeof(string), typeof(ToolboxSettings));

        //    public string Name
        //    {
        //        get => (string)GetValue(NameProperty);
        //        set => SetValue(NameProperty, value);
        //    }

        //    public static readonly DependencyProperty ItemsSettingsProperty =
        //        DependencyProperty.Register("ItemsSettings", typeof(IList<ToolboxItemSettings>), typeof(ToolboxSettings),
        //            new PropertyMetadata(new List<ToolboxItemSettings>()));


        //    public IList<ToolboxItemSettings> ItemsSettings
        //    {
        //        get => (IList<ToolboxItemSettings>)GetValue(ItemsSettingsProperty);
        //        set => SetValue(ItemsSettingsProperty, value);
        //    }

        public string Name { get; set; }

        public ToolboxItemSettingsCollection ItemsSettings { get; set; } = new ToolboxItemSettingsCollection();
    }

    public class ToolboxItemSettingsCollection : List<ToolboxItemSettings> { }

    //[ContentProperty(nameof(ConnectorsSettingsCollection))]
    public class ToolboxItemSettings
    {
        public string DisplayName { get; set; }

        public Style PathStyle { get; set; }

        public Style PathStyle_DragThumb { get; set; }

        public ConnectorsSettingsCollection ConnectorsSettings { get; set; } = new ConnectorsSettingsCollection();
    }

    public class ConnectorsSettingsCollection : List<ConnectorSettings> { }

    public class ConnectorSettings
    {
        public string Name { get; set; }

        public ConnectorOrientation Orientation { get; set; } = ConnectorOrientation.None;

        public Point RelativePosition { get; set; }
    }
    
    public enum ConnectorOrientation
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }
}
