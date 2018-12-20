Imports System.ComponentModel

Public Module ColorHelper

    Sub New()
        For Each member In GetType(Colors).GetProperties
            colorNames.Add(member.Name, CType(member.GetValue(member, Nothing), Windows.Media.Color))
        Next
    End Sub

    Private colorNames As New Dictionary(Of String, Color)

    ''' <summary>
    ''' Gets the name for the System.Windows.Media.Color.
    ''' </summary>
    ''' <param name="color">The System.Windows.Media.Color.</param>
    Public Function GetColorName(ByVal color As Windows.Media.Color) As String
        Dim colorName As String = ""
        For Each col As KeyValuePair(Of String, Color) In colorNames
            If col.Value = color Then
                colorName = col.Key
                Exit For
            End If
        Next
        If colorName = "" Then colorName = color.ToString
        Return colorName
    End Function
    ''' <summary>
    ''' Gets a color object from a its name.
    ''' </summary>
    ''' <param name="name">The name of the color.</param>
    Public Function GetColor(ByVal name As String) As Color
        If colorNames.ContainsKey(name) Then
            Return colorNames(name)
        Else
            Dim col = System.Drawing.ColorTranslator.FromHtml(name)
            Return Color.FromArgb(col.A, col.R, col.G, col.B)
        End If
    End Function
    ''' <summary>
    ''' Returns whether the color has a name.
    ''' </summary>
    ''' <param name="color">The color to check.</param>
    Public Function GetIsNamedColor(ByVal color As Color) As Boolean
        Return colorNames.ContainsKey(GetColorName(color))
    End Function

End Module
