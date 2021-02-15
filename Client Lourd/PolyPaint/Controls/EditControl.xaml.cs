using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour EditControl.xaml
    /// </summary>
    public partial class EditControl : UserControl
    {
        public FenetreUML myDrawingWindow { get; set; }

        public EditControl()
        {
            InitializeComponent();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            switch (this.Name)
            {
                case ("CreateCanvas"):
                    myDrawingWindow.CreateNewCanvas( this.canvasName.Text, this.passwordBox.Password);
                    break;
                case ("ModifyCanvas"):
                    myDrawingWindow.ModifyCanvas(this.passwordBox.Password);
                    break;
                default:break;
            }

        }
    }
}
