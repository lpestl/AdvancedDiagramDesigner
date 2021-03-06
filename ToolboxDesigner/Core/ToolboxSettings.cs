﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace ToolboxDesigner.Core
{
    public class ToolboxSettings
    {
        public string Name { get; set; }
        public ToolboxGrid ToolboxGridType { get; set; }
        public ToolboxItemSettingsCollection ItemsSettings { get; set; } = new ToolboxItemSettingsCollection();
    }

    public class ToolboxItemSettingsCollection : List<ToolboxItemSettings> { }

    public class ToolboxItemSettings
    {
        public string DisplayName { get; set; }
        public Style PathStyle { get; set; }
        public Style PathStyle_DragThumb { get; set; }
        public bool NoDelete { get; set; }
        public bool Invisible { get; set; }
        public bool Proportional { get; set; }
        public Container Container { get; set; }
        public ConnectorsSettingsCollection ConnectorsSettings { get; set; } //= new ConnectorsSettingsCollection();
        public PropertiesCollection Properties { get; set; } = new PropertiesCollection();
    }

    public class PropertiesCollection : List<Property> { }

    public class ConnectorsSettingsCollection : List<ConnectorSettings> { }

    public class Container
    {
        public Thickness RelativeMargin { get; set; } = new Thickness(0.1, 0.7, 0.1, 0.1);
    }

    public class ConnectorSettings
    {
        public string Name { get; set; }
        public ConnectorOrientation Orientation { get; set; } = ConnectorOrientation.None;
        public Point RelativePosition { get; set; }
        public uint MaxInConnections { get; set; } = uint.MaxValue;
        public uint MaxOutConnections { get; set; } = uint.MaxValue;
        public string Caption { get; set; }
    }
    
    public enum ConnectorOrientation
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    public enum ToolboxGrid
    {
        Grid,
        List
    }
}
