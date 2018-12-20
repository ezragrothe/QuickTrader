Imports System.Math
Imports System.Windows.Markup
Imports System.ComponentModel

Public Class ItemChangedEventArgs(Of T)
    Public Sub New(ByVal item As T, ByVal index As Integer)
        _item = item
        _index = index
    End Sub
    Private _item As T
    Public ReadOnly Property Item As T
        Get
            Return _item
        End Get
    End Property
    Private _index As Integer
    Public ReadOnly Property ItemIndex As Integer
        Get
            Return _index
        End Get
    End Property
    Public Property Cancel As Boolean
End Class