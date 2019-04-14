Imports QuickTrader.AnalysisTechniques
Imports System.Windows.Controls.Primitives
Public MustInherit Class ChartDrawingVisual
    Inherits DrawingVisual
    Implements IChartObject

    Public Property AnalysisTechnique As AnalysisTechnique Implements IChartObject.AnalysisTechnique

    Private _pen As Pen = New Pen(Nothing, 1)
    Public Overridable Property Pen As Pen
        Get
            Return _pen
        End Get
        Set(ByVal value As Pen)
            _pen = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Overridable Property IsEditable As Boolean

    Public Enum LockPositionOrientationTypes
        None
        Horizontally
        Vertically
    End Enum
    ''' <summary>
    ''' Forces the control to be locked for a specific axis when moved by the user. Only implemented for the Rectangle control so far.
    ''' </summary>
    Public Property LockPositionOrientation As LockPositionOrientationTypes = LockPositionOrientationTypes.None
    ''' <summary>
    ''' Gets or sets whether the user can resize the control. Only implemented for the Rectangle control so far.
    ''' </summary>
    Public Property CanResize As Boolean = True
    ''' <summary>
    ''' Gets or sets whether the visible selection handles are shown. Only implemented for the Rectangle control so far.
    ''' </summary>
    Public Property ShowSelectionHandles As Boolean = True

    Private _useNegativeCoordinates As Boolean = True
    Public Overridable Property UseNegativeCoordinates As Boolean
        Get
            Return _useNegativeCoordinates
        End Get
        Set(ByVal value As Boolean)
            _useNegativeCoordinates = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Private _isMouseDown As Boolean
    Public ReadOnly Property IsMouseDown As Boolean Implements IChartObject.IsMouseDown
        Get
            Return _isMouseDown
        End Get
    End Property
    Private _isRightDown As Boolean
    Public ReadOnly Property IsRightMouseButtonDown As Boolean Implements IChartObject.IsRightMouseButtonDown
        Get
            Return _isRightDown
        End Get
    End Property
    Private _parent As Chart
    Protected Overrides Sub OnVisualParentChanged(ByVal oldParent As System.Windows.DependencyObject)
        If VisualParent Is Nothing OrElse (TypeOf VisualParent Is DrawingVisualCanvas AndAlso TypeOf CType(VisualParent, DrawingVisualCanvas).Parent Is Chart) Then
            MyBase.OnVisualParentChanged(oldParent)
            If VisualParent IsNot Nothing Then
                _parent = CType(VisualParent, DrawingVisualCanvas).Parent
                RaiseEvent GotParent(Me, New EventArgs)
                If AutoRefresh Then RefreshVisual()
            Else
                If popup IsNot Nothing Then popup.IsOpen = False
                _parent = Nothing
            End If
        Else
            Throw New InvalidOperationException("Chart controls can only be owned by the Chart control.")
        End If
    End Sub
    Public Property HasPhantomParent As Boolean = False Implements IChartObject.HasPhantomParent
    Public Event GotParent(ByVal sender As Object, ByVal e As EventArgs)
    Public ReadOnly Property HasParent As Boolean Implements IChartObject.HasParent
        Get
            Return _parent IsNot Nothing
        End Get
    End Property
    Public Overridable Overloads ReadOnly Property Parent As Chart Implements IChartObject.Parent
        Get
            Return _parent
        End Get
    End Property
    Public MustOverride Function ContainsPoint(ByVal point As Point) As Boolean

    'Public Overridable ReadOnly Property DoesNotUseGeometry As Boolean
    '    Get
    '        Return False
    '    End Get
    'End Property
    Public Overridable Property ClickLeniency As Double = 5
    Public Overrides Sub RefreshVisual()
        'If _parent Is Nothing OrElse Not _parent.IsLoaded OrElse DoesNotUseGeometry OrElse _parent.GetRealFromRelative(_parent.Bounds).ToWindowsRect.IntersectsWith(Geometry.Bounds) Then
        MyBase.RefreshVisual()

        'Else
        'MyBase.ClearVisual()
        'End If
    End Sub
    Public Overridable Sub ParentBoundsChanged() Implements IChartObject.ParentBoundsChanged
        RefreshVisual()
    End Sub

    Protected Overridable Function PopupInformation() As List(Of KeyValuePair(Of String, String))
        Return Nothing
    End Function
    Private popup As New ToolTip
    Private popupClickLocation As Point
    Public Overridable Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseLeftButtonDown
        _isMouseDown = True
        If My.Application.ShowMousePopups AndAlso Parent.Mode <> ClickMode.Pan AndAlso PopupInformation() IsNot Nothing AndAlso PopupInformation.Count > 0 Then
            popup.HorizontalOffset = 0
            popup.VerticalOffset = 0
            popup.IsOpen = True
            popup.StaysOpen = True
            popup.BorderThickness = New Thickness(0)
            popup.Background = Brushes.Transparent
            popup.Placement = PlacementMode.Relative
            popup.PlacementRectangle = New Rect(Parent.PointToScreen(New Point(Parent.ActualWidth / 3 * 2, Parent.Settings("TimeBarHeight").Value + 4)), New Size(100, 300))
            TextOptions.SetTextFormattingMode(popup, TextFormattingMode.Display)
            popupClickLocation = location
            DrawPopup()
        End If
        RaiseEvent MouseDown(Me, location)
    End Sub
    Public Event MouseDown(sender As Object, location As Point)
    Public Event RightMouseDown(sender As Object)
    Public Event MiddleMouseDown(sender As Object, location As Point)
    Protected Property IsPopupEnabled As Boolean = True
    Protected Property IsPopupMoveable As Boolean = True
    Protected Sub DrawPopup()
        If IsPopupEnabled AndAlso My.Application.ShowMousePopups AndAlso Parent.Mode <> ClickMode.Pan AndAlso IsMouseDown AndAlso PopupInformation() IsNot Nothing AndAlso PopupInformation.Count > 0 Then
            Dim grd As New Grid
            Dim bd As New Border
            popup.Content = bd
            bd.BorderThickness = New Thickness(2)
            bd.CornerRadius = New CornerRadius(2)
            bd.BorderBrush = Brushes.LightGray
            bd.Background = Brushes.Black
            bd.Child = grd
            grd.Margin = New Thickness(3)
            grd.MinWidth = 200
            grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            For Each item In PopupInformation()
                grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
                Dim textName As New TextBlock With {.Text = item.Key, .FontSize = 20, .Foreground = Brushes.White, .HorizontalAlignment = HorizontalAlignment.Left, .VerticalAlignment = VerticalAlignment.Center}
                Grid.SetRow(textName, grd.RowDefinitions.Count - 1)
                Dim textValue As New TextBlock With {.Text = item.Value, .FontSize = 20, .Foreground = Brushes.White, .HorizontalAlignment = HorizontalAlignment.Right, .VerticalAlignment = VerticalAlignment.Center}
                Grid.SetRow(textValue, grd.RowDefinitions.Count - 1)
                Grid.SetColumn(textValue, 1)
                grd.Children.Add(textName)
                grd.Children.Add(textValue)
            Next
        End If
    End Sub
    Public Overridable Sub ParentMouseLeftButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseLeftButtonUp
        _isMouseDown = False
        popup.IsOpen = False
        RaiseEvent MouseUp(Me)
    End Sub
    Public Overridable Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseMove
        DrawPopup()
        'If IsPopupMoveable Then
        'popup.HorizontalOffset = location.X - popupClickLocation.X
        'popup.VerticalOffset = location.Y - popupClickLocation.Y
        'End If
    End Sub
    Public Overridable Sub ParentMouseDoubleClick(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseDoubleClick
    End Sub
    Public Overridable Sub ParentMouseRightButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseRightButtonDown
        _isRightDown = True
        RaiseEvent RightMouseDown(Me)
    End Sub
    Public Overridable Sub ParentMouseRightButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseRightButtonUp
        _isRightDown = False
    End Sub
    Public Overridable Sub ParentMouseMiddleButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseMiddleButtonDown
        RaiseEvent MiddleMouseDown(Me, location)
    End Sub
    Public Overridable Sub ParentMouseMiddleButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point) Implements IChartObject.ParentMouseMiddleButtonUp

    End Sub
    Public Overridable Sub ParentMouseWheel(ByVal e As System.Windows.Input.MouseWheelEventArgs) Implements IChartObject.ParentMouseWheel
    End Sub

    Public Property SnapToRelativePixels As Boolean = True Implements IChartObject.SnapToRelativePixels

    Public Event MouseDragged(sender As Object)
    Protected Sub RaiseMouseDraggedEvent()
        RaiseEvent MouseDragged(Me)
    End Sub
    Public Event MouseUp(sender As Object)
    Protected Sub RaiseMouseUpEvent()
        RaiseEvent MouseUp(Me)
    End Sub
    Public Property Tag As Object Implements IChartObject.Tag

    Friend Overridable Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs) Implements IChartObject.Command_Executed
        If e.Command Is ChartCommands.RemoveObject Then
            Parent.Children.Remove(Me)
        ElseIf e.Command Is ChartCommands.RemoveAllObjectsOfSelectedType Then
            Dim parent As Chart = Me.Parent
            Dim i As Integer
            While i < parent.Children.Count
                If Me.GetType = parent.Children(i).GetType AndAlso parent.Children(i).AnalysisTechnique Is Nothing Then
                    parent.Children.RemoveAt(i)
                Else
                    i += 1
                End If
            End While
        End If
    End Sub

    Private _cursor As Cursor = Cursors.Arrow
    Public Overridable Property Cursor As Cursor
        Get
            Return _cursor
        End Get
        Set(ByVal value As Cursor)
            _cursor = value
            If HasParent Then
                Parent.ChangeCursor(Cursor)
            End If
        End Set
    End Property
End Class
