using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using System.Collections;
using System.Windows.Media.Imaging;
using System.Windows.Media;



//    Source : https://www.codeproject.com/Articles/23871/WPF-Diagram-Designer-Part
//    Author : sukram 
//    Date : 29 Feb 2008

namespace PolyPaint
{
    public class DesignerCanvas : Canvas
    {
        private Size DEFAULT_SIZE = new Size(50,70);
        private Stack<ISelectable> itemStack { get; set; }

        private bool lasso = false;

        private string canvasId;
        public string CanvasID
        {
            get
            {
                return canvasId;
            }

            set
            {
                canvasId = value;
            }
        }

        private string userId;
        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        private string ownerId;
        public string OwnerId
        {
            get
            {
                return ownerId;
            }
            set
            {
                ownerId = value;
            }
        }

        private string canvasPassword;
        public string CanvasPassword
        {
            get
            {
                return canvasPassword;
            }
            set
            {
                canvasPassword = value;
            }
        }

        private List<ISelectable> removed;
        public List<ISelectable> Removed
        {
            get
            {
                if (removed == null)
                    removed = new List<ISelectable>();
                return removed;
            }
            set
            {
                removed = value;
            }
        }

        // start point of the rubberband drag operation
        private Point? rubberbandSelectionStartPoint = null;

        // keep track of selected items 
        private List<ISelectable> selectedItems;
        public List<ISelectable> SelectedItems
        {
            get
            {
                if (selectedItems == null)
                    selectedItems = new List<ISelectable>();
                return selectedItems;
            }
            set
            {
                selectedItems = value;
            }
        }

        public List<Collaborator> collaborators { get; set; }
        public CanvasData currentCanvasData { get; set; }

