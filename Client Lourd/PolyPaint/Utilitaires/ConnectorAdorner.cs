using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


//    Source : https://www.codeproject.com/Articles/23871/WPF-Diagram-Designer-Part
//    Author : sukram 
//    Date : 29 Feb 2008
namespace PolyPaint
{
    public class ConnectorAdorner : Adorner
    {
        private PathGeometry pathGeometry;
        private DesignerCanvas designerCanvas;
        private Connector sourceConnector;
        private Pen drawingPen;

        private DesignerItem hitDesignerItem;
        private DesignerItem HitDesignerItem
        {
            get { return hitDesignerItem; }
            set
            {
                if (hitDesignerItem != value)
                {
                    if (hitDesignerItem != null)
                        hitDesignerItem.IsDragConnectionOver = false;

                    hitDesignerItem = value;

                    if (hitDesignerItem != null)
                        hitDesignerItem.IsDragConnectionOver = true;
                }
            }
        }

        private Connector hitConnector;
        private Connector HitConnector
        {
            get { return hitConnector; }
            set
            {
                if (hitConnector != value)
                {
                    hitConnector = value;
                }
            }
        }

        public ConnectorAdorner(DesignerCanvas designer, Connector sourceConnector)
            : base(designer)
        {
            this.designerCanvas = designer;
            this.sourceConnector = sourceConnector;
            drawingPen = new Pen(Brushes.LightSlateGray, 1);
            drawingPen.LineJoin = PenLineJoin.Round;
            this.Cursor = Cursors.Cross;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (HitConnector != null)
            {
                Connector sourceConnector = this.sourceConnector;
                Connector sinkConnector = this.HitConnector;
                if (Communication.IsConnected())
                {
                    CreateConnectionOperation crCo = new CreateConnectionOperation
                    {
                        canvasId = this.designerCanvas.CanvasID,
                        userId = this.designerCanvas.UserId,
                        element = new CanvasConnection
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
                        }
                    };
                    Communication.Send_Operation(crCo);
                }else
                {
                    Connection newConnection = new Connection(sourceConnector, sinkConnector);

                    // connections are added with z-index of zero
                    this.designerCanvas.Children.Insert(0, newConnection);
                }
            }
            if (HitDesignerItem != null)
            {
                this.HitDesignerItem.IsDragConnectionOver = false;
            }

            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured) this.CaptureMouse();
                HitTesting(e.GetPosition(this));
                this.pathGeometry = GetPathGeometry(e.GetPosition(this));
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, drawingPen, this.pathGeometry);

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }

        private PathGeometry GetPathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();

            ConnectorOrientation targetOrientation;
            if (HitConnector != null)
                targetOrientation = HitConnector.Orientation;
            else
                targetOrientation = ConnectorOrientation.None;

            List<Point> pathPoints = PathFinder.GetConnectionLine(sourceConnector.GetInfo(), position, targetOrientation);

            if (pathPoints.Count > 0)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = pathPoints[0];
                pathPoints.Remove(pathPoints[0]);
                figure.Segments.Add(new PolyLineSegment(pathPoints, true));
                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        private void HitTesting(Point hitPoint)
        {
            bool hitConnectorFlag = false;

            DependencyObject hitObject = designerCanvas.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null &&
                   hitObject != sourceConnector.ParentDesignerItem &&
                   hitObject.GetType() != typeof(DesignerCanvas))
            {
                if (hitObject is Connector)
                {
                    HitConnector = hitObject as Connector;
                    hitConnectorFlag = true;
                }

                if (hitObject is DesignerItem)
                {
                    HitDesignerItem = hitObject as DesignerItem;
                    if (!hitConnectorFlag)
                        HitConnector = null;
                    return;
                }
                hitObject = VisualTreeHelper.GetParent(hitObject);
            }

            HitConnector = null;
            HitDesignerItem = null;
        }
    }
}
