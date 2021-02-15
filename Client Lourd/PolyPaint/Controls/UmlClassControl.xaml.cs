using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour UmlClassControl.xaml
    /// </summary>
    public partial class UmlClassControl : UserControl
    {
        //private System.Timers.Timer timer = null;
        public UmlClassControl()
        {
            InitializeComponent();

        }
      
        private void TextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Keyboard.ClearFocus();
            if (Communication.IsConnected())
            {
                DesignerItem designerItem = Parent as DesignerItem;
                DesignerCanvas designer = VisualTreeHelper.GetParent(designerItem) as DesignerCanvas;
                designer.SendModifyElement(designerItem.toElement());
            }
        }
    }
}
