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
    /// Interaction logic for passwordPrompt.xaml
    /// </summary>
    public partial class passwordPrompt : UserControl
    {
        public GalleryControl myGallery { get; set; }

        public passwordPrompt()
        {
            InitializeComponent();
        }

        private void EnterClick(object sender, RoutedEventArgs e)
        {
            myGallery.canvasPassword = passwordBox.Password;
            myGallery.passwordDialog.IsOpen = false;
            myGallery.joinProtectedCanvas();
        }
        
    }
}
