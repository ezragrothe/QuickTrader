Public Class AdvancedMathConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim expr As String = LCase(Expression).Replace(LCase(Var), CDbl(value))
        Dim innerOpenPara As Integer = -1
        Dim innerClosePara As Integer
        While expr.IndexOf("(") <> -1
            For i = 0 To expr.Length - 1
                If expr.Substring(i, 1) = "(" Then
                    innerOpenPara = i
                ElseIf expr.Substring(i, 1) = ")" Then
                    innerClosePara = i
                    Exit For
                End If
            Next
            expr = StringReplace(expr, SolveExpr(expr.Substring(innerOpenPara + 1, innerClosePara - 1 - innerOpenPara)), innerOpenPara, innerClosePara - innerOpenPara + 1)
        End While
        expr = SolveExpr(expr)
        Return expr
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return Nothing
    End Function

    Private Function SolveExpr(ByVal expr As String) As Double
        Dim lastNumIndex As Integer
        Dim i As Integer = -1
        While expr.Contains("^")
            i += 1
            If IsNumeric(expr.Substring(i, 1)) And (i = 0 OrElse Not IsNumeric(expr.Substring(i - 1, 1))) Then
                lastNumIndex = i
            End If
            If expr.Substring(i, 1) = "^" Then
                Dim nextNumIndex As Integer = expr.Length - 1
                For j = i + 1 To expr.Length - 1
                    If Not IsNumeric(expr.Substring(j, 1)) Then
                        nextNumIndex = j - 1
                        Exit For
                    End If
                Next
                Dim answer As Double = CDbl(expr.Substring(lastNumIndex, i - lastNumIndex)) ^ CDbl(expr.Substring(i + 1, nextNumIndex - i))
                expr = StringReplace(expr, answer, lastNumIndex, nextNumIndex - lastNumIndex + 1)
                i = -1
                lastNumIndex = 0
            End If
        End While
        While expr.Contains("*") Or expr.Contains("/")
            i += 1
            If IsNumeric(expr.Substring(i, 1)) And (i = 0 OrElse Not IsNumeric(expr.Substring(i - 1, 1))) Then
                lastNumIndex = i
            End If
            If expr.Substring(i, 1) = "*" Or expr.Substring(i, 1) = "/" Then
                Dim nextNumIndex As Integer = expr.Length - 1
                For j = i + 1 To expr.Length - 1
                    If Not IsNumeric(expr.Substring(j, 1)) Then
                        nextNumIndex = j - 1
                        Exit For
                    End If
                Next
                Dim answer As Double = expr.Substring(lastNumIndex, i - lastNumIndex) * expr.Substring(i + 1, nextNumIndex - i)
                If expr.Substring(i, 1) = "/" Then answer = 1 / answer
                expr = StringReplace(expr, answer, lastNumIndex, nextNumIndex - lastNumIndex + 1)
                i = -1
                lastNumIndex = 0
            End If
        End While
        While expr.Contains("+") Or expr.Contains("-")
            i += 1
            If IsNumeric(expr.Substring(i, 1)) And (i = 0 OrElse Not IsNumeric(expr.Substring(i - 1, 1))) Then
                lastNumIndex = i
            End If
            If expr.Substring(i, 1) = "+" Or expr.Substring(i, 1) = "-" Then
                Dim nextNumIndex As Integer = expr.Length - 1
                For j = i + 1 To expr.Length - 1
                    If Not IsNumeric(expr.Substring(j, 1)) Then
                        nextNumIndex = j - 1
                        Exit For
                    End If
                Next
                Dim answer As Double = CDbl(expr.Substring(lastNumIndex, i - lastNumIndex)) + CDbl(expr.Substring(i + 1, nextNumIndex - i))
                If expr.Substring(i, 1) = "-" Then answer = expr.Substring(lastNumIndex, i - lastNumIndex) - expr.Substring(i + 1, nextNumIndex - i)
                expr = StringReplace(expr, answer, lastNumIndex, nextNumIndex - lastNumIndex + 1)
                i = -1
                lastNumIndex = 0
            End If
        End While
        Return expr
    End Function
    Private Function StringReplace(ByVal str As String, ByVal newStr As String, ByVal index As Integer, ByVal count As Integer) As String
        Return str.Substring(0, index) & newStr & str.Substring(index + count)
    End Function
    Public Sub New(ByVal expr As String)
        Expression = expr
    End Sub
    Public Sub New()
    End Sub

    Private _expr As String
    Public Property Var As Char = "x"
    Public Property Expression As String
        Get
            Return _expr
        End Get
        Set(ByVal value As String)
            _expr = value
        End Set
    End Property

End Class
