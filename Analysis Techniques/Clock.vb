Namespace AnalysisTechniques
    Public Class Clock
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
        End Sub
#End Region
        Public Enum TimeDisplayStyle
            hhmmss
            HHMM
            MMDDYY_HHMMSS
            MMDDYY_HHMM
        End Enum
        <Input("")> Property DisplayStyle As String = "hh:mm:ss tt"
        <Input("")> Property Font As FontFamily = New FontFamily("Aerial")
        <Input("")> Property FontSize As Decimal = 12
        <Input("")> Property FontColor As Color = Colors.Gray
        <Input("")> Property FontWeight As FontWeight = FontWeights.Normal
        <Input("")> Property FontStyle As FontStyle = FontStyles.Normal
        <Input("")> Property Location As Point = New Point(200, 50)
        ' <Input("")> Property TextColor As Color = Colors.Gray
        Private timer As System.Windows.Forms.Timer
        Public Overrides Property Name As String = "Clock"
        Private timeLabel As Label
        <DebuggerStepThrough()> Private Sub Elapsed()
            If Chart IsNot Nothing And timeLabel IsNot Nothing Then
                Dim timeString As String = ""
                Dim startMarkerIndex As Integer
                Dim currentMarker As Char
                Dim i As Integer
                While i < DisplayStyle.Length
                    If currentMarker = Char.MinValue Then
                        Select Case LCase(DisplayStyle(i))
                            Case "m", "d", "y", "h", "s", "t"
                                If (i > 0 AndAlso DisplayStyle(i - 1) <> "\") OrElse i = 0 Then
                                    currentMarker = DisplayStyle(i)
                                    startMarkerIndex = i
                                Else
                                    timeString &= DisplayStyle(i)
                                End If
                            Case "\"
                                'If (i > 0 AndAlso DisplayStyle(i - 1) = "\") OrElse i = 0 Then
                                '    timeString &= "\"
                                'End If
                            Case Else
                                'If i > 1 AndAlso DisplayStyle(i - 1) = "\" Then
                                '    timeString &= "\"
                                'End If
                                timeString &= DisplayStyle(i)
                        End Select
                    Else
                        If DisplayStyle(i) <> currentMarker Or i = DisplayStyle.Length - 1 Then
                            Dim num As Integer = i - startMarkerIndex
                            Select Case currentMarker
                                Case "M"
                                    timeString &= FormatNumberLength(CInt(RepString(Now.Month, Ceiling(num / 2))), num)
                                Case "d"
                                    timeString &= FormatNumberLength(RepString(Now.Day, Ceiling(num / 2)), num)
                                Case "D"
                                    Dim day As String = [Enum].GetName(GetType(DayOfWeek), Now.DayOfWeek)
                                    If num >= day.Length Then
                                        timeString &= day
                                    Else
                                        timeString &= day.Substring(0, num)
                                    End If
                                Case "y", "Y"
                                    If num <= 2 Then
                                        timeString &= Now.Year.ToString.Substring(2)
                                    Else
                                        timeString &= RepString(Now.Year.ToString, (num + 1) \ 4)
                                    End If
                                Case "h"
                                    timeString &= FormatNumberLength(RepString(Now.Hour Mod 12, Ceiling(num / 2)), num)
                                Case "H"
                                    timeString &= FormatNumberLength(RepString(Now.Hour, Ceiling(num / 2)), num)
                                Case "m"
                                    timeString &= FormatNumberLength(RepString(Now.Minute, Ceiling(num / 2)), num)
                                Case "s", "S"
                                    timeString &= FormatNumberLength(RepString(Now.Second, Ceiling(num / 2)), num)
                                Case "t", "T"
                                    timeString &= RepString(If(Now.Hour >= 12, "PM", "AM"), Ceiling(num / 2))
                            End Select
                            currentMarker = Char.MinValue
                            startMarkerIndex = 0
                            i -= 1
                        End If
                    End If
                    i += 1
                End While
                timeLabel.Text = timeString
                AddObjectToChart(timeLabel)
                timeLabel.Location = Chart.GetRelativeFromReal(Location)
                'Font = timeLabel.Font.FontFamily
                'FontSize = timeLabel.Font.FontSize
                'FontColor = CType(timeLabel.Font.Brush, SolidColorBrush).Color
                'FontWeight = timeLabel.Font.FontWeight
                'FontStyle = timeLabel.Font.FontStyle
            End If
        End Sub
        Protected Overrides Sub Begin()
            MyBase.Begin()
            timeLabel = NewLabel(Now.ToShortTimeString, FontColor, Chart.GetRelativeFromReal(Location))
            timeLabel.UseNegativeCoordinates = True
            Dim f As New Font With {.FontFamily = Font, .FontSize = FontSize, .FontStyle = FontStyle, .FontWeight = FontWeight, .Brush = New SolidColorBrush(FontColor)}
            timeLabel.Font = f
            timer = New System.Windows.Forms.Timer
            timer.Interval = 1000
            timer.Start()
            AddHandler timer.Tick, AddressOf Elapsed
        End Sub
        Protected Overrides Sub Main()
            If IsLastBarOnChart Then
                timeLabel.Location = Chart.GetRelativeFromReal(Location)
            End If
        End Sub
    End Class
End Namespace