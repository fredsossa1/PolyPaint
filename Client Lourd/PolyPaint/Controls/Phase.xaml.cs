using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour Phase.xaml
    /// </summary>
    public partial class Phase : UserControl
    {
        public Phase()
        {
            InitializeComponent();
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
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
