Imports System.Math
Imports System.Windows.Markup
Imports System.ComponentModel

Public Class NotifyingCollection(Of T)
    Implements ICollection(Of T)
    Implements IList(Of T)
    Implements IEnumerable(Of T)

    Dim _items As New List(Of T)
    Public Event ItemAdded(ByVal sender As Object, ByVal e As ItemChangedEventArgs(Of T))
    Public Event ItemRemoved(ByVal sender As Object, ByVal e As ItemChangedEventArgs(Of T))

    Public Sub Add(ByVal item As T) Implements System.Collections.Generic.ICollection(Of T).Add
        Dim args As New ItemChangedEventArgs(Of T)(item, Count)
        RaiseEvent ItemAdded(Me, args)
        If args.Cancel = False Then
            _items.Add(item)
        End If
    End Sub
    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of T).Clear
        For i = 1 To _items.Count
            If _items.Count > 0 Then RemoveAt(0)
        Next
    End Sub
    Public Function Contains(ByVal item As T) As Boolean Implements System.Collections.Generic.ICollection(Of T).Contains
        Return _items.Contains(item)
    End Function
    Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of T).CopyTo
        _items.CopyTo(array, arrayIndex)
    End Sub
    Public ReadOnly Property Count As Integer Implements System.Collections.Generic.ICollection(Of T).Count
        Get
            Return _items.Count
            '
        End Get
    End Property
    Public ReadOnly Property IsReadOnly As Boolean Implements System.Collections.Generic.ICollection(Of T).IsReadOnly
        Get
            Return False
        End Get
    End Property
    Public Function Remove(ByVal item As T) As Boolean Implements System.Collections.Generic.ICollection(Of T).Remove
        Dim args As New ItemChangedEventArgs(Of T)(item, IndexOf(item))
        RaiseEvent ItemRemoved(Me, args)
        If args.Cancel = False Then
            Return _items.Remove(item)
        End If
        Return Nothing
    End Function
    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
        Return _items.GetEnumerator
    End Function
    Public Function IndexOf(ByVal item As T) As Integer Implements System.Collections.Generic.IList(Of T).IndexOf
        Return _items.IndexOf(item)
    End Function
    Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements System.Collections.Generic.IList(Of T).Insert
        Throw New Exception()
        'Dim args As New ItemChangedEventArgs(Of T)(item, index)
        'RaiseEvent ItemAdded(Me, args)
        'If args.Cancel = False Then
        '    _items.Insert(index, item)
        'End If
    End Sub
    Default Public Overloads Property Item(ByVal index As Integer) As T Implements System.Collections.Generic.IList(Of T).Item
        Get
            Return _items(index)
        End Get
        Set(ByVal value As T)
            _items(index) = value
        End Set
    End Property
    Public Sub RemoveAt(ByVal index As Integer) Implements System.Collections.Generic.IList(Of T).RemoveAt
        Dim args As New ItemChangedEventArgs(Of T)(Item(index), index)
        RaiseEvent ItemRemoved(Me, args)
        If args.Cancel = False Then
            _items.RemoveAt(index)
        End If
    End Sub
    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return _items.GetEnumerator
    End Function
End Class