Public Class Label
    Inherits ChartDrawingVisual
    Implements ISelectable, ISerializable


    Public Sub New()
        TextOptions.SetTextFormattingMode(Me, TextFormattingMode.Display)
        VisualTextRenderingMode = TextRenderingMode.ClearType
        AutoRefresh = True
    End Sub

    Private _text As String
    Public Property Text As String
        Get
            Return _text
        End Get
        Set(ByVal value As String)
            _text = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Private _font As New Font

    Public Property Font As Font
        Get
            Return _font
        End Get
        Set(ByVal value As Font)
            _font = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
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
    Public Property Fill As Brush = Brushes.Transparent

    Private _horizontalAlignment As LabelHorizontalAlignment = LabelHorizontalAlignment.Left
    Public Property HorizontalAlignment As LabelHorizontalAlignment
        Get
            Return _horizontalAlignment
        End Get
        Set(ByVal value As LabelHorizontalAlignment)
            _horizontalAlignment = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _verticalAlignment As LabelVerticalAlignment = LabelVerticalAlignment.Center
    Public Property VerticalAlignment As LabelVerticalAlignment
        Get
            Return _verticalAlignment
        End Get
        Set(ByVal value As LabelVerticalAlignment)
            _verticalAlignment = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Public ReadOnly Property ActualWidth As Double
        Get
            Return (New FormattedText(Me.Text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Font.FontFamily, Font.FontStyle, Font.FontWeight, FontStretches.Normal), Font.FontSize, Font.Brush)).WidthIncludingTrailingWhitespace
        End Get
    End Property
    Public ReadOnly Property ActualHeight As Double
        Get
            Return Font.FontSize
        End Get
    End Property
    Public Overrides Sub RefreshVisual()
        If Text IsNot Nothing And HasParent Then
            Dim location As Point
            Dim text As New FormattedText(Me.Text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Font.FontFamily, Font.FontStyle, Font.FontWeight, FontStretches.Normal), Font.FontSize, Font.Brush)
            If UseNegativeCoordinates Then
                location = Parent.GetRealFromRelative(Me.Location)
            Else
                location = Parent.GetRealFromRelative(NegateY(Me.Location))
            End If
            Select Case HorizontalAlignment
                Case LabelHorizontalAlignment.Right
                    location.X -= text.WidthIncludingTrailingWhitespace
                Case LabelHorizontalAlignment.Center
                    location.X -= text.WidthIncludingTrailingWhitespace / 2
            End Select
            Select Case VerticalAlignment
                Case LabelVerticalAlignment.Center
                    location.Y -= text.Height / 2
                Case LabelVerticalAlignment.Bottom
                    location.Y -= text.Height
            End Select
            If DrawVisual Then
                Using dc As DrawingContext = RenderOpen()
                    dc.DrawRectangle(Fill, New Pen(Fill, 1), New Rect(CType(New Point(0, 0) - New Point(0.5, 0.5), Point), New Size(text.WidthIncludingTrailingWhitespace + 2, text.Height + 2)))
                    dc.DrawText(text, New Point(0, 0))
                    If IsSelected Then
                        dc.DrawRectangle(Brushes.Transparent, New Pen(Font.Brush, 0.85), New Rect(CType(New Point(0, 0) - New Point(0.5, 0.5), Point), New Size(text.WidthIncludingTrailingWhitespace + 2, text.Height + 2)))
                    End If
                End Using
            End If
            Dim group As New TransformGroup
            group.Children.Add(Font.Transform)
            group.Children.Add(New TranslateTransform(location.X, location.Y))
            Transform = group
            Effect = Font.Effect
        End If
    End Sub

    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        Dim point As Point = New Point(Round(Location.X, 0), Location.Y)
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
    End Function

    Private mouseOffset As Point
    Public Overrides Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point)
        Dim parentLocation As Point = Parent.PointFromScreen(PointToScreen(location))
        If UseNegativeCoordinates Then
            mouseOffset = parentLocation - Parent.GetRealFromRelative(Me.Location)
        Else
            mouseOffset = parentLocation - Parent.GetRealFromRelative(NegateY(Me.Location))
        End If
        IsSelected = True
        MyBase.ParentMouseLeftButtonDown(e, parentLocation)
    End Sub
    Public Overrides Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As System.Windows.Point)
        Dim parentLocation As Point = Parent.PointFromScreen(PointToScreen(location))
        If IsMouseDown And IsEditable Then
            If UseNegativeCoordinates Then
                Me.Location = Parent.GetRelativeFromReal(CType(parentLocation - mouseOffset, Point))
            Else
                Me.Location = NegateY(Parent.GetRelativeFromReal(CType(parentLocation - mouseOffset, Point)))
            End If
            'Debug.WriteLine(Me.Location.ToString)
            If Not AutoRefresh Then RefreshVisual()
        End If
        MyBase.ParentMouseMove(e, parentLocation)
    End Sub
    Public Overrides Sub ParentMouseRightButtonDown(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonDown(e, location)
        If Parent.mainCanvas.ContextMenu IsNot Nothing Then ctx = Parent.mainCanvas.ContextMenu
        Parent.mainCanvas.ContextMenu = Nothing
        'Parent.maincanvas 
        'IsSelected = True
    End Sub
    Dim ctx As ContextMenu
    Public Overrides Sub ParentMouseRightButtonUp(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)

        If IsEditable Then Dim formatTextWin As New FormatLabelWindow(Me)
        If ctx IsNot Nothing Then Parent.mainCanvas.ContextMenu = ctx
        MyBase.ParentMouseRightButtonUp(e, location)
    End Sub

    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup
    'Public Overrides Property Pen As System.Windows.Media.Pen
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
    Public Overrides Property IsEditable As Boolean = True
    Friend Overrides Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs)
        MyBase.Command_Executed(sender, e)
        If e.Command Is ChartCommands.CreateObjectDuplicate Then
            Dim lbl As New Label
            Dim realPos As Point = Parent.GetRealFromRelative(Location)
            realPos.Y -= 40
            lbl.Location = Parent.GetRelativeFromReal(realPos)
            lbl.Text = Text
            lbl.Font = Font.Clone
            lbl.UseNegativeCoordinates = UseNegativeCoordinates
            lbl.IsEditable = IsEditable
            lbl.IsSelectable = IsSelectable
            Parent.Children.Add(lbl)
        End If
    End Sub

    Public Overrides Function ContainsPoint(ByVal point As System.Windows.Point) As Boolean
        Dim location As Point
        Dim text As New FormattedText(Me.Text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Font.FontFamily, Font.FontStyle, Font.FontWeight, FontStretches.Normal), Font.FontSize, Font.Brush)
        If UseNegativeCoordinates Then
            location = Parent.GetRealFromRelative(Me.Location)
        Else
            location = Parent.GetRealFromRelative(NegateY(Me.Location))
        End If
        Select Case HorizontalAlignment
            Case LabelHorizontalAlignment.Right
                location.X -= text.WidthIncludingTrailingWhitespace
            Case LabelHorizontalAlignment.Center
                location.X -= text.WidthIncludingTrailingWhitespace / 2
        End Select
        Select Case VerticalAlignment
            Case LabelVerticalAlignment.Center
                location.Y -= text.Height / 2
            Case LabelVerticalAlignment.Bottom
                location.Y -= text.Height
        End Select
        Return New Rect(location, New Size(text.WidthIncludingTrailingWhitespace, Font.FontSize)).Contains(point)
    End Function

    Public Sub Deserialize(ByVal serializedString As String) Implements ISerializable.Deserialize
        Try
            Dim v() As String = Split(serializedString, "/")
            If v.GetUpperBound(0) = 7 Then
                Pen = PenFromString(v(0))
                Text = v(1)
                Location = (New PointConverter).ConvertFromString(v(2))
                Font = (New FontConverter).ConvertFromString(v(3))
                HorizontalAlignment = v(4)
                VerticalAlignment = v(5)
                AutoRefresh = v(6)
                If (New Date(CLng(v(7)))).Ticks <> 0 And Parent IsNot Nothing Then Location = New Point(Parent.GetBarNumberFromDate(New Date(CLng(v(7)))), Location.Y)
            End If
        Catch ex As Exception
            Throw New ArgumentException("'Label.Deserialize' got an error parsing the string '" & serializedString & "'.")
        End Try
    End Sub
    Public Function Serialize() As String Implements ISerializable.Serialize
        Return Join({
                    PenToString(Pen),
                    Text,
                    (New PointConverter).ConvertToString(Location),
                    (New FontConverter).ConvertToString(Font),
                    HorizontalAlignment,
                    VerticalAlignment,
                    AutoRefresh,
                    If(Parent IsNot Nothing, Parent.GetDateFromBarNumber(Location.X), (New Date(0))).Ticks
                }, "/")
    End Function
End Class

