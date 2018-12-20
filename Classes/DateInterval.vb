Public Structure DateInterval
    Public Sub New(firstDate As Date, secondDate As Date)
        Me.FirstDate = firstDate
        Me.SecondDate = secondDate
    End Sub
    Public Property FirstDate As Date
    Public Property SecondDate As Date
    Public ReadOnly Property Duration As TimeSpan
        Get
            Return SecondDate - FirstDate
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return FirstDate.ToFileTime & "-" & SecondDate.ToFileTime
    End Function
    Public Shared Function FromString(value As String) As DateInterval
        Return New DateInterval(Date.FromFileTime(value.Split("-")(0)), Date.FromFileTime(value.Split("-")(1)))
    End Function
End Structure