        public DesignerCanvas()
        {
            this.AllowDrop = true;
            itemStack = new Stack<ISelectable>();
            collaborators = new List<Collaborator>();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this && lasso)
            {
                // in case that this click is the start for a 
                // drag operation we cache the start point
                this.rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

                // if you click directly on the canvas all 
                // selected items are 'de-selected'
                if (Communication.IsConnected())
                {
                    SendSelectOperation(SelectType.DESELECT_ALL, null, true);
                }else
                {
                    foreach (ISelectable item in SelectedItems)
                        item.IsSelected = false;
                    selectedItems.Clear();
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this.rubberbandSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (this.rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, rubberbandSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        //Deprecated (keeped for future devlopement)
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (dragObject != null && !String.IsNullOrEmpty(dragObject.Xaml))
            {
                DesignerItem newItem = null;
                Object content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));

                if (content != null)
                {
                    newItem = new DesignerItem();
                    newItem.Content = content;

                    Point position = e.GetPosition(this);

                    if (dragObject.DesiredSize.HasValue)
                    {
                        Size desiredSize = dragObject.DesiredSize.Value;
                        newItem.Width = desiredSize.Width;
                        newItem.Height = desiredSize.Height;

                        DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                        DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    }
                    else
                    {
                        DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X));
                        DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y));
                    }
                }

                e.Handled = true;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            foreach (UIElement element in base.Children)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            // add margin 
            size.Width += 10;
            size.Height += 10;
            return size;
        }

        public void SendSelectOperation(string selectionType, ISelectable item, bool toShare)
        {
            if (toShare)
            {
                switch (selectionType)
                {
                    case (SelectType.DESELECT_ALL):
                        Communication.Send_SelectOperation(selectionType, null, UserId, CanvasID);
                        break;
                    default:
                        Communication.Send_SelectOperation(selectionType, item._id, UserId, CanvasID);
                        break;
                }
            }else{
                switch (selectionType){
                    case (SelectType.SELECT):
                        item.IsSelected = true;
                        SelectedItems.Add(item);
                        break;
                    case (SelectType.DESELECT):
                        item.IsSelected = false;
                        SelectedItems.Remove(item);
                        break;
                    case (SelectType.DESELECT_ALL):
                        foreach (ISelectable selectedItem in SelectedItems)
                            selectedItem.IsSelected = false;
                        SelectedItems.Clear();
                        break;
                    default:
                        break;
                }
            }
        }

        internal Collaborator getCollaboratorById(string collabId)
        {
            foreach(Collaborator collab in collaborators)
            {
                if (collab.userId == collabId)
                    return collab;
            }

            return null;
        }

        private ISelectable getItemById(string id)
        {
            foreach (ISelectable item in this.Children)
            {
                if(item._id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private UIElement getUIElementById(string id)
        {
            foreach (UIElement item in this.Children)
            {
                ISelectable sElement = (ISelectable)item;
                if (sElement._id == id)
                {
                    return item;
                }
            }

            return null;
        }

        public void SendCreateElementOperation(DesignerItem newItem, bool ToShare)
        {
            if (ToShare)
            {
                Element newElement = newItem.toElement();
                if (newElement.id != null)
                    newElement.id = null;
                CreateElementOperation crOp = new CreateElementOperation
                {
                    element = newElement,
                    userId = UserId,
                    canvasId = CanvasID
                };
                Communication.Send_Operation(crOp);
            }
            else
            {
                this.Children.Add((UIElement)newItem);
            }

        }

        public void SendCreateConnectionOperation(Connection connection)
        {
            Connector sourceConnector = connection.Source;
            Connector sinkConnector = connection.Sink;
            CanvasConnection newConnection = new CanvasConnection
            {
                type = ConnectionType.PROCESS,
                start = new ElementConnection
                {
                    elementId = sourceConnector.ParentDesignerItem._id,
                    direction = ConnectionDirection.OUT,
                    side = (int)sourceConnector.Orientation
                },
                end = new ElementConnection
                {
                    elementId = sinkConnector.ParentDesignerItem._id,
                    direction = ConnectionDirection.IN,
                    side = (int)sinkConnector.Orientation
                }
            };
            CreateConnectionOperation crCo = new CreateConnectionOperation
            {
                element = newConnection,
                userId = UserId,
                canvasId = CanvasID
            };
            Communication.Send_Operation(crCo);
        }

        internal void handleModifyOperation(Element element)
        {
            DesignerItem targetItem = (getItemById(element.id) as DesignerItem);
            Element oldElement = targetItem.toElement();
            if(element.position != oldElement.position)
            {
                targetItem.itemPosition = element.position;
                DesignerCanvas.SetLeft(targetItem, element.position.x);
                DesignerCanvas.SetTop(targetItem, element.position.y);
            }

            if (element.title != oldElement.title)
            {
                if (element.type == ElementType.PHASE)
                {
                    TextBox textbox = (targetItem.Content as ContentControl).FindName("phaseName") as TextBox;
                    textbox.Text = element.title;
                }else if(element.type == ElementType.UMLCLASS)
                {
                    //Doesn't apply Class has no title
                }
                else if(( element.type == ElementType.COMMENT) || 
                        (element.type == ElementType.FLOATINGTEXT))
                {
                    TextBox textbox = (targetItem.Content as ContentControl).FindName("myTextBox") as TextBox;
                    textbox.Text = element.title;
                }
                else
                {
                    TextBlock textblock = targetItem.Template.FindName("myTextBlock", targetItem) as TextBlock;
                    TextBox textBox = targetItem.Template.FindName("myTextBox", targetItem) as TextBox;
                    textBox.Text = element.title;
                    textblock.Text = element.title;
                    targetItem._name = element.title;
                    textBox.Visibility = Visibility.Hidden;
                    textblock.Visibility = Visibility.Visible;
                }
            }

            if((element.content != oldElement.content) && (element.type == ElementType.UMLCLASS))
            {
                TextBox namebox = (targetItem.Content as ContentControl).FindName("className") as TextBox;
                namebox.Text = element.content.name;
                TextBox attributesbox = (targetItem.Content as ContentControl).FindName("attributesText") as TextBox;
                attributesbox.Text = element.content.attributes;
                TextBox methodsbox = (targetItem.Content as ContentControl).FindName("methodsText") as TextBox;
                methodsbox.Text = element.content.methods;
            }

            if(element.rotation != oldElement.rotation)
            {
                targetItem.RenderTransform = new RotateTransform(0);
                (targetItem.RenderTransform as RotateTransform).Angle = element.rotation;
            }

            if((element.width != oldElement.width) || (element.height != oldElement.height))
            {
                targetItem.Width = element.width;
                targetItem.Height = element.height;
            }
        }

        internal void HandleSessionJoin(DataResponse data)
        {
            foreach (Collaborator collab in data.collaborators)
                this.collaborators.Add(collab);
            currentCanvasData = data.canvasData;
            CanvasID = currentCanvasData._id;
            OwnerId = currentCanvasData.creator;
            foreach (Element item in currentCanvasData.elements)
            {
                AddToChildren(item);
            }
        }

        internal void HandleSessionSelection()
        {
            foreach (Collaborator collaborator in collaborators)
            {
                if (collaborator.userId != UserId)
                    foreach (string elementId in collaborator.selected)
                        getItemById(elementId).IsLocked = true;
            }
        }

        private void SendDeleteOperation(string elementId)
        {
            DeleteOperation dlOp = new DeleteOperation
            {
                deleteType = getItemById(elementId).type,
                element = elementId,
                userId = UserId,
                canvasId = CanvasID
            };
            Communication.Send_Operation(dlOp);
        }

        public void SendModifyElement(Element modifiedElement)
        {
            ElementModifyOperation newPos = new ElementModifyOperation
            {
                canvasId = CanvasID,
                userId = UserId,
                element = modifiedElement
            };
            Communication.Send_Operation(newPos);
        }

        public void AddToChildren(Element newElement)
        {

            switch (newElement.type)
            {
                case (ElementType.ACTIVITY):
                    AddActivity(false, newElement);
                    break;
                case (ElementType.ARTEFACT):
                    AddArtefact(false, newElement);
                    break;
                case (ElementType.COMMENT):
                    AddComment(false, newElement);
                    break;
                case (ElementType.FLOATINGTEXT):
                    AddFloatingText(false, newElement);
                    break;
                case (ElementType.PHASE):
                    AddPhase(false, newElement);
                    break;
                case (ElementType.ROLE):
                    AddRole(false, newElement);
                    break;
                case (ElementType.UMLCLASS):
                    AddClass(false, newElement);
                    break;
                default:
                    break;
            }

        }

        internal void AddConnectionToChildren(CanvasConnection element)
        {
            if(element.start != null && element.end != null)
            {
                Connector sourceConnector = new Connector();
                Connector sinkConnector = new Connector();
                try
                {
                    sourceConnector = (getItemById(element.start.elementId) as DesignerItem).getDesignerItemConnector((ConnectorOrientation)element.start.side);
                    sinkConnector = (getItemById(element.end.elementId) as DesignerItem).getDesignerItemConnector((ConnectorOrientation)element.end.side);
                }catch(Exception e)
                {
                    sourceConnector = null;
                    sinkConnector = null;
                    MessageBox.Show(e.Message);
                }
                

                if (sourceConnector != null && sinkConnector != null)
                {
                    Connection newConnection = new Connection(sourceConnector, sinkConnector);
                    newConnection._id = element.id;
                    newConnection.IsLocked = false;
                    this.Children.Insert(0, newConnection);
                }
            }
        }
        
        private void ProcessItem(DesignerItem newItem, Element newElement, bool toShare)
        {
            newItem.itemPosition = newElement.position;
            DesignerCanvas.SetLeft(newItem, newElement.position.x);
            DesignerCanvas.SetTop(newItem, newElement.position.y);

            if (toShare)
            {
                SendSelectOperation(SelectType.DESELECT_ALL, newItem, Communication.IsConnected());
                SendCreateElementOperation(newItem, toShare);
            }
            else if (!Communication.IsConnected())
            {
                this.Children.Add(newItem);
                foreach (ISelectable item in this.SelectedItems)
                    item.IsSelected = false;
                SelectedItems.Clear();
                newItem.IsSelected = true;
                SelectedItems.Add(newItem);
            }else
            {
                this.Children.Add(newItem);

                if (newElement.title != null)
                {
                    if (newElement.type == ElementType.PHASE)
                    {
                        TextBox textbox = (newItem.Content as ContentControl).FindName("phaseName") as TextBox;
                        textbox.Text = newElement.title;
                    }

                    if ((newElement.type == ElementType.COMMENT) ||
                            (newElement.type == ElementType.FLOATINGTEXT))
                    {
                        TextBox textbox = (newItem.Content as ContentControl).FindName("myTextBox") as TextBox;
                        textbox.Text = newElement.title;
                    }
                    
                    //Process other elements title
                    if((newElement.type == ElementType.ROLE) ||
                       (newElement.type == ElementType.ACTIVITY) ||
                       (newElement.type == ElementType.ARTEFACT))
                    {
                        newItem._name = newElement.title;
                    }
                }

                if ((newElement.content != null) && (newElement.type == ElementType.UMLCLASS))
                {
                    TextBox namebox = (newItem.Content as ContentControl).FindName("className") as TextBox;
                    namebox.Text = newElement.content.name;
                    TextBox attributesbox = (newItem.Content as ContentControl).FindName("attributesText") as TextBox;
                    attributesbox.Text = newElement.content.attributes;
                    TextBox methodsbox = (newItem.Content as ContentControl).FindName("methodsText") as TextBox;
                    methodsbox.Text = newElement.content.methods;
                }

                if (newElement.rotation != 0.0)
                {
                    newItem.RenderTransform = new RotateTransform(0);
                    (newItem.RenderTransform as RotateTransform).Angle = newElement.rotation;
                }
            }
        }

        public void updateSelection(SelectOperation selectOp, bool isCurrentUser)
        {
            switch (selectOp.selectType)
            {
                case (SelectType.SELECT):
                    foreach (string elementId in selectOp.elements)
                    {
                        ISelectable selectedItem = getItemById(elementId);
                        if (selectedItem != null)
                        {
                            if (isCurrentUser)
                            {
                                selectedItem.IsSelected = true;
                                this.SelectedItems.Add(selectedItem);
                            }
                            else
                            {
                                selectedItem.IsLocked = true;
                            }
                        }
                    }
                    break;
                case (SelectType.DESELECT):
                    foreach (string elementId in selectOp.elements)
                    {
                        ISelectable selectedItem = getItemById(elementId);
                        if (selectedItem != null)
                        {
                            if (isCurrentUser)
                            {
                                selectedItem.IsSelected = false;
                                this.SelectedItems.Remove(selectedItem);
                            }
                            else
                            {
                                selectedItem.IsLocked = false;
                            }
                        } 
                    }
                    break;
                case (SelectType.DESELECT_ALL):
                    if (isCurrentUser)
                    {
                        foreach (ISelectable item in this.SelectedItems)
                            item.IsSelected = false;
                        SelectedItems.Clear();
                    }else
                    {
                        foreach (string elementId in selectOp.elements)
                        {
                            ISelectable selectedItem = getItemById(elementId);
                            if (selectedItem!=null) selectedItem.IsLocked = false;
                        }
                    }
                    break;
                default:
                    break;
            }
            
        }

        public void AddClass(bool toShare = false, Element newElement = null)
        {

            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            newItem._withTextBox = false;
            newItem.Content = new UmlClassControl();
            newItem._id = newElement.id;
            newItem.designerType = ElementType.UMLCLASS;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void AddRole(bool toShare = false, Element newElement = null)
        {
            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            Image img = new Image();
            img.Source = new BitmapImage(new Uri("../Resources/Images/Role.png", UriKind.Relative));
            newItem.Content = img;
            newItem.Width = DEFAULT_SIZE.Width;
            newItem.Height = DEFAULT_SIZE.Height;
            newItem._id = newElement.id;
            newItem.designerType = ElementType.ROLE;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void AddArtefact(bool toShare = false, Element newElement = null)
        {

            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            Image img = new Image();
            img.Source = new BitmapImage(new Uri("../Resources/Images/Artefact.png", UriKind.Relative));
            newItem.Content = img;
            newItem.Width = DEFAULT_SIZE.Width;
            newItem.Height = DEFAULT_SIZE.Height;
            newItem._id = newElement.id;
            newItem.designerType = ElementType.ARTEFACT;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void AddActivity(bool toShare = false, Element newElement = null)
        {

            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            Image img = new Image();
            img.Source = new BitmapImage(new Uri("../Resources/Images/Activity.png", UriKind.Relative));
            newItem.Content = img;
            newItem.Width = DEFAULT_SIZE.Width;
            newItem.Height = DEFAULT_SIZE.Height;
            newItem._id = newElement.id;
            newItem.designerType = ElementType.ACTIVITY;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void AddPhase(bool toShare = false, Element newElement = null)
        {

            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            newItem._withTextBox = false;
            newItem._withConnectors = false;
            newItem._id = newElement.id;
            newItem.Content = new Phase();
            newItem.designerType = ElementType.PHASE;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void AddComment(bool toShare = false, Element newElement = null)
        {

            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            newItem._withTextBox = false;
            newItem._withConnectors = false;
            newItem._id = newElement.id;
            newItem.Content = new Comment();
            newItem.designerType = ElementType.COMMENT;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void AddFloatingText(bool toShare = false, Element newElement = null)
        {

            DesignerItem newItem = null;
            if (newElement == null)
            {
                newElement = new Element();
                newElement.position = new CanvasPosition
                {
                    x = 25,
                    y = 25,
                };
            }
            newItem = new DesignerItem();
            newItem._withTextBox = false;
            newItem._withConnectors = false;
            newItem._id = newElement.id;
            newItem.Content = new FloatingText();
            newItem.designerType = ElementType.FLOATINGTEXT;
            ProcessItem(newItem, newElement, toShare);
            lasso = false;

        }

        public void dupliquer(bool toShare)
        {
            if (toShare)
            {
                if (SelectedItems.Count != 0)
                {
                    foreach (ISelectable item in this.SelectedItems)
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                            if(item.type == OperationTargetType.ELEMENT)
                            {
                                DesignerItem toDuplicateItem = (DesignerItem)item;
                                SendCreateElementOperation(toDuplicateItem, toShare);
                            }else
                            {
                                Connection toDuplicateConnection = (Connection)item;
                                SendCreateConnectionOperation(toDuplicateConnection);
                            }
                        }
                    }
                    SelectedItems.Clear();
                }
                else
                {
                    foreach (ISelectable item in this.Removed)
                    {
                        if (item.type == OperationTargetType.ELEMENT)
                        {
                            DesignerItem toDuplicateItem = (DesignerItem)item;
                            SendCreateElementOperation(toDuplicateItem, toShare);
                        }
                    }
                    Removed.Clear();
                }

            }
            else
            {
                if (SelectedItems.Count != 0)
                {
                    foreach (ISelectable item in this.SelectedItems)
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                            if(item is DesignerItem)
                            {
                                this.Children.Add((UIElement)DeepCopy((DesignerItem)item));
                            }
                        }
                    }
                    SelectedItems.Clear();
                }
                else
                {
                    foreach (ISelectable item in this.Removed)
                    {
                        this.Children.Add((UIElement)((DesignerItem)item));
                        item.IsSelected = false;
                    }
                    Removed.Clear();
                }
            }
        }

        public void couper(bool toShare)
        {
            Removed.Clear();
            foreach (ISelectable item in this.SelectedItems)
            {
                item.IsSelected = false;
                Removed.Add(item);
                if (toShare)
                {
                    SendDeleteOperation(item._id);
                }else
                {
                    this.Children.Remove((UIElement)item);
                }
            }
            SelectedItems.Clear();
            
        }

        public void Reinitialiser(bool toShare)
        {
            if (toShare)
            {
                ResetOperation rsOp = new ResetOperation
                {
                    canvasId = CanvasID,
                };
                Communication.Send_Operation(rsOp);
            }else
            {
                this.Children.Clear();
            }
        }

        public void empiler(bool toShare) {
            foreach (ISelectable item in this.SelectedItems)
            {
                if (item.IsSelected)
                {
                    itemStack.Push(item);
                    if (toShare)
                    {
                        SendDeleteOperation(item._id);
                    }
                    else
                    {
                        this.Children.Remove((UIElement)item);
                        item.IsSelected = false;
                    }                    
                }
            }
        }

        public void depiler(bool toShare)
        {
            if(itemStack.Count != 0)
                if (toShare)
                {
                    ISelectable item = (ISelectable)itemStack.Pop();
                    if(item.type == OperationTargetType.ELEMENT)
                    {
                        DesignerItem toDuplicateItem = (DesignerItem)item;
                        SendCreateElementOperation(toDuplicateItem, toShare);
                    }else
                    {
                        Connection toDuplicateConnection = (Connection)item;
                        SendCreateConnectionOperation(toDuplicateConnection);
                    }
                }else
                {
                    this.Children.Add((UIElement)itemStack.Pop());
                }
        }

        public void effacer(bool toShare, string elementId = null, string targetType = null)
        {
            if (toShare)
            {
                foreach (ISelectable item in this.SelectedItems)
                {
                    if (item.IsSelected)
                    {
                        SendDeleteOperation(item._id);
                        item.IsSelected = false;
                    }

                }
                this.SelectedItems.Clear();
            }else
            {
                if (elementId != null)
                {
                    this.Children.Remove(getUIElementById(elementId));
                }else
                {
                    foreach (ISelectable item in this.SelectedItems)
                    {
                        if (item.IsSelected)
                        {
                            this.Children.Remove(item as UIElement);
                            item.IsSelected = false;
                        }

                    }
                    this.SelectedItems.Clear();
                }
            }
        }

        public void ActiverLasso() { lasso = true; }
        

        // Source : http://shrinandvyas.blogspot.com/2011/08/wpf-how-to-deep-copy-wpf-object-eg.html
        // Author : SHRINAND VYAS
        // Date : August 22, 2011
        public  DesignerItem DeepCopy(DesignerItem element)
        {
            var xaml = XamlWriter.Save(element);
            var xamlString = new StringReader(xaml);
            var xmlTextReader = new XmlTextReader(xamlString);
            var deepCopyObject = (DesignerItem)XamlReader.Load(xmlTextReader);
           
            DesignerCanvas.SetLeft(deepCopyObject, DesignerCanvas.GetLeft(element)+25);
            DesignerCanvas.SetTop(deepCopyObject, DesignerCanvas.GetTop(element)+25);
            if (element.Content.GetType() == typeof(Comment) || element.Content.GetType() == typeof(FloatingText) ||
                element.Content.GetType() == typeof(UmlClassControl) || element.Content.GetType() == typeof(Phase))
                deepCopyObject._withTextBox = false;
            return deepCopyObject;
        }

        public void ExportFile()
        {
            System.Windows.Forms.SaveFileDialog saveAsDialog = new System.Windows.Forms.SaveFileDialog
            {
                Title = "Save UML to JPG",
                Filter = "JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png",
                AddExtension = true
            };

            if(saveAsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            string filePath = Path.GetFullPath(saveAsDialog.FileName);

            
            MemoryStream imageStream = new MemoryStream();
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)this.ActualWidth,
                                    (int)this.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Pbgra32);
            rtb.Render(this);

            BitmapEncoder encoder = null;
            switch(Path.GetExtension(filePath))
            {
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
            }

            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(imageStream);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                imageStream.WriteTo(fs);
                imageStream.Close();
                fs.Close();
            }
        }

    }
}
