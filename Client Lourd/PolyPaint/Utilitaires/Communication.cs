using System;
using System.Text;
using System.Net.Sockets;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace PolyPaint
{
    // ENUM
    public enum DataType
    {
        AUTH = 0,   // Authentication
        MSG = 1,    // Chat message
        ERROR = 2,  // Error 
        EVENT = 3,  // Event 
        OPERATION = 4, // Operation
        REQUEST = 5, // Request for data
        RESPONSE = 6, // Response to data request
    }

    public enum OperationType
    {
        CREATE = 0,
        DELETE = 1,
        MODIFY = 2,
        SELECT = 3,
        RESET = 4,
    }

    public static class ElementType
    {
        public const string ELEMENT = "element";
        public const string ROLE = "role";
        public const string ACTIVITY = "activity";
        public const string ARTEFACT = "artefact";
        public const string PHASE = "phase";
        public const string COMMENT = "comment";
        public const string FLOATINGTEXT = "floatingtext";
        public const string UMLCLASS = "umlclass";
        public const string CONNECTION = "connection";
    }

    public static class CollabEventType
    {
        public const string SESSION_JOIN = "sessionJoin";
        public const string SESSION_LEAVE = "sessionLeave";
        public const string SESSION_CREATE = "sessionCreate";
        public const string SESSION_REMOVE = "sessionRemove";
        public const string CREATE_CANVAS = "createCanvas";
        public const string DELETE_CANVAS = "deleteCanvas";
    }

    public static class ChatEventType
    {
        public const string CHAT_JOIN = "chatJoin";
        public const string CHAT_LEAVE = "chatLeave";
        public const string CHAT_CREATE = "chatCreate";
        public const string CHAT_REMOVE = "chatRemove";
    }

    public static class ConnectionDirection
    {
        public const string IN = "in";
        public const string OUT = "out";
    }

    public enum ConnectionSide
    {
        NONE = 0,
        LEFT = 1,
        TOP = 2,
        RIGHT = 3,
        BOTTOM = 4,
    }

    public static class ConnectionType
    {
        public const string CLASS = "class";
        public const string PROCESS = "process";
    }

    public static class OperationTargetType
    {
        public const string CANVAS = "canvas";
        public const string ELEMENT = "element";
        public const string CONNECTION = "connection";
    }

    public static class SelectType
    {
        public const string SELECT = "select";
        public const string DESELECT = "deselect";
        public const string DESELECT_ALL = "deselectAll";
    }

    public static class DataRequestType
    {
        public const string PUBLIC_CANVAS_LIST = "publicCanvasList";
        public const string USER_CANVAS_LIST = "userCanvasList";
        public const string PUBLIC_SESSION_LIST = "publicSessionList";
        public const string CHAT_LIST = "chatList";
        public const string CANVAS = "canvas";
        public const string CHAT = "chat";
        public const string SESSION = "session";
    }

    public static class AuthenticationResponseType
    {
        public const string AUTH_SUCCESS = "authSuccess"; // Authentication sucessful
        public const string AUTH_FAIL = "authFail"; // Authentication failed
        public const string SIGNUP_SUCCESS = "signupSuccess"; // Signup successful
        public const string SIGNUP_FAIL = "signupFail"; // Signup failed
        public const string TAKEN_ID = "signupIdTaken"; // ID is already taken (Signup)
    }

    //PACKETS INTERFACES
    public class Data
    {
        public virtual DataType type { get; set; }
    }

    public class Message : Data
    {
		public override DataType type { get; set; } = DataType.MSG;
		public string sender { get; set; }
        public string content { get; set; }
        public string channel { get; set; }
        public DateTime timestamp { get; set; }
	}

    public class Authentication : Data
    {
        public override DataType type { get; set; } = DataType.AUTH;
        public bool isNew { get; set; }
		public string id { get; set; }
        public string password { get; set; }
    }

    public class Error : Data
    {
        public override DataType type { get; set; } = DataType.ERROR;
		public string message { get; set; }
    }

    public class Event : Data
    {
        public override DataType type { get; set; } = DataType.EVENT;
		public string name { get; set; }
        public string context { get; set; }
		public string source { get; set; }
		public DateTime timestamp { get; set; }
        public string password { get; set; }
	}

    public class Operation : Data
    {
        public override DataType type { get; set; } = DataType.OPERATION;
        public virtual OperationType operation { get; set; }
        public string userId { get; set; }
        public string canvasId { get; set; }
    }

    public class CreateOperation: Operation
    {
        public override OperationType operation { get; set; } = OperationType.CREATE;
        public virtual string createType { get; set; }
    }

    public class CreateElementOperation: CreateOperation
    {
        public override string createType { get; set; } = OperationTargetType.ELEMENT;
        public Element element { get; set; }
    }

    public class CreateConnectionOperation: CreateOperation
    {
        public override string createType { get; set; } = OperationTargetType.CONNECTION;
        public CanvasConnection element { get; set; }
    }

    public class SelectOperation : Operation
    {
        public override OperationType operation { get; set; } = OperationType.SELECT;
        public string selectType { get; set; }
        public string[] elements { get; set; }
    }

    public class DeleteOperation : Operation
    {
        public override OperationType operation { get; set; } = OperationType.DELETE;
        public string deleteType { get; set; }
        public string element { get; set; }
    }

    public class ResetOperation : Operation
    {
        public override OperationType operation { get; set; } = OperationType.RESET;
    }

    public class ModifyOperation : Operation
    {
        public override OperationType operation { get; set; } = OperationType.MODIFY;
        public virtual string targetType { get; set; }
    }

    public class CanvasModifyOperation : ModifyOperation
    {
        public override string targetType { get; set; } = OperationTargetType.CANVAS;
        public string field { get; set; }
        public string value { get; set; }
    }

    public class ElementModifyOperation : ModifyOperation
    {
        public override string targetType { get; set; } = OperationTargetType.ELEMENT;
        public Element element { get; set; }
    }

    public class Request: Data
    {
        public override DataType type { get; set; } = DataType.REQUEST;
        public string request { get; set; }
    }
    public class Response: Data
    {
        public override DataType type { get; set; } = DataType.RESPONSE;
        public string request { get; set; }
        public DataResponse data { get; set; }
    }
    public class DataResponse
    {
        public string[] channels { get; set; }
        public CanvasBaseInfo[] sessions { get; set; }
        public CanvasBaseInfo[] canvases { get; set; }
        public CanvasData canvasData { get; set; }
        public Collaborator[] collaborators { get; set; }
        public string chatID { get; set; }
        public string[] users { get; set; }
    }
    public class AuthenticationResponse : Data
    {
        public override DataType type { get; set; } = DataType.AUTH;
        public string response { get; set; }
    }
    public class CanvasData : WithDimensions
    {
        public string _id                      { get; set; }
        public string creator                  { get; set; }
        public DateTime timestamp              { get; set; }
        public bool isPrivate                  { get; set; }
        public string password                 { get; set; }
        public Element[] elements              { get; set; }
        public CanvasConnection[] connections  { get; set; }
    }
    public class CanvasBaseInfo
    {
        public string _id { get; set; }
        public string creator { get; set; }
        public bool isPrivate { get; set; }
        public string password { get; set; }
    }
    public class Element : WithDimensions
    {
        public string id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public CanvasPosition position { get; set; }
        public double rotation { get; set; }
        public UmlClassContent content { get; set; }
    }
    public class CanvasConnection
    {
        public string id   { get; set; }
        public string type { get; set; }
        public ElementConnection start { get; set; }
        public ElementConnection end   { get; set; }
    }
    public class ElementConnection
    {
        public string elementId { get; set; }
        public int side { get; set; }
        public string direction { get; set; }
    }
    public class CanvasPosition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; } = 0;
    }
    public class Collaborator
    {
        public string userId { get; set; }
        public string[] selected { get; set; } // Element Ids selected by this user
    }
    public class UmlClassContent
    {
        public string name  { get; set; }
        public string attributes { get; set; }
        public string methods { get; set; }
    }

    public class WithDimensions
    {
        public int width { get; set; }
        public int height { get; set; }
    }


    public static class Communication
    {

        static TcpClient client;
        static NetworkStream stream = default(NetworkStream);

        public static void Connect_Client(string hostname, int port)
        {
            client = new TcpClient(hostname, port);
            stream = client.GetStream();
        }

        public static void Disconnect_Client()
        {
            stream.Close();
            client.Close();
            
        }

        public static void Authenticate( string username, string password, bool isNewAccount)
        {
            string buffer = "";
            Byte[] data;
            Authentication auth = new Authentication
            {
                id = username,
                password = password,
                isNew = isNewAccount
            };
            buffer = JsonConvert.SerializeObject(auth);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        internal static void Send_SelectOperation(string selectionType, string elementId, string userId, string canvasId)
        {
            string buffer = "";
            Byte[] data;
            string[] selection = new string[1];
            selection[0] = elementId;
            SelectOperation selectToSend = new SelectOperation
            {
                userId = userId,
                canvasId = canvasId,
                selectType = selectionType,
                elements = selection
            };
            buffer = JsonConvert.SerializeObject(selectToSend);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void Send_Message(DataType type, string username, string msg, string curchannel = "Main")
        {
            string buffer="";
            Byte[] data;
			Message messageToSend = new Message
			{
				sender = username,
                channel = curchannel,
				content = msg
			};
			buffer = JsonConvert.SerializeObject(messageToSend);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void Create_Canvas(string userId, string canvasId, string password = null)
        {
            string buffer = "";
            Byte[] data;
            Event createCanvas = new Event
            {
                name = CollabEventType.CREATE_CANVAS,
                source = userId,
                context = canvasId,
                password = password
            };
            buffer = JsonConvert.SerializeObject(createCanvas);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void Send_Operation(Operation opToSend)
        {
            string buffer = "";
            Byte[] data;            
            buffer = JsonConvert.SerializeObject(opToSend);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

		public static List<Data> Read_Data()
		{
            List<Data> parsedPackets = new List<Data>();
			Byte[] data = new Byte[512];
			StringBuilder dataString = new StringBuilder();
			try
			{
				int nBytesRead = 0;
				do
				{
					nBytesRead = stream.Read(data, 0, data.Length);
					dataString.Append(Encoding.UTF8.GetString(data, 0, nBytesRead));
				} while (stream.DataAvailable);
			}
			catch (System.IO.IOException e)
			{
				Error ex = new Error();
				ex.message = e.Message;
                parsedPackets.Add(ex);
			}
            catch (System.Net.Sockets.SocketException e)
            {
                MessageBox.Show(e.Message);
            }

			List<string> jsons = SeparateJsonPackets(dataString);

         
            foreach (string json in jsons)
            {
                Data packet = JsonConvert.DeserializeObject<Data>(json);
                switch (packet.type)
                {
                    case DataType.AUTH:
                        parsedPackets.Add(JsonConvert.DeserializeObject<AuthenticationResponse>(json));
                        break;
                    case DataType.MSG:
                        parsedPackets.Add(JsonConvert.DeserializeObject<Message>(json));
                        break;
                    case DataType.ERROR:
                        parsedPackets.Add(JsonConvert.DeserializeObject<Error>(json));
                        break;
                    case DataType.EVENT:
                        parsedPackets.Add(JsonConvert.DeserializeObject<Event>(json));
                        break;
                    case DataType.RESPONSE:
                        parsedPackets.Add(JsonConvert.DeserializeObject<Response>(json));
                        break;
                    case DataType.OPERATION:
                        Operation opPacket = JsonConvert.DeserializeObject<Operation>(json);
                        switch (opPacket.operation)
                        {
                            case (OperationType.CREATE):
                                CreateOperation crPacket = JsonConvert.DeserializeObject<CreateOperation>(json);
                                switch (crPacket.createType)
                                {
                                    case (OperationTargetType.ELEMENT):
                                        parsedPackets.Add(JsonConvert.DeserializeObject<CreateElementOperation>(json));
                                        break;
                                    case (OperationTargetType.CONNECTION):
                                        parsedPackets.Add(JsonConvert.DeserializeObject<CreateConnectionOperation>(json));
                                        break;
                                }
                                break;
                            case (OperationType.MODIFY):
                                ModifyOperation mdPacket = JsonConvert.DeserializeObject<ModifyOperation>(json);
                                switch (mdPacket.targetType)
                                {
                                    case (OperationTargetType.ELEMENT):
                                        parsedPackets.Add(JsonConvert.DeserializeObject<ElementModifyOperation>(json));
                                        break;
                                    case (OperationTargetType.CANVAS):
                                        parsedPackets.Add(JsonConvert.DeserializeObject<CanvasModifyOperation>(json));
                                        break;
                                    default:
                                        parsedPackets.Add(mdPacket);
                                        break;
                                }
                                break;
                            case (OperationType.DELETE):
                                parsedPackets.Add(JsonConvert.DeserializeObject<DeleteOperation>(json));
                                break;
                            case (OperationType.SELECT):
                                parsedPackets.Add(JsonConvert.DeserializeObject<SelectOperation>(json));
                                break;
                            case (OperationType.RESET):
                                parsedPackets.Add(JsonConvert.DeserializeObject<ResetOperation>(json));
                                break;
                            default:
                                parsedPackets.Add(JsonConvert.DeserializeObject<Operation>(json));
                                break;
                        }
                        break;
                    default:
                        parsedPackets.Add(packet);
                        break;
                }
            }
            return parsedPackets;
        }

        private static List<String> SeparateJsonPackets(StringBuilder stringBuilder)
        {
            List<String> packets = new List<String>();
            if (stringBuilder.Length > 0)
            {
                int lastStart = 0;
                int lastClosingBracket = 0;
                char previousChar = stringBuilder[lastStart];
                char currentChar;
                for (int i = 1; i < stringBuilder.Length; i++)
                {
                    currentChar = stringBuilder[i];
                    if (currentChar == '}')
                    {
                        lastClosingBracket = i;
                    }
                    if (previousChar == '}' && currentChar == '{')
                    {
                        packets.Add(stringBuilder.ToString(lastStart, i - lastStart));
                        lastStart = i;
                    }
                    previousChar = currentChar;
                }
                packets.Add(stringBuilder.ToString(lastStart, lastClosingBracket + 1 - lastStart));
            }
            return packets;
        }

        public static bool IsConnected()
        {
            if (client == null)
            {
                return false;
            }
            else 
            {
                if (client.Client.Connected)
                {
                    bool IsYouConnectionAlright = client.Client.Poll(01, SelectMode.SelectWrite)
                        && client.Client.Poll(01, SelectMode.SelectRead)
                        && !client.Client.Poll(01, SelectMode.SelectError) ? true : false;
                }
                return client.Connected;
            }
        }

        public static void Join_Session(string sessionID, string userId, string password = null)
        {
            string buffer = "";
            Byte[] data;
            Event joinSession = new Event
            {
                name = CollabEventType.SESSION_JOIN,
                source = userId,
                context = sessionID,
                password = password
            };
            buffer = JsonConvert.SerializeObject(joinSession);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        internal static void send_Request(string dataRequestType)
        {
            string buffer = "";
            Byte[] data;
            Request request = new Request
            {
                request = dataRequestType,
            };
            buffer = JsonConvert.SerializeObject(request);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        internal static void Delete_Canvas(CanvasBaseInfo canvasBase)
        {
            string buffer = "";
            Byte[] data;
            Event deleteCanvas = new Event
            {
                name = CollabEventType.DELETE_CANVAS,
                source = canvasBase.creator,
                context = canvasBase._id
            };
            buffer = JsonConvert.SerializeObject(deleteCanvas);
            data = Encoding.UTF8.GetBytes(buffer);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }
}
