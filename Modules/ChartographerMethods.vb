Imports System.Math
Imports System.IO
Imports System.IO.File
Imports System.Windows.Controls.Primitives
Imports System.Collections.Specialized

Public Module QuickTraderMethods

    <DebuggerStepThrough()>
    Public Sub ShowInfoBox(ByVal text As String, ByVal owner As Window)
        'My.Application.SplashWindow.Close()
        Windows.Application.Current.Dispatcher.Invoke(
            Sub()
                Dim box As New InfoBox(text, owner)
            End Sub, Windows.Threading.DispatcherPriority.Send, Nothing)
    End Sub
    <DebuggerStepThrough()>
    Public Sub ShowInfoBox(ByVal text As String, ByVal button1Text As String, ByVal owner As Window)
        'My.Application.SplashWindow.Close()
        Application.Current.Dispatcher.Invoke(
            Sub()
                Dim box As New InfoBox(text, owner, False, button1Text)
            End Sub, Windows.Threading.DispatcherPriority.Send, Nothing)
    End Sub
    <DebuggerStepThrough()>
    Public Function ShowInfoBoxAsync(ByVal text As String) As InfoBox
        'My.Application.SplashWindow.Close()
        Dim box As InfoBox = Nothing
        Application.Current.Dispatcher.Invoke(
            Sub()
                box = New InfoBox(text, Nothing, False, Nothing)
            End Sub, Windows.Threading.DispatcherPriority.Send, Nothing)
        Return box
    End Function
    <DebuggerStepThrough()>
    Public Sub ShowTransientInfoBox(ByVal text As String, duration As Integer)
        'My.Application.SplashWindow.Close()
        Dim a = (Async Sub()
                     Dim box As InfoBox = Nothing
                     box = New InfoBox(text, Nothing, False, Nothing)
                     Await System.Threading.Tasks.Task.Delay(duration)
                     box.Close()
                 End Sub)
        a.Invoke()
    End Sub
    <DebuggerStepThrough()>
    Public Function ShowInfoBox(ByVal text As String, ByVal owner As Window, ByVal ParamArray buttonTexts() As String) As Integer
        'My.Application.SplashWindow.Close()
        Dim result As Integer
        Application.Current.Dispatcher.Invoke(
            Sub()
                Dim box As New InfoBox(text, owner, False, buttonTexts)
                result = box.ButtonResultIndex
            End Sub, Windows.Threading.DispatcherPriority.Send, Nothing)
        Return result
    End Function
    Public Delegate Sub QuantityChosenEventHandler(ByVal newQuantity As Integer)
    Public Sub ShowQuantityMenu(ByVal currentQuantity As Integer, ByVal defaultQuantity As Integer, ByVal quantityChosenMethod As QuantityChosenEventHandler)
        Dim desktops As List(Of DesktopWindow) = My.Application.Desktops.ToList
        If desktops.Count > 0 Then
            Dim ctxMenu As New ContextMenu
            ctxMenu.Resources = desktops(0).Resources
            Dim quantities As List(Of Integer) = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.ToList
            If quantities.Contains(defaultQuantity) Then quantities.Remove(defaultQuantity)
            quantities.Insert(0, defaultQuantity)
            For Each i In quantities
                Dim mnu As New MenuItem With {.Header = i}
                If currentQuantity = i Then mnu.IsChecked = True
                AddHandler mnu.Click,
                    Sub(sender As Object, e As EventArgs)
                        quantityChosenMethod.Invoke(sender.Header)
                    End Sub
                ctxMenu.Items.Add(mnu)
            Next
            Dim mnuTextBox As New MenuItem
            'mnuTextBox.IsCheckable = True
            Dim txt As New TextBox With {.Margin = New Thickness(0, 0, -8, 0), .Width = 40}
            If Not quantities.Contains(currentQuantity) Then
                txt.Text = currentQuantity
                mnuTextBox.IsChecked = True
            End If
            mnuTextBox.Header = txt
            ctxMenu.Items.Add(mnuTextBox)
            AddHandler mnuTextBox.Click,
                Sub(sender As Object, e As EventArgs)
                    If IsNumeric(sender.Header.Text) Then
                        quantityChosenMethod.Invoke(sender.Header.Text)
                    Else
                        ShowInfoBox("Please enter a numeric quantity.", Nothing)
                    End If
                End Sub
            ctxMenu.Placement = Controls.Primitives.PlacementMode.Mouse
            'ctxMenu.Width = 60
            ctxMenu.IsOpen = True
        End If
    End Sub


    'Public Sub WriteTickToFile(ByVal file As String, ByVal [date] As Date, ByVal tickPrice As Decimal)
    '    Exit Sub
    '    'Dim binValue As List(Of Boolean) = BinaryCondenseNumber(CULng([date].ToFileTime))
    '    'AddArrayToList(binValue, {True, False, True, True}) ' 1011: colon (:) binary equivalent
    '    'binValue.AddRange(BinaryCondenseNumber(tickPrice))
    '    'If binValue.Count Mod 8 = 0 Then AddArrayToList(binValue, {True, True, True, True})
    '    'AddArrayToList(binValue, {True, True, False, False}) ' 1100: newline binary equivalent
    '    'Dim bytes(binValue.Count / 8 - 1) As Byte
    '    'For i = 0 To binValue.Count / 8 - 1
    '    '    bytes(i) = BinaryToDecimal(CStr(Abs(CInt(binValue(i * 8)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 1)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 2)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 3)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 4)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 5)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 6)))) &
    '    '                               CStr(Abs(CInt(binValue(i * 8 + 7)))))
    '    'Next
    '    ''My.Computer.FileSystem.ReadAllText(file)
    '    'My.Computer.FileSystem.WriteAllBytes(file, bytes, True)
    'End Sub
    'Public Function ReadTicksFromFile(ByVal file As String) As Dictionary(Of Date, Double)
    '    If Not IO.File.Exists(file) Then Return New Dictionary(Of Date, Double)
    '    Dim bytes() As Byte = My.Computer.FileSystem.ReadAllBytes(file)
    '    Dim nums(bytes.GetUpperBound(0) * 2 + 1, 3) As Boolean
    '    Dim index As Integer
    '    For Each [byte] In bytes
    '        Dim bin As String = DecimalToBinary([byte])
    '        Dim zeros As String = ""
    '        For i = 1 To 8 - bin.Length
    '            zeros &= "0"
    '        Next
    '        bin = zeros & bin
    '        For i = 0 To 1
    '            For j = 0 To 3
    '                If bin(j + i * 4) = "0" Then nums(index + i, j) = False Else nums(index + i, j) = True
    '            Next
    '        Next
    '        index += 2
    '    Next
    '    Dim chars As New List(Of Char)
    '    For i = 0 To nums.GetUpperBound(0)
    '        Dim str As String = CStr(Abs(CInt(nums(i, 0)))) & CStr(Abs(CInt(nums(i, 1)))) & CStr(Abs(CInt(nums(i, 2)))) & CStr(Abs(CInt(nums(i, 3))))
    '        Select Case str
    '            Case "0000" : chars.Add("0")
    '            Case "0001" : chars.Add("1")
    '            Case "0010" : chars.Add("2")
    '            Case "0011" : chars.Add("3")
    '            Case "0100" : chars.Add("4")
    '            Case "0101" : chars.Add("5")
    '            Case "0110" : chars.Add("6")
    '            Case "0111" : chars.Add("7")
    '            Case "1000" : chars.Add("8")
    '            Case "1001" : chars.Add("9")
    '            Case "1010" : chars.Add(".")
    '            Case "1011" : chars.Add(":")
    '            Case "1100" : chars.Add("n")
    '            Case "1111" ' fill character
    '        End Select
    '    Next
    '    Dim dates As New List(Of Date)
    '    Dim values As New List(Of Double)
    '    Dim currentValue As String = ""
    '    For Each [char] In chars
    '        If [char] = ":" Then
    '            dates.Add(Date.FromFileTime(CLng(currentValue)))
    '            currentValue = ""
    '            Continue For
    '        ElseIf [char] = "n" Then
    '            values.Add(CDbl(currentValue))
    '            currentValue = ""
    '            Continue For
    '        End If
    '        currentValue &= [char]
    '    Next
    '    Dim dictionary As New Dictionary(Of Date, Double)
    '    For i = 0 To dates.Count - 1
    '        dictionary.Add(dates(i), values(i))
    '    Next
    '    Return dictionary
    'End Function
    'Public Function BinaryCondenseNumber(ByVal num As Decimal) As List(Of Boolean)
    '    Dim binValue As New List(Of Boolean)
    '    For Each item In CStr(num)
    '        Select Case item
    '            Case "0" : AddArrayToList(binValue, {False, False, False, False}) '0000
    '            Case "1" : AddArrayToList(binValue, {False, False, False, True}) ' 0001
    '            Case "2" : AddArrayToList(binValue, {False, False, True, False}) ' 0010
    '            Case "3" : AddArrayToList(binValue, {False, False, True, True})  ' 0011
    '            Case "4" : AddArrayToList(binValue, {False, True, False, False}) ' 0100
    '            Case "5" : AddArrayToList(binValue, {False, True, False, True})  ' 0101
    '            Case "6" : AddArrayToList(binValue, {False, True, True, False})  ' 0110
    '            Case "7" : AddArrayToList(binValue, {False, True, True, True})   ' 0111
    '            Case "8" : AddArrayToList(binValue, {True, False, False, False}) ' 1000
    '            Case "9" : AddArrayToList(binValue, {True, False, False, True})  ' 1001
    '            Case "." : AddArrayToList(binValue, {True, False, True, False})  ' 1010
    '        End Select
    '    Next
    '    Return binValue
    'End Function

    Public Function AddToX(ByVal point As Point, ByVal value As Double) As Point
        point.X += value
        Return point
    End Function
    Public Function AddToX(ByVal lineCoordinates As LineCoordinates, ByVal value As Double) As LineCoordinates
        Return New LineCoordinates(lineCoordinates.StartPoint.X + value, lineCoordinates.StartPoint.Y, lineCoordinates.EndPoint.X + value, lineCoordinates.EndPoint.Y)
    End Function
    Public Function AddToY(ByVal point As Point, ByVal value As Double) As Point
        point.Y += value
        Return point
    End Function
    Public Function AddToXY(ByVal point As Point, ByVal xValue As Double, yValue As Double) As Point
        point.Y += yValue
        point.X += xValue
        Return point
    End Function
    Public Function NegateY(ByVal point As Point) As Point
        point.Y = -point.Y
        Return point
    End Function
    Public Function NegateY(ByVal lineCoordinates As LineCoordinates) As LineCoordinates
        lineCoordinates.StartPoint = NegateY(lineCoordinates.StartPoint)
        lineCoordinates.EndPoint = NegateY(lineCoordinates.EndPoint)
        Return lineCoordinates
    End Function
    Public Function NegateY(ByVal rect As Rect) As Rect
        Return New Rect(rect.X, -rect.Y, rect.Width, rect.Height)
    End Function
    Public Function ContractsEqual(contract1 As IBApi.Contract, contract2 As IBApi.Contract) As Boolean
        If contract1.Currency <> contract2.Currency Or
            contract1.Exchange <> contract2.Exchange Or
            contract1.LastTradeDateOrContractMonth <> contract2.LastTradeDateOrContractMonth Or
            contract1.ConId <> contract2.ConId Or
            contract1.IncludeExpired <> contract2.IncludeExpired Or
            contract1.Multiplier <> contract2.Multiplier Or
            contract1.PrimaryExch <> contract2.PrimaryExch Or
            contract1.Strike <> contract2.Strike Or
            contract1.Symbol <> contract2.Symbol Or
            contract1.LocalSymbol <> contract2.LocalSymbol Or
            contract1.SecType <> contract2.SecType Or
            contract1.Multiplier <> contract2.Multiplier Then
            Return False
        Else
            Return True
        End If
    End Function
    ''' <summary>
    ''' sets contract1 to equal contract2
    ''' </summary>
    Public Sub SetContractToContract(contract1 As IBApi.Contract, contract2 As IBApi.Contract)
        contract1.Currency = contract2.Currency
        contract1.Exchange = contract2.Exchange
        contract1.LastTradeDateOrContractMonth = contract2.LastTradeDateOrContractMonth
        contract1.ConId = contract2.ConId
        contract1.IncludeExpired = contract2.IncludeExpired
        contract1.Multiplier = contract2.Multiplier
        contract1.PrimaryExch = contract2.PrimaryExch
        contract1.Strike = contract2.Strike
        contract1.Symbol = contract2.Symbol
        contract1.LocalSymbol = contract2.LocalSymbol
        contract1.SecType = contract2.SecType
        contract1.Multiplier = contract2.Multiplier
    End Sub

    Public Function ParseBarSize(barSize As BarSize) As String
        Select Case barSize
            Case BarSize.OneSecond
                Return "1 sec"
            Case BarSize.FiveSeconds
                Return "5 secs"
            Case BarSize.FifteenSeconds
                Return "15 secs"
            Case BarSize.ThirtySeconds
                Return "30 secs"
            Case BarSize.OneMinute
                Return "1 min"
            Case BarSize.FiveMinutes
                Return "5 mins"
            Case BarSize.FifteenMinutes
                Return "15 mins"
            Case BarSize.ThirtyMinutes
                Return "30 mins"
            Case BarSize.OneHour
                Return "1 hour"
            Case BarSize.OneDay
                Return "1 day"
            Case BarSize.OneWeek
                Return "1 week"
            Case BarSize.OneMonth
                Return "1 month"
            Case BarSize.OneYear
                Return "1 year"
        End Select
    End Function
End Module
