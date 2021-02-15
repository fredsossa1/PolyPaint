using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour GalleryControl.xaml
    /// </summary>
    public partial class GalleryControl : UserControl
    {
        public FenetreUML myDrawingWindow { get; set; }

        public string canvasPassword { get; set; }

        public GalleryControl()
        {
            InitializeComponent();
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            ModifyCanvas.myDrawingWindow = myDrawingWindow;
            CanvasBaseInfo canvasBase = myDrawingWindow.CanvasList.Find(canvas => canvas._id.Contains(((myListView.SelectedItem as ListViewItem).Content as TextBlock).Text));
            if(canvasBase.creator == myDrawingWindow.UserId)
            {
                ModifyCanvas.canvasNamePanel.Visibility = Visibility.Hidden;
                EditDialog.IsOpen = true;
            }else
            {
                MessageBox.Show("You can't edit this canvas properties\nbecause you are not the owner of this canvas","PolyPaint Pro");
            }
        }

        private void JoinClick(object sender, RoutedEventArgs e)
        {
            CanvasBaseInfo canvasBase = myDrawingWindow.CanvasList.Find(canvas => canvas._id.Contains(((myListView.SelectedItem as ListViewItem).Content as TextBlock).Text));
            if (canvasBase.password == "?")
            {
                passwordBox.myGallery = this;
                passwordDialog.IsOpen = true;
            }else
            {
                myDrawingWindow.JoinSession(canvasBase._id);
            }
        }

        internal void joinProtectedCanvas()
        {
            CanvasBaseInfo canvasBase = myDrawingWindow.CanvasList.Find(canvas => canvas._id.Contains(((myListView.SelectedItem as ListViewItem).Content as TextBlock).Text));
            myDrawingWindow.JoinSession(canvasBase._id, canvasPassword);
            passwordBox.passwordBox.Clear();
        }

        public void parseCanvasesList(List<CanvasBaseInfo> canvases)
        {
            myListView.Items.Clear();
            foreach(CanvasBaseInfo canvas in canvases)
            {
                TextBlock newItemTxtBlock = new TextBlock();
                newItemTxtBlock.Text = canvas._id;
                ListViewItem newItem = new ListViewItem();
                newItem.Content = newItemTxtBlock;
                myListView.Items.Add(newItem);
            }
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            CanvasBaseInfo canvasBase = myDrawingWindow.CanvasList.Find(canvas => canvas._id.Contains(((myListView.SelectedItem as ListViewItem).Content as TextBlock).Text));
            if (canvasBase.creator == myDrawingWindow.UserId)
            {
                Communication.Delete_Canvas(canvasBase);
            }
            else
            {
                MessageBox.Show("You can't delete this canvas properties\nbecause you are not the owner of this canvas", "PolyPaint Pro");
            }
        }
    }
}
