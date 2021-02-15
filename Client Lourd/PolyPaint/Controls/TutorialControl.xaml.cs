using System.Windows.Controls;
using System;
using System.Windows.Media.Imaging;



namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour TutorialControl.xaml
    /// </summary>
    public partial class TutorialControl : UserControl
    {
        // ajouter un array avec les paths pour les images du tuto
        public TutorialControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrentImage.Source = new BitmapImage(new Uri("../Resources/Images/Role.png", UriKind.Relative));
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrentImage.Source = new BitmapImage(new Uri("../Resources/Images/Artefact.png", UriKind.Relative));
        }
    }
}
