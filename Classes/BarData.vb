<System.ComponentModel.TypeConverterAttribute(GetType(BarDataConverter))> _
Public Structure BarData
    Public Sub New(ByVal o As Double, ByVal h As Double, ByVal l As Double, ByVal c As Double, ByVal d As Date, ByVal duration As TimeSpan, ByVal index As Integer)
        _Open = o
        _High = h
        _Low = l
        _Close = c
        [Date] = d
        _Number = index
        _Duration = duration
    End Sub
    Public Sub New(ByVal o As Double, ByVal h As Double, ByVal l As Double, ByVal c As Double, ByVal d As Date, ByVal duration As TimeSpan, ByVal index As Integer, direction As Boolean)
        _Open = o
        _High = h
        _Low = l
        _Close = c
        [Date] = d
        _Number = index
        _Duration = duration
        _Direction = direction
    End Sub
    Public Sub New(ByVal o As Double, ByVal h As Double, ByVal l As Double, ByVal c As Double, ByVal d As Date, ByVal duration As TimeSpan)
        _Open = o
        _High = h
        _Low = l
        _Close = c
        [Date] = d
        _Duration = duration
    End Sub
    Public Sub New(ByVal o As Double, ByVal h As Double, ByVal l As Double, ByVal c As Double)
        _Open = o
        _High = h
        _Low = l
        _Close = c
    End Sub
    Public Sub New(ByVal o As Double, ByVal h As Double, ByVal l As Double)
        _Open = o
        _High = h
        _Low = l
        _Close = -1
    End Sub

    Public ReadOnly Property Volume As Long
        Get
            Return Math.Abs(High - Low)
        End Get
    End Property
    Public Property Duration As TimeSpan
    Public Property [Date] As Date
    Public Property Close As Decimal
    Public Property High As Decimal
    Public Property Low As Decimal
    Public Property Open As Decimal
    Public Property Number As Integer
    Public Property Direction As Boolean
    Public ReadOnly Property Avg As Decimal
        Get
            Return Methods.Avg(High, Low)
        End Get
    End Property
    Public ReadOnly Property Range As Decimal
        Get
            Return Abs(High - Low)
        End Get
    End Property
    Public Function Clone() As BarData
        Dim bar As New BarData(Open, High, Low, Close, [Date], Duration)
        bar.Number = Number
        bar.Duration = Duration
        Return bar
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("{0}; {1}, {2}, {3}, {4}", [Date].ToShortDateString & " " & [Date].ToShortTimeString, Math.Round(Open, 2), Math.Round(High, 2), Math.Round(Low, 2), Math.Round(Close, 2))
    End Function
    Public Function SetDuration(ByVal value As TimeSpan) As BarData
        Duration = value
        Return Me
    End Function
    Public Function SetDate(ByVal value As Date) As BarData
        [Date] = value
        Return Me
    End Function
    Public Function SetClose(ByVal value As Decimal) As BarData
        Close = value
        Return Me
    End Function
    Public Function SetHigh(ByVal value As Decimal) As BarData
        High = value
        Return Me
    End Function
    Public Function SetDirection(ByVal value As Boolean) As BarData
        Direction = value
        Return Me
    End Function
    Public Function SetLow(ByVal value As Decimal) As BarData
        Low = value
        Return Me
    End Function
    Public Function SetOpen(ByVal value As Decimal) As BarData
        Open = value
        Return Me
    End Function
    Public Function SetIndex(ByVal value As Decimal) As BarData
        Number = value
        Return Me
    End Function
End Structure