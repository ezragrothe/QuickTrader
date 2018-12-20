Imports System.Windows.Markup
Imports System.Reflection

<ContentProperty("Content")>
Public Class UIChartControl
    Inherits ContentControl
    Implements IChartObject

    Public Sub New()
        'Content = GetType(T).GetConstructors()(0).Invoke(Nothing)
    End Sub

    Public Property Left As Double
        Get
            Return Canvas.GetLeft(Me)
        End Get
        Set(ByVal value As Double)
            Canvas.SetLeft(Me, value)
        End Set
    End Property
    Public Property Top As Double
        Get
            Return Canvas.GetTop(Me)
        End Get
        Set(ByVal value As Double)
            Canvas.SetTop(Me, value)
        End Set
    End Property

    Private _contentChildren As Boolean
    Public ReadOnly Property ContentHasChildren As Boolean
        <DebuggerStepThrough()>
        Get
            Return _contentChildren
        End Get
    End Property
    Public Property ContentChildren As IList
        Get
            If ContentHasChildren Then
                Return contentChildrenProperty.GetValue(Content, Nothing)
            End If
            Return Nothing
        End Get
        Set(ByVal value As IList)
            If ContentHasChildren Then
                contentChildrenProperty.SetValue(Content, value, Nothing)
            End If
        End Set
    End Property


    Private Sub UIChartControl_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded

    End Sub
    Private contentChildrenProperty As PropertyInfo
    Public Overloads Property Content As Object
        Get
            Return MyBase.Content
        End Get
        Set(ByVal value As Object)
            MyBase.Content = value
            If value IsNot Nothing Then
                For Each item In value.GetType.GetCustomAttributes(True)
                    If TypeOf item Is ContentPropertyAttribute Then
                        contentChildrenProperty = value.GetType.GetProperty(CType(item, ContentPropertyAttribute).Name)
                        _contentChildren = TypeOf contentChildrenProperty.GetValue(Content, Nothing) Is IList
                        Exit Property
                    End If
                Next
            End If
            _contentChildren = False
        End Set
    End Property

    Protected Overrides Sub OnContentChanged(ByVal oldContent As Object, ByVal newContent As Object)
        MyBase.OnContentChanged(oldContent, newContent)

        BindingOperations.ClearBinding(Me, WidthProperty)
        BindingOperations.ClearBinding(Me, HeightProperty)

        Dim widthBinding As New Binding
        widthBinding.Mode = BindingMode.TwoWay
        widthBinding.ElementName = CType(newContent, FrameworkElement).Name
        widthBinding.Path = New PropertyPath(ContentControl.WidthProperty)

        Dim heightBinding As New Binding
        heightBinding.ElementName = CType(newContent, FrameworkElement).Name
        heightBinding.Path = New PropertyPath(ContentControl.HeightProperty)
        heightBinding.Mode = BindingMode.TwoWay

        SetBinding(WidthProperty, widthBinding)
        SetBinding(HeightProperty, heightBinding)

    End Sub
    Public Property HasPhantomParent As Boolean = False Implements IChartObject.HasPhantomParent
    Private _isMouseDown As Boolean
    Public ReadOnly Property IsMouseDown As Boolean Implements IChartObject.IsMouseDown
        Get
            Return _isMouseDown
        End Get
    End Property
    Private _parent As Chart
    Protected Overrides Sub OnVisualParentChanged(ByVal oldParent As System.Windows.DependencyObject)
        If VisualParent Is Nothing OrElse (TypeOf VisualParent Is DrawingVisualCanvas AndAlso TypeOf CType(VisualParent, DrawingVisualCanvas).Parent Is Chart) Then
            MyBase.OnVisualParentChanged(oldParent)
            If VisualParent IsNot Nothing Then
                _parent = CType(VisualParent, DrawingVisualCanvas).Parent
            End If
        Else
            Throw New InvalidOperationException("Chart controls can only be owned by the Chart control.")
        End If
    End Sub
    Public ReadOnly Property HasParent As Boolean Implements IChartObject.HasParent
        Get
            Return Parent IsNot Nothing
        End Get
    End Property
    Public Overloads ReadOnly Property Parent As Chart Implements IChartObject.Parent
        Get
            Return _parent
        End Get
    End Property

    Public Sub ParentBoundsChanged() Implements IChartObject.ParentBoundsChanged
        'If Scaleable = ScaleType.MoveableOnly Or Scaleable = ScaleType.MoveableAndScaleable Then
        '    Canvas.SetTop(Me, Parent.GetRealFromRelativeY(_rect.Y))
        '    Canvas.SetLeft(Me, Parent.GetRealFromRelativeX(_rect.X))
        '    If Scaleable = ScaleType.MoveableAndScaleable Then
        '        Width = Parent.GetRealFromRelativeWidth(_rect.Width)
        '        Height = Parent.GetRealFromRelativeHeight(_rect.Height)
        '    End If
        'End If
    End Sub

    Private mouseOffset As Point
    Private Sub UIControl_LostMouseCapture(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles Me.LostMouseCapture
        '_isMouseDown = False
    End Sub
    Private Sub UIControl_GotMouseCapture(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles Me.GotMouseCapture
        '_isMouseDown = True
        'mouseOffset = e.GetPosition(Parent) - New Point(Canvas.GetLeft(Me), Canvas.GetTop(Me))
    End Sub
    Public Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseLeftButtonDown
        If (Not IgnoreDragOnButtonClick) = (TypeOf e.Source Is Button Or TypeOf e.Source Is Slider) Then
            _isMouseDown = True
            mouseOffset = e.GetPosition(Parent) - New Point(Canvas.GetLeft(Me), Canvas.GetTop(Me))
        End If
    End Sub
    Public Sub ParentMouseLeftButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseLeftButtonUp
        _isMouseDown = False
    End Sub
    Public Sub ParentMouseMiddleButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseMiddleButtonDown

    End Sub
    Public Sub ParentMouseMiddleButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseMiddleButtonUp

    End Sub
    Public Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseMove
        If IsMouseDown And IsDraggable Then
            Dim loc As Point = e.GetPosition(Parent) - mouseOffset
            Top = loc.Y
            Left = loc.X
        End If
    End Sub
    Public Property IgnoreDragOnButtonClick As Boolean = True
    'Public Property Scaleable As ScaleType
    Public Property SnapToRelativePixels As Boolean Implements IChartObject.SnapToRelativePixels
    Public Property IsDraggable As Boolean = False

    Friend Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs) Implements IChartObject.Command_Executed

    End Sub

    Public Sub ParentMouseDoubleClick(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseDoubleClick
    End Sub
    Public Sub ParentMouseRightButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseRightButtonDown
        _rightButtonDown = True
    End Sub
    Public Sub ParentMouseRightButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseRightButtonUp
        _rightButtonDown = False
    End Sub
    Private _rightButtonDown As Boolean
    Public ReadOnly Property IsRightMouseButtonDown As Boolean Implements IChartObject.IsRightMouseButtonDown
        Get
            Return _rightButtonDown
        End Get
    End Property
    Public Overloads Property Tag As Object Implements IChartObject.Tag

    Public Property AnalysisTechnique As AnalysisTechniques.AnalysisTechnique Implements IChartObject.AnalysisTechnique

    Public Sub ParentMouseWheel(ByVal e As System.Windows.Input.MouseWheelEventArgs) Implements IChartObject.ParentMouseWheel

    End Sub
End Class
