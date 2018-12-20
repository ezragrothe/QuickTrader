Public Class DrawingVisualCanvas
    Inherits Canvas

    Public Sub New()
        SnapsToDevicePixels = True
    End Sub

    Dim _visuals As New List(Of Visual)
    Public ReadOnly Property Visuals As List(Of Visual)
        <DebuggerStepThrough()>
        Get
            Return _visuals
        End Get
    End Property
    Protected Overrides ReadOnly Property VisualChildrenCount As Integer
        <DebuggerStepThrough()>
        Get
            Return _visuals.Count
        End Get
    End Property
    <DebuggerStepThrough()>
    Protected Overrides Function GetVisualChild(ByVal index As Integer) As System.Windows.Media.Visual
        Return Visuals(index)
    End Function
    Public Sub AddVisual(ByVal visual As Visual)
        _visuals.Add(visual)
        If TypeOf visual Is ContentControl Then
            MyBase.Children.Add(visual)
            MyBase.AddLogicalChild(visual)
        Else
            MyBase.AddVisualChild(visual)
            MyBase.AddLogicalChild(visual)
        End If
    End Sub
    Public Sub RemoveVisual(ByVal visual As Visual)
        _visuals.Remove(visual)
        If TypeOf visual Is ContentControl Then
            MyBase.Children.Remove(visual)
            MyBase.RemoveLogicalChild(visual)
        Else
            MyBase.RemoveVisualChild(visual)
            MyBase.RemoveLogicalChild(visual)
        End If

    End Sub
    Public Function AllChildren() As List(Of Visual)
        Dim children As New List(Of Visual)
        For Each item In Visuals
            children.Add(item)
        Next
        For Each item In Me.Children
            children.Add(item)
        Next
        Return children
    End Function
    Protected Overrides Sub OnVisualChildrenChanged(ByVal visualAdded As System.Windows.DependencyObject, ByVal visualRemoved As System.Windows.DependencyObject)
        If TypeOf visualAdded Is IChartObject Or visualAdded Is Nothing Then
            MyBase.OnVisualChildrenChanged(visualAdded, visualRemoved)
        Else
            MyBase.Children.Remove(visualAdded)
            Throw New ArgumentException("Chart children must implement ChartObject.")
        End If
    End Sub
End Class