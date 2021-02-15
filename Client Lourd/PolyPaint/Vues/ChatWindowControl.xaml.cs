using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for myChatWindowControl.xaml
    /// </summary>
    public partial class ChatWindowControl : UserControl
    {
        private string _username;
        private string _channel;

        public ChatWindowControl()
        {
            InitializeComponent();
        }
      
        public ChatWindowControl(string username, string channel = "Main")
        {
            if (!this.IsInitialized)
            {
                InitializeComponent();
            }
            _username = username;
            _channel = channel;
        }

        public void setUsername( string username)
        {
            _username = username;
        }

        public string getUsername()
        {
            return _username;
        }

        public void setChannel( string channel)
        {
            _channel = channel;
        }

        public string getChannel()
        {
            return _channel;
        }
        /// <summary>
        /// Add the text bubble in the window and send the text to server
        /// </summary>
        private void SendTextMessage(object sender, RoutedEventArgs e)
        {
            String TextToSend = CheckMessage();
            if (TextToSend != null)
            {
                Communication.Send_Message(DataType.MSG, getUsername(), TextToSend, getChannel());
            }

        }

        /// <summary>
        /// Send the message when the Enter button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendTextMessage(messageBox, null);
        }

        /// <summary>
        /// Display the message bubble in the window
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sent"></param>
        public void addMessageBubble(Message message, bool sent)
        {
            ChatBubbleControl newMessageBubble = new ChatBubbleControl(message, sent);
            stackPanel.Children.Add(newMessageBubble);
            if (sent)
            {
                messageBox.Text = String.Empty;
            }
            ScrollViewer.ScrollToEnd();
        }

        /// <summary>
        /// Check the string in the text box is not empty and remove useless characters
        /// </summary>
        /// <returns> Text to be sent </returns>
        private String CheckMessage()
        {
            String TextToSend = messageBox.Text;
            if (String.IsNullOrWhiteSpace(TextToSend))
                return null;
            Char[] charToRemove = { ' ', '\t', '\n', '\v', '\r' };
            TextToSend = TextToSend.TrimStart(charToRemove);
            TextToSend = TextToSend.TrimEnd(charToRemove);
            return TextToSend;
        }

    }
}
