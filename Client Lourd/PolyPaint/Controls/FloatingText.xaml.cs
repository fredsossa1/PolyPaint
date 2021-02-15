using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour FloatingText.xaml
    /// </summary>
    public partial class FloatingText : UserControl
    {
        public FloatingText()
        {
            InitializeComponent();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();
            myTextBox.Background = Brushes.Transparent;
            if (Communication.IsConnected())
            {
                DesignerItem designerItem = Parent as DesignerItem;
                DesignerCanvas designer = VisualTreeHelper.GetParent(designerItem) as DesignerCanvas;
                designer.SendModifyElement(designerItem.toElement());
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            myTextBox.Background = Brushes.LightGray;
        }
    }
}
