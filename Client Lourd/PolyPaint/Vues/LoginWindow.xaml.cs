using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Shapes;
using PolyPaint;
using Newtonsoft.Json;

namespace PolyPaint
{
    /// <summary>
    /// Logique d'interaction pour LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public Thread ReadingThread;
        public string _username;
        public string _hostname = "35.203.113.71";
        private string _password;
        public int _port;
        private FenetreUML myDrawingWindow;
        private int nbrOfAttemps;
        private bool isNewUser = false;

        public LoginWindow()
        {
            InitializeComponent();

#if DEBUG
            ServerIpInput.Visibility = Visibility.Visible;
#endif
            ReadingThread = new Thread(new ThreadStart(ThreadRead));
            ReadingThread.Start();
        }
        
        private void Offline_Click(object sender, RoutedEventArgs e)
        {
            if (myDrawingWindow == null)
            {
                myDrawingWindow = new FenetreUML();
            }
            if(!myDrawingWindow.IsActive)
            {
                myDrawingWindow.Activate();
                myDrawingWindow.Owner = this;
            }
            myDrawingWindow.SignOutButton.Content = "Connect";
            myDrawingWindow.Show();
            this.Hide();
         
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Communication.IsConnected() 
                && (_username == userName.Text)
                && (_hostname == IpAddress_Box.Text)
                && (!isNewUser))
            {
                myDrawingWindow.Show();
                this.Hide();
            }
            else
            {
                if (Communication.IsConnected()) Communication.Disconnect_Client();

                if (userName.Text.Length == 0)
                {
                    MessageBox.Show("Username missing: Please enter a username");
                    return;
                }
#if DEBUG
                else if (!ValidateIP(IpAddress_Box.Text))
                {
                    MessageBox.Show("Invalid IP: Please reenter a valid IP");
                    return;
                }
                _hostname = IpAddress_Box.Text;
#endif
                _username = userName.Text;
                _password = Password_Box.Password;
                _port = 5000;
                try
                {
                    Communication.Connect_Client(_hostname, _port);
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                isNewUser = false;
                Communication.Authenticate(_username, _password, isNewUser);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ReadingThread.Abort();
            if (Communication.IsConnected())
            {
                Communication.Disconnect_Client();
            }
            Application.Current.Shutdown();
        }

        public static bool ValidateIP(string ipString)
        {
            if (ipString.Count(c => c == '.') != 3) return false;
            IPAddress address;
            return IPAddress.TryParse(ipString, out address);
        }


        private void TryReconnect(Object sender, System.Timers.ElapsedEventArgs args)
        {
            nbrOfAttemps++;
            Communication.Connect_Client(_hostname, _port);
            Communication.Authenticate(_username, _password, false);
        }

        /// <summary>
        /// Function executed by the thread to read what the server sent
        /// and add it to the chat window
        /// </summary>
        private void ThreadRead()
        {
            bool loadConnections = false;
            //Data packet;
            while (true)
            {
                while (Communication.IsConnected())
                {
                    List<Data> packets = Communication.Read_Data();
                    foreach (dynamic packet in packets)
                    {
                        if(packet.type == DataType.AUTH)
                        {
                            AuthenticationResponse authPacket = (AuthenticationResponse)packet;
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                switch (authPacket.response)
                                {
                                    case (AuthenticationResponseType.AUTH_SUCCESS):
                                        // Set Environment
                                        if (myDrawingWindow == null)
                                        {
                                            myDrawingWindow = new FenetreUML();
                                        }
                                        if (!myDrawingWindow.IsActive)
                                        {
                                            myDrawingWindow.Activate();
                                            myDrawingWindow.Owner = this;
                                        }

                                        myDrawingWindow.UserId = _username;
                                        myDrawingWindow.MyCanvas.UserId = _username;
                                        // TODO Handle Channels properly
                                        myDrawingWindow.mainChat.setUsername(_username);
                                        myDrawingWindow.mainChat.setChannel("Main");
                                        Communication.send_Request(DataRequestType.PUBLIC_CANVAS_LIST);
                                        myDrawingWindow.SignOutButton.Content = "Sign out";
                                        myDrawingWindow.Show();
                                        this.Hide();
                                        break;
                                    case (AuthenticationResponseType.AUTH_FAIL):
                                        MessageBox.Show("Authentification failed!\nPlease verify your credentials and try again \n", "PolyPaint Pro");
                                        break;
                                    case (AuthenticationResponseType.TAKEN_ID):
                                        Communication.Disconnect_Client();
                                        MessageBox.Show("ID is already in use. \nTry another username? \n", "PolyPaint Pro");
                                        this.Show();
                                        break;
                                    case (AuthenticationResponseType.SIGNUP_SUCCESS):
                                        MessageBox.Show("Signup Success! Please Sign In", "PolyPaint Pro");
                                        break;
                                    case (AuthenticationResponseType.SIGNUP_FAIL):
                                        MessageBox.Show("Signup Failed! Please Try again", "PolyPaint Pro");
                                        break;
                                    default:
                                        break;
                                }
                            }));
                        }
                        else if (packet.type == DataType.MSG)
                        {
                            bool IsSameSender = (packet.sender == _username) ? true : false;
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                myDrawingWindow.mainChat.addMessageBubble((Message)packet, IsSameSender);
                            }));
                        }
                        else if (packet.type == DataType.EVENT)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Event eventPacket = (Event)packet;
                                if (eventPacket.source != "notFound")
                                {
                                    if (eventPacket.name == "clientConnection")
                                    {
                                        Message message = new Message();
                                        message.content = "'" + eventPacket.source + "' just joined the chat!";
                                        message.timestamp = DateTime.Now;
                                        myDrawingWindow.mainChat.addMessageBubble(message, false);
                                    }
                                    else if (eventPacket.name == "clientDisconnection")
                                    {
                                        Message message = new Message();
                                        message.content = "'" + eventPacket.source + "' just left the chat!";
                                        message.timestamp = DateTime.Now;
                                        myDrawingWindow.mainChat.addMessageBubble(message, false);
                                    }
                                    else if (eventPacket.name == CollabEventType.CREATE_CANVAS)
                                    {
                                        if(eventPacket.source == _username)
                                        {
                                            myDrawingWindow.MyCanvas.Reinitialiser(false);
                                            myDrawingWindow.CanvasList.Add(new CanvasBaseInfo { creator = eventPacket.source, _id = eventPacket.context });
                                            myDrawingWindow.MyCanvas.CanvasID = eventPacket.context;
                                            myDrawingWindow.MyCanvas.OwnerId = eventPacket.source;
                                        }else
                                        {
                                            myDrawingWindow.CanvasList.Add(new CanvasBaseInfo { creator = eventPacket.source, _id = eventPacket.context });
                                        }
                                    }
                                    else if(eventPacket.name == CollabEventType.DELETE_CANVAS)
                                    {
                                        if (eventPacket.source == _username)
                                        {
                                            if(eventPacket.context == myDrawingWindow.MyCanvas.CanvasID)
                                                myDrawingWindow.MyCanvas.Reinitialiser(false);
                                        }
                                        myDrawingWindow.CanvasList.Remove(myDrawingWindow.getCanvasById(eventPacket.context));
                                    }
                                    else if ((eventPacket.name == CollabEventType.SESSION_CREATE)
                                            || (eventPacket.name == CollabEventType.SESSION_REMOVE))
                                    {
                                        //Do nothing
                                    }
                                    else if (eventPacket.name == CollabEventType.SESSION_JOIN)
                                    {
                                        if(eventPacket.context == myDrawingWindow.MyCanvas.CanvasID)
                                        {
                                            myDrawingWindow.MyCanvas.collaborators.Add(new Collaborator { userId = eventPacket.source });
                                        }
                                    }
                                    else if (eventPacket.name == CollabEventType.SESSION_LEAVE)
                                    {
                                        if(eventPacket.context == myDrawingWindow.MyCanvas.CanvasID)
                                        {
                                            myDrawingWindow.MyCanvas.collaborators.Remove(myDrawingWindow.MyCanvas.getCollaboratorById(eventPacket.source));
                                        }
                                    }
                                    else if ((eventPacket.name == ChatEventType.CHAT_CREATE)
                                            || (eventPacket.name == ChatEventType.CHAT_JOIN)
                                            || (eventPacket.name == ChatEventType.CHAT_LEAVE)
                                            || (eventPacket.name == ChatEventType.CHAT_REMOVE))
                                    {
                                        //TODO HANDLE CHAT EVENTS
                                    }
                                    else
                                    {
                                        MessageBox.Show("UNhandled Event packet! Content: '\n'" + JsonConvert.SerializeObject(eventPacket));
                                    }
                                }
                            }));
                        }else if(packet.type == DataType.OPERATION)
                        {
                            Operation opPacket = (Operation)packet;
                            if (opPacket.canvasId == myDrawingWindow.MyCanvas.CanvasID)
                            {
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    if (opPacket.operation == OperationType.CREATE)
                                    {
                                        CreateOperation crOp = (CreateOperation)packet;
                                        if(crOp.createType == OperationTargetType.ELEMENT)
                                        {
                                            CreateElementOperation crEl = (CreateElementOperation)packet;
                                            myDrawingWindow.MyCanvas.AddToChildren(crEl.element);
                                        }else if(crOp.createType == OperationTargetType.CONNECTION)
                                        {
                                            CreateConnectionOperation crCo = (CreateConnectionOperation)packet;
                                            myDrawingWindow.MyCanvas.AddConnectionToChildren(crCo.element);
                                        }
                                        
                                    }
                                    else if (opPacket.operation == OperationType.MODIFY)
                                    {
                                        if(opPacket.userId != _username)
                                        {
                                            ModifyOperation mdPacket = (ModifyOperation)packet;
                                            switch (mdPacket.targetType)
                                            {
                                                case (OperationTargetType.ELEMENT):
                                                    ElementModifyOperation elMdPacket = (ElementModifyOperation)packet;
                                                    myDrawingWindow.MyCanvas.handleModifyOperation(elMdPacket.element);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    else if (opPacket.operation == OperationType.SELECT)
                                    {
                                        SelectOperation selectPacket = (SelectOperation)packet;
                                        myDrawingWindow.MyCanvas.updateSelection(selectPacket, (selectPacket.userId == _username) ? true : false);
                                        
                                    }
                                    else if (opPacket.operation == OperationType.DELETE)
                                    {
                                        DeleteOperation dlOp = (DeleteOperation)packet;
                                        myDrawingWindow.MyCanvas.effacer(false, dlOp.element, dlOp.deleteType);
                                    }
                                    else if (opPacket.operation == OperationType.RESET)
                                    {
                                        if(opPacket.canvasId == myDrawingWindow.MyCanvas.CanvasID)
                                        {
                                            myDrawingWindow.MyCanvas.Reinitialiser(false);
                                        }
                                    }
                                }));
                            }
                        }
                        else if (packet.type == DataType.RESPONSE)
                        {
                            Response rsPacket = (Response)packet;
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                switch (rsPacket.request)
                                {
                                    case (DataRequestType.SESSION):
                                        myDrawingWindow.dialog2.IsOpen = false;
                                        myDrawingWindow.CloseMenu();
                                        myDrawingWindow.MyCanvas.HandleSessionJoin(rsPacket.data);
                                        break;
                                    case (DataRequestType.CHAT_LIST):
                                        myDrawingWindow.parseChannelsList(rsPacket.data.channels);
                                        break;
                                    case (DataRequestType.PUBLIC_CANVAS_LIST):
                                        myDrawingWindow.parseCanvasList(rsPacket.data.canvases);
                                        break;
                                    default:
                                        MessageBox.Show("Response packet! Content: '\n'" + JsonConvert.SerializeObject(packet));
                                        break;
                                }
                            }));
                            if (rsPacket.request == DataRequestType.SESSION) //Load Connections after all other items finished rendering
                            {
                                loadConnections = true;
                            }
                        }
                        else if (packet.type == DataType.ERROR)
                        {
                            Error erPacket = (Error)packet;
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                
                                {
                                    MessageBox.Show(erPacket.message);
                                }
                            }));
                        }else
                        {
#if DEBUG
                            MessageBox.Show("UNhandled packet! Content: '\n'"+ JsonConvert.SerializeObject(packet));
#endif
                        }
                    }
                    if (loadConnections)
                    {
                        System.Threading.Thread.Sleep(500); // Wait  100 ms for the canvas to load DesignerItems before adding connections
                        this.Dispatcher.BeginInvoke(new Action(() => {
                                foreach (CanvasConnection connection in myDrawingWindow.MyCanvas.currentCanvasData.connections)
                                    myDrawingWindow.MyCanvas.AddConnectionToChildren(connection);
                                myDrawingWindow.MyCanvas.HandleSessionSelection();
                            }));
                         loadConnections = false;
                    }
                }
            }
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (Communication.IsConnected()) Communication.Disconnect_Client();

            if (userName.Text.Length == 0)
            {
                MessageBox.Show("Username missing: Please enter a username");
                return;
            }
#if DEBUG
            else if (!ValidateIP(IpAddress_Box.Text))
            {
                MessageBox.Show("Invalid IP: Please reenter a valid IP");
                return;
            }
            _hostname = IpAddress_Box.Text;
#endif
            _username = userName.Text;
            _password = Password_Box.Password;
            _port = 5000;
            try
            {
                Communication.Connect_Client(_hostname, _port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            isNewUser = true;
            Communication.Authenticate(_username, _password, isNewUser);
            
        }
    }
}
