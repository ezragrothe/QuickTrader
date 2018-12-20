Imports System.Math
Imports System.IO
Imports System.IO.File
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media.Effects
Public Module Methods
	Structure Hotkey
		Public Modifier As ModifierKeys
		Public Key As Key
		Public Sub New(modifier As ModifierKeys, key As Key)
			Me.Modifier = modifier
			Me.Key = key
		End Sub
    End Structure
    Public Enum Direction
        Up = -1
        Down = 0
    End Enum
	Public Function GetModifierString(modifier As ModifierKeys) As String
		Dim modifierString As String = ""
		Select Case modifier
			Case ModifierKeys.Alt
				modifierString = "Alt"
			Case ModifierKeys.Control
				modifierString = "Control"
			Case ModifierKeys.Shift
				modifierString = "Shift"
			Case ModifierKeys.Alt + ModifierKeys.Control
				modifierString = "Control+Alt"
			Case ModifierKeys.Alt + ModifierKeys.Shift
				modifierString = "Shift+Alt"
			Case ModifierKeys.Control + ModifierKeys.Shift
				modifierString = "Control+Shift"
			Case ModifierKeys.Control + ModifierKeys.Shift + ModifierKeys.Alt
				modifierString = "Control+Shift+Alt"
		End Select
		Return modifierString
	End Function
    Private _once As Boolean = True
    Public Sub Once(ByVal handler As [Delegate])
        If _once Then
            handler.DynamicInvoke(Nothing)
            _once = False
        End If
    End Sub
    Public Function ShowColorDialog(ByVal color As Color) As Color
        Dim colDialog As New ColorDialog
        colDialog.Color = color
        colDialog.ShowDialog()
        If colDialog.ButtonOKPressed Then
            Return colDialog.Color
        Else
            Return Nothing
        End If
    End Function
    Public Function GetDashStyle(thickness As Decimal) As DashStyle
        If thickness = 0 Then
            Return New DashStyle(New DoubleCollection({3}), 3)
        Else
            Return New DashStyle(New DoubleCollection({1 / thickness * 3}), 1 / thickness * 3)
        End If

    End Function
    Private waitThread As System.Threading.Thread = Nothing
    Public Sub Log(ByVal text As String)
        'MsgBox(text)
        'Exit Sub
        Application.Current.Dispatcher.BeginInvoke(
            Sub()
                Dim foundWindow As Boolean
                For Each item In Application.Current.Windows
                    If TypeOf item Is LogWindow Then
                        foundWindow = True
                        CType(item, LogWindow).Show()
                        CType(item, LogWindow).WriteLine(text)
                        Exit For
                    End If
                Next
                If Not foundWindow Then
                    Dim logWin As New LogWindow
                    logWin.Show()

                    logWin.WriteLine(text)
                End If
                If My.Application.HideLogWindow Then
                    If waitThread IsNot Nothing Then
                        waitThread.Suspend()
                    End If
                    waitThread = New System.Threading.Thread(
                        Sub()
                            System.Threading.Thread.Sleep(5000)
                            Application.Current.Dispatcher.Invoke(
                                Sub()
                                    For Each item In Application.Current.Windows
                                        If TypeOf item Is LogWindow Then
                                            CType(item, LogWindow).SaveSettings()
                                            CType(item, LogWindow).Hide()
                                            Exit For
                                        End If
                                    Next
                                End Sub)
                            waitThread = Nothing
                        End Sub)
                    waitThread.Start()
                End If
            End Sub)
    End Sub
    Public Function GetSplitBrush(firstColor As Color, secondColor As Color, divisionPercentage As Decimal, isVertical As Boolean) As LinearGradientBrush
        Dim brush As New LinearGradientBrush With {.StartPoint = New Point(0, 0), .EndPoint = New Point(If(isVertical, 0, 1), If(isVertical, 1, 0))}
        brush.GradientStops.Add(New GradientStop(firstColor, divisionPercentage))
        brush.GradientStops.Add(New GradientStop(secondColor, divisionPercentage))
        Return brush
    End Function
    Public Function NewLinearGradientBrushWrapper(ByVal beginColor As Color, ByVal endColor As Color, Optional ByVal beginOffset As Double = 0, Optional ByVal endOffset As Double = 1,
                                                  Optional ByVal startPoint As Point = Nothing, Optional ByVal endPoint As Point = Nothing, Optional ByVal spreadMethod As GradientSpreadMethod = GradientSpreadMethod.Pad) As LinearGradientBrush
        If startPoint.X = endPoint.X And startPoint.Y = endPoint.Y And startPoint.X = 0 And endPoint.Y = 0 Then
            startPoint = New Point(0, 0)
            endPoint = New Point(0, 1)
        End If
        Dim collection As New GradientStopCollection
        collection.Add(New GradientStop(beginColor, beginOffset))
        collection.Add(New GradientStop(endColor, endOffset))
        Dim brush As New LinearGradientBrush(collection)
        brush.SpreadMethod = spreadMethod
        brush.StartPoint = startPoint
        brush.EndPoint = endPoint
        Return brush
    End Function
    Public Function NewRadialGradientBrushWrapper(ByVal beginColor As Color, ByVal endColor As Color, Optional ByVal beginOffset As Double = 0, Optional ByVal endOffset As Double = 1,
                                                 Optional ByVal center As Point = Nothing, Optional ByVal radiusX As Double = -1, Optional ByVal radiusY As Double = -1, Optional ByVal spreadMethod As GradientSpreadMethod = GradientSpreadMethod.Pad) As RadialGradientBrush
        center = New Point(0.5, 0.5)
        Dim collection As New GradientStopCollection
        collection.Add(New GradientStop(beginColor, beginOffset))
        collection.Add(New GradientStop(endColor, endOffset))
        Dim brush As New RadialGradientBrush(collection)
        brush.SpreadMethod = spreadMethod
        brush.Center = center
        If radiusX <> -1 Then brush.RadiusX = radiusX
        If radiusY <> -1 Then brush.RadiusY = radiusY
        Return brush
    End Function
    Public Function NewMenuItemWrapper(ByVal header As Object, ByVal name As String, Optional ByVal command As RoutedUICommand = Nothing, Optional ByVal clickHandler As RoutedEventHandler = Nothing) As MenuItem
        Dim menu As CommandMenuItem
        menu = New CommandMenuItem
        menu.Name = name
        menu.Header = header
        menu.Command = command
        If clickHandler IsNot Nothing Then AddHandler menu.Click, clickHandler
        Return menu
    End Function

    Public Sub DeleteAllChildrenOfType(Of T)(ByVal children As IList(Of T), ByVal childrenInstance As Object)
        Dim i As Integer
        While i < children.Count
            If childrenInstance.GetType = children(i).GetType Then
                children.RemoveAt(i)
            Else
                i += 1
            End If
        End While
    End Sub
    Public Sub DeleteAllChildrenOfType(Of T)(ByVal children As IList(Of T), ByVal childrenType As Type)
        Dim i As Integer
        While i < children.Count
            If childrenType.Name = children(i).GetType.Name Then
                children.RemoveAt(i)
            Else
                i += 1
            End If
        End While
    End Sub

    Public Function RandomChoice(ByVal num1 As Double, ByVal num2 As Double, ByVal percentageChanceOfNum1BeingTaken As Double) As Double
        If Rnd() < percentageChanceOfNum1BeingTaken Then Return num1
        Return num2
    End Function
    ''' <summary>
    ''' Returns the value found at point x on a curve defined by points 'a' and 'b'. These are the points on the graph located at x=0 and x=1, respectively. 'a' must be bigger than 'b' creating a \ slope. 'c' at 25 is pretty linear, 'c' at 1 is pretty curved.
    ''' </summary>
    Public Function RecipCalc(a As Double, b As Double, curvature As Double, x As Double) As Double
        Dim y = (Sqrt((a - b) * (4 * curvature + a - b)) - a + b) / (2 * (a - b))
        Dim z = (-(Sqrt((a - b) * (4 * curvature + a - b)) - a - b)) / 2
        Return 1 / ((x + y) / curvature) + z
    End Function
    Public Function LinCalc(ByVal a As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double, ByVal x As Double) As Double
        If a = c Then
            Return Double.NaN
        Else
            Return (((b - d) * x) / (a - c)) + ((a * d - b * c) / (a - c))
        End If
    End Function
    Public Function LinCalc(line As LineCoordinates, ByVal x As Double) As Double
        Return LinCalc(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y, x)
    End Function

    Public Function LineIntersection(ByVal line1 As LineCoordinates, ByVal line2 As LineCoordinates) As Point
        Dim p1() As Double = {0, line1.StartPoint.X, line1.StartPoint.Y}
        Dim p2() As Double = {0, line1.EndPoint.X, line1.EndPoint.Y}
        Dim p3() As Double = {0, line2.StartPoint.X, line2.StartPoint.Y}
        Dim p4() As Double = {0, line2.EndPoint.X, line2.EndPoint.Y}
        Dim x As Double = p2(1) - (p1(1) - p2(1)) * (p2(2) * (p3(1) - p4(1)) - p2(1) * (p3(2) - p4(2)) + p3(2) * p4(1) - p3(1) *
                          p4(2)) / (p1(2) * (p3(1) - p4(1)) - p1(1) * (p3(2) - p4(2)) - p2(2) * (p3(1) - p4(1)) + p2(1) * (p3(2) - p4(2)))
        Dim y As Double = (p1(2) * (p2(1) * (p3(2) - p4(2)) - p3(2) * p4(1) + p3(1) * p4(2)) - (p1(1) * (p3(2) - p4(2)) - p3(2) * p4(1) + p3(1) *
                          p4(2)) * p2(2)) / (p1(2) * (p3(1) - p4(1)) - p1(1) * (p3(2) - p4(2)) - p2(2) * (p3(1) - p4(1)) + p2(1) * (p3(2) - p4(2)))
        If Not Double.IsNaN(x) And Not Double.IsNaN(y) And x > 0 And y > 0 And New Rect(line1.StartPoint, line1.EndPoint).Contains(New Point(x, y)) Then
            Return New Point(x, y)
        Else
            Return Nothing
        End If
    End Function
    ''' <summary>
    ''' Floors the x value of the point.
    ''' </summary>
    Public Function FloorP(p As Point) As Point
        Return New Point(Floor(p.X), p.Y)
    End Function
    ''' <summary>
    ''' Rounds the x value of the point.
    ''' </summary>
    Public Function RoundP(p As Point) As Point
        Return New Point(Round(p.X), p.Y)
    End Function
    Public Function Round(ByVal num As Double) As Double
        Return Microsoft.JScript.MathObject.round(num)
    End Function
    Public Function Round(ByVal num As Double, ByVal digits As Integer) As Double
        If Not Double.IsNaN(num) And Decimal.TryParse(num, New Decimal) = True Then
            Return Decimal.Round(CDec(num), digits)
        Else
            Return num
        End If
    End Function
    Public Function RoundTo(ByVal num As Double, ByVal roundToNum As Double) As Double
        If roundToNum = 0 Then Return num
        Dim val As Double = Round(num / roundToNum) * roundToNum
        If CStr(CDec(val)).Contains(".") Then
            Return Round(val, CStr(CDec(val)).Split(".")(1).Length)
        Else
            Return val
        End If
    End Function
    Public Function CeilingTo(ByVal num As Double, ByVal roundToNum As Double) As Double
        If roundToNum = 0 Then Return num
        Dim val As Double = Ceiling(num / roundToNum) * roundToNum
        Return val
    End Function
    Public Function FloorTo(ByVal num As Double, ByVal roundToNum As Double) As Double
        If roundToNum = 0 Then Return num
        Dim val As Double = Floor(num / roundToNum) * roundToNum
        Return val
    End Function
    Public Function Avg(num1 As Double, num2 As Double) As Double
        Return (num1 + num2) / 2
    End Function
    Public Function FindClosestNumber(targetVal As Double, [set] As Double()) As Integer
        Dim dif As Double = Double.MaxValue, cand As Integer = 0
        For i = 0 To [set].ToList.Count - 1
            If Math.Abs([set](i) - targetVal) < dif Then
                dif = Math.Abs([set](i) - targetVal)
                cand = i
            End If
        Next
        Return cand
    End Function

    Public Function Distance(ByVal point1 As Point, ByVal point2 As Point) As Double
        Return Sqrt((point1.X - point2.X) ^ 2 + (point1.Y - point2.Y) ^ 2)
    End Function
    Public Function StringToDate(str As String, indexFile As Boolean) As Date
        If str.Length = 11 Then
            Dim d As New Date(str.Substring(0, 2) + 2000, 1, 1, str.Substring(5, 2), str.Substring(7, 2), str.Substring(9, 2))
            d = d.AddDays(str.Substring(2, 3) - 1)
            Return d
        Else
            If indexFile Then
                Return Date.FromFileTime(str)
            Else
                Return New Date(CLng(str))
            End If
        End If
    End Function
    Public Function DateToString(d As Date) As String
        Return d.Year - 2000 & FormatNumberLength(d.DayOfYear, 3) & FormatNumberLength(d.Hour, 2) & FormatNumberLength(d.Minute, 2) & FormatNumberLength(d.Second, 2)
    End Function
    Public Function RepString(ByVal [string] As String, ByVal repeatTimes As Integer) As String
        Dim returnValue As String = ""
        For i = 1 To repeatTimes
            returnValue &= [string]
        Next
        Return returnValue
    End Function
    Public Function RemovePrefixZero(ByVal number As Double) As String
        If CStr(number).Length > 0 AndAlso CStr(number).Substring(0, 1) = 0 And number <> 0 Then
            Return CStr(number).Substring(1, CStr(number).Length - 1)
        Else
            Return CStr(number)
        End If
    End Function
    Public Function RemovePrefixZero(ByVal number As String) As String
        If number.Length > 0 AndAlso number.Substring(0, 1) = "0" And number <> "0" Then
            Return number.Substring(1, CStr(number).Length - 1)
        Else
            Return number
        End If
    End Function
    Public Function FormatNumberLength(ByVal number As Double, ByVal length As Integer, Optional insertAfter As Boolean = False) As String
        If CStr(number).Length < length Then
            Return If(insertAfter, CStr(number) & RepString("0", length - CStr(number).Length), RepString("0", length - CStr(number).Length) & CStr(number))
        Else
            Return CStr(number)
        End If
    End Function
    Public Function TripleDESEncrypt(ByVal text As String) As String
        Dim tripleDES As New Security.Cryptography.TripleDESCryptoServiceProvider
        Dim key As String = "QuickTraderchartograph"
        Dim initialValue As String = "chartogr"
        Dim keyBytes(key.Length - 1) As Byte
        Dim initialValueBytes(initialValue.Length - 1) As Byte
        Dim inputBytes(text.Length - 1) As Byte
        Dim outputBytes() As Byte
        Dim outputString As String = ""
        For i = 0 To key.Length - 1
            keyBytes(i) = CByte(Asc(key.Substring(i, 1)))
        Next
        For i = 0 To initialValue.Length - 1
            initialValueBytes(i) = CByte(Asc(initialValue.Substring(i, 1)))
        Next
        For i = 0 To text.Length - 1
            inputBytes(i) = Asc(text.Substring(i, 1))
        Next
        outputBytes = tripleDES.CreateEncryptor(keyBytes, initialValueBytes).TransformFinalBlock(inputBytes, 0, inputBytes.Count)
        For Each item In outputBytes
            outputString &= Chr(item)
        Next
        Return outputString
    End Function
    Public Function TripleDESDecrypt(ByVal text As String) As String
        Dim tripleDES As New Security.Cryptography.TripleDESCryptoServiceProvider
        Dim key As String = "QuickTraderchartograph"
        Dim initialValue As String = "chartogr"
        Dim keyBytes(key.Length - 1) As Byte
        Dim initialValueBytes(initialValue.Length - 1) As Byte
        Dim inputBytes(text.Length - 1) As Byte
        Dim outputBytes() As Byte
        Dim outputString As String = ""
        For i = 0 To key.Length - 1
            keyBytes(i) = CByte(Asc(key.Substring(i, 1)))
        Next
        For i = 0 To initialValue.Length - 1
            initialValueBytes(i) = CByte(Asc(initialValue.Substring(i, 1)))
        Next
        For i = 0 To text.Length - 1
            inputBytes(i) = Asc(text.Substring(i, 1))
        Next
        outputBytes = tripleDES.CreateDecryptor(keyBytes, initialValueBytes).TransformFinalBlock(inputBytes, 0, inputBytes.Count)
        For Each item In outputBytes
            outputString &= Chr(item)
        Next
        Return outputString
    End Function
    Public Function BinaryToDecimal(ByVal Binary As String) As Long
        Dim n As Long
        Dim s As Integer
        For s = 1 To Len(Binary)
            n = n + (Mid(Binary, Len(Binary) - s + 1, 1) * (2 ^ (s - 1)))
        Next s

        BinaryToDecimal = n
    End Function
    Public Function DecimalToBinary(ByVal DecimalNum As Long) As String
        Dim tmp As String
        Dim n As Long
        n = DecimalNum
        tmp = Trim(Str(n Mod 2))
        n = n \ 2
        Do While n <> 0
            tmp = Trim(Str(n Mod 2)) & tmp
            n = n \ 2
        Loop
        DecimalToBinary = tmp
    End Function
    Public Function Spacify(ByVal [string] As String) As String
        Dim newString As String = [string]
        Dim insertions As Integer = 0
        'name = name.Substring(0, 1) & LCase(name.Substring(1))
        'For i = 0 To [string].Length - 1
        '    If UCase([string].Substring(i, 1)) = [string].Substring(i, 1) And i <> 0 Then
        '        newString = newString.Insert(i + insertions, " ")
        '        insertions += 1
        '    End If
        'Next
        For i = 0 To [string].Length - 1
            If UCase([string].Substring(i, 1)) = [string].Substring(i, 1) Then
                If i > 0 Then
                    'Debug.WriteLine("string: " & [string] & "; lastindex: " & [string].Length - 1 & "; index: " & i)
                    If i < [string].Length - 1 AndAlso [string].Substring(i + 1, 1).ToUpper <> [string].Substring(i + 1, 1) Then
                        'Debug.WriteLine(newString & " inserting to " & i + insertions)
                        newString = newString.Insert(i + insertions, " ")
                        insertions += 1
                    ElseIf [string].Substring(i - 1, 1).ToUpper <> [string].Substring(i - 1, 1) Then
                        newString = newString.Insert(i + insertions, " ")
                        insertions += 1
                    End If
                End If
            End If
        Next
        Return newString
    End Function
    Public Function FormatNumberLengthAndPrefix(num As Decimal, placesAfterDecimal As Integer) As String
        Return RemovePrefixZero(FormatNumber(num, placesAfterDecimal))
    End Function
    Public Function VectorToPoint(vector As Vector) As Point
        Return New Point(vector.X, vector.Y)
    End Function
    ''' <summary>
    ''' Returns -1 for false and 1 for true.
    ''' </summary>
    Public Function BooleanToInteger(ByVal bool As Boolean) As Integer
        Return -(CInt(bool) * 2 + 1)
    End Function
    Public Function Despacify(ByVal [string] As String) As String
        Return [string].Replace(" ", "")
    End Function
    Public Function Singularize(ByVal word As String) As String
        Return word.TrimEnd("s"c)
    End Function
    Public Function Pluralize(ByVal word As String) As String
        If Not word.EndsWith("s") Then Return word & "s" Else Return word
    End Function
    Public Function LightenColor(ByVal color As Color, ByVal inverseStrength As Double) As Color
        color.R += (255 - color.R) / inverseStrength
        color.G += (255 - color.G) / inverseStrength
        color.B += (255 - color.B) / inverseStrength
        Return color
    End Function
    Public Function GetForegroundColor(ByVal backgroundColor As Color) As Color
        If CInt(backgroundColor.R) + CInt(backgroundColor.G) + CInt(backgroundColor.B) < 255 * 3 / 2 Then Return Colors.White Else Return Colors.Black
    End Function
    Public Function GetInverseColor(ByVal color As Color) As Color
        Return color.FromArgb(255, 255 - color.R, 255 - color.G, 255 - color.B)
    End Function
    'Public Function GetExpiryAsDate(ByVal expiry As String) As Date
    '    If expiry IsNot Nothing Then
    '        If expiry.Length = 8 Then
    '            Return New Date(expiry.Substring(2, 2) + 2000, expiry.Substring(4, 2), expiry.Substring(6, 2))
    '        ElseIf expiry.Length = 6 Then
    '            Return New Date(expiry.Substring(2, 2) + 2000, expiry.Substring(4, 2), 1)
    '        Else
    '            Return Nothing
    '        End If
    '    Else
    '        Return Nothing
    '    End If
    'End Function
    Public Function GetContractMonthAsDate(ByVal contractMonth As String) As Date
        If contractMonth IsNot Nothing Then
            If contractMonth.Length = 6 Then
                Return New Date(CInt(contractMonth.Substring(0, 4)), CInt(contractMonth.Substring(4, 2)), 1)
            ElseIf contractMonth.Length = 8 Then
                Return New Date(contractMonth.Substring(0, 4), contractMonth.Substring(4, 2), contractMonth.Substring(6, 2))
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function
    Public Function GetContractMonthStringFromDate(ByVal [date] As Date) As String
        Return [date].Year & FormatNumberLength([date].Month, 2) & FormatNumberLength([date].Day, 2)
    End Function
    Public Function GetContractMonthFormattedString(ByVal contractMonth As String) As String
        If contractMonth IsNot Nothing Then
            If contractMonth.Length = 6 Then
                Return GetMonthAbreviation(contractMonth.Substring(4, 2)) & " " & contractMonth.Substring(0, 4)
            ElseIf contractMonth.Length = 8 Then
                Return GetMonthAbreviation(contractMonth.Substring(4, 2)) & " " & contractMonth.Substring(6, 2) & " " & contractMonth.Substring(0, 4)
            Else
                Return ""
            End If
        Else
            Return ""
        End If
    End Function
    Public Function GetMonthAbreviation(ByVal monthNumber As Integer) As String
        Dim monthName As String = ""
        Select Case monthNumber
            Case 1
                monthName = "Jan"
            Case 2
                monthName = "Feb"
            Case 3
                monthName = "Mar"
            Case 4
                monthName = "Apr"
            Case 5
                monthName = "May"
            Case 6
                monthName = "Jun"
            Case 7
                monthName = "Jul"
            Case 8
                monthName = "Aug"
            Case 9
                monthName = "Sep"
            Case 10
                monthName = "Oct"
            Case 11
                monthName = "Nov"
            Case 12
                monthName = "Dec"
        End Select
        Return monthName
    End Function
    'Function baseN2dec(ByVal value, ByVal inBase) As String
    '    'Converts any base to base 10
    '    Dim strValue, i, x, y
    '    strValue = StrReverse(CStr(UCase(value)))
    '    For i = 0 To Len(strValue) - 1
    '        x = Mid(strValue, i + 1, 1)
    '        If Not IsNumeric(x) Then
    '            y = y + ((Asc(x) - 65) + 10) * (inBase ^ i)
    '        Else
    '            y = y + ((inBase ^ i) * CInt(x))
    '        End If
    '    Next
    '    baseN2dec = y
    'End Function
    'Function dec2baseN(ByVal value, ByVal outBase) As String
    '    'Converts base 10 to any base
    '    Dim q 'quotient
    '    Dim r 'remainder
    '    Dim m 'denominator
    '    Dim y 'converted value
    '    m = outBase
    '    q = value
    '    Do
    '        r = q Mod m
    '        q = Int(q / m)
    '        If r >= 10 Then
    '            r = Chr(65 + (r - 10))
    '        End If
    '        y = y & CStr(r)
    '    Loop Until q = 0
    '    dec2baseN = StrReverse(y)
    'End Function

    Public Sub AddMenuHandler(ByVal menuItem As MenuItem, ByVal handler As RoutedEventHandler)
        For Each item In menuItem.Items
            If TypeOf item Is MenuItem Then
                AddMenuHandler(item, handler)
            End If
        Next
        AddHandler menuItem.Click, handler
    End Sub
    Public Sub RemoveMenuHandler(ByVal menuItem As MenuItem, ByVal handler As RoutedEventHandler)
        For Each item In menuItem.Items
            If TypeOf item Is MenuItem Then
                RemoveMenuHandler(item, handler)
            End If
        Next
        RemoveHandler menuItem.Click, handler
    End Sub

    Public Function ArrayToList(Of T)(ByVal array As Array) As List(Of T)
        Dim list As New List(Of T)
        For Each item In array
            If TypeOf item Is T Then
                list.Add(item)
            End If
        Next
        Return list
    End Function
    Public Sub InsertArrayInList(Of T)(ByVal list As List(Of T), ByVal array As Array, ByVal index As Integer)
        For i = 0 To array.GetUpperBound(0)
            list.Insert(index + i, array.GetValue(i))
        Next
    End Sub
    Public Sub AddArrayToList(Of T)(ByVal list As List(Of T), ByVal array As Array)
        For Each item In array
            list.Add(item)
        Next
    End Sub
    Public Function ConcatenateLists(Of T)(ByVal list1 As List(Of T), ByVal list2 As List(Of T)) As List(Of T)
        list1.AddRange(list2)
        Return list1
    End Function
	Public Function CalculateRegression(pointCollection As List(Of Point)) As Vector
		Dim n As Decimal = pointCollection.Count
		Dim a As Decimal, b As Decimal
		Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
		For Each point In pointCollection
			sumx += point.X
			sumy += point.Y
			sumxy += point.X * point.Y
			sumx2 += point.X ^ 2
		Next
		b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
		a = (sumy - b * sumx) / n
		Return New Vector(a, b)
	End Function
	Public Function CalculateRegressionY(pointCollection As List(Of Point), x As Decimal) As Decimal
		Dim n As Decimal = pointCollection.Count
		Dim a As Decimal, b As Decimal
		Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
		For Each point In pointCollection
			sumx += point.X
			sumy += point.Y
			sumxy += point.X * point.Y
			sumx2 += point.X ^ 2
		Next
		b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
		a = (sumy - b * sumx) / n
		Return a + b * x
    End Function

    Public Sub ResetFile(ByVal filename As String)
        Dim baseDirectoryWithFileName As String = Path.Combine(ApplicationDirectory, filename)
        If Exists(baseDirectoryWithFileName) Then
            If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                filesToRemember.Remove(baseDirectoryWithFileName)
                File.WriteAllText(baseDirectoryWithFileName, "")
            End If
        End If
    End Sub
    Public Sub ResetSetting(ByVal filename As String, ByVal name As String, Optional writeFile As Boolean = True)
        Dim baseDirectoryWithFileName As String = Path.Combine(ApplicationDirectory, filename)
        If Exists(baseDirectoryWithFileName) Then
            Dim lines As New List(Of String)
            If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                lines = filesToRemember(baseDirectoryWithFileName)
            Else
                Dim reader As New StreamReader(baseDirectoryWithFileName)
                While reader.Peek <> -1
                    lines.Add(reader.ReadLine)
                End While
                reader.Close()
                If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                    filesToRemember(baseDirectoryWithFileName) = lines
                Else
                    filesToRemember.Add(baseDirectoryWithFileName, lines)
                End If
            End If
            If writeFile Then
                Dim writer As New StreamWriter(baseDirectoryWithFileName)
                Dim i As Integer
                Do While i < lines.Count
                    Dim line As String = lines(i)
                    If line <> "" AndAlso LCase(line.Substring(0, line.IndexOf(":"))) <> LCase(name) Then
                        writer.WriteLine(line)
                    Else
                        filesToRemember(baseDirectoryWithFileName).RemoveAt(i)
                        i -= 1
                    End If
                    i += 1
                Loop
                writer.Close()
            Else
                For i = 0 To lines.Count - 1
                    Dim line As String = lines(i)
                    If line <> "" AndAlso LCase(line.Substring(0, line.IndexOf(":"))) <> LCase(name) Then
                    Else
                        filesToRemember(baseDirectoryWithFileName).RemoveAt(i)
                        Exit For
                    End If
                Next
            End If
        End If
    End Sub
    Public filesToRemember As New Dictionary(Of String, List(Of String))
    Public Sub WriteSetting(ByVal filename As String, ByVal name As String, ByVal value As Object, Optional writeFile As Boolean = True)
        Try
            Dim baseDirectoryWithFileName As String = Path.Combine(ApplicationDirectory, filename)
            If Exists(baseDirectoryWithFileName) Then
                Dim lines As New List(Of String)

                If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                    lines = filesToRemember(baseDirectoryWithFileName)
                Else
                    Dim reader As New StreamReader(baseDirectoryWithFileName)
                    While reader.Peek <> -1
                        lines.Add(reader.ReadLine)
                    End While
                    reader.Close()
                    If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                        filesToRemember(baseDirectoryWithFileName) = lines
                    Else
                        filesToRemember.Add(baseDirectoryWithFileName, lines)
                    End If
                End If
                If writeFile Then
                    Dim writer As New StreamWriter(baseDirectoryWithFileName)
                    Dim inFile As Boolean
                    For i = 0 To lines.Count - 1
                        Dim line As String = lines(i)
                        If Line <> "" AndAlso LCase(Line.Substring(0, Line.IndexOf(":"))) = LCase(name) Then
                            If value IsNot Nothing Then
                                Line = name & ":" & value.ToString
                            Else
                                Line = name & ":"
                            End If
                            inFile = True
                            filesToRemember(baseDirectoryWithFileName)(i) = line
                        End If
                        writer.WriteLine(Line)
                    Next
                    If Not inFile And value IsNot Nothing Then
                        writer.WriteLine(name & ":" & value.ToString)
                        lines.Add(name & ":" & value.ToString)
                    End If
                    writer.Close()
                    If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                        filesToRemember(baseDirectoryWithFileName) = lines
                    Else
                        filesToRemember.Add(baseDirectoryWithFileName, lines)
                    End If
                    '    counter(0) += 1
                Else
                    If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
                        Dim inFile As Boolean
                        For i = 0 To lines.Count - 1
                            If lines(i) <> "" AndAlso LCase(lines(i).Substring(0, lines(i).IndexOf(":"))) = LCase(name) Then
                                If value IsNot Nothing Then
                                    lines(i) = name & ":" & value.ToString
                                Else
                                    lines(i) = name & ":"
                                End If
                                inFile = True
                                Exit For
                            End If
                        Next
                        If Not inFile And value IsNot Nothing Then lines.Add(name & ":" & value.ToString)
                        filesToRemember(baseDirectoryWithFileName) = lines
                    Else
                        ShowInfoBox("The code should not get here. 'writeFile' was attributed when it shouldn't have been.", Application.Current.MainWindow)
                    End If
                End If
            Else
                If value IsNot Nothing Then My.Computer.FileSystem.WriteAllText(baseDirectoryWithFileName, name & ":" & value.ToString & vbNewLine, True)
            End If
        Catch ex As Exception
            Throw New Exception("There was an error saving the setting '" & name & "' in file '" & filename & "' to the value '" & value.ToString & "'." & vbNewLine & "Original message: " & ex.Message)
        End Try
    End Sub
    Public Function SettingExists(ByVal filename As String, ByVal name As String) As Boolean
        Dim baseDirectoryWithFileName As String = Path.Combine(ApplicationDirectory, filename)
        If Not Exists(baseDirectoryWithFileName) Then Return False
        Dim reader As New StreamReader(baseDirectoryWithFileName)
        While reader.Peek <> -1
            Dim line As String = reader.ReadLine
            If line <> "" AndAlso LCase(line.Substring(0, line.IndexOf(":"))) = LCase(name) Then
                reader.Close()
                Return True
            End If
        End While
        reader.Close()
        Return False
    End Function
    Public Function ReadSetting(ByVal filename As String, ByVal name As String, ByVal defaultValue As String) As String
        Dim baseDirectoryWithFileName As String = Path.Combine(ApplicationDirectory, filename)
        If Not Exists(baseDirectoryWithFileName) Then Return defaultValue
        If filesToRemember.ContainsKey(baseDirectoryWithFileName) Then
            For Each line In filesToRemember(baseDirectoryWithFileName)
                If line <> "" AndAlso LCase(line.Substring(0, line.IndexOf(":"))) = LCase(name) Then
                    Return line.Substring(line.IndexOf(":") + 1)
                End If
            Next
        Else
            Dim reader As New StreamReader(baseDirectoryWithFileName)
            Dim lines As New List(Of String)
            Dim result As String = ""
            While reader.Peek <> -1
                Dim line As String = reader.ReadLine
                If line <> "" AndAlso LCase(line.Substring(0, line.IndexOf(":"))) = LCase(name) Then
                    result = line.Substring(line.IndexOf(":") + 1)
                End If
                lines.Add(line)
            End While
            filesToRemember.Add(baseDirectoryWithFileName, lines)
            reader.Close()
            Return result
        End If

        Return defaultValue
    End Function

    Public Function TimeSpanFromBarSize(ByVal barSize As BarSize) As TimeSpan
        Select Case barSize
            Case BarSize.FifteenMinutes
                Return TimeSpan.FromMinutes(15)
            Case BarSize.FifteenSeconds
                Return TimeSpan.FromSeconds(15)
            Case BarSize.FiveMinutes
                Return TimeSpan.FromMinutes(5)
            Case BarSize.FiveSeconds
                Return TimeSpan.FromSeconds(5)
            Case BarSize.OneDay
                Return TimeSpan.FromDays(1)
            Case BarSize.OneHour
                Return TimeSpan.FromHours(1)
            Case BarSize.OneMinute
                Return TimeSpan.FromMinutes(1)
            Case BarSize.OneSecond
                Return TimeSpan.FromSeconds(1)
            Case BarSize.OneWeek
                Return TimeSpan.FromDays(7)
            Case BarSize.OneYear
                Return TimeSpan.FromDays(365)
            Case BarSize.ThirtyMinutes
                Return TimeSpan.FromMinutes(30)
            Case BarSize.ThirtySeconds
                Return TimeSpan.FromSeconds(30)
            Case BarSize.TwoMinutes
                Return TimeSpan.FromMinutes(2)
        End Select
        Return Nothing
    End Function
    ''' <summary>
    ''' Returns time1 - time2, but if time1 is less than time2 then a the previous day is taken into consideration. Example: 7:00 - 8:00 would return 23:00 hours.
    ''' </summary>

    Public Function GetTimeDifferenceWithRespectToDate(time1 As TimeSpan, time2 As TimeSpan) As TimeSpan
        If time1 < time2 Then
            Return TimeSpan.FromDays(1) - (time2 - time1)
        Else
            Return time1 - time2
        End If
    End Function
    Public Function TimeAndDateStringFromDate([date] As Date) As String
        Return [date].ToShortTimeString & ", " & [date].ToShortDateString
    End Function
    Public Function DurationStringFromTimeSpan(ByVal timeSpan As TimeSpan) As String
        If timeSpan.TotalSeconds < 86400 Then
            Return CStr(Int(timeSpan.TotalSeconds)) & " S"
        ElseIf timeSpan.TotalDays <= 365 Then
            Return CStr(Int(timeSpan.TotalDays)) & " D"
        End If
        Return "365 D"
    End Function
    Public Function ModifierKeysDisplayString(ByVal modifierKeys As ModifierKeys) As String
        Select Case modifierKeys
            Case 1
                Return "Alt"
            Case 2
                Return "Ctrl"
            Case 3
                Return "Ctrl+Alt"
            Case 4
                Return "Shift"
            Case 5
                Return "Shift+Alt"
            Case 6
                Return "Ctrl+Shift"
            Case 7
                Return "Ctrl+Alt+Shift"
            Case 8
                Return "Windows"
            Case 9
                Return "Alt+Windows"
            Case 10
                Return "Ctrl+Windows"
            Case 11
                Return "Ctrl+Alt+Windows"
            Case 12
                Return "Shift+Windows"
            Case 13
                Return "Shift+Alt+Windows"
            Case 14
                Return "Ctrl+Shift+Windows"
            Case 15
                Return "Ctrl+Alt+Shift+Windows"
            Case Else
                Return ""
        End Select
    End Function
    Public Function HotkeyDisplayString(ByVal modifierKeys As ModifierKeys, ByVal key As Key) As String
        Dim modifierKeyString As String = ModifierKeysDisplayString(modifierKeys)
        Return modifierKeyString & If(modifierKeyString = "", "", "+") & HotkeyDisplayString(key)
    End Function
    Public Function HotkeyDisplayString(ByVal key As Key) As String
        Return [Enum].GetName(GetType(Key), key)
    End Function
    Public Function GetRealFromRelativeBounds(ByVal point As Point, ByVal bounds As Bounds, ByVal parentSize As Size) As Point
        Return New Point(((point.X - bounds.X1) / (bounds.X2 - bounds.X1)) * parentSize.Width, _
                        ((point.Y - bounds.Y1) / (bounds.Y2 - bounds.Y1)) * parentSize.Height)
    End Function
    Public Function GetRelativeFromRealBounds(ByVal point As Point, ByVal bounds As Bounds, ByVal parentSize As Size) As Point
        Return New Point(((bounds.X2 - bounds.X1) * point.X) / parentSize.Width + bounds.X1, _
                            ((bounds.Y2 - bounds.Y1) * point.Y) / parentSize.Height + bounds.Y1)
    End Function

    Public Function GetParent(ByVal [object] As DependencyObject, ByVal parentType As Type) As Window
        If VisualTreeHelper.GetParent([object]) Is Nothing Then
            Return [object]
        Else
            Dim parent As DependencyObject = GetParent(VisualTreeHelper.GetParent([object]), parentType)
            If parent Is Nothing Or [object].GetType = parentType Then
                Return [object]
            Else
                Return parent
            End If
        End If
    End Function
    Public Function GetIsChild(ByVal element As DependencyObject, ByVal childElement As DependencyObject) As Boolean
        If VisualTreeHelper.GetParent(childElement) Is Nothing Then
            Return False
        ElseIf VisualTreeHelper.GetParent(childElement) Is element Then
            Return True
        Else
            Return GetIsChild(VisualTreeHelper.GetParent(childElement), element)
        End If
    End Function
    Public Function GetIsParent(ByVal element As DependencyObject, ByVal parentElement As DependencyObject) As Boolean
        If TypeOf element Is Visual Then
            If VisualTreeHelper.GetParent(element) Is Nothing Then
                Return False
            ElseIf VisualTreeHelper.GetParent(element) Is parentElement Then
                Return True
            Else
                Return GetIsParent(VisualTreeHelper.GetParent(element), parentElement)
            End If
        Else
            Return False
        End If
    End Function
    Public Function GetFirstParentOfType(ByVal element As DependencyObject, ByVal parentType As Type) As DependencyObject
        If TypeOf element Is Visual Then
            Dim parent As DependencyObject = VisualTreeHelper.GetParent(element)
            If parent Is Nothing Then
                Return Nothing
            ElseIf parent.GetType.IsSubclassOf(parentType) Or parent.GetType = parentType Then
                Return parent
            Else
                Return GetFirstParentOfType(parent, parentType)
            End If
        Else
            Return Nothing
        End If
    End Function

    Public Function ElementRelation(ByVal element1 As DependencyObject, ByVal relatedElement As DependencyObject) As Relation
        If GetIsChild(relatedElement, element1) Then
            Return Relation.Child
        ElseIf GetIsParent(relatedElement, element1) Then
            Return Relation.Parent
        Else
            Return Relation.Unrelated
        End If
    End Function

    Public Function NewSelectorBoxColorItem(ByVal color As Color, ByVal useClassicItemStyle As Boolean, Optional ByVal displayText As String = "") As ListBoxItem
        Dim height As Double = 18
        Dim item As New ListBoxItem
        Dim grd As New Grid
        If useClassicItemStyle Then
            Dim col1 As New ColumnDefinition
            col1.Width = New GridLength(height - 2)
            grd.ColumnDefinitions.Add(col1)
            Dim col2 As New ColumnDefinition
            col2.Width = New GridLength(1, GridUnitType.Star)
            grd.ColumnDefinitions.Add(col2)
            Dim rect As New Shapes.Rectangle
            rect.Width = height - 2
            rect.Height = height - 2
            Dim oppositeColor As Color
            If CInt(color.R) + CInt(color.G) + CInt(color.B) > 382.5 Then oppositeColor = Windows.Media.Colors.Black Else oppositeColor = Windows.Media.Colors.White
            rect.Fill = NewRadialGradientBrushWrapper(color, oppositeColor, 0.5, 0.5)
            Dim txt As New TextBlock
            txt.VerticalAlignment = Windows.VerticalAlignment.Center
            If displayText = "" Then txt.Text = ColorHelper.GetColorName(color) Else txt.Text = displayText
            txt.Margin = New Thickness(2)
            Grid.SetColumn(txt, 1)
            Grid.SetColumn(rect, 0)
            grd.Children.Add(txt)
            grd.Children.Add(rect)
            item.Content = grd
        Else
            grd.Background = New SolidColorBrush(color)
            grd.Height = height
            grd.Margin = New Thickness(1)
            Dim binding As New Binding
            binding.RelativeSource = New RelativeSource(RelativeSourceMode.FindAncestor, GetType(Selector), 1)
            binding.Path = New PropertyPath("ActualWidth")
            grd.SetBinding(Grid.WidthProperty, binding)
            item.Content = grd
        End If

        Return item
    End Function
    Public Function GetColorFromColorSelectorBoxItem(ByVal selectorBoxItem As ListBoxItem, ByVal useClassicItemStyle As Boolean) As Color
        If selectorBoxItem IsNot Nothing Then
            If useClassicItemStyle Then
                Return ColorHelper.GetColor(selectorBoxItem.Content.Children(0).Text)
            Else
                Return CType(selectorBoxItem.Content.Background, SolidColorBrush).Color
            End If
        Else
            Return Nothing
        End If
    End Function
    Public Function GetTextFromColorSelectorBoxItem(ByVal selectorBoxItem As ListBoxItem, ByVal useClassicItemStyle As Boolean) As String
        If useClassicItemStyle Then
            Return selectorBoxItem.Content.Children(0).Text
        Else
            Return ColorHelper.GetColorName(CType(selectorBoxItem.Content.Background, SolidColorBrush).Color)
        End If
    End Function
    Public Function ChooseOtherColorForSelectorBox(ByVal selector As Selector, ByVal useClassicItemStyle As Boolean) As Color
        Dim colDialog As New ColorDialog
        Dim col As Color
        If selector.SelectedItem IsNot Nothing Then
            col = ColorHelper.GetColor(GetTextFromColorSelectorBoxItem(selector.SelectedItem, useClassicItemStyle))
            colDialog.Color = col
        End If
        colDialog.ShowDialog()
        If colDialog.ButtonOKPressed Then
            col = colDialog.Color
            Dim colName As String = ColorHelper.GetColorName(col)
            selector.SelectedIndex = -1
            For Each item As ListBoxItem In selector.Items
                If GetTextFromColorSelectorBoxItem(item, useClassicItemStyle) = colName Then
                    selector.SelectedItem = item
                    Exit For
                End If
            Next
            If selector.SelectedIndex = -1 Then
                Return col
            Else
                Return Nothing
            End If
        End If
    End Function
    Public Sub SelectColorItemInSelectorBox(ByVal selector As Selector, ByVal color As Color, ByVal useClassicItemStyle As Boolean)
        For Each item In selector.Items
            If GetColorFromColorSelectorBoxItem(item, useClassicItemStyle) = color Then
                selector.SelectedItem = item
                Exit For
            End If
        Next

    End Sub

    Public Function PenToString(ByVal pen As Pen) As String
        Return Join({
                    (New BrushConverter).ConvertToString(pen.Brush),
                    pen.DashCap,
                    (New DoubleCollectionConverter).ConvertToString(pen.DashStyle.Dashes),
                    pen.DashStyle.Offset,
                    pen.EndLineCap,
                    pen.LineJoin,
                    pen.MiterLimit,
                    pen.StartLineCap,
                    pen.Thickness}, "!")
    End Function
    Public Function PenFromString(ByVal penString As String) As Pen
        Try
            Dim pen As New Pen
            Dim props() As String = Split(penString, "!")
            pen.Brush = (New BrushConverter).ConvertFromString(If(props(0) = "", "Transparent", props(0)))
            pen.DashCap = props(1)
            pen.DashStyle = New DashStyle((New DoubleCollectionConverter).ConvertFromString(props(2)), props(3))
            pen.EndLineCap = props(4)
            pen.LineJoin = props(5)
            pen.MiterLimit = props(6)
            pen.StartLineCap = props(7)
            pen.Thickness = props(8)
            Return pen
        Catch ex As Exception
            Throw New ArgumentException("'PenFromString' got an error parsing the string '" & penString & "'.")
        End Try
    End Function
    <DebuggerStepThrough()>
    Public Function EffectToString(ByVal effect As Effect) As String
        If effect IsNot Nothing Then
            If TypeOf effect Is BlurEffect Then
                Dim e As BlurEffect = effect
                Return Join({"BlurEffect", e.Radius}, "!")
            ElseIf TypeOf effect Is DropShadowEffect Then
                Dim e As Effects.DropShadowEffect = effect
                Return Join({"DropShadowEffect", e.BlurRadius, (New ColorConverter).ConvertToString(e.Color), e.Direction, e.Opacity, e.ShadowDepth}, "!")
            Else
                Throw New ArgumentException("The effect can only be of type System.Windows.Media.Effects.BlurEffect or System.Windows.Media.Effects.DropShadowEffect.")
                Return ""
            End If
        Else
            Return ""
        End If
    End Function
    Public Function EffectFromString(ByVal effectString As String) As Effect
        Try
            Dim props() As String = Split(effectString, "!")
            If effectString <> "" Then
                If props(0) = "BlurEffect" Then
                    Return New BlurEffect With {.Radius = props(1)}
                ElseIf props(0) = "DropShadowEffect" Then
                    Return New DropShadowEffect With {.BlurRadius = props(1), .Color = (ColorConverter).ConvertFromString(props(2)), .Direction = props(3),
                                                      .Opacity = props(4), .ShadowDepth = props(5)}
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Throw New ArgumentException("'EffectFromString' got an error parsing the string '" & effectString & "'.")
        End Try
    End Function
    Public Function RotateTransformToString(ByVal transform As RotateTransform) As String
        Return Join({transform.Angle, transform.CenterX, transform.CenterY}, "!")
    End Function
    Public Function RotateTransformFromString(ByVal transformString As String) As RotateTransform
        Try
            Dim transform As New RotateTransform
            Dim props() As String = Split(transformString, "!")
            transform.Angle = props(0)
            transform.CenterX = props(1)
            transform.CenterY = props(2)
            Return transform
        Catch ex As Exception
            Throw New ArgumentException("'RotateTransformFromString' got an error parsing the string '" & transformString & "'.")
        End Try
    End Function
End Module