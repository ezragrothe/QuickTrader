Namespace AnalysisTechniques

    Public Class BoxTool

#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
            Description = "Offers a manual DayBox control."
        End Sub
#End Region
        <Input()> Public Property Box1Color As Color = Colors.Red
        <Input()> Public Property Box2Color As Color = Colors.Red
        <Input()> Public Property Box3Color As Color = Colors.Red
        '<Input> Public Property ShowBox1Only As Boolean = False
        <Input> Public Property LineThickness As Decimal
        <Input> Public Property LineColor As Color = Colors.Gray
        <Input> Public Property DashStyle As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString("11 6 3 6 3 6")
        <Input> Public Property MinimumHorizontalBoxSize As Decimal = 10
        <Input> Public Property TextColor As Color = Colors.Gray
        <Input> Public Property TextFontSize As Decimal = 11
        <Input> Public Property TextFontWeight As FontWeight = FontWeights.Normal
        '<Input> Public Property SnapToPriceAndTimeHotkey As Key = Key.Q
        <Input> Public Property Box1StartTime As String = "1700"
        <Input> Public Property Box3StartTime As String = "0700"
        <Input> Public Property Box1SnapSize As Size = New Size(93, 50) 'y is in min moves
        <Input> Public Property Box2SnapSize As Size = New Size(56, 25) 'y is in min moves
        <Input> Public Property Box3SnapSize As Size = New Size(43, 40) 'y is in min moves
        <Input> Public Property BindEndLineToDayBox As Boolean = True

        <Input("", "Box Locations")> Public Property Box1Rect As Rect = New Rect(100, 100, 300, 300)
        <Input> Public Property Box2Rect As Rect = New Rect(100, 100, 100, 150)
        <Input> Public Property Box3Rect As Rect = New Rect(200, 200, 200, 200)


        Public Property ChartPadVisible As Boolean = True Implements IChartPadAnalysisTechnique.ChartPadVisible
        Public Property ChartPadLocation As Point Implements IChartPadAnalysisTechnique.ChartPadLocation
            Get
                Return New Point(ChartPad.Left, ChartPad.Top)
            End Get
            Set(value As Point)
                ChartPad.Left = value.X
                ChartPad.Top = value.Y
            End Set
        End Property
        Public Property ChartPad As UIChartControl Implements IChartPadAnalysisTechnique.ChartPad

        Dim box1 As Rectangle
        Dim box2 As Rectangle
        Dim box3 As Rectangle
        Dim range1 As Label
        Dim range2 As Label
        Dim range3 As Label
        Dim clickCoordinates1 As Rect
        Dim clickCoordinates2 As Rect
        Dim clickCoordinates3 As Rect
        Dim mouseOffset1 As Point
        Dim mouseOffset2 As Point
        Dim mouseOffset3 As Point

        Dim SnapModeActive As Boolean

        'Case 1 ' top left
        'Case 2 ' top right
        'Case 3 ' bottom left
        'Case 4 ' bottom right
        'Case 5 ' left
        'Case 6 ' top
        'Case 7 ' right
        'Case 8 ' bottom

        Protected Overrides Sub Begin()
            MyBase.Begin()
            Dim rect1 As Rect = NegateY(Chart.GetRelativeFromReal(Box1Rect))
            Dim rect2 As Rect = NegateY(Chart.GetRelativeFromReal(Box2Rect))
            Dim rect3 As Rect = NegateY(Chart.GetRelativeFromReal(Box3Rect))

            box1 = NewRectangle(LineColor, Box1Color, rect1.TopLeft, rect1.BottomRight) : box1.Pen.DashStyle = New DashStyle(DashStyle, 0) : box1.Pen.Thickness = LineThickness : box1.ShowSelectionHandles = False
            box2 = NewRectangle(LineColor, Box2Color, rect2.TopLeft, rect2.BottomRight) : box2.Pen.DashStyle = New DashStyle(DashStyle, 0) : box2.Pen.Thickness = LineThickness : box2.ShowSelectionHandles = False
            box3 = NewRectangle(LineColor, Box3Color, rect3.TopLeft, rect3.BottomRight) : box3.Pen.DashStyle = New DashStyle(DashStyle, 0) : box3.Pen.Thickness = LineThickness : box3.ShowSelectionHandles = False

            AddHandler box1.ManuallyMoved, AddressOf BoxMoved
            AddHandler box2.ManuallyMoved, AddressOf BoxMoved
            AddHandler box3.ManuallyMoved, AddressOf BoxMoved
            AddHandler box1.MouseDown, AddressOf BoxMouseDown
            AddHandler box2.MouseDown, AddressOf BoxMouseDown
            AddHandler box3.MouseDown, AddressOf BoxMouseDown

            range1 = NewLabel("", TextColor, New Point(0, 0)) : range1.HorizontalAlignment = LabelHorizontalAlignment.Right : range1.VerticalAlignment = LabelVerticalAlignment.Bottom : range1.Font.FontSize = TextFontSize : range1.Font.FontWeight = TextFontWeight : range1.IsSelectable = False : range1.IsEditable = False
            range2 = NewLabel("", TextColor, New Point(0, 0)) : range2.HorizontalAlignment = LabelHorizontalAlignment.Right : range2.VerticalAlignment = LabelVerticalAlignment.Top : range2.Font.FontSize = TextFontSize : range2.Font.FontWeight = TextFontWeight : range2.IsSelectable = False : range2.IsEditable = False
            range3 = NewLabel("", TextColor, New Point(0, 0)) : range3.HorizontalAlignment = LabelHorizontalAlignment.Right : range3.VerticalAlignment = LabelVerticalAlignment.Top : range3.Font.FontSize = TextFontSize : range3.Font.FontWeight = TextFontWeight : range3.IsSelectable = False : range3.IsEditable = False
            If Not Double.IsNaN(box1.Height) Then
                range1.Location = FloorP(New Point(box1.X + box1.Width, box1.Y))
                range2.Location = FloorP(New Point(box2.X + box2.Width, box2.Y))
                range3.Location = FloorP(New Point(box3.X + box3.Width, box3.Y))
                range1.Text = Round(box1.Height, 2) * 1000
                range2.Text = Round(box2.Height, 2) * 1000
                range3.Text = Round(box3.Height, 2) * 1000
            End If
        End Sub
        Private Sub BoxMouseDown(sender As Object, location As Point)
            clickCoordinates1 = Chart.GetRealFromRelative(NegateY(box1.Coordinates))
            clickCoordinates2 = Chart.GetRealFromRelative(NegateY(box2.Coordinates))
            clickCoordinates3 = Chart.GetRealFromRelative(NegateY(box3.Coordinates))
            mouseOffset1 = location - Chart.GetRealFromRelative(NegateY(box1.Location))
            mouseOffset2 = location - Chart.GetRealFromRelative(NegateY(box2.Location))
            mouseOffset3 = location - Chart.GetRealFromRelative(NegateY(box3.Location))
        End Sub
        Dim location As Point
        Private Sub BoxMoved(sender As Object, index As Integer, location As Point)
            Me.location = location
            If sender Is box1 Then ' bigbox
                Select Case index
                    Case 1 'top left
                        Box1Left()
                    Case 2 'top right
                        Box1Top()
                        Box1Right()
                    Case 3 'bottom left
                        Box1Left()
                    Case 4 'bottom right
                        Box1Bottom()
                        Box1Right()
                    Case 5 'left 
                        Box1Left()
                    Case 6 'top
                        Box1Top()
                    Case 7 'right
                        Box1Right()
                    Case 8 'bottom 
                        Box1Bottom()
                    Case 9
                        Box1Middle()
                End Select
            ElseIf sender Is box2 Then
                Select Case index
                    Case 1 'top left
                        Box2Left()
                    Case 2 'top right
                        Box2Top()
                        Box2Right()
                    Case 3 'bottom left
                        Box2Left()
                    Case 4 'bottom right
                        Box2Bottom()
                        Box2Right()
                    Case 5 'left 
                        Box2Left()
                    Case 6 'top
                        Box2Top()
                    Case 7 'right
                        Box2Right()
                    Case 8 'bottom 
                        Box2Bottom()
                    Case 9
                        Box2Middle()
                End Select
            ElseIf sender Is box3 Then
                Select Case index
                    Case 1 'top left
                        Box3Top()
                        Box3Left()
                    Case 2 'top right
                        Box3Top()
                        Box3Right()
                    Case 3 'bottom left
                        Box3Bottom()
                        Box3Left()
                    Case 4 'bottom right
                        Box3Bottom()
                        Box3Right()
                    Case 5 'left 
                        Box3Left()
                    Case 6 'top
                        Box3Top()
                    Case 7 'right
                        Box3Right()
                    Case 8 'bottom 
                        Box3Bottom()
                    Case 9
                        Box3Middle()
                End Select
            End If
            RefreshLocation()
        End Sub
        Private Sub RefreshLocation()
            'Exit Sub
            box1.Coordinates = New Rect(Round(box1.X), box1.Y, Round(box1.Width), box1.Height)
            box2.Coordinates = New Rect(Round(box1.X), box2.Y, Round(box2.Width), box2.Height)
            box3.Coordinates = New Rect(Round(box1.X + box2.Width), box3.Y, Round(box1.Width - box2.Width), box3.Height)
            Box1Rect = Chart.GetRealFromRelative(NegateY(box1.Coordinates))
            Box2Rect = Chart.GetRealFromRelative(NegateY(box2.Coordinates))
            Box3Rect = Chart.GetRealFromRelative(NegateY(box3.Coordinates))
            range1.Location = FloorP(New Point(box1.X + box1.Width, box1.Y))
            range2.Location = FloorP(New Point(box2.X + box2.Width, box2.Y))
            range3.Location = FloorP(New Point(box3.X + box3.Width, box3.Y))
            range1.Text = Round(box1.Height, 2) * 1000
            range2.Text = Round(box2.Height, 2) * 1000
            range3.Text = Round(box3.Height, 2) * 1000
        End Sub
        Sub Box1Left()
            box1.Location = NegateY(Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset1)))
            box1.Width = Chart.GetRelativeFromRealWidth(clickCoordinates1.Width)
            box1.Height = Chart.GetRelativeFromRealHeight(clickCoordinates1.Height)
            box2.Location = New Point(box1.X, -Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset2)).Y)
            box3.Location = New Point(box1.X + box2.Width, -Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset3)).Y)
            SnapModeActive = False
            Box2Locked = False
        End Sub
        Sub Box1Middle()
            box1.Location = New Point(Chart.GetRelativeFromRealX(clickCoordinates1.X), -Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset1)).Y)
            If SnapModeActive Then
                Constrain1()
            End If
            box2.Location = New Point(box2.X, -Chart.GetRelativeFromRealY(clickCoordinates2.Y) + Chart.GetRelativeFromRealY(clickCoordinates1.Y) + box1.Y)
            box3.Location = New Point(box3.X, -Chart.GetRelativeFromRealY(clickCoordinates3.Y) + Chart.GetRelativeFromRealY(clickCoordinates1.Y) + box1.Y)
            If SnapModeActive Then
                Constrain2()
                Constrain3()
                Constrain1EmptySpace()
            End If
        End Sub
        Sub Box1Right()
            If box1.Width < box2.Width + Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize) Then
                box1.Width = box2.Width + Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize)
            End If
            If SnapModeActive Then
                If box1.X + box1.Width <= CurrentBar.Number Then
                    box1.Width = CurrentBar.Number - box1.X
                End If
            End If
            box3.Width = (box1.Width - box2.Width)
        End Sub
        Sub Box1Top()
            If box1.Y - (box2.Y - box2.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                box1.Location = New Point(box1.X, box2.Y - box2.Height + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                box1.Height = box1.Y - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
            End If
            If box1.Y - (box3.Y - box3.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                box1.Location = New Point(box1.X, box3.Y - box2.Height + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                box1.Height = box1.Y - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
            End If
            If Box2Locked = False Then
                If Round(clickCoordinates1.Y) = Round(clickCoordinates2.Y) Then
                    box2.Location = New Point(box2.X, box1.Y)
                    box2.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                End If
            End If

            If Round(clickCoordinates1.Y) = Round(clickCoordinates3.Y) Then
                box3.Location = New Point(box3.X, box1.Y)
                box3.Height = box3.Y - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
            End If
            If SnapModeActive Then
                If box1.Y < h1 Then
                    box1.Y = h1
                    box1.Height = h1 - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
                End If
                If Box2Locked = False Then
                    If box2.Y < h2 Then
                        box2.Y = h2
                        box2.Height = h2 - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                    End If
                End If
                If box3.Y < h3 Then
                    box3.Y = h3
                    box3.Height = h3 - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                End If
            End If
            If Box2Locked = False Then
                If box1.Y < box2.Y Then
                    box2.Location = New Point(box2.X, box1.Y)
                    box2.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                End If
            End If
            If box1.Y < box3.Y Then
                box3.Location = New Point(box3.X, box1.Y)
                box3.Height = box3.Y - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
            End If
            If SnapModeActive Then Constrain1EmptySpace()
        End Sub
        Sub Box1Bottom()
            If box3.Y - (box1.Y - box1.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                box1.Height = box1.Y - (box3.Y - Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
            End If
            If box2.Y - (box1.Y - box1.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                box1.Height = box1.Y - (box2.Y - Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
            End If
            If box3.Y - box3.Height < box1.Y - box1.Height Then
                box3.Height = box3.Y - (box1.Y - box1.Height)
            End If
            If Box2Locked = False Then
                If box2.Y - box2.Height < box1.Y - box1.Height Then
                    box2.Height = box2.Y - (box1.Y - box1.Height)
                End If
                If Round(clickCoordinates1.Bottom) = Round(clickCoordinates2.Bottom) Then
                    box2.Height = box2.Y - (box1.Y - box1.Height)
                End If
            End If
            If Round(clickCoordinates1.Bottom) = Round(clickCoordinates3.Bottom) Then
                box3.Height = box3.Y - (box1.Y - box1.Height)
            End If
            If SnapModeActive Then
                If box1.Y - box1.Height > l1 Then
                    box1.Height = -Chart.GetRelativeFromRealY(clickCoordinates1.Top) - l1
                End If
                If Box2Locked = False Then
                    If box2.Y - box2.Height > l2 Then
                        box2.Height = -Chart.GetRelativeFromRealY(clickCoordinates2.Top) - l2
                    End If
                End If
                If box3.Y - box3.Height > l3 Then
                    box3.Height = -Chart.GetRelativeFromRealY(clickCoordinates3.Top) - l3
                End If
            End If
            If SnapModeActive Then Constrain1EmptySpace()
        End Sub
        Sub Box2Middle()
            If Box2Locked = False Then
                Dim desiredPos = -Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset2)).Y
                If box2.Y < box1.Y And box2.Y - box2.Height > box1.Y - box1.Height Then
                    box2.Location = (New Point(Chart.GetRelativeFromRealX(clickCoordinates2.X), desiredPos))
                ElseIf box2.Y >= box1.Y Then
                    box2.Location = (New Point(Chart.GetRelativeFromRealX(clickCoordinates2.X), box1.Y))
                ElseIf box2.Y - box2.Height <= box1.Y - box1.Height Then
                    box2.Location = (New Point(Chart.GetRelativeFromRealX(clickCoordinates2.X), box1.Y - box1.Height + box2.Height))
                End If
                If box3.Y - box3.Height > box2.Y Then
                    box3.Height = box3.Y - box2.Y
                End If
                If box3.Y < box2.Y - box2.Height Then
                    box3.Location = New Point(box3.X, box2.Y - box2.Height)
                    box3.Height = box3.Y - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                End If
                Constrain2()
            Else
                box2.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates2))
            End If
            If SnapModeActive Then Constrain1EmptySpace()
        End Sub
        Sub Box2Left()
            box1.Location = (NegateY(Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset1))))
            box2.Location = (NegateY(Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset2))))
            box3.Location = (NegateY(Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset3))))
            box2.Width = (Chart.GetRelativeFromRealWidth(clickCoordinates2.Width))
            box2.Height = Chart.GetRelativeFromRealHeight(clickCoordinates2.Height)
            SnapModeActive = False
            Box2Locked = False
        End Sub
        Sub Box2Right()
            If Box2Locked = False Then
                If box2.X + box2.Width > box1.X + box1.Width - Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize) Then
                    box2.Width = (box1.Width - Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize))
                End If
                If box2.Width < Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize) Then
                    box2.Width = (Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize))
                End If
                box3.Location = (New Point(box2.X + box2.Width, box3.Y))
                If SnapModeActive Then
                    If box2.X + box2.Width <= CurrentBar.Number Then
                        box2.Width = CurrentBar.Number - box2.X
                    End If
                End If
                box3.Width = (box1.X + box1.Width - (box2.X + box2.Width))
            Else
                box2.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates2))
            End If
        End Sub
        Sub Box2Top()
            If Box2Locked = False Then
                If Round(clickCoordinates2.Y + clickCoordinates2.Height) = Round(clickCoordinates1.Y + clickCoordinates1.Height) And Round(clickCoordinates1.Y) = Round(clickCoordinates2.Y) Then
                    If box2.Y > -Chart.GetRelativeFromRealY(clickCoordinates1.Y) Then
                        box1.Height = box2.Height
                        box1.Location = New Point(box1.X, box2.Y)
                    Else
                        box1.Height = Chart.GetRelativeFromRealHeight(clickCoordinates1.Height)
                        box1.Location = NegateY(Chart.GetRelativeFromReal(clickCoordinates1.TopLeft))
                        If box2.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                            box2.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                            box2.Location = New Point(box2.X, Chart.GetRelativeFromRealY(clickCoordinates2.Bottom - Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)))
                        End If
                    End If
                ElseIf Round(clickCoordinates1.Y) = Round(clickCoordinates2.Y) Then
                    If box2.Y - (box3.Y - box3.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                        box2.Location = New Point(box2.X, -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom) + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                        box2.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                    End If
                    If box2.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                        box2.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                        box2.Location = New Point(box2.X, -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom) + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                    End If
                    box1.Location = New Point(box1.X, box2.Y)
                    box1.Height = box1.Y - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
                    If box3.Y > box1.Y Then
                        box3.Location = New Point(box3.X, box1.Y)
                        box3.Height = box3.Y - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                    End If
                Else
                    If box2.Y > box1.Y Then
                        box2.Location = (New Point(box2.X, box1.Y))
                        box2.Height = (box1.Y + Chart.GetRelativeFromRealY(clickCoordinates2.Y + clickCoordinates2.Height))
                    End If
                    If box2.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                        box2.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                        box2.Location = New Point(box2.X, -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom - Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)) + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                    End If
                End If
                If box3.Y - box3.Height > box2.Y Then
                    box3.Height = box3.Y - box2.Y
                End If
                If SnapModeActive Then
                    If box1.Y < h1 Then
                        box1.Y = h1
                        box1.Height = h1 - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
                    End If
                    If box2.Y < h2 Then
                        box2.Y = h2
                        box2.Height = h2 - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                    End If
                    If box3.Y < h3 Then
                        box3.Y = h3
                        box3.Height = h3 - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                    End If
                End If
            Else
                box2.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates2))
            End If
            If SnapModeActive Then Constrain1EmptySpace()
        End Sub
        Sub Box2Bottom()
            If Box2Locked = False Then
                If Round(clickCoordinates2.Y + clickCoordinates2.Height) = Round(clickCoordinates1.Y + clickCoordinates1.Height) And Round(clickCoordinates1.Y) = Round(clickCoordinates2.Y) Then
                    If box2.Y - box2.Height < -Chart.GetRelativeFromRealY(clickCoordinates1.Y + clickCoordinates1.Height) Then
                        box1.Height = box2.Height
                    Else
                        box1.Height = Chart.GetRelativeFromRealHeight(clickCoordinates1.Height)
                        If box2.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                            box2.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                        End If
                    End If
                ElseIf Round(clickCoordinates2.Y + clickCoordinates2.Height) = Round(clickCoordinates1.Y + clickCoordinates1.Height) Then
                    If box3.Y - (box2.Y - box2.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                        box2.Height = box2.Y - box3.Y + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                    End If
                    If box2.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                        box2.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                    End If
                    box1.Height = box1.Y - (box2.Y - box2.Height)
                    If box3.Y - box3.Height < box1.Y - box1.Height Then
                        box3.Height = box3.Y - (box1.Y - box1.Height)
                    End If
                Else
                    If box2.Y - box2.Height < box1.Y - box1.Height Then
                        box2.Height = box2.Y - (box1.Y - box1.Height)
                    End If
                    If box2.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                        box2.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                    End If
                End If
                If box3.Y < box2.Y - box2.Height Then
                    box3.Location = New Point(box3.X, box2.Y - box2.Height)
                    box3.Height = box3.Y - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                End If
                If SnapModeActive Then
                    If box2.Y - box2.Height > l2 Then
                        box2.Height = -Chart.GetRelativeFromRealY(clickCoordinates2.Top) - l2
                    End If
                    If box3.Y - box3.Height > l3 Then
                        box3.Height = -Chart.GetRelativeFromRealY(clickCoordinates3.Top) - l3
                    End If
                    If box1.Y - box1.Height > l1 Then
                        box1.Height = -Chart.GetRelativeFromRealY(clickCoordinates1.Top) - l1
                    End If
                End If
            Else
                box2.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates2))
            End If
            If SnapModeActive Then Constrain1EmptySpace()
        End Sub
        Sub Box3Middle()
            Dim desiredPos = -Chart.GetRelativeFromReal(VectorToPoint(location - mouseOffset3)).Y
            If box3.Y < box1.Y And box3.Y - box3.Height > box1.Y - box1.Height Then
                box3.Location = (New Point(Chart.GetRelativeFromRealX(clickCoordinates3.X), desiredPos))
            ElseIf box3.Y >= box1.Y Then
                box3.Location = (New Point(Chart.GetRelativeFromRealX(clickCoordinates3.X), box1.Y))
            ElseIf box3.Y - box3.Height <= box1.Y - box1.Height Then
                box3.Location = (New Point(Chart.GetRelativeFromRealX(clickCoordinates3.X), box1.Y - box1.Height + box3.Height))
            End If
            If box2.Y - box2.Height > box3.Y Then
                box2.Height = box2.Y - box3.Y
            End If
            If box2.Y < box3.Y - box3.Height Then
                box2.Location = New Point(box2.X, box3.Y - box3.Height)
                box2.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
            End If
            Constrain3()
            If SnapModeActive Then Constrain1EmptySpace()
        End Sub
        Sub Box3Left()
            If Box2Locked = False Then
                If box3.Width < Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize) Then
                    box3.Width = (Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize))
                    box3.Location = (New Point(box1.X + box1.Width - Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize), box3.Y))
                End If
                If box3.X < box1.X + Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize) Then
                    box3.Location = (New Point(box1.X + Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize), box3.Y))
                    box3.Width = (box1.Width - Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize))
                End If
                box2.Width = (box3.X - box1.X)
                If SnapModeActive Then
                    If box2.X + box2.Width <= CurrentBar.Number Then
                        box2.Width = CurrentBar.Number - box2.X
                    End If
                End If

            Else
                box3.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates3))
            End If
        End Sub
        Sub Box3Right()
            If box3.Width < Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize) Then
                box3.Width = (Chart.GetRelativeFromRealWidth(MinimumHorizontalBoxSize))
            End If
            If SnapModeActive Then
                If box3.X + box3.Width <= CurrentBar.Number Then
                    box3.Width = CurrentBar.Number - box3.X
                End If
            End If
            box1.Width = (box2.Width + box3.Width)
        End Sub
        Sub Box3Top()
            If SnapModeActive Then
                Dim maxTop As Double = Round(Max(box2.Y, box3.Y), 4)
                Dim minBot As Double = Round(Min(box2.Y - box2.Height, box3.Y - box3.Height), 4)
                If Round(box1.Y, 4) > maxTop Then
                    box1.Height = box3.Y - (box1.Y - box1.Height)
                    box1.Y = box3.Y

                End If
                If Round(box1.Y - box1.Height, 4) < minBot Then
                    box1.Height = box3.Height
                    'box3.Y = maxTop
                End If
            End If
            If Round(clickCoordinates3.Y + clickCoordinates3.Height) = Round(clickCoordinates1.Y + clickCoordinates1.Height) And Round(clickCoordinates1.Y) = Round(clickCoordinates3.Y) Then
                If box3.Y > -Chart.GetRelativeFromRealY(clickCoordinates1.Y) Then
                    box1.Height = box3.Height
                    box1.Location = New Point(box1.X, box3.Y)
                Else
                    box1.Height = box3.Height
                    box1.Location = New Point(box1.X, box3.Y)
                    If SnapModeActive Then
                        If box3.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                            box3.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                            box3.Location = New Point(box3.X, Chart.GetRelativeFromRealY(clickCoordinates3.Bottom - Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)))
                        End If
                    Else
                        box1.Height = Chart.GetRelativeFromRealHeight(clickCoordinates1.Height)
                        box1.Location = NegateY(Chart.GetRelativeFromReal(clickCoordinates1.TopLeft))
                    End If
                End If
                    ElseIf Round(clickCoordinates1.Y) = Round(clickCoordinates3.Y) Then
                If box3.Y - (box2.Y - box2.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                    box3.Location = New Point(box3.X, -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom) + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                    box3.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                End If
                If box3.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                    box3.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                    box3.Location = New Point(box3.X, -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom) + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                End If
                box1.Location = New Point(box1.X, box3.Y)
                box1.Height = box1.Y - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
                If box2.Y > box1.Y Then
                    box2.Location = New Point(box2.X, box1.Y)
                    box2.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                End If
            Else
                If box3.Y > box1.Y Then
                    box3.Location = (New Point(box3.X, box1.Y))
                    box3.Height = (box1.Y + Chart.GetRelativeFromRealY(clickCoordinates3.Y + clickCoordinates3.Height))
                End If
                If box3.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                    box3.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                    box3.Location = New Point(box3.X, -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom - Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)) + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize))
                End If
            End If
            If box2.Y - box2.Height > box3.Y Then
                box2.Height = box2.Y - box3.Y
            End If
            If SnapModeActive Then
                If box1.Y < h1 Then
                    box1.Y = h1
                    box1.Height = h1 - -Chart.GetRelativeFromRealY(clickCoordinates1.Bottom)
                End If
                If Box2Locked = False Then
                    If box2.Y < h2 Then
                        box2.Y = h2
                        box2.Height = h2 - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
                    End If
                Else
                    box2.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates2))
                End If
                If box3.Y < h3 Then
                    box3.Y = h3
                    box3.Height = h3 - -Chart.GetRelativeFromRealY(clickCoordinates3.Bottom)
                End If
            End If
        End Sub
        Sub Box3Bottom()
            If SnapModeActive Then
                Dim maxTop As Double = Round(Max(box2.Y, box3.Y), 4)
                Dim minBot As Double = Round(Min(box2.Y - box2.Height, box3.Y - box3.Height), 4)
                If Round(box1.Y, 4) > maxTop Then
                    box3.Height = box1.Y - (box3.Y - box3.Height)
                    box3.Y = box1.Y
                End If
                If Round(box1.Y - box1.Height, 4) < minBot Then
                    box1.Height = box3.Height
                    'box3.Y = maxTop
                End If
            End If
            If Round(clickCoordinates3.Y + clickCoordinates3.Height) = Round(clickCoordinates1.Y + clickCoordinates1.Height) And Round(clickCoordinates1.Y) = Round(clickCoordinates3.Y) Then
                If box3.Y - box3.Height < -Chart.GetRelativeFromRealY(clickCoordinates1.Y + clickCoordinates1.Height) Then
                    box1.Height = box3.Height
                Else
                    box1.Height = box3.Height
                    If SnapModeActive Then
                        If box3.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                            box3.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                        End If
                    Else
                        box1.Height = Chart.GetRelativeFromRealHeight(clickCoordinates1.Height)
                    End If
                End If
                    ElseIf Round(clickCoordinates3.Y + clickCoordinates3.Height) = Round(clickCoordinates1.Y + clickCoordinates1.Height) Then
                If box2.Y - (box3.Y - box3.Height) < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                    box3.Height = box3.Y - box2.Y + Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                End If
                If box3.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                    box3.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                End If
                box1.Height = box1.Y - (box3.Y - box3.Height)
                If box2.Y - box2.Height < box1.Y - box1.Height Then
                    box2.Height = box2.Y - (box1.Y - box1.Height)
                End If
            Else
                If box3.Y - box3.Height < box1.Y - box1.Height Then
                    box3.Height = box3.Y - (box1.Y - box1.Height)
                End If
                If box3.Height < Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize) Then
                    box3.Height = Chart.GetRelativeFromRealHeight(MinimumHorizontalBoxSize)
                End If
            End If
            If box2.Y < box3.Y - box3.Height Then
                box2.Location = New Point(box2.X, box3.Y - box3.Height)
                box2.Height = box2.Y - -Chart.GetRelativeFromRealY(clickCoordinates2.Bottom)
            End If
            If SnapModeActive Then
                If box3.Y - box3.Height > l3 Then
                    box3.Height = -Chart.GetRelativeFromRealY(clickCoordinates3.Top) - l3
                End If
                If Box2Locked = False Then
                    If box2.Y - box2.Height > l2 Then
                        box2.Height = -Chart.GetRelativeFromRealY(clickCoordinates2.Top) - l2
                    End If
                Else
                    box2.Coordinates = NegateY(Chart.GetRelativeFromReal(clickCoordinates2))
                End If
                If box1.Y - box1.Height > l1 Then
                    box1.Height = -Chart.GetRelativeFromRealY(clickCoordinates1.Top) - l1
                End If
            End If

        End Sub
        Dim Box2Locked As Boolean
        Dim sTime As DateTime, mTime As DateTime
        Dim l1 As Decimal = Decimal.MaxValue, h1 As Decimal = 0, l2 As Decimal = Decimal.MaxValue, h2 As Decimal = 0, l3 As Decimal = Decimal.MaxValue, h3 As Decimal = 0
        Private Sub ConstrainToBars()
            l1 = Decimal.MaxValue
            l2 = Decimal.MaxValue
            l3 = Decimal.MaxValue
            h1 = 0
            h2 = 0
            h3 = 0
            For i = Max(1, box1.X - 1) To Min(Chart.bars.Count - 1, box1.X + box1.Width - 1)
                If i >= box1.X - 1 And i < box1.X + box2.Width Then
                    l2 = Min(l2, Chart.bars(i).Data.Low)
                    h2 = Max(h2, Chart.bars(i).Data.High)
                End If
                If i >= box3.X - 1 And i < box3.X + box3.Width Then
                    l3 = Min(l3, Chart.bars(i).Data.Low)
                    h3 = Max(h3, Chart.bars(i).Data.High)
                End If
                l1 = Min(l1, Chart.bars(i).Data.Low)
                h1 = Max(h1, Chart.bars(i).Data.High)
            Next
            Constrain1()
            Constrain2()
            Constrain3()
            Constrain1EmptySpace()
            RefreshLocation()
        End Sub
        Private Sub Constrain1()
            If SnapModeActive Then
                If h1 - l1 > box1.Height Then
                    box1.Height = h1 - l1
                End If
                If box1.Y < h1 Then
                    box1.Y = h1
                End If
                If box1.Y - box1.Height > l1 Then
                    Dim dif = box1.Y - box1.Height - l1
                    box1.Y -= dif
                End If
            End If
        End Sub
        Private Sub Constrain1EmptySpace()
            If SnapModeActive Then
                Dim maxTop As Double = Round(Max(box2.Y, box3.Y), 4)
                Dim minBot As Double = Round(Min(box2.Y - box2.Height, box3.Y - box3.Height), 4)
                If Round(box1.Y, 4) > maxTop Then
                    box3.Height = box1.Y - (box3.Y - box3.Height)
                    box3.Y = box1.Y
                End If
                If Round(box1.Y - box1.Height, 4) < minBot Then

                    box3.Height = box1.Y - (box1.Y - box1.Height)
                    'box3.Y = maxTop
                End If
            End If
        End Sub
        Private Sub Constrain2()
            If SnapModeActive Then
                If Box2Locked Then
                    box2.Height = h2 - l2
                End If
                If h2 - l2 > box2.Height Then
                    box2.Height = h2 - l2
                End If
                If box2.Y < h2 Then
                    box2.Y = h2
                End If
                If box2.Y - box2.Height > l2 Then
                    Dim dif = box2.Y - box2.Height - l2
                    box2.Y -= dif
                End If
                If box2.Y > box1.Y Then
                    box2.Y = box1.Y
                End If
                If box2.Y - box2.Height < box1.Y - box1.Height Then
                    box2.Y = box1.Y - box1.Height + box2.Height
                End If
            End If
        End Sub
        Private Sub Constrain3()
            If SnapModeActive Then
                If h3 - l3 > box3.Height Then
                    box3.Height = h3 - l3
                End If
                If box3.Y < h3 Then
                    box3.Y = h3
                End If
                If box3.Y - box3.Height > l3 Then
                    Dim dif = box3.Y - box3.Height - l3
                    box3.Y -= dif
                End If
                If box3.Y > box1.Y Then
                    box3.Y = box1.Y
                End If
                If box3.Y - box3.Height < box1.Y - box1.Height Then
                    box3.Y = box1.Y - box1.Height + box3.Height
                End If
            End If
        End Sub
        Private Sub SnapToTime(Optional snapPreference As Direction = Direction.Neutral)
            If Date.TryParseExact(Box1StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) And Date.TryParseExact(Box3StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) Then
                sTime = Date.ParseExact(Box1StartTime, "HHmm", Nothing)
                mTime = Date.ParseExact(Box3StartTime, "HHmm", Nothing)
                box1.Width = Box1SnapSize.Width
                box1.Height = Box1SnapSize.Height / 1000
                box2.Width = Box2SnapSize.Width
                box2.Height = Box2SnapSize.Height / 1000
                box3.Width = Box3SnapSize.Width
                box3.Height = Box3SnapSize.Height / 1000

                box2.Fill = New SolidColorBrush(Box2Color)
                Box2Locked = False
                If snapPreference = Direction.Up Then
                    box1.Y = h1
                ElseIf snapPreference = Direction.Down Then
                    box1.Y = l1 + box1.Height
                End If
                For i = Chart.bars.Count - 1 To 1 Step -1
                    If IsTimeBetween(Chart.bars(i - 1).Data.Date, Chart.bars(i).Data.Date, sTime) Then
                        box1.X = i + 1
                        box2.X = i + 1
                        SnapModeActive = True
                        For j = i + 1 To Chart.bars.Count - 1
                            If IsTimeBetween(Chart.bars(j - 1).Data.Date, Chart.bars(j).Data.Date, mTime) Then
                                Box2Locked = True
                                box2.Fill = Brushes.Transparent
                                box2.Width = j - i
                                Exit For
                            End If
                        Next
                        box1.Width = Max(box1.Width, Max(box2.Width + MinimumHorizontalBoxSize, Chart.bars.Count - box1.X))
                        box3.Height = Min(box3.Height, box1.Height)
                        box2.Height = Min(box2.Height, box1.Height)
                        If box3.X + box3.Width <= CurrentBar.Number Then
                            box3.Width = CurrentBar.Number - box3.X
                        End If
                        If Box2Locked = False And box2.X + box2.Width <= CurrentBar.Number Then
                            box2.Width = CurrentBar.Number - box2.X
                            box3.X = box2.X + box2.Width
                        End If
                        RefreshLocation()
                        '
                        Exit For
                    End If
                Next
                If snapPreference <> Direction.Neutral Then
                    'box2.Height = Box2SnapSize.Height / 1000
                    'box3.Height = Box3SnapSize.Height / 1000
                    If snapPreference = Direction.Up Then
                        box1.Y = h1
                        If Box2Locked = False Then
                            box2.Y = h1
                            box3.Y = box1.Y - box1.Height + box3.Height  'Max(h1, box1.Y)
                        Else
                            If h3 - (box1.Y - box1.Height) < box3.Height Then
                                box3.Y = box1.Y - box1.Height + box3.Height
                            Else
                                box3.Y = h3
                                box3.Height = box3.Y - (box1.Y - box1.Height)
                            End If

                        End If
                    ElseIf snapPreference = Direction.Down Then
                        box1.Y = l1 + box1.Height
                        If Box2Locked = False Then
                            box2.Y = l1 + box2.Height
                            box3.Y = box1.Y
                        Else
                            If box1.Y - l3 < box3.Height Then
                                box3.Y = box1.Y
                            Else
                                box3.Height = box1.Y - l3
                                box3.Y = l3 + box3.Height
                            End If
                            'If l3 < l2 Then
                            '    box3.Y = l3 + box3.Height
                            'Else
                            '    box3.Y = l1 + box3.Height
                            'End If
                        End If
                        'box3.Height = Max(box3.Height, box1.Height)
                        ' box3.Y = box1.Y
                    End If
                    ConstrainToBars()
                    Constrain1EmptySpace()
                    RefreshLocation()
                End If

            End If
        End Sub
        Protected Overrides Sub Main()
            If SnapModeActive And IsLastBarOnChart Then
                If CurrentBar.Number > 1 Then
                    If Date.TryParseExact(Box1StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) And Date.TryParseExact(Box3StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) Then
                        Dim barDate As Date = CurrentBar.Date
                        Dim prevBarDate As Date = Chart.bars(CurrentBar.Number - 2).Data.Date
                        sTime = Date.ParseExact(Box1StartTime, "HHmm", Nothing)
                        mTime = Date.ParseExact(Box3StartTime, "HHmm", Nothing)
                        If IsTimeBetween(prevBarDate, barDate, sTime) Then
                            box1.X = CurrentBar.Number
                            Box2Locked = False
                            box2.Fill = New SolidColorBrush(Box2Color)
                            RefreshLocation()
                            ConstrainToBars()
                        End If
                        If IsTimeBetween(prevBarDate, barDate, mTime) Then
                            box2.Fill = Brushes.Transparent
                            Box2Locked = True
                            box2.Width = CurrentBar.Number - box1.X
                            RefreshLocation()
                            ConstrainToBars()
                        End If
                        If box3.X + box3.Width <= CurrentBar.Number Then
                            box3.Width = CurrentBar.Number - box3.X
                        End If
                        If Box2Locked = False And box2.X + box2.Width <= CurrentBar.Number Then
                            box2.Width = CurrentBar.Number - box2.X
                            box3.X = box2.X + box2.Width
                        End If
                        l1 = Min(l1, CurrentBar.Low)
                        h1 = Max(h1, CurrentBar.High)
                        If Box2Locked Then
                            l3 = Min(l3, CurrentBar.Low)
                            h3 = Max(h3, CurrentBar.High)
                        Else
                            l2 = Min(l2, CurrentBar.Low)
                            h2 = Max(h2, CurrentBar.High)
                        End If
                        Constrain1()
                        Constrain2()
                        Constrain3()
                        Constrain1EmptySpace()
                        RefreshLocation()

                    End If
                End If
                If BindEndLineToDayBox Then
                    Dim db As DayBox = Nothing
                    For Each technique In Chart.AnalysisTechniques
                        If TypeOf technique.AnalysisTechnique Is DayBox And technique.AnalysisTechnique.IsEnabled Then
                            db = technique.AnalysisTechnique
                            Exit For
                        End If
                    Next
                    If db IsNot Nothing Then
                        Dim location As Decimal = Chart.GetRealFromRelativeX(db.CurrentBox.RectObj.Coordinates.Right)
                        If location > Chart.GetRealFromRelativeX(box3.X + MinimumHorizontalBoxSize) Then
                            box3.Width = Chart.GetRelativeFromRealX(location) - box3.X
                            box1.Width = box2.Width + box3.Width
                        End If
                    End If
                End If
            End If
        End Sub
        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled Then
                Dim key As Key
                If e.SystemKey = Key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
                'If key = SnapToPriceAndTimeHotkey Then
                ' SnapToTime()
                ' End If
            End If
        End Sub
        Private Function IsTimeBetween(firstDateTime As Date, secondDateTime As Date, dateTime As Date) As Boolean
            If (dateTime.TimeOfDay > firstDateTime.TimeOfDay And dateTime.TimeOfDay <= secondDateTime.TimeOfDay And firstDateTime.Date = secondDateTime.Date) Or
                   ((dateTime.TimeOfDay > firstDateTime.TimeOfDay Or dateTime.TimeOfDay < secondDateTime.TimeOfDay) And firstDateTime.Date <> secondDateTime.Date) Or (firstDateTime.Date - secondDateTime.Date).TotalDays > 1 Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Property Name As String = Me.GetType.Name
#Region "AutoTrendPad"

        Dim SnapHButton As Button
        Dim SnapLButton As Button
        Dim SetButton As Button

        Dim bd As Border
        Dim grd As Grid
        Dim presetSize1b1 As New Rect
        Dim presetSize1b2 As New Rect
        Dim presetSize1b3 As New Rect
        Dim presetSize2b1 As New Rect
        Dim presetSize2b2 As New Rect
        Dim presetSize2b3 As New Rect
        Dim grabArea As Border
        Private Sub InitGrid()
            grd = New Grid
            bd = New Border
            grd.Margin = New Thickness(0)
            grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
            For i = 1 To 4
                grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
            Next
            For i = 1 To 26
                grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
            Next

            grd.Background = Chart.Background
            grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
            grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative) 'AutoTrendPadButtonStyle
            bd.Child = grd
            bd.BorderBrush = Brushes.Transparent ' New SolidColorBrush(Color.FromArgb(40, 255, 255, 255))
            bd.BorderThickness = New Thickness(0)
            bd.Background = Chart.Background
            bd.CornerRadius = New CornerRadius(0)
            bd.HorizontalAlignment = Windows.HorizontalAlignment.Center

            Dim c As New ContextMenu
            AddHandler c.Opened, Sub() c.IsOpen = False
            bd.ContextMenu = c
            grabArea = New Border With {.BorderThickness = New Thickness(1), .BorderBrush = Brushes.Gray, .Background = New SolidColorBrush(Color.FromArgb(255, 252, 184, 41)), .Margin = New Thickness(0.5)}
            grabArea.SetValue(Grid.RowProperty, 10)
            grabArea.Height = 17
            grd.Children.Add(grabArea)

        End Sub
        Sub SetRows()
            Grid.SetRow(SnapHButton, 0)
            Grid.SetRow(SnapLButton, 1)
            Grid.SetRow(SetButton, 2)

            grd.Children.Add(SnapHButton)
            grd.Children.Add(SnapLButton)
            grd.Children.Add(SetButton)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 12

            SnapHButton = New Button With {.Background = Brushes.LightPink, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .FontWeight = FontWeights.Bold, .MinWidth = 34, .MinHeight = 34, .HorizontalContentAlignment = HorizontalAlignment.Center, .Content = "Snap" & vbNewLine & "   H"}
            SnapLButton = New Button With {.Background = Brushes.LightGreen, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .FontWeight = FontWeights.Bold, .MinWidth = 34, .MinHeight = 34, .HorizontalContentAlignment = HorizontalAlignment.Center, .Content = "Snap" & vbNewLine & "   L"}
            SetButton = New Button With {.Background = Brushes.LightBlue, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .FontWeight = FontWeights.Bold, .MinWidth = 34, .MinHeight = 34, .HorizontalContentAlignment = HorizontalAlignment.Center, .Content = "Set"}

        End Sub
        Sub AddHandlers()

            AddHandler SnapHButton.Click,
                Sub()
                    SnapToTime(Direction.Up)
                End Sub
            AddHandler SnapLButton.Click,
                Sub()
                    SnapToTime(Direction.Down)
                End Sub

            AddHandler SetButton.Click,
                Sub()
                    Box1SnapSize = New Size(Round(box1.Width), Round(box1.Height, 2) * 1000)
                    Box2SnapSize = New Size(Round(box2.Width), Round(box2.Height, 2) * 1000)
                    Box3SnapSize = New Size(Round(box3.Width), Round(box3.Height, 2) * 1000)
                End Sub
        End Sub
        Private Sub ReapplyMe()
            Chart.ReApplyAnalysisTechnique(Me)
            For Each item In Chart.AnalysisTechniques
                If TypeOf item.AnalysisTechnique Is DayBox Then
                    Chart.ReApplyAnalysisTechnique(item.AnalysisTechnique)
                End If
            Next
        End Sub
        Public Sub InitChartPad() Implements IChartPadAnalysisTechnique.InitChartPad
            ChartPad = New UIChartControl
            ChartPad.Left = ChartPadLocation.X
            ChartPad.Top = ChartPadLocation.Y
            ChartPad.IsDraggable = True
            InitGrid()
            InitControls()
            SetRows()
            AddHandlers()
            ChartPad.Content = bd
        End Sub

        Public Sub UpdateChartPad() Implements IChartPadAnalysisTechnique.UpdateChartPad

        End Sub
        Public Sub UpdateChartPadColor(color As Color) Implements IChartPadAnalysisTechnique.UpdateChartPadColor
            If ChartPad.Content IsNot Nothing Then
                If color.A = 255 Then
                    ChartPad.Content.Child.Background = New SolidColorBrush(color)
                Else
                    ChartPad.Content.Child.Background = New SolidColorBrush(Color.FromArgb(255, color.R, color.G, color.B))
                End If
            End If
        End Sub
#End Region
    End Class
End Namespace

