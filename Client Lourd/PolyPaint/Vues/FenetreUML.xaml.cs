using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows.Input;

namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour FenetreUML.xaml
    /// </summary>
    public partial class FenetreUML : Window
    {
        public FenetreUML()
        {
            InitializeComponent();
            Copier.Click += new RoutedEventHandler(DupliquerSelection);
            Couper.Click += new RoutedEventHandler(couper);
            addClassButton.Click += new RoutedEventHandler(addClass);
            addPhaseButton.Click += new RoutedEventHandler(addPhase);
            addCommentButton.Click += new RoutedEventHandler(addComment);
            addFloatingTextButton.Click += new RoutedEventHandler(addFloatingText);
            exportFileButton.Click += new RoutedEventHandler(ExportFile);
            addArtefactButton.Click += new RoutedEventHandler(AddArtefact);
            addRoleButton.Click += new RoutedEventHandler(AddRole);
            addActivityButton.Click += new RoutedEventHandler(AddActivity);
            lassoButton.Click += new RoutedEventHandler(ActiverLasso);
            ButtonOpen.Click += new RoutedEventHandler(ButtonOpen_Click);
            ButtonClose.Click += new RoutedEventHandler(ButtonClose_Click);
            ButtonOpenChat.Click += new RoutedEventHandler(OpenChat);
            ButtonCloseChat.Click += new RoutedEventHandler(CloseChat);
            ChannelsList = new List<string>();
            CanvasList = new List<CanvasBaseInfo>();
            CreateCanvas.myDrawingWindow = this;
            MyGallery.myDrawingWindow = this;
        }

        internal void ModifyCanvas(string password)
        {
            CanvasBaseInfo canvasBase = CanvasList.Find(canvas => canvas._id.Contains(((MyGallery.myListView.SelectedItem as ListViewItem).Content as TextBlock).Text));
                       
            CanvasModifyOperation modifyCanvas = new CanvasModifyOperation
            {
                canvasId = canvasBase._id,
                field = "password",
                value = password
            };
            Communication.Send_Operation(modifyCanvas);
            Communication.send_Request(DataRequestType.PUBLIC_CANVAS_LIST);
            MyGallery.EditDialog.IsOpen = false;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            ButtonClose.Visibility = Visibility.Collapsed;
            ButtonOpen.Visibility = Visibility.Visible;
            UserName.Visibility = Visibility.Hidden;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            ButtonClose.Visibility = Visibility.Visible;
            ButtonOpen.Visibility = Visibility.Collapsed;
            UserName.Visibility = Visibility.Visible;
            if (Communication.IsConnected())
            {
                UsernameText.Text = UserId;
            }
        }

        private void OpenChat(object sender, RoutedEventArgs e)
        {
            Storyboard sb = this.FindResource("OpenChat") as Storyboard;
            if (sb != null) { BeginStoryboard(sb); }
            ButtonCloseChat.Visibility = Visibility.Visible;
            ButtonOpenChat.Visibility = Visibility.Collapsed;
            mainChat.Visibility = Visibility.Visible;
        }

        private void CloseChat(object sender, RoutedEventArgs e)
        {
            Storyboard sb = this.FindResource("CloseChat") as Storyboard;
            if (sb != null) { BeginStoryboard(sb); }
            ButtonCloseChat.Visibility = Visibility.Collapsed;
            ButtonOpenChat.Visibility = Visibility.Visible;
            mainChat.Visibility = Visibility.Collapsed;
        }


        public string UserId { get; set; } = "Not connected";
        public List<string> ChannelsList { get; set; }
        public List<CanvasBaseInfo> CanvasList { get; set; }

        private void DupliquerSelection(object sender, RoutedEventArgs e)
        {
            MyCanvas.dupliquer(Communication.IsConnected());
        }

        private void couper(object sender, RoutedEventArgs e)
        {
            MyCanvas.couper(Communication.IsConnected());
        }

        //private void SupprimerSelection(object sender, RoutedEventArgs e) => surfaceDessin.CutSelection();

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.Owner != null)
            {
                e.Cancel = true;
                this.Hide();
                this.Owner.Show();
            }
            else
            {
                Close();
            }
        }

        private void GlisserCommence(object sender, DragStartedEventArgs e) => (sender as Thumb).Background = Brushes.Black;
        private void GlisserTermine(object sender, DragCompletedEventArgs e) => (sender as Thumb).Background = Brushes.White;
        private void GlisserMouvementRecu(object sender, DragDeltaEventArgs e)
        {
            String nom = (sender as Thumb).Name;
            if (nom == "horizontal" || nom == "diagonal") colonne.Width = new GridLength(Math.Max(32, colonne.Width.Value + e.HorizontalChange));
            if (nom == "vertical" || nom == "diagonal") ligne.Height = new GridLength(Math.Max(32, ligne.Height.Value + e.VerticalChange));
        }

        private void addClass(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddClass(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }

        private void addPhase(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddPhase(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }

        private void addComment(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddComment(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }
        private void addFloatingText(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddFloatingText(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }

        private void Reinitialiser(object sender, RoutedEventArgs e)
        {
            MyCanvas.Reinitialiser(Communication.IsConnected());
        }

        private void Empiler(object sender, RoutedEventArgs e)
        {
            MyCanvas.empiler(Communication.IsConnected());
        }

        private void Depiler(object sender, RoutedEventArgs e)
        {
            MyCanvas.depiler(Communication.IsConnected());
        }

        private void Effacer(object sender, RoutedEventArgs e)
        {
            MyCanvas.effacer(Communication.IsConnected());
        }

        private void ExportFile(object sender, RoutedEventArgs e)
        {
            MyCanvas.ExportFile();
        }

        private void AddRole(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddRole(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }

        private void AddArtefact(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddArtefact(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }

        private void AddActivity(object sender, RoutedEventArgs e)
        {
            MyCanvas.AddActivity(Communication.IsConnected());
            lassoButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        }
        private void ActiverLasso(object sender, RoutedEventArgs e)
        {
            MyCanvas.ActiverLasso();
            ((Button)sender).Background = Brushes.Gray;
        }

        public void CreateNewCanvas(string canvasName, string password)
        {
            Communication.Create_Canvas(MyCanvas.UserId, canvasName, password);
            createDialog.IsOpen = false;
            CloseMenu();
        }

        public void JoinSession(string canvasId, string password = "")
        {
            Communication.Join_Session(canvasId, MyCanvas.UserId, password);
        }

        public void CloseMenu()
        {
            Storyboard sb = this.FindResource("CloseMenu") as Storyboard;
            if (sb != null) { BeginStoryboard(sb); }
            ButtonOpen.Visibility = Visibility.Visible;
            UserName.Visibility = Visibility.Collapsed;
            ButtonClose.Visibility = Visibility.Collapsed;
            UserName.Visibility = Visibility.Hidden;
        }
        
        private void DialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {}

        private void Tutorial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dialog1.IsOpen = true;
        }

        private void NewCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            createDialog.IsOpen = true;
        }

        internal CanvasBaseInfo getCanvasById(string canvasId)
        {
            foreach(CanvasBaseInfo canvas in CanvasList)
            {
                if (canvas._id == canvasId)
                    return canvas;
            }
            return null;
        }

        private void Gallery_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dialog2.IsOpen = true;
        }

        internal void parseChannelsList(string[] channels)
        {
            foreach (string channel in channels)
                ChannelsList.Add(channel);
        }

        internal void parseCanvasList(CanvasBaseInfo[] canvases)
        {
            CanvasList.Clear();
            foreach (CanvasBaseInfo canvas in canvases)
                CanvasList.Add(canvas);
            MyGallery.parseCanvasesList(CanvasList);
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            if (Communication.IsConnected())
            {
                Communication.Disconnect_Client();
                MyCanvas.Reinitialiser(false);
                MyCanvas.CanvasID = null;
                MyCanvas.UserId = null;
                UsernameText.Visibility = Visibility.Hidden;
                SignOutButton.Content = "Connect";
            }else
            {
                this.Owner.Show();
                this.Hide();
            }
            CloseMenu();
        }
    }   
}
