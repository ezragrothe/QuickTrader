'Imports System.Windows.Markup
'<ContentProperty("Content")>
'Public Class UIControl(Of T)
'    Inherits ContentControl
'    Implements ChartObject

'    Public Sub New()
'        Content = GetType(T).GetConstructors()(0).Invoke(Nothing)

'        Dim widthBinding As New Binding
'        widthBinding.RelativeSource = RelativeSource.Self
'        widthBinding.Path = New PropertyPath("Width")
'        SetBinding(RelativeWidthProperty, widthBinding)
'        Dim heightBinding As New Binding
'        heightBinding.RelativeSource = RelativeSource.Self
'        heightBinding.Path = New PropertyPath("Height")
'        SetBinding(RelativeHeightProperty, heightBinding)

'        'Dim leftBinding As New Binding
'        'leftBinding.RelativeSource = RelativeSource.Self
'        'leftBinding.Path = New PropertyPath("Left")
'        'SetBinding(RelativeHeightProperty, leftBinding)
'    End Sub

'    Public ReadOnly RelativeHeightProperty As DependencyProperty = DependencyProperty.Register("RelativeHeight", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RelativeRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RelativeWidthProperty As DependencyProperty = DependencyProperty.Register("RelativeWidth", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RelativeRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RelativeLeftProperty As DependencyProperty = DependencyProperty.Register("RelativeLeft", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RelativeRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RelativeTopProperty As DependencyProperty = DependencyProperty.Register("RelativeTop", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RelativeRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RealHeightProperty As DependencyProperty = DependencyProperty.Register("RealHeight", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RealRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RealWidthProperty As DependencyProperty = DependencyProperty.Register("RealWidth", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RealRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RealLeftProperty As DependencyProperty = DependencyProperty.Register("RealLeft", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RealRectChanged), AddressOf IsPropertyValid)
'    Public ReadOnly RealTopProperty As DependencyProperty = DependencyProperty.Register("RealTop", GetType(Double), GetType(UIControl(Of T)), New PropertyMetadata(CDbl(0), AddressOf RealRectChanged), AddressOf IsPropertyValid)

'    Private Function IsPropertyValid(ByVal value As Double) As Boolean
'        Return value >= 0
'    End Function
'    Private Sub RelativeRectChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
'        If HasParent Then
'            If e.Property Is RelativeWidthProperty Then
'                SetValue(RealWidthProperty, Parent.GetRealFromRelativeWidth(e.NewValue))
'            ElseIf e.Property Is RelativeHeightProperty Then
'                SetValue(RealHeightProperty, Parent.GetRealFromRelativeHeight(e.NewValue))
'            ElseIf e.Property Is RelativeLeftProperty Then
'                SetValue(RealLeftProperty, Parent.GetRealFromRelativeX(e.NewValue))
'            ElseIf e.Property Is RelativeTopProperty Then
'                SetValue(RealTopProperty, Parent.GetRealFromRelativeY(e.NewValue))
'            End If
'        Else
'            If e.Property Is RelativeWidthProperty Then
'                SetValue(RealWidthProperty, e.NewValue)
'            ElseIf e.Property Is RelativeHeightProperty Then
'                SetValue(RealHeightProperty, e.NewValue)
'            ElseIf e.Property Is RelativeLeftProperty Then
'                SetValue(RealLeftProperty, e.NewValue)
'            ElseIf e.Property Is RelativeTopProperty Then
'                SetValue(RealTopProperty, e.NewValue)
'            End If
'        End If
'    End Sub
'    Private Sub RealRectChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
'        If HasParent Then
'            If e.Property Is RealWidthProperty Then
'                SetValue(RelativeWidthProperty, Parent.GetRelativeFromRealWidth(e.NewValue))
'                Width = GetValue(RealWidthProperty)
'            ElseIf e.Property Is RealHeightProperty Then
'                SetValue(RelativeHeightProperty, Parent.GetRelativeFromRealHeight(e.NewValue))
'                Height = GetValue(RealHeightProperty)
'            ElseIf e.Property Is RealLeftProperty Then
'                SetValue(RelativeLeftProperty, Parent.GetRelativeFromRealX(e.NewValue))
'                Canvas.SetLeft(Me, GetValue(RealLeftProperty))
'            ElseIf e.Property Is RealTopProperty Then
'                SetValue(RelativeTopProperty, Parent.GetRelativeFromRealY(e.NewValue))
'                Canvas.SetTop(Me, GetValue(RealTopProperty))
'            End If
'        Else
'            If e.Property Is RealWidthProperty Then
'                SetValue(RealWidthProperty, e.NewValue)
'            ElseIf e.Property Is RealHeightProperty Then
'                SetValue(RelativeHeightProperty, e.NewValue)
'            ElseIf e.Property Is RealLeftProperty Then
'                SetValue(RelativeLeftProperty, e.NewValue)
'            ElseIf e.Property Is RealTopProperty Then
'                SetValue(RelativeTopProperty, e.NewValue)
'            End If
'        End If
'    End Sub

