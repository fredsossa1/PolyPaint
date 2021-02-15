using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour Comment.xaml
    /// </summary>
    public partial class Comment : UserControl
    {
        public Comment()
        {
            InitializeComponent();
        }

        private void TextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

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
