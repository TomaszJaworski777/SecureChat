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

namespace SecureChat.Client.Views.ChatViewComponents
{
    public partial class MessageEntry : UserControl
    {
        public MessageEntry(string username, string message, string date, bool isOurs)
        {
            InitializeComponent();

            MessageUsername.Text = username;
            MessageBorder.Background = isOurs ?
                new SolidColorBrush(Color.FromRgb(45, 125, 70)) :
                new SolidColorBrush(Color.FromRgb(96, 96, 96));

            MessageRoot.Margin = isOurs ?
                new Thickness(150, 0, 20, 5) :
                new Thickness(20, 5, 150, 0);

            MessageRoot.HorizontalAlignment = isOurs ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            MessageText.Text = message;
            MessageDate.Text = date;
        }
    }
}