'    'Private _rect As Rect
'    Public Property RelativeTop As Double
'        Get
'            Return GetValue(RelativeTopProperty)
'        End Get
'        Set(ByVal value As Double)
'            SetValue(RelativeTopProperty, value)
'        End Set
'    End Property
'    Public Property RelativeLeft As Double
'        Get
'            Return GetValue(RelativeLeftProperty)
'        End Get
'        Set(ByVal value As Double)
'            SetValue(RelativeLeftProperty, value)
'        End Set
'    End Property
'    Public Property RealWidth As Double
'        Get
'            Return GetValue(RealWidthProperty)
'        End Get
'        Set(ByVal value As Double)
'            SetValue(RealWidthProperty, value)
'        End Set
'    End Property
'    Public Property RealHeight As Double
'        Get
'            Return GetValue(RealHeightProperty)
'        End Get
'        Set(ByVal value As Double)
'            SetValue(RealHeightProperty, value)
'        End Set
'    End Property
'    Public Property RealLeft As Double
'        Get
'            Return GetValue(RealLeftProperty)
'        End Get
'        Set(ByVal value As Double)
'            SetValue(RealLeftProperty, value)
'        End Set
'    End Property
'    Public Property RealTop As Double
'        Get
'            Return GetValue(RealTopProperty)
'        End Get
'        Set(ByVal value As Double)
'            SetValue(RealTopProperty, value)
'        End Set
'    End Property

'    'Public Property RelativeWidth As Double
'    '    Get
'    '        Return _rect.Width
'    '    End Get
'    '    Set(ByVal value As Double)
'    '        _rect.Width = value
'    '        If HasParent Then Width = Parent.GetRealFromRelativeWidth(value)
'    '    End Set
'    'End Property
'    'Public Property RelativeHeight As Double
'    '    Get
'    '        Return _rect.Height
'    '    End Get
'    '    Set(ByVal value As Double)
'    '        _rect.Height = value
'    '        If HasParent Then Height = Parent.GetRealFromRelativeHeight(value)
'    '    End Set
'    'End Property
'    'Public Property RelativeTop As Double
'    '    Get
'    '        Return _rect.Y
'    '    End Get
'    '    Set(ByVal value As Double)
'    '        _rect.Y = value
'    '        If HasParent Then Canvas.SetTop(Me, Parent.GetRealFromRelativeY(value))
'    '    End Set
'    'End Property
'    'Public Property RelativeLeft As Double
'    '    Get
'    '        Return _rect.X
'    '    End Get
'    '    Set(ByVal value As Double)
'    '        _rect.X = value
'    '        If HasParent Then Canvas.SetLeft(Me, Parent.GetRealFromRelativeX(value))
'    '    End Set
'    'End Property
'    'Public Property RealWidth As Double
'    'Public Property RealHeight As Double
'    'Public Property RealLeft As Double
'    'Public Property RealTop As Double

'    Public Overloads Property Content As T
'        Get
'            Return MyBase.Content
'        End Get
'        Set(ByVal value As T)
'            MyBase.Content = value
'        End Set
'    End Property

'    Protected Overrides Sub OnContentChanged(ByVal oldContent As Object, ByVal newContent As Object)
'        MyBase.OnContentChanged(oldContent, newContent)

'        BindingOperations.ClearBinding(Me, WidthProperty)
'        BindingOperations.ClearBinding(Me, HeightProperty)

'        Dim widthBinding As New Binding
'        widthBinding.Mode = BindingMode.TwoWay
'        widthBinding.ElementName = CType(newContent, FrameworkElement).Name
'        widthBinding.Path = New PropertyPath(ContentControl.WidthProperty)

'        Dim heightBinding As New Binding
'        heightBinding.ElementName = CType(newContent, FrameworkElement).Name
'        heightBinding.Path = New PropertyPath(ContentControl.HeightProperty)
'        heightBinding.Mode = BindingMode.TwoWay

'        SetBinding(WidthProperty, widthBinding)
'        SetBinding(HeightProperty, heightBinding)

'    End Sub

'    Private _isMouseDown As Boolean
'    Public ReadOnly Property IsMouseDown As Boolean Implements ChartObject.IsMouseDown
'        Get
'            Return _isMouseDown
'        End Get
'    End Property
'    Private _parent As Chart
'    Protected Overrides Sub OnVisualParentChanged(ByVal oldParent As System.Windows.DependencyObject)
'        If VisualParent Is Nothing OrElse (TypeOf VisualParent Is ChartDrawingVisualCanvas AndAlso TypeOf CType(VisualParent, ChartDrawingVisualCanvas).Parent Is Chart) Then
'            MyBase.OnVisualParentChanged(oldParent)
'            If VisualParent IsNot Nothing Then
'                _parent = CType(VisualParent, ChartDrawingVisualCanvas).Parent
'                SetValue(RealWidthProperty, Parent.GetRealFromRelativeWidth(GetValue(RelativeWidthProperty)))
'                SetValue(RealHeightProperty, Parent.GetRealFromRelativeWidth(GetValue(RelativeHeightProperty)))
'                SetValue(RealLeftProperty, Parent.GetRealFromRelativeWidth(GetValue(RelativeLeftProperty)))
'                SetValue(RealTopProperty, Parent.GetRealFromRelativeWidth(GetValue(RelativeTopProperty)))
'                'Height = Parent.GetRealFromRelativeHeight(_rect.Height)
'                'Canvas.SetTop(Me, Parent.GetRealFromRelativeY(_rect.Y))
'                'Canvas.SetLeft(Me, Parent.GetRealFromRelativeX(_rect.X))
'            End If
'        Else
'            Throw New InvalidOperationException("Chart controls can only be owned by the Chart control.")
'        End If
'    End Sub
'    Public ReadOnly Property HasParent As Boolean Implements ChartObject.HasParent
'        Get
'            Return Parent IsNot Nothing
'        End Get
'    End Property
'    Public Overloads ReadOnly Property Parent As Chart Implements ChartObject.Parent
'        Get
'            Return _parent
'        End Get
'    End Property

'    Public Sub ParentBoundsChanged() Implements ChartObject.ParentBoundsChanged
'        If ScaleableType = UIControl(Of T).ScaleType.MoveableOnly Or ScaleableType = UIControl(Of T).ScaleType.MoveableAndScaleable Then

'            SetValue(RelativeLeftProperty, GetValue(RelativeLeftProperty))
'            SetValue(RelativeTopProperty, GetValue(RelativeTopProperty))
'            'Canvas.SetTop(Me, Parent.GetRealFromRelativeY(_rect.Y))
'            'Canvas.SetLeft(Me, Parent.GetRealFromRelativeX(_rect.X))
'            If ScaleableType = UIControl(Of T).ScaleType.MoveableAndScaleable Then
'                SetValue(RelativeWidthProperty, GetValue(RelativeWidthProperty))
'                SetValue(RelativeHeightProperty, GetValue(RelativeHeightProperty))
'                'Width = Parent.GetRealFromRelativeWidth(_rect.Width)
'                'Height = Parent.GetRealFromRelativeHeight(_rect.Height)
'            End If
'        End If
'    End Sub

'    Private mouseOffset As Point
'    Private Sub UIControl_LostMouseCapture(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles Me.LostMouseCapture
'        _isMouseDown = False
'    End Sub
'    Private Sub UIControl_GotMouseCapture(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles Me.GotMouseCapture
'        _isMouseDown = True
'        mouseOffset = e.GetPosition(Me) - New Point(Canvas.GetLeft(Me), Canvas.GetTop(Me))
'    End Sub
'    Public Sub ParentMouseLeftButtonDown(ByVal location As System.Windows.Point) Implements ChartObject.ParentMouseLeftButtonDown
'    End Sub
'    Public Sub ParentMouseLeftButtonUp(ByVal location As System.Windows.Point) Implements ChartObject.ParentMouseLeftButtonUp
'    End Sub
'    Public Sub ParentMouseMove(ByVal location As System.Windows.Point) Implements ChartObject.ParentMouseMove
'        If IsMouseDown And IsDraggable Then
'            Dim loc As Point = location - mouseOffset
'            Canvas.SetTop(Me, loc.Y)
'            Canvas.SetLeft(Me, loc.X)
'            'If New Point(RelativeLeft, RelativeTop) <> loc Then
'            '    showinfobox("")
'            'End If
'            'RelativeLeft = loc.X
'            'RelativeTop = loc.Y
'        End If
'    End Sub

'    Public Property ScaleableType As ScaleType
'    Public Property SnapToRelativePixels As Boolean Implements ChartObject.SnapToRelativePixels
'    Public Property IsDraggable As Boolean

'    Enum ScaleType As Integer
'        Fixed = 2
'        MoveableOnly = 1
'        MoveableAndScaleable = 0
'    End Enum

'    Friend Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs) Implements ChartObject.Command_Executed

'    End Sub

'    Public ReadOnly Property ContainingBounds As System.Windows.Rect Implements ChartObject.ContainingBounds
'        Get
'            'Return New Rect(loc
'        End Get
'    End Property

'    Public Sub ParentMouseDoubleClick(ByVal location As System.Windows.Point) Implements ChartObject.ParentMouseDoubleClick

'    End Sub
'End Class
