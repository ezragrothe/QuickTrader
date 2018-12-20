Public Structure Tick
    Public Sub New(ByVal time As Date, ByVal price As Decimal)
        Me.Time = time
        Me.Price = price
    End Sub
    Public Property Time As Date
    Public Property Price As Decimal
End Structure
