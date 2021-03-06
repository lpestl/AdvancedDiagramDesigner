﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace DiagramDesigner.Functionality
{
    internal class SelectionService
    {
        private Functionality.DesignerCanvas designerCanvas;

        // UPD: Updated and readable style
        internal ObservableCollection<ISelectable> CurrentSelection { get; } = new ObservableCollection<ISelectable>();

        public SelectionService(Functionality.DesignerCanvas canvas)
        {
            this.designerCanvas = canvas;
        }

        internal DesignerItem GetSelectedDesignItem()
        {
            DesignerItem selectedItem = null;
            foreach (var selectable in CurrentSelection)
            {
                if (selectable is DesignerItem designerItem)
                {
                    selectedItem = designerItem;
                }
            }

            return selectedItem;
        }

        internal void SelectItem(ISelectable item)
        {
            this.ClearSelection();
            this.AddToSelection(item);
        }
        
        internal void AddToSelection(ISelectable item)
        {
            if (item is IGroupable)
            {
                List<IGroupable> groupItems = GetGroupMembers(item as IGroupable);

                foreach (ISelectable groupItem in groupItems)
                {
                    groupItem.IsSelected = true;
                    CurrentSelection.Add(groupItem);
                }
            }
            else
            {
                item.IsSelected = true;
                CurrentSelection.Add(item);
            }
        }

        internal void RemoveFromSelection(ISelectable item)
        {
            if (item is IGroupable)
            {
                List<IGroupable> groupItems = GetGroupMembers(item as IGroupable);

                foreach (ISelectable groupItem in groupItems)
                {
                    groupItem.IsSelected = false;
                    CurrentSelection.Remove(groupItem);
                }
            }
            else
            {
                item.IsSelected = false;
                CurrentSelection.Remove(item);
            }
        }

        internal void ClearSelection()
        {
            //CurrentSelection.ForEach(item => item.IsSelected = false);
            foreach (var item in CurrentSelection)
            {
                item.IsSelected = false;
            }
            CurrentSelection.Clear();
        }

        internal void SelectAll()
        {
            ClearSelection();
            //CurrentSelection.AddRange(designerCanvas.Children.OfType<ISelectable>());
            var children = designerCanvas.Children.OfType<ISelectable>();
            foreach (var selectable in children)
            {
                CurrentSelection.Add(selectable);
            }
            //CurrentSelection.ForEach(item => item.IsSelected = true);
            foreach (var item in CurrentSelection)
            {
                item.IsSelected = true;
            }
        }

        internal List<IGroupable> GetGroupMembers(IGroupable item)
        {
            IEnumerable<IGroupable> list = designerCanvas.Children.OfType<IGroupable>();
            IGroupable rootItem = GetRoot(list, item);
            return GetGroupMembers(list, rootItem);
        }

        internal IGroupable GetGroupRoot(IGroupable item)
        {
            IEnumerable<IGroupable> list = designerCanvas.Children.OfType<IGroupable>();
            return GetRoot(list, item);
        }

        private IGroupable GetRoot(IEnumerable<IGroupable> list, IGroupable node)
        {
            if (node == null || node.ParentID == Guid.Empty)
            {
                return node;
            }
            else
            {
                foreach (IGroupable item in list)
                {
                    if (item.ID == node.ParentID)
                    {
                        return GetRoot(list, item);
                    }
                }
                return null;
            }
        }

        private List<IGroupable> GetGroupMembers(IEnumerable<IGroupable> list, IGroupable parent)
        {
            List<IGroupable> groupMembers = new List<IGroupable>();
            groupMembers.Add(parent);

            var children = list.Where(node => node.ParentID == parent.ID);

            foreach (IGroupable child in children)
            {
                groupMembers.AddRange(GetGroupMembers(list, child));
            }

            return groupMembers;
        }
    }
}
