//    Source : https://www.codeproject.com/Articles/23871/WPF-Diagram-Designer-Part
//    Author : sukram 
//    Date : 29 Feb 2008
namespace PolyPaint
{
    // Common interface for items that can be selected
    // on the DesignerCanvas; used by DesignerItem and Connection
    public interface ISelectable
    {
        bool IsSelected { get; set; }
        bool IsLocked { get; set; }
        string _id { get; set; }
        string type { get; }
    }
}
