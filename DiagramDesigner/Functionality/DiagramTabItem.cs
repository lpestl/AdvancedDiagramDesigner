﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DiagramDesigner.Functionality
{
    public class DiagramTabItem
    {
        // This will be the text in the tab control
        public string Header { get; set; }
        // This will be the content of the tab control It is a UserControl whits you need to create manualy
        public UserControl Content { get; set; }
    }
}
