Imports System.Collections.ObjectModel

Namespace AnalysisTechniques
    Public Class ChartLine
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
            If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonDown, AddressOf MouseDown
            If chart IsNot Nothing Then AddHandler chart.MouseMove, AddressOf MouseMove
            Description = "A technique that draws a adjustable line."
        End Sub
#End Region
        Sub MouseMove(sender As Object, e As MouseEventArgs)
            If e.LeftButton = MouseButtonState.Pressed And Keyboard.IsKeyDown(SetLocationHotKey) And IsEnabled Then
                HandleMouseEvent(e.GetPosition(Chart))
            End If
        End Sub
        Private Sub HandleMouseEvent(location As Point)
            lastHeight = 0
            If isBoxActive = False Then
                currentLine = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
                currentLine.AutoRefresh = True
            End If
            Dim index As Integer = Round(Chart.GetRelativeFromReal(location).X)
            DrawLine(index)
            If index > 0 And index < Chart.bars.Count - 1 Then
                Dim time As Date = Chart.bars(Chart.GetRelativeFromReal(location).X).Data.Date
                For Each c In Chart.Parent.Charts
                    If c IsNot Chart Then
                        For Each item In c.AnalysisTechniques
                            If TypeOf item.AnalysisTechnique Is ChartLine And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
                                CType(item.AnalysisTechnique, ChartLine).Stationary = Stationary
                                If Stationary Then
                                    CType(item.AnalysisTechnique, ChartLine).DrawLine(c.bars.Count - BarsFromLast)
                                Else
                                    If c.bars(0).Data.Date < time Then
                                        For i = 0 To c.bars.Count - 1
                                            If c.bars(i).Data.Date >= time Then
                                                CType(item.AnalysisTechnique, ChartLine).DrawLine(i) ', If(isBoxActive, Chart.GetRealFromRelativeHeight(Abs(currentBox.TopLine.StartPoint.Y - currentBox.BottomLine.StartPoint.Y)), 0))
                                                Exit For
                                            End If
                                        Next
                                    Else
                                        CType(item.AnalysisTechnique, ChartLine).DrawLine(1) ', If(isBoxActive, Chart.GetRealFromRelativeHeight(Abs(currentBox.TopLine.StartPoint.Y - currentBox.BottomLine.StartPoint.Y)), 0))
                                    End If
                                End If
                                Exit For
                            End If
                        Next
                    End If
                Next
            End If

        End Sub
        Sub MouseDown(sender As Object, e As MouseButtonEventArgs)
            If Keyboard.IsKeyDown(SetLocationHotKey) And IsEnabled Then
                HandleMouseEvent(e.GetPosition(Chart))
            End If
        End Sub
        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
                Dim key As Key
                If e.SystemKey = Key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
                If key = RemoveLineHotKey Then
                    For Each c In Chart.Parent.Charts
                        For Each item In c.AnalysisTechniques
                            If TypeOf item.AnalysisTechnique Is ChartLine And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
                                CType(item.AnalysisTechnique, ChartLine).RemoveBox()
                            End If
                        Next
                    Next
                End If
            End If
        End Sub
        <Input("The key to set the location.")> Public Property SetLocationHotKey As Key = Key.LeftShift
        <Input("The key to remove the line.")> Public Property RemoveLineHotKey As Key = Key.Z
        <Input("The color for the line.")> Public Property Color As Color = Colors.Gray
        <Input("The thickness for the line.")> Public Property Thickness As Decimal = 1
        <Input("The line height as a percentage of the chart height.")> Public Property LineHeightPercentage As Decimal = 0.5
        Public Property Stationary As Boolean = False
        <Input> Public Property BarsFromLast As Integer = 50
        <Input> Public Property InfoTextVisible As Boolean = True
        <Input> Public Property InfoTextFontSize As Decimal = 12
        Public Property DashStyle As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString("0") '("11 6 3 6 3 6")
        Public Overrides Property Name As String = "ChartLines"
        Dim startBar As Integer = 0
        Dim lastHeight As Decimal = 0
        Dim infotext As Label
        Protected Overrides Sub Begin()
            MyBase.Begin()
            currentLine = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
            currentLine.AutoRefresh = True
            infotext = NewLabel("", Color, New Point(0, 0), InfoTextVisible) : infotext.Font.FontSize = InfoTextFontSize : infotext.HorizontalAlignment = LabelHorizontalAlignment.Right : infotext.VerticalAlignment = LabelVerticalAlignment.Top
            If startBar <> 0 Then
                DrawLine(startBar)
            End If
        End Sub
        Dim currentLine As TrendLine
        Private isBoxActive As Boolean
        Public Sub DrawLine(startBar As Integer, Optional realHeight As Decimal = 0)
            Me.startBar = startBar
            'If Stationary Then
            BarsFromLast = Chart.bars.Count - startBar
                'End If
                If startBar < Chart.bars.Count And startBar > 0 Then
                isBoxActive = True
                DrawLines(realHeight)
            End If
        End Sub
        Public Sub RemoveBox()
            isBoxActive = False
            startBar = 0
            RemoveObjectFromChart(currentLine)
            currentLine = Nothing
        End Sub
        Private Sub DrawLines(Optional height As Decimal = 0)
            If currentLine Is Nothing Then
                currentLine = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
                currentLine.AutoRefresh = True
            End If
            If height = 0 Then height = lastHeight Else lastHeight = height
            Dim lowPoint As Decimal = Decimal.MaxValue
            Dim highPoint As Decimal = Decimal.MinValue
            Dim lowBar As Integer
            Dim highBar As Integer
            For i = startBar - 1 To Chart.bars.Count - 1
                If Chart.bars(i).Data.Low <= lowPoint Then
                    lowPoint = Chart.bars(i).Data.Low
                    lowBar = i
                End If
                If Chart.bars(i).Data.High >= highPoint Then
                    highPoint = Chart.bars(i).Data.High
                    highBar = i
                End If
            Next

            If lowPoint <> Decimal.MaxValue And highPoint <> Decimal.MinValue And currentLine IsNot Nothing Then
                currentLine.IsSelectable = False
                currentLine.IsEditable = False
                Dim chartHeight As Decimal = Chart.Height
                currentLine.StartPoint = New Point(startBar, -Chart.GetRelativeFromRealY(chartHeight / 2 - chartHeight * LineHeightPercentage * 0.5))
                currentLine.EndPoint = New Point(startBar, -Chart.GetRelativeFromRealY(chartHeight / 2 + chartHeight * LineHeightPercentage * 0.5))
                currentLine.Pen.Thickness = Thickness
                currentLine.RefreshVisual()
                If Stationary Then
                    infotext.Location = New Point(startBar, -Chart.GetRelativeFromRealY(chartHeight / 2 - chartHeight * LineHeightPercentage * 0.5))
                    infotext.Text = BarsFromLast & " "
                Else
                    infotext.Text = ""
                End If

            End If
        End Sub
        Protected Overrides Sub Main()
            If isBoxActive And IsLastBarOnChart Then
                If Stationary Then startBar = Chart.bars.Count - BarsFromLast
                DrawLines()
            End If
        End Sub
    End Class
