Public MustInherit Class DrawingVisual
    Inherits Windows.Media.DrawingVisual

    Public Sub New()

        'SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased)
    End Sub

    Protected MustOverride Overloads Property Drawing As System.Windows.Media.DrawingGroup
    'Public MustOverride ReadOnly Property Geometry As Geometry

    Public Property AutoRefresh As Boolean = False

    Public Property CanRefresh As Boolean = True

    Private _cleared As Boolean
    Public ReadOnly Property IsVisualCleared As Boolean
        Get
            Return _cleared
        End Get
    End Property

    Public Property DrawVisual As Boolean = True

    Public Overridable Sub RefreshVisual()
        'Log(ToString() & " redrawn at " & Now.ToLongTimeString)
        _cleared = False
        If DrawVisual Then
            Using dc As DrawingContext = RenderOpen()

                If CanRefresh Then
                    'Dim m As Matrix = PresentationSource.FromVisual(Me).CompositionTarget.TransformToDevice
                    'Dim dpiFactor As Double = 1 / m.M11
                    'Dim scaledPen As Pen = New Pen(Brushes.Black, 1 * dpiFactor)

                    'Dim pen As New Pen(Brushes.Black, 1)
                    'Dim rect As New Rect(20, 20, 50, 60)

                    'Dim halfPenWidth As Double = pen.Thickness / 2

                    '' Create a guidelines set
                    'Dim guidelines As New GuidelineSet()
                    'guidelines.GuidelinesX.Add(rect.Left + halfPenWidth)
                    'guidelines.GuidelinesX.Add(rect.Right + halfPenWidth)
                    'guidelines.GuidelinesY.Add(rect.Top + halfPenWidth)
                    'guidelines.GuidelinesY.Add(rect.Bottom + halfPenWidth)

                    'dc.PushGuidelineSet(guidelines)

                    dc.DrawDrawing(Drawing)


                End If

            End Using
        End If
    End Sub
    Public Overridable Sub ClearVisual()
        'Log(ToString() & " cleared at " & Now.ToLongTimeString)
        Using dc As DrawingContext = RenderOpen()
            If CanRefresh Then dc.DrawDrawing(Nothing)
        End Using
        _cleared = True
    End Sub
End Class
