using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


//    Source : https://www.codeproject.com/Articles/23871/WPF-Diagram-Designer-Part
//    Author : sukram 
//    Date : 29 Feb 2008
    
namespace PolyPaint
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItem designerItem = this.DataContext as DesignerItem;
            DesignerCanvas designer = VisualTreeHelper.GetParent(designerItem) as DesignerCanvas;
            if (designerItem != null && designer != null && designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                var designerItems = from item in designer.SelectedItems
                                    where item is DesignerItem
                                    select item;

                foreach (DesignerItem item in designerItems)
                {
                    double left = Canvas.GetLeft(item);
                    double top = Canvas.GetTop(item);

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (DesignerItem item in designerItems)
                {
                    double left = Canvas.GetLeft(item);
                    double top = Canvas.GetTop(item);

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    Canvas.SetLeft(item, left + deltaHorizontal);
                    Canvas.SetTop(item, top + deltaVertical);
                    item.itemPosition = new CanvasPosition
                    {
                        x = (int)(left + deltaHorizontal),
                        y = (int)(top + deltaVertical),
                    };
                    if(Communication.IsConnected())
                        designer.SendModifyElement(item.toElement());
                }

                designer.InvalidateMeasure();
                e.Handled = true;
            }
        }
    }
}
