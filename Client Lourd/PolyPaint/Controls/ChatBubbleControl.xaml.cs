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
    /// Logique d'interaction pour ChatBubbleControl.xaml
    /// </summary>
    public partial class ChatBubbleControl : UserControl
    {
        public ChatBubbleControl(Message message, bool IsSent)
        {
            InitializeComponent();
            SenderName.Text = message.sender;
            MessageBubble.Text = message.content;
            TimeStamp.Text = message.timestamp.ToLocalTime().TimeOfDay.Hours.ToString("D2") 
				+ ':' + message.timestamp.TimeOfDay.Minutes.ToString("D2") 
				+ ':' + message.timestamp.TimeOfDay.Seconds.ToString("D2");
            
            SetMessageStyle(IsSent);
        }

        /// <summary>
        /// Change the style of the message bubble 
        /// </summary>
        private void SetMessageStyle(bool IsSent)
        {
            if (IsSent)
            {
                MessageBorder.Background = new SolidColorBrush(Colors.LightBlue);
                MessageBorder.HorizontalAlignment = HorizontalAlignment.Right;
                TimeStamp.HorizontalAlignment = HorizontalAlignment.Right;
                SenderName.Text = String.Empty;

            } else {
                MessageBorder.Background = new SolidColorBrush(Colors.LightGray);
                MessageBorder.HorizontalAlignment = HorizontalAlignment.Left;
                TimeStamp.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

    }
}
