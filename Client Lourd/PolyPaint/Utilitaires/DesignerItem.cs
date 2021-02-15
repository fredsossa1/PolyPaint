using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


//    Source : https://www.codeproject.com/Articles/23871/WPF-Diagram-Designer-Part
//    Author : sukram 
//    Date : 29 Feb 2008

namespace PolyPaint
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "RotateDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "myTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "myTextBlock", Type = typeof(TextBlock))]
    public class DesignerItem : ContentControl, ISelectable
    {
        #region IsSelected Property

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected",
                                       typeof(bool),
                                       typeof(DesignerItem),
                                       new FrameworkPropertyMetadata(false));

        #endregion

        #region DragThumbTemplate Property

        // can be used to replace the default template for the DragThumb
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(DragThumbTemplateProperty, value);
        }

        #endregion

        #region ConnectorDecoratorTemplate Property

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
            DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(ConnectorDecoratorTemplateProperty);
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(ConnectorDecoratorTemplateProperty, value);
        }

        #endregion

        #region IsDragConnectionOver

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver
        {
            get { return (bool)GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }
        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver",
                                         typeof(bool),
                                         typeof(DesignerItem),
                                         new FrameworkPropertyMetadata(false));

        #endregion

        public bool _withTextBox = true;

        public bool _withConnectors = true;

        public string _name { get; set; } = "name";

        public bool IsLocked { get; set; } = false;

        public string _id { get; set; }
        public string type { get; private set; } = OperationTargetType.ELEMENT;
        public string designerType { get; set; }
        public CanvasPosition itemPosition { get; set; }

        public static readonly DependencyProperty myTextBox =
           DependencyProperty.RegisterAttached("myTextBox", typeof(TextBox), typeof(DesignerItem));

        public static ControlTemplate GetTextBox(UIElement element)
        {
            return (ControlTemplate)element.GetValue(myTextBox);
        }

        public static readonly DependencyProperty myTextBlock =
          DependencyProperty.RegisterAttached("myTextBlock", typeof(TextBlock), typeof(DesignerItem));

        public static ControlTemplate GetTextBlock(UIElement element)
        {
            return (ControlTemplate)element.GetValue(myTextBlock);
        }
        
        static DesignerItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem()
        {
            this.Loaded += new RoutedEventHandler(DesignerItem_Loaded);
            _withTextBox = true;
            _withConnectors = true;
        }

        public Element toElement()
        {
            string elementTitle="";
            switch (designerType)
            {
                case (ElementType.PHASE):
                    elementTitle = ((this.Content as ContentControl).FindName("phaseName") as TextBox).Text;
                    break;
                case (ElementType.UMLCLASS):
                    //Doesn't apply Class has no title
                    break;
                case (ElementType.COMMENT):
                case (ElementType.FLOATINGTEXT):
                    elementTitle = ((this.Content as ContentControl).FindName("myTextBox") as TextBox).Text;
                    break;
                default:
                    elementTitle = _name;
                    break;
            }

            UmlClassContent content = new UmlClassContent();
            if(designerType == ElementType.UMLCLASS)
            {
                content.name = ((this.Content as UmlClassControl).FindName("className") as TextBox).Text;
                content.attributes = ((this.Content as UmlClassControl).FindName("attributesText") as TextBox).Text;
                content.methods = ((this.Content as UmlClassControl).FindName("methodsText") as TextBox).Text;
            }

            double rotationAngle = ((RenderTransform as RotateTransform) != null)? (RenderTransform as RotateTransform).Angle : 0;

            Element element = new Element
            {
                id = this._id,
                title = elementTitle,
                type = designerType,
                content = content,
                position = itemPosition,
                rotation = rotationAngle,
                width = (int)Width,
                height = (int)Height
            };
            return element;
        }

        public Connector getDesignerItemConnector(ConnectorOrientation orientation)
        {
            Control connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
            DependencyObject childElement = VisualTreeHelper.GetChild(connectorDecorator, 0);
            DependencyObject element;
            for(int i = 0; i< VisualTreeHelper.GetChildrenCount(childElement); i++)
            {
                element = VisualTreeHelper.GetChild(childElement, i);
                if((element is Connector) && ((element as Connector).Orientation == orientation))
                {
                    // Found our connector!
                    return element as Connector;
                }
            }
            return null;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

            // update selection
            if (designer != null && (!this.IsLocked))
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (this.IsSelected)
                    {
                        designer.SendSelectOperation(SelectType.DESELECT,(ISelectable)this,Communication.IsConnected());
                    }
                    else
                    {
                        designer.SendSelectOperation(SelectType.SELECT, (ISelectable)this, Communication.IsConnected());
                    }
                else if (!this.IsSelected)
                {
                        designer.SendSelectOperation(SelectType.DESELECT_ALL, (ISelectable)this, Communication.IsConnected());
                        designer.SendSelectOperation(SelectType.SELECT, (ISelectable)this, Communication.IsConnected());
                }
            e.Handled = false;
        }

        void DesignerItem_Loaded(object sender, RoutedEventArgs e)
        {
            // if DragThumbTemplate and ConnectorDecoratorTemplate properties of this class
            // are set these templates are applied; 
            // Note: this method is only executed when the Loaded event is fired, so
            // setting DragThumbTemplate or ConnectorDecoratorTemplate properties after
            // will have no effect.
            if (base.Template != null)
            {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null)
                {
                    if (_withTextBox)
                        contentPresenter.IsHitTestVisible = false;
                    UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                    if (contentVisual != null)
                    {
                        DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
                        Control connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
                        TextBox textbox = this.Template.FindName("myTextBox", this) as TextBox;
                        TextBlock textblock = this.Template.FindName("myTextBlock", this) as TextBlock;

                        if (textblock != null)
                        {
                            textblock.MouseLeftButtonDown += new MouseButtonEventHandler(ClickOnTextBlock);
                        }

                        if (textbox != null)
                        {
                            if (_withTextBox)
                            {
                                textbox.Visibility = Visibility.Visible;
                                textbox.KeyDown += new KeyEventHandler(textBoxKeydown);
                            }

                            else
                                textbox.Visibility = Visibility.Hidden;
                        }

                        if (thumb != null)
                        {
                            ControlTemplate template =
                                DesignerItem.GetDragThumbTemplate(contentVisual) as ControlTemplate;
                            if (template != null)
                                thumb.Template = template;
                        }


                        if (connectorDecorator != null && _withConnectors)
                        {
                            ControlTemplate template =
                                DesignerItem.GetConnectorDecoratorTemplate(contentVisual) as ControlTemplate;
                            if (template != null)
                                connectorDecorator.Template = template;
                        }else
                        {
                            connectorDecorator.IsHitTestVisible = false;
                            connectorDecorator.Visibility = Visibility.Hidden;
                        }

                        this.Height = this.ActualHeight+20;
                        this.Width = this.ActualWidth+20;
                    }
                }
            }
        }
        

        private void textBoxKeydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBlock textblock = this.Template.FindName("myTextBlock", this) as TextBlock;
                textblock.Text = ((TextBox)sender).Text;
                _name = textblock.Text;
                ((TextBox)sender).Text = string.Empty;
                textblock.Visibility = Visibility.Visible;
                ((TextBox)sender).Visibility = Visibility.Hidden;
                DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
                if(Communication.IsConnected())
                    designer.SendModifyElement(this.toElement());
            }

        }

        private void ClickOnTextBlock(object sender, MouseButtonEventArgs e)
        {

            TextBox textbox = this.Template.FindName("myTextBox", this) as TextBox;
            textbox.Text = ((TextBlock)sender).Text;
            ((TextBlock)sender).Text = string.Empty;
            textbox.Visibility = Visibility.Visible;
            ((TextBlock)sender).Visibility = Visibility.Hidden;


        }
    }
}
