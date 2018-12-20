Imports System.Collections.ObjectModel

Namespace AnalysisTechniques
    Public Class ChartArrow
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
            If chart IsNot Nothing Then AddHandler chart.ChartKeyUp, AddressOf KeyUp
            If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonDown, AddressOf MouseDown
            If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonUp, AddressOf MouseUp
            If chart IsNot Nothing Then AddHandler chart.MouseMove, AddressOf MouseMove
            Description = "A technique that draws a adjustable arrow."
        End Sub
#End Region
        Sub MouseMove(sender As Object, e As MouseEventArgs)
            If e.LeftButton = MouseButtonState.Pressed And Keyboard.IsKeyDown(CreateArrowHotKey) And IsEnabled Then
                HandleMouseEvent(e.GetPosition(Chart), False)
            ElseIf e.LeftButton = MouseButtonState.Pressed And Keyboard.IsKeyDown(MoveArrowHotKey) And IsEnabled Then
                HandleMouseEvent(e.GetPosition(Chart), True)
            End If
        End Sub
        Private Sub HandleMouseEvent(location As Point, isMove As Boolean)
            lastHeight = 0
            Dim index As Integer = Round(Chart.GetRelativeFromReal(location).X)
            Dim drawToBottom As Boolean = If(index <= Chart.bars.Count, -Chart.GetRelativeFromReal(location).Y < Chart.bars(index - 1).Data.Avg, False)
            Dim arrowIndex As Integer
            If isMove Then
                arrowIndex = FindClosestArrow(location)
                MoveArrow(index, drawToBottom, arrowIndex)
            Else
                If arrowCreated = False Then
                    DrawArrow(index, drawToBottom)
                    arrowCreated = True
                Else
                    arrowIndex = currentArrows.Count - 1
                    isMove = True
                End If
            End If
            If index > 0 And index < Chart.bars.Count - 1 Then
                Dim time As Date = Chart.bars(Chart.GetRelativeFromReal(location).X).Data.Date
                For Each c In Chart.Parent.Charts
                    If c IsNot Chart Then
                        For Each item In c.AnalysisTechniques
                            If TypeOf item.AnalysisTechnique Is ChartArrow And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
                                If c.bars(0).Data.Date < time Then
                                    For i = 0 To c.bars.Count - 1
                                        If c.bars(i).Data.Date >= time Then
                                            If isMove Then
                                                CType(item.AnalysisTechnique, ChartArrow).MoveArrow(i, drawToBottom, arrowIndex)
                                            Else
                                                CType(item.AnalysisTechnique, ChartArrow).DrawArrow(i, drawToBottom)
                                            End If
                                            Exit For
                                        End If
                                    Next
                                Else
                                    If isMove Then
                                        CType(item.AnalysisTechnique, ChartArrow).MoveArrow(1, drawToBottom, arrowIndex)
                                    Else
                                        CType(item.AnalysisTechnique, ChartArrow).DrawArrow(1, drawToBottom)
                                    End If
                                End If
                                Exit For
                            End If
                        Next
                    End If
                Next
            End If
        End Sub
        Private Sub RemoveArrowMouseEvent(location As Point)
            Dim index As Integer = Round(Chart.GetRelativeFromReal(location).X)
            Dim arrowIndex As Integer
            arrowIndex = FindClosestArrow(location)
            For Each c In Chart.Parent.Charts
                For Each item In c.AnalysisTechniques
                    If TypeOf item.AnalysisTechnique Is ChartArrow And item.AnalysisTechnique.IsEnabled Then
                        CType(item.AnalysisTechnique, ChartArrow).RemoveArrow(arrowIndex)
                    End If
                Next
            Next
        End Sub
        Sub MouseDown(sender As Object, e As MouseButtonEventArgs)
            If Keyboard.IsKeyDown(CreateArrowHotKey) And IsEnabled Then
                HandleMouseEvent(e.GetPosition(Chart), False)
            ElseIf Keyboard.IsKeyDown(MoveArrowHotKey) And IsEnabled Then
                HandleMouseEvent(e.GetPosition(Chart), True)
            ElseIf Keyboard.IsKeyDown(RemoveArrowHotKey) And IsEnabled Then
                RemoveArrowMouseEvent(e.GetPosition(Chart))
            End If
        End Sub
        Sub MouseUp(sender As Object, e As MouseButtonEventArgs)
            If Keyboard.IsKeyDown(CreateArrowHotKey) And IsEnabled Then
                arrowCreated = False
            End If
        End Sub
        Protected Sub KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs)

        End Sub
        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
                Dim key As Key
                If e.SystemKey = Key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
                If key = ClearAllHotKey Then
                    For Each c In Chart.Parent.Charts
                        For Each item In c.AnalysisTechniques
                            If TypeOf item.AnalysisTechnique Is ChartArrow And item.AnalysisTechnique.IsEnabled Then
                                For Each arrow In CType(item.AnalysisTechnique, ChartArrow).currentArrows
                                    c.Children.Remove(arrow)
                                Next
                                CType(item.AnalysisTechnique, ChartArrow).currentArrows.Clear()
                            End If
                        Next
                    Next
                End If
            End If
        End Sub
        <Input> Public Property CreateArrowHotKey As Key = Key.LeftShift
        <Input> Public Property MoveArrowHotKey As Key = Key.Z
        <Input> Public Property RemoveArrowHotKey As Key = Key.X
        <Input> Public Property ClearAllHotKey As Key = Key.C
        <Input> Public Property ArrowLineThickness As Decimal = 1
        <Input> Public Property Color As Color = Colors.Gray
        <Input> Public Property ArrowHeight As Decimal = 0.5
        <Input> Public Property ArrowLength As Decimal = 0.5
        Public Overrides Property Name As String = "ChartArrows"
        Dim startBar As Integer = 0
        Dim lastHeight As Decimal = 0
        Dim arrowCreated As Boolean = False
        Protected Overrides Sub Begin()
            MyBase.Begin()
            If currentArrows Is Nothing Then
                currentArrows = New List(Of Arrow)
            Else
                For Each arrow In currentArrows
                    Chart.Children.Add(arrow)
                Next
            End If
            arrowCreated = False
        End Sub
        Dim currentArrows As List(Of Arrow)
        Private isArrowActive As Boolean
        Public Sub DrawArrow(startBar As Integer, drawToBottomOfBar As Boolean)
            Me.startBar = startBar
            If startBar < Chart.bars.Count And startBar > 0 Then
                isArrowActive = True
                currentArrows.Add(NewArrow(Color, New Point(0, 0), False, True))
                currentArrows(currentArrows.Count - 1).Pen.Thickness = ArrowLineThickness
                currentArrows(currentArrows.Count - 1).Width = ArrowLength
                currentArrows(currentArrows.Count - 1).Height = ArrowHeight
                'If lowPoint <> Decimal.MaxValue And highPoint <> Decimal.MinValue And currentBox IsNot Nothing Then
                currentArrows(currentArrows.Count - 1).IsSelectable = False
                currentArrows(currentArrows.Count - 1).IsEditable = False
                If startBar <= Chart.bars.Count Then currentArrows(currentArrows.Count - 1).Location = New Point(startBar, -If(drawToBottomOfBar, Chart.bars(startBar - 1).Data.Low, Chart.bars(startBar - 1).Data.High))
            End If
        End Sub
        Public Sub MoveArrow(startBar As Integer, drawToBottomOfBar As Boolean, index As Integer)
            If currentArrows.Count > index And index >= 0 Then
                If startBar < Chart.bars.Count And startBar > 0 Then
                    If startBar <= Chart.bars.Count Then currentArrows(index).Location = New Point(startBar, -If(drawToBottomOfBar, Chart.bars(startBar - 1).Data.Low, Chart.bars(startBar - 1).Data.High))
                End If
            End If
        End Sub
        Private Function FindClosestArrow(mouseLocation As Point) As Integer
            Dim ind As Integer = -1
            If currentArrows.Count <> 0 Then
                Dim min As Double = Double.MaxValue
                For i = 0 To currentArrows.Count - 1
                    Dim val = Abs((mouseLocation - Chart.GetRealFromRelative(currentArrows(i).Location)).Length)
                    If val < min Then
                        min = val
                        ind = i
                    End If
                Next
            End If
            Return ind
        End Function
        Public Sub RemoveArrow(index As Integer)
            If currentArrows.Count > index Then
                RemoveObjectFromChart(currentArrows(index))
                currentArrows.RemoveAt(index)
            End If
        End Sub

        Protected Overrides Sub Main()

        End Sub
    End Class
End Namespace
