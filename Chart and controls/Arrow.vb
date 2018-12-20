<Serializable()>
Public Class Arrow
    Inherits ChartDrawingVisual
    Implements ISelectable, ISerializable

    Private _geometry As GeometryGroup
    Private _drawing As DrawingGroup

    Public Sub New()
        Pen.EndLineCap = PenLineCap.Triangle
        Pen.StartLineCap = PenLineCap.Triangle
    End Sub

    Private _location As Point
    Public Property Location As Point
        Get
            Return _location
        End Get
        Set(ByVal value As Point)
            _location = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _isflipped As Boolean = True
    Public Property IsFlipped As Boolean
        Get
            Return _isflipped
        End Get
        Set(ByVal value As Boolean)
            _isflipped = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _width As Double = 12
    Public Property Width As Double
        Get
            Return _width
        End Get
        Set(ByVal value As Double)
            _width = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _height As Double = 4
    Public Property Height As Double
        Get
            Return _height
        End Get
        Set(ByVal value As Double)
            _height = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup
        Get
            Return _drawing
        End Get
        Set(ByVal value As System.Windows.Media.DrawingGroup)
            _drawing = value
        End Set
    End Property
    Public Overrides Property IsEditable As Boolean = True

    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        If HasParent Then
            Dim point As Point = New Point(Round(Location.X, 0), RoundTo(Location.Y, Parent.GetMinTick))

            Dim items As New List(Of KeyValuePair(Of String, String))
            items.Add(New KeyValuePair(Of String, String)("Price: ", FormatNumber(If(UseNegativeCoordinates, -1, 1) * point.Y, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
            items.Add(New KeyValuePair(Of String, String)("Bar: ", point.X))
            If HasParent AndAlso point.X > 0 AndAlso Parent.bars.Count > point.X Then
                items.Add(New KeyValuePair(Of String, String)("Date: ", Parent.bars(point.X).Data.Date.ToShortDateString))
                items.Add(New KeyValuePair(Of String, String)("Time: ", Parent.bars(point.X).Data.Date.ToShortTimeString))
            Else
                items.Add(New KeyValuePair(Of String, String)("Date: ", "N/A"))
                items.Add(New KeyValuePair(Of String, String)("Time: ", "N/A"))
            End If
            If AnalysisTechnique IsNot Nothing Then
                items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", AnalysisTechnique.Name))
            Else
                items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", "N/A"))
            End If
            Return items
        Else
            Return Nothing
        End If
    End Function

    Private mouseOffset As Point
    Public Overrides Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonDown(e, location)
        mouseOffset = location - Parent.GetRealFromRelative(Me.Location)
        IsSelected = True
    End Sub
    Public Overrides Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As System.Windows.Point)
        If IsMouseDown And IsEditable Then
            Me.Location = Parent.GetRelativeFromReal(CType(location - mouseOffset, Point))
            If Not AutoRefresh Then RefreshVisual()
        End If
        MyBase.ParentMouseMove(e, location)
    End Sub
    Public Overrides Sub ParentMouseRightButtonDown(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonDown(e, location)
        'IsSelected = True
    End Sub
    Public Overrides Sub ParentMouseRightButtonUp(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonUp(e, location)
        If IsEditable Then Dim formatTextWin As New FormatArrowWindow(Me)
    End Sub

    Public Overrides Sub RefreshVisual()
        If HasParent Then
            _drawing = New DrawingGroup
            _geometry = New GeometryGroup
            Dim borderGeometry As New GeometryGroup
            '
            Dim realLocation As Point = Parent.GetRealFromRelative(New Point(Round(_location.X), RoundTo(_location.Y, Parent.GetMinTick)))
            'Dim isFlipped As Integer = -(Me.IsFlipped * 2 + 1)
            Dim b As Double = 2
            If IsFlipped Then
                _geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X + Width, realLocation.Y)))
                _geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X + Width / 2, realLocation.Y - Height / 2)))
                _geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X + Width / 2, realLocation.Y + Height / 2)))
                If IsSelected Then
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X - b, realLocation.Y - Height / 2 - b), New Point(realLocation.X - b, realLocation.Y + Height / 2 + b))) 'topleft to bottomleft
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X - b, realLocation.Y - Height / 2 - b), New Point(realLocation.X + Width + b, realLocation.Y - Height / 2 - b))) 'topleft to topright
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X + Width + b, realLocation.Y - Height / 2 - b), New Point(realLocation.X + Width + b, realLocation.Y + Height / 2 + b))) 'topright to bottomright
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X + Width + b, realLocation.Y + Height / 2 + b), New Point(realLocation.X - b, realLocation.Y + Height / 2 + b))) 'bottomright to bottomleft
                End If
            Else
                _geometry.Children.Add(New LineGeometry(New Point(realLocation.X - Width, realLocation.Y), realLocation))
                _geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X - Width / 2, realLocation.Y - Height / 2)))
                _geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X - Width / 2, realLocation.Y + Height / 2)))
                If IsSelected Then
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X + b, realLocation.Y - Height / 2 - b), New Point(realLocation.X + b, realLocation.Y + Height / 2 + b))) 'topright to bottomright
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X + b, realLocation.Y - Height / 2 - b), New Point(realLocation.X - Width - b, realLocation.Y - Height / 2 - b))) 'topright to topleft
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X - Width - b, realLocation.Y - Height / 2 - b), New Point(realLocation.X - Width - b, realLocation.Y + Height / 2 + b))) ' topleft to bottomleft
                    borderGeometry.Children.Add(New LineGeometry(New Point(realLocation.X - Width - b, realLocation.Y + Height / 2 + b), New Point(realLocation.X + b, realLocation.Y + Height / 2 + b))) ' bottomleft to bottomright
                End If
            End If
            _drawing.Children.Add(New GeometryDrawing(Pen.Brush, Pen, _geometry))
            Dim borderPen As Pen = Pen.Clone
            borderPen.Thickness = 1
            _drawing.Children.Add(New GeometryDrawing(Pen.Brush, borderPen, borderGeometry))
        End If
        MyBase.RefreshVisual()
    End Sub

    Private _isSelected As Boolean
    Public Property IsSelected As Boolean Implements ISelectable.IsSelected
        Get
            Return _isSelected
        End Get
        Set(ByVal value As Boolean)
            _isSelected = value
            RefreshVisual()
        End Set
    End Property
    Public Property IsSelectable As Boolean = True Implements ISelectable.IsSelectable

    Public Overrides Function ContainsPoint(ByVal point As System.Windows.Point) As Boolean
        Return GetContainingBox.Contains(point)
    End Function
    Private Function GetContainingBox() As Rect
        Dim realLocation As Point = Parent.GetRealFromRelative(New Point(Round(_location.X), RoundTo(_location.Y, Parent.GetMinTick)))
        Dim b As Double = 2
        If IsFlipped Then
            Return New Rect(New Point(realLocation.X - b, realLocation.Y - Height / 2 - b), New Point(realLocation.X + Width + b, realLocation.Y + Height / 2 + b))
        Else
            Return New Rect(New Point(realLocation.X - Width - b, realLocation.Y - Height / 2 - b), New Point(realLocation.X + b, realLocation.Y + Height / 2 + b))
        End If
    End Function
    Friend Overrides Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs)
        MyBase.Command_Executed(sender, e)
        If e.Command Is ChartCommands.ToggleArrowDirection Then
            IsFlipped = Not IsFlipped
            If Not AutoRefresh Then RefreshVisual()
        End If
    End Sub
    Public Sub Deserialize(ByVal serializedString As String) Implements ISerializable.Deserialize
        Try
            Dim v() As String = Split(serializedString, "/")
            Pen = PenFromString(v(0))
            Width = v(1)
            Height = v(2)
            IsFlipped = v(3)
            Location = (New PointConverter).ConvertFromString(v(4))
            AutoRefresh = v(5)
            If (New Date(CLng(v(6)))).Ticks <> 0 And Parent IsNot Nothing Then Location = New Point(Parent.GetBarNumberFromDate(New Date(v(6))), Location.Y)
        Catch ex As Exception
            Throw New ArgumentException("'Arrow.Deserialize()' got an error parsing the string '" & serializedString & "'.")
        End Try
    End Sub
    Public Function Serialize() As String Implements ISerializable.Serialize
        Return Join({
                    PenToString(Pen),
                    Width,
                    Height,
                    IsFlipped,
                    (New PointConverter).ConvertToString(Location),
                    AutoRefresh,
                    If(Parent IsNot Nothing, Parent.GetDateFromBarNumber(Location.X), (New Date(0))).Ticks
                }, "/")
    End Function
End Class