End Namespace


' BEGIN OLD METHOD THAT IS BASED OFF TIME
'Imports System.Collections.ObjectModel

'Namespace AnalysisTechniques
'	Public Class ChartLine
'#Region "AnalysisTechnique Inherited Code"
'		Inherits AnalysisTechnique

'		' Inherit the one-argument constructor from the base class.
'		Public Sub New(ByVal chart As Chart)
'			MyBase.New(chart) ' Call the base class constructor.
'			If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
'			If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonDown, AddressOf MouseDown
'			If chart IsNot Nothing Then AddHandler chart.MouseMove, AddressOf MouseMove
'			Description = "A technique that draws a adjustable line."
'		End Sub
'#End Region
'		Sub MouseMove(sender As Object, e As MouseEventArgs)
'            If e.LeftButton = MouseButtonState.Pressed And Keyboard.IsKeyDown(SetLocationHotKey) And IsEnabled Then
'                HandleMouseEvent(e.GetPosition(Chart))
'            ElseIf e.LeftButton = MouseButtonState.Pressed And Keyboard.IsKeyDown(SetPriceLineHotKey) And IsEnabled Then
'                HandlePriceMoveMouse(e.GetPosition(Chart))
'            End If
'        End Sub
'        Private Sub HandleMouseEvent(location As Point)
'            lastHeight = 0
'            If isBoxActive = False Then
'                currentBox = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
'            End If
'            Dim index As Integer = Round(Chart.GetRelativeFromReal(location).X)
'            DrawBox(index)
'            If index > 0 And index < Chart.bars.Count - 1 Then
'                Dim time As Date = Chart.bars(Chart.GetRelativeFromReal(location).X).Data.Date
'                For Each c In Chart.Parent.Charts
'                    If c IsNot Chart Then
'                        For Each item In c.AnalysisTechniques
'                            If TypeOf item.AnalysisTechnique Is ChartLine And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
'                                If c.bars(0).Data.Date < time Then
'                                    For i = 0 To c.bars.Count - 1
'                                        If c.bars(i).Data.Date >= time Then
'                                            CType(item.AnalysisTechnique, ChartLine).DrawBox(i) ', If(isBoxActive, Chart.GetRealFromRelativeHeight(Abs(currentBox.TopLine.StartPoint.Y - currentBox.BottomLine.StartPoint.Y)), 0))
'                                            Exit For
'                                        End If
'                                    Next
'                                Else
'                                    CType(item.AnalysisTechnique, ChartLine).DrawBox(1) ', If(isBoxActive, Chart.GetRealFromRelativeHeight(Abs(currentBox.TopLine.StartPoint.Y - currentBox.BottomLine.StartPoint.Y)), 0))
'                                End If
'                                Exit For
'                            End If
'                        Next
'                    End If
'                Next
'            End If

'        End Sub
'        Private Sub HandlePriceMoveMouse(location As Point)
'            If currentPriceLine IsNot Nothing Then
'                Dim price As Decimal = Chart.GetRelativeFromReal(location).Y
'                For Each c In My.Application.Charts
'                    For Each item In c.AnalysisTechniques
'                        If TypeOf item.AnalysisTechnique Is ChartLine And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
'                            CType(item.AnalysisTechnique, ChartLine).DrawPriceLine(price)
'                            Exit For
'                        End If
'                    Next
'                Next
'            End If
'        End Sub
'        Sub MouseDown(sender As Object, e As MouseButtonEventArgs)
'            If Keyboard.IsKeyDown(SetLocationHotKey) And IsEnabled Then
'                HandleMouseEvent(e.GetPosition(Chart))
'                'ElseIf Keyboard.IsKeyDown(DrawBoxHotKey) Then
'            ElseIf Keyboard.IsKeyDown(SetPriceLineHotKey) And IsEnabled Then
'                HandlePriceMoveMouse(e.GetPosition(Chart))
'            End If
'		End Sub
'        Public Sub DrawPriceLine(price As Decimal)
'            currentPriceLine.Coordinates = New LineCoordinates(Chart.bars.Count, -price, Chart.bars.Count + 40, -price)
'        End Sub
'        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
'			If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
'				Dim key As Key
'				If e.SystemKey = key.None Then
'					key = e.Key
'				Else
'					key = e.SystemKey
'				End If
'				If key = RemoveLineHotKey Then
'                    For Each c In Chart.Parent.Charts
'                        For Each item In c.AnalysisTechniques
'                            If TypeOf item.AnalysisTechnique Is ChartLine And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
'                                CType(item.AnalysisTechnique, ChartLine).RemoveBox()
'                            End If
'                        Next
'                    Next
'                    For Each c In My.Application.Charts
'                        For Each item In c.AnalysisTechniques
'                            If TypeOf item.AnalysisTechnique Is ChartLine And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
'                                CType(item.AnalysisTechnique, ChartLine).RemoveObjectFromChart(CType(item.AnalysisTechnique, ChartLine).currentPriceLine)
'                            End If
'                        Next
'                    Next
'                End If
'			End If
'		End Sub
'        <Input("The key to set the location.")> Public Property SetLocationHotKey As Key = Key.LeftShift
'        <Input("The key to set the location.")> Public Property SetPriceLineHotKey As Key = Key.LeftCtrl
'        <Input("The key to remove the line.")> Public Property RemoveLineHotKey As Key = Key.Z
'		<Input("The color for the line.")> Public Property Color As Color = Colors.Gray
'        <Input("The thickness for the line.")> Public Property Thickness As Decimal = 1
'        <Input("The line height as a percentage of the chart height.")> Public Property LineHeightPercentage As Decimal = 0.5
'        <Input("EXPERIMENTAL. Use with Automation technique.")> Public Property ConnectToDivergence As Boolean
'        <Input("EXPERIMENTAL. Use with Automation technique and moving average.")> Public Property LinkToMovingAverage As Boolean
'        Public Property DashStyle As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString("0") '("11 6 3 6 3 6")
'		Public Overrides Property Name As String = "ChartLines"
'		Dim startBar As Integer = 0
'		Dim lastHeight As Decimal = 0
'		Protected Overrides Sub Begin()
'			MyBase.Begin()
'            currentBox = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
'            currentPriceLine = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
'            If startBar <> 0 Then
'				DrawBox(startBar)
'			End If
'		End Sub
'        Dim currentBox As TrendLine
'        Dim currentPriceLine As TrendLine
'        Private isBoxActive As Boolean
'		Public Sub DrawBox(startBar As Integer, Optional realHeight As Decimal = 0)
'			Me.startBar = startBar
'			If startBar < Chart.bars.Count And startBar > 0 Then
'				isBoxActive = True
'				DrawLines(realHeight)
'			End If
'		End Sub
'		Public Sub RemoveBox()
'			isBoxActive = False
'			startBar = 0
'			RemoveObjectFromChart(currentBox)
'			currentBox = Nothing
'		End Sub
'		Private Sub DrawLines(Optional height As Decimal = 0)
'			If currentBox Is Nothing Then
'				currentBox = NewTrendLine(Color, New Point(0, 0), New Point(0, 0), True)
'			End If
'			If height = 0 Then height = lastHeight Else lastHeight = height
'			Dim lowPoint As Decimal = Decimal.MaxValue
'			Dim highPoint As Decimal = Decimal.MinValue
'			Dim lowBar As Integer
'			Dim highBar As Integer
'			For i = startBar - 1 To Chart.bars.Count - 1
'				If Chart.bars(i).Data.Low <= lowPoint Then
'					lowPoint = Chart.bars(i).Data.Low
'					lowBar = i
'				End If
'				If Chart.bars(i).Data.High >= highPoint Then
'					highPoint = Chart.bars(i).Data.High
'					highBar = i
'				End If
'			Next

'			If lowPoint <> Decimal.MaxValue And highPoint <> Decimal.MinValue And currentBox IsNot Nothing Then
'				currentBox.IsSelectable = False
'                currentBox.IsEditable = False
'                Dim chartHeight As Decimal = Chart.Height
'                currentBox.StartPoint = New Point(startBar, -Chart.GetRelativeFromRealY(chartHeight / 2 - chartHeight * LineHeightPercentage * 0.5))
'                currentBox.EndPoint = New Point(startBar, -Chart.GetRelativeFromRealY(chartHeight / 2 + chartHeight * LineHeightPercentage * 0.5))
'                If ConnectToDivergence Then
'                    For Each tech In Chart.AnalysisTechniques
'                        If TypeOf tech.AnalysisTechnique Is Automation Then
'                            If LinkToMovingAverage Then
'                                Dim avg = Chart.bars(startBar).Data.Avg
'                                For Each tech2 In Chart.AnalysisTechniques
'                                    If TypeOf tech2.AnalysisTechnique Is MovingAverage Then
'                                        avg = CType(tech2.AnalysisTechnique, MovingAverage).GetAvgAtBar(startBar)
'                                        Exit For
'                                    End If
'                                Next
'                                CType(tech.AnalysisTechnique, Automation).AlignToBar(startBar, avg)
'                            Else
'                                CType(tech.AnalysisTechnique, Automation).AlignToBar(startBar)
'                            End If
'                        End If
'                    Next
'                End If
'                'currentBox.ExtendLeft = True
'                'currentBox.ExtendRight = True
'                currentBox.Pen.Thickness = Thickness
'			End If
'		End Sub
'		Protected Overrides Sub Main()
'            If isBoxActive And IsLastBarOnChart Then
'                DrawLines()
'            End If
'		End Sub
'	End Class
'End Namespace
