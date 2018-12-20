Imports System.Math
Imports System.Windows.Markup
Imports System.ComponentModel
Imports System.Windows.Threading
Imports System.IO
Imports System.Threading
Imports Krs.Ats.IBNet
Imports Krs.Ats
Imports System.Collections.ObjectModel

Public Class Order
    Inherits ChartDrawingVisual
    Implements ISelectable

    Private WithEvents IB As IBDataStream
    Private isExistingOrder As Boolean
    Private existingContract As Contract
    Private chart As Chart
    Public Sub New(ByVal chart As Chart)
        ClickLeniency = 8
        AutoRefresh = True
        IB = chart.IB
        Me.chart = chart
        AddHandler IB.NextValidId, AddressOf IB_NextValidID
        AddHandler IB.OrderStatus, AddressOf IB_OrderStatus
        If UseRandom() Then
            AddHandler chart.RAND.MarketPriceData, AddressOf RandPriceChange
            'AddHandler chart.RAND.HistoricalDataBarRecieved, AddressOf RandPriceChange
        End If
    End Sub
    Public Sub New(ByVal chart As Chart, ByVal id As Integer, ByVal state As OrderState, ByVal contract As Contract)
        Me.New(chart)
        _orderID = id
        _orderStatus = state.Status
        isExistingOrder = True
        existingContract = contract
        'AddHandler IB.OpenOrder,
        '    Sub(sender As Object, e As OpenOrderEventArgs)

        '    End Sub
        'AddHandler IB.OpenOrderEnd,
        '    Sub(sender As Object, e As EventArgs)

        '    End Sub
        'IB.RequestAllOpenOrders()
    End Sub
    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup

    Public Property IsSelectable As Boolean = True Implements ISelectable.IsSelectable
    Private _isSelected As Boolean
    Public Property IsSelected As Boolean Implements ISelectable.IsSelected
        Get
            Return _isSelected
        End Get
        Set(ByVal value As Boolean)
            If value And HasParent Then Parent.DeselectAllChildren()
            _isSelected = value
            RefreshVisual()
        End Set
    End Property

    Private _price As Double
    Public Property Price As Double
        Get
            Return If(HasParent, RoundTo(_price, Parent.GetMinTick), _price)
        End Get
        Set(ByVal value As Double)
            _price = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _orderAction As ActionSide = ActionSide.Buy
    Public Property OrderAction As ActionSide
        Get
            Return _orderAction
        End Get
        Set(ByVal value As ActionSide)
            _orderAction = value
            Resend()
        End Set
    End Property
    Private _orderType As OrderType = Krs.Ats.IBNet.OrderType.Limit
    Public Property OrderType As OrderType
        Get
            Return _orderType
        End Get
        Set(ByVal value As OrderType)
            _orderType = value
            Resend()
        End Set
    End Property
    Private _quantity As Integer = 1
    Public Property Quantity As Integer
        Get
            Return _quantity
        End Get
        Set(ByVal value As Integer)
            _quantity = value
            Resend()
        End Set
    End Property
    Private _orderStatus As OrderStatus = Krs.Ats.IBNet.OrderStatus.Inactive
    Public ReadOnly Property OrderStatus As OrderStatus
        Get
            Return _orderStatus
        End Get
    End Property
    Private _orderID As Integer
    Public ReadOnly Property OrderID As Integer
        Get
            Return _orderID
        End Get
    End Property
    Public Sub SetFillBar(bar As Integer)
        _fillBar = bar
        RefreshVisual()
    End Sub
    Private _fillBar As Integer
    Public ReadOnly Property FillBar As Integer
        Get
            Return _fillBar
        End Get
    End Property
    Private _fillPrice As Double
    Public ReadOnly Property FillPrice As Double
        Get
            Return _fillPrice
        End Get
    End Property
    Private _fillDate As Date
    Public ReadOnly Property FillDate As Date
        Get
            Return _fillDate
        End Get
    End Property
    Private _openShares As Integer
    Public ReadOnly Property OpenShares As Integer
        Get
            Return _openShares
        End Get
    End Property
    Private _exitedShares As Integer
    Public ReadOnly Property ExitedShares As Integer
        Get
            Return _exitedShares
        End Get
    End Property


    Private Function UseRandom() As Boolean
        Return chart.Settings("UseRandom").Value
    End Function
    Private Sub RandPriceChange(ByVal tickType As TickType, ByVal price As Double, ByVal id As Integer)
        If tickType = IBNet.TickType.LastPrice And OrderStatus = IBNet.OrderStatus.Submitted And IsUpdateNeeded = False Then
            Dim fill As Boolean
            If OrderAction = ActionSide.Buy Then
                If OrderType = IBNet.OrderType.Limit Then
                    If Me.Price >= price Then fill = True
                ElseIf OrderType = IBNet.OrderType.Stop Then
                    If Me.Price = price Then fill = True
                End If
            Else
                If OrderType = IBNet.OrderType.Limit Then
                    If Me.Price <= price Then fill = True
                ElseIf OrderType = IBNet.OrderType.Stop Then
                    If Me.Price = price Then fill = True
                End If
            End If
            If fill Then IB_OrderStatus(Me, New OrderStatusEventArgs(OrderID, Krs.Ats.IBNet.OrderStatus.Filled, Quantity, 0, chart.Price, 0, 0, chart.Price, 0, ""))
        End If
    End Sub

    Private _transmittedPriceBeforeMove As Double
    Public ReadOnly Property TransmittedPriceBeforeMove As Double
        Get
            Return _transmittedPriceBeforeMove
        End Get
    End Property
    Private _isUpdateNeeded As Boolean
    Public ReadOnly Property IsUpdateNeeded As Boolean
        Get
            Return _isUpdateNeeded
        End Get
    End Property
    Private _transmitUpdateButtonVisible As Boolean = True
    Public ReadOnly Property TransmitUpdateButtonVisible As Boolean
        Get
            Return _transmitUpdateButtonVisible
        End Get
    End Property

    Public Property Color As Color = Colors.DarkGreen
    Public Property SelectedColor As Color = Colors.Green
    Public Property ButtonHoverColor As Color = Colors.LightGreen

    Public Sub ChooseQuantity()
        If OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled Then
            'Dim formatWin As New FormatOrderWindow(Me)
            ShowQuantityMenu(Quantity, Parent.Settings("DefaultOrderQuantity").Value,
                Sub(newQuantity As Integer)
                    _quantity = newQuantity
                    RefreshVisual()
                    Resend()
                End Sub)
            Parent.Parent.ContextMenu = Nothing
            'If formatWin.ButtonOKPressed Then
            '    RefreshVisual()
            '    Resend()
            'End If
        End If
    End Sub
    Public Sub Cancel()
        If OrderType <> IBNet.OrderType.Market Then
            If OrderStatus = Krs.Ats.IBNet.OrderStatus.Submitted Or OrderStatus = Krs.Ats.IBNet.OrderStatus.PreSubmitted Then
                _orderStatus = Krs.Ats.IBNet.OrderStatus.ApiCancelled
                If UseRandom() Then
                    IB_OrderStatus(Me, New OrderStatusEventArgs(OrderID, Krs.Ats.IBNet.OrderStatus.Canceled, 0, 0, 0, 0, 0, 0, 0, ""))
                Else
                    IB_OrderStatus(Me, New OrderStatusEventArgs(OrderID, Krs.Ats.IBNet.OrderStatus.ApiCancelled, 0, 0, 0, 0, 0, 0, 0, ""))
                    IB.CancelOrder(OrderID)
                End If
            ElseIf OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled Then
                IsSelected = False
                Parent.RefreshOrderBox()
                Parent.Children.Remove(Me)
            End If
        End If
    End Sub
    Public Sub Transmit()
        'MsgBox("transmit")
        If OrderStatus <> Krs.Ats.IBNet.OrderStatus.Submitted And OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled Then
            getNewID = True
            If UseRandom() Then
                Send()
            Else
                IB.RequestIds(0)
            End If
        End If
    End Sub
    Private Sub Filled()
        _transmittedPriceBeforeMove = 0
        _exitedShares = Quantity
        IsSelected = False
        RefreshVisual()
    End Sub
    Private Sub Send()
        _transmittedPriceBeforeMove = Price
        Dim order As New IBNet.Order
        order.Action = OrderAction
        order.TotalQuantity = Quantity
        order.OrderType = OrderType
        order.LimitPrice = Price
        order.AuxPrice = Price
        order.OutsideRth = True
        order.Tif = TimeInForce.GoodTillCancel
        If UseRandom() Then
            If OrderType = IBNet.OrderType.Market Then
                IB_OrderStatus(Me, New OrderStatusEventArgs(OrderID, Krs.Ats.IBNet.OrderStatus.Filled, Quantity, 0, chart.Price, 0, 0, chart.Price, 0, ""))
            Else
                IB_OrderStatus(Me, New OrderStatusEventArgs(OrderID, Krs.Ats.IBNet.OrderStatus.Submitted, 0, 0, 0, 0, 0, 0, 0, ""))
            End If
            RandPriceChange(TickType.LastPrice, chart.CurrentBar.Close, 0)
        Else
            IB_OrderStatus(Me, New OrderStatusEventArgs(OrderID, Krs.Ats.IBNet.OrderStatus.ApiPending, 0, 0, 0, 0, 0, 0, 0, ""))
            IB.PlaceOrder(OrderID, If(isExistingOrder, existingContract, IB.Contract(chart.TickerID)), order)
        End If
        _isUpdateNeeded = False
    End Sub
    Public Sub Resend()
        If HasParent And OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled And OrderStatus <> Krs.Ats.IBNet.OrderStatus.Inactive Then
            Send()
        End If
    End Sub

    Private getNewID As Boolean
    Private Sub IB_OrderStatus(ByVal sender As Object, ByVal e As Krs.Ats.IBNet.OrderStatusEventArgs)
        If OrderID = e.OrderId And Parent IsNot Nothing Then
            Dispatcher.BeginInvoke(
                Sub()
                    'Log("Order " & e.OrderId & " is " & Spacify([Enum].GetName(GetType(OrderStatus), e.Status).ToLower) & ". Order #" & OrderID & ".")
                    _orderStatus = e.Status
                    'Cursor = Cursors.SizeNS
                    Select Case OrderStatus
                        Case Krs.Ats.IBNet.OrderStatus.Filled
                            _fillBar = Parent.BarIndex
                            _fillPrice = e.LastFillPrice
                            _fillDate = Now
                            Filled()
                        Case Krs.Ats.IBNet.OrderStatus.Canceled
                            IsSelected = False
                            Parent.RefreshOrderBox()
                            Parent.Children.Remove(Me)
                    End Select
                    RefreshVisual()
                End Sub)
        End If
    End Sub
    Private Sub IB_NextValidID(ByVal sender As Object, ByVal e As NextValidIdEventArgs)
        If getNewID Then
            _orderID = e.OrderId
            getNewID = False
            Send()
        End If
    End Sub

    Private TextSpacing As Double = 3
    Private FontSize As Double = 12

    Private OrderTypeTextHighlightGeometry As Geometry
    Private TransmitTextHighlightGeometry As Geometry
    Private CancelTextHighlightGeometry As Geometry
    Private QuantityTextHighlightGeometry As Geometry

    Private OrderTypeText As FormattedText
    Private TransmitText As FormattedText
    Private CancelText As FormattedText
    Private QuantityText As FormattedText
    Private OrderTypeTextLocation As Point
    Private TransmitTextLocation As Point
    Private CancelTextLocation As Point
    Private QuantityTextLocation As Point

    Private LineGeometry As Geometry

    Private Sub RefreshLocationsAndGeometries()
        Dim color As Color
        If IsSelected Then color = SelectedColor Else color = Me.Color
        Dim orderTypeColor = color, transmitColor = color, cancelColor = color, quantityColor = color

        Select Case buttonMouseOverID
            Case 1
                transmitColor = ButtonHoverColor
            Case 2
                cancelColor = ButtonHoverColor
            Case 3
                quantityColor = ButtonHoverColor
        End Select
        Dim orderActionText As String = [Enum].GetName(GetType(ActionSide), OrderAction)
        If OrderAction = ActionSide.Buy Then
            orderActionText = "B"
        ElseIf OrderAction = ActionSide.Sell Then
            orderActionText = "S"
        End If
        OrderTypeText = New FormattedText(" " & orderActionText & "  " & [Enum].GetName(GetType(OrderType), OrderType) & " ", Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Arial"), FontSize, New SolidColorBrush(GetForegroundColor(orderTypeColor)))
        TransmitText = New FormattedText(If(IsUpdateNeeded, " U ", " T "), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Arial"), FontSize, New SolidColorBrush(GetForegroundColor(transmitColor)))
        CancelText = New FormattedText(" C ", Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Arial"), FontSize, New SolidColorBrush(GetForegroundColor(cancelColor)))
        QuantityText = New FormattedText(" " & Quantity & " ", Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Arial"), FontSize, New SolidColorBrush(GetForegroundColor(quantityColor)))
        Dim textYLocation As Double = Parent.GetRealFromRelativeY(-Price) - FontSize / 2
        QuantityTextLocation = New Point(Parent.GetRealFromRelativeX(Parent.BarIndex) - 70, textYLocation)
        'QuantityTextLocation = New Point(ParentPriceBarLocation() - 5 - QuantityText.WidthIncludingTrailingWhitespace, textYLocation)
        CancelTextLocation = New Point(QuantityTextLocation.X - TextSpacing - CancelText.WidthIncludingTrailingWhitespace, textYLocation)
        TransmitTextLocation = New Point(CancelTextLocation.X - TextSpacing - TransmitText.WidthIncludingTrailingWhitespace, textYLocation)
        OrderTypeTextLocation = New Point(TransmitTextLocation.X - TextSpacing - OrderTypeText.WidthIncludingTrailingWhitespace, textYLocation)

        OrderTypeTextHighlightGeometry = OrderTypeText.BuildHighlightGeometry(OrderTypeTextLocation)
        TransmitTextHighlightGeometry = TransmitText.BuildHighlightGeometry(TransmitTextLocation)
        CancelTextHighlightGeometry = CancelText.BuildHighlightGeometry(CancelTextLocation)
        QuantityTextHighlightGeometry = QuantityText.BuildHighlightGeometry(QuantityTextLocation)

        LineGeometry = New LineGeometry(New Point(Max(QuantityTextLocation.X + TextSpacing + QuantityText.WidthIncludingTrailingWhitespace, Parent.GetRealFromRelativeX(Parent.BarIndex)), textYLocation + FontSize / 2),
                                        New Point(QuantityTextLocation.X + TextSpacing + QuantityText.WidthIncludingTrailingWhitespace, textYLocation + FontSize / 2))
    End Sub
    Private Function ParentPriceBarLocation() As Double
        Return Parent.ActualWidth - Parent.Settings("PriceBarWidth").Value
    End Function
    Private Function GetOrderStatusIsSubmitted() As Boolean
        Return OrderStatus = Krs.Ats.IBNet.OrderStatus.PendingSubmit Or OrderStatus = Krs.Ats.IBNet.OrderStatus.Submitted Or OrderStatus = Krs.Ats.IBNet.OrderStatus.PreSubmitted
    End Function
    Private Function GetArrowGeometry(ByVal flipped As Boolean, ByVal location As Point) As Geometry
        Dim geometry As New GeometryGroup
        Dim realLocation As Point = Parent.GetRealFromRelative(New Point(location.X, -location.Y))
        If flipped Then
            geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X + 12, realLocation.Y)))
            geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X + 12 / 2, realLocation.Y - 4 / 2)))
            geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X + 12 / 2, realLocation.Y + 4 / 2)))
        Else
            geometry.Children.Add(New LineGeometry(New Point(realLocation.X - 12, realLocation.Y), realLocation))
            geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X - 12 / 2, realLocation.Y - 4 / 2)))
            geometry.Children.Add(New LineGeometry(realLocation, New Point(realLocation.X - 12 / 2, realLocation.Y + 4 / 2)))
        End If
        Return geometry
    End Function
    Private Function GetText(ByVal text As String, ByVal brush As Brush) As FormattedText
        Dim fText As New FormattedText(text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Arial"), FontSize, brush)
        Return fText
    End Function
    Private Function GetOriginalPriceLineGeometry() As Geometry
        If TransmittedPriceBeforeMove <> 0 Then
            Return New LineGeometry(Parent.GetRealFromRelative(New Point(Parent.BarIndex, -TransmittedPriceBeforeMove)), New Point(ParentPriceBarLocation, Parent.GetRealFromRelativeY(-TransmittedPriceBeforeMove)))
        Else
            Return Nothing
        End If
    End Function
    Public Overrides Sub RefreshVisual()
        If HasParent Then
            RefreshLocationsAndGeometries()
            Using dc As DrawingContext = RenderOpen()

                Dim color As Color : If IsSelected Then color = SelectedColor Else color = Me.Color
                Dim orderTypeColor = color, transmitColor = color, cancelColor = color, quantityColor = color

                Select Case buttonMouseOverID
                    Case 1
                        transmitColor = ButtonHoverColor
                    Case 2
                        cancelColor = ButtonHoverColor
                    Case 3
                        quantityColor = ButtonHoverColor
                End Select

                If IsUpdateNeeded And TransmitUpdateButtonVisible Then dc.DrawGeometry(New SolidColorBrush(Me.Color), New Pen(New SolidColorBrush(Me.Color), 2), GetOriginalPriceLineGeometry)

                If OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled And
                    OrderStatus <> Krs.Ats.IBNet.OrderStatus.ApiPending And
                    OrderStatus <> Krs.Ats.IBNet.OrderStatus.ApiCancelled And (
                    Not GetOrderStatusIsSubmitted() Or (GetOrderStatusIsSubmitted() And IsUpdateNeeded)) Then
                    _transmitUpdateButtonVisible = True
                    dc.DrawGeometry(New SolidColorBrush(transmitColor), New Pen(New SolidColorBrush(transmitColor), 1), TransmitTextHighlightGeometry)
                    dc.DrawText(TransmitText, TransmitTextLocation)
                Else
                    _transmitUpdateButtonVisible = False
                End If

                If OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled And
                    OrderStatus <> Krs.Ats.IBNet.OrderStatus.ApiCancelled Then
                    dc.DrawGeometry(New SolidColorBrush(cancelColor), New Pen(New SolidColorBrush(cancelColor), 1), CancelTextHighlightGeometry)
                    dc.DrawText(CancelText, CancelTextLocation)
                End If

                If OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled Then
                    dc.DrawGeometry(New SolidColorBrush(quantityColor), New Pen(New SolidColorBrush(quantityColor), 1), QuantityTextHighlightGeometry)
                    dc.DrawText(QuantityText, QuantityTextLocation)

                    dc.DrawGeometry(New SolidColorBrush(orderTypeColor), New Pen(New SolidColorBrush(orderTypeColor), 1), OrderTypeTextHighlightGeometry)
                    dc.DrawText(OrderTypeText, OrderTypeTextLocation)

                    dc.DrawGeometry(New SolidColorBrush(color), New Pen(New SolidColorBrush(color), 1), LineGeometry)
                End If

                If OrderStatus = Krs.Ats.IBNet.OrderStatus.Filled Then
                    Dim brush As Brush
                    If IsSelected Then
                        brush = New SolidColorBrush(ButtonHoverColor)
                    Else
                        brush = New SolidColorBrush(SelectedColor)
                    End If
                    If ExitedShares <> 0 Then
                        dc.DrawGeometry(brush, New Pen(brush, 2), GetArrowGeometry(False, New Point(FillBar, FillPrice)))
                        Dim text As FormattedText = GetText(ExitedShares, brush)
                        Dim point As Point = Parent.GetRealFromRelative(New Point(FillBar, -FillPrice))
                        point.X -= 13 + text.WidthIncludingTrailingWhitespace
                        point.Y -= FontSize / 2
                        dc.DrawText(text, point)
                    End If
                    If OpenShares <> 0 Then
                        dc.DrawGeometry(brush, New Pen(brush, 2), GetArrowGeometry(True, New Point(FillBar, FillPrice)))
                        Dim text As FormattedText = GetText(OpenShares, brush)
                        Dim point As Point = Parent.GetRealFromRelative(New Point(FillBar, -FillPrice))
                        point.X += 13
                        point.Y -= FontSize / 2
                        dc.DrawText(text, point)
                    End If
                End If

            End Using
            Parent.RefreshOrderBox()
        End If
    End Sub
    Private mouseOffset As Double
    Private mousePriceChanged As Boolean
    Private buttonMouseOverID As Integer = -1
    Public Overrides Sub ParentMouseMove(ByVal e As System.Windows.Input.MouseEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseMove(e, location)
        RefreshLocationsAndGeometries()
        Dim prevID As Integer = buttonMouseOverID
        Select Case True
            Case OrderTypeTextHighlightGeometry.FillContains(location)
                buttonMouseOverID = 0
            Case TransmitTextHighlightGeometry.FillContains(location)
                buttonMouseOverID = 1
            Case CancelTextHighlightGeometry.FillContains(location)
                buttonMouseOverID = 2
            Case QuantityTextHighlightGeometry.FillContains(location)
                buttonMouseOverID = 3
            Case Else
                buttonMouseOverID = -1
        End Select
        If IsMouseDown AndAlso Price <> RoundTo(-Parent.GetRelativeFromRealY(location.Y), Parent.GetMinTick) AndAlso OrderStatus <> Krs.Ats.IBNet.OrderStatus.Filled Then
            mousePriceChanged = True
            Price = RoundTo(-Parent.GetRelativeFromRealY(location.Y), Parent.GetMinTick)
        End If
        If prevID <> buttonMouseOverID Then RefreshVisual()
    End Sub
    Public Overrides Sub ParentMouseLeftButtonDown(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonDown(e, location)
        If buttonMouseOverID <= 0 And OrderStatus <> IBNet.OrderStatus.Filled Then Cursor = Cursors.SizeNS
        If IsSelectable Then IsSelected = True
        mousePriceChanged = False
    End Sub
    Public Overrides Sub ParentMouseLeftButtonUp(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonUp(e, location)
        RefreshLocationsAndGeometries()
        Parent.ResetCursor()
        Select Case True
            Case OrderTypeTextHighlightGeometry.FillContains(location)
                'MsgBox("order type")
            Case TransmitTextHighlightGeometry.FillContains(location)
                If IsUpdateNeeded Then
                    Resend()
                Else
                    Transmit()
                End If
            Case CancelTextHighlightGeometry.FillContains(location)
                Cancel()
            Case QuantityTextHighlightGeometry.FillContains(location)
                ChooseQuantity()
        End Select
        If mousePriceChanged And (OrderStatus = Krs.Ats.IBNet.OrderStatus.Submitted Or OrderStatus = Krs.Ats.IBNet.OrderStatus.PreSubmitted) Then
            Resend()
            '_isUpdateNeeded = True
        End If
        RefreshVisual()
    End Sub
    Public Overrides Sub ParentMouseWheel(ByVal e As System.Windows.Input.MouseWheelEventArgs)
        If IsSelected And (GetOrderStatusIsSubmitted() Or OrderStatus = IBNet.OrderStatus.Inactive) Then
            If GetOrderStatusIsSubmitted() Then _isUpdateNeeded = True
            Price += Parent.GetMinTick * Sign(e.Delta)
            RefreshVisual()
        End If
        MyBase.ParentMouseWheel(e)
    End Sub

    Friend Overrides Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs)
        If e.Command Is ChartCommands.RemoveObject Then
            If OrderStatus = IBNet.OrderStatus.Filled Then
                Parent.Children.Remove(Me)
            Else
                Cancel()
            End If
        ElseIf e.Command Is ChartCommands.RemoveAllObjectsOfSelectedType Then
            Dim parent As Chart = Me.Parent
            Dim i As Integer
            While i < parent.Children.Count
                If TypeOf Children(i) Is Order AndAlso CType(Children(i), Order).OrderStatus = IBNet.OrderStatus.Filled Then
                    parent.Children.RemoveAt(i)
                Else
                    i += 1
                End If
            End While
        ElseIf e.Command Is ChartCommands.MoveOrderUp Then
            If OrderStatus = IBNet.OrderStatus.Submitted Or OrderStatus = IBNet.OrderStatus.Inactive Then
                If OrderStatus = IBNet.OrderStatus.Submitted Then _isUpdateNeeded = True
                Price += Parent.GetMinTick
                RefreshVisual()
            End If
        ElseIf e.Command Is ChartCommands.MoveOrderDown Then
            If OrderStatus = IBNet.OrderStatus.Submitted Or OrderStatus = IBNet.OrderStatus.Inactive Then
                If OrderStatus = IBNet.OrderStatus.Submitted Then _isUpdateNeeded = True
                Price -= Parent.GetMinTick
                RefreshVisual()
            End If
        End If
    End Sub

    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        Dim items As New List(Of KeyValuePair(Of String, String))
        items.Add(New KeyValuePair(Of String, String)([Enum].GetName(GetType(ActionSide), OrderAction) & " " & [Enum].GetName(GetType(OrderType), OrderType) & " (" & [Enum].GetName(GetType(OrderStatus), OrderStatus) & ")", ""))
        items.Add(New KeyValuePair(Of String, String)("-------------------", ""))
        items.Add(New KeyValuePair(Of String, String)("Quantity: ", Quantity))
        If OrderStatus = IBNet.OrderStatus.Filled Then
            items.Add(New KeyValuePair(Of String, String)("Fill Price: ", FormatNumber(Price, chart.Settings("DecimalPlaces").Value).Replace(",", "")))
            items.Add(New KeyValuePair(Of String, String)("Fill Date: ", FillDate.ToShortDateString))
            items.Add(New KeyValuePair(Of String, String)("Fill Time: ", FillDate.ToShortTimeString))
            items.Add(New KeyValuePair(Of String, String)("Fill Bar: ", FillBar))
            IsPopupMoveable = False
        Else
            IsPopupMoveable = True
        End If
        Return items
    End Function

    Public Overrides Function ContainsPoint(ByVal point As System.Windows.Point) As Boolean
        RefreshLocationsAndGeometries()
        Dim geometryGroup As New GeometryGroup
        If OrderStatus <> IBNet.OrderStatus.Filled Then
            geometryGroup.Children.Add(OrderTypeTextHighlightGeometry)
            geometryGroup.Children.Add(OrderTypeTextHighlightGeometry)
            geometryGroup.Children.Add(TransmitTextHighlightGeometry)
            geometryGroup.Children.Add(CancelTextHighlightGeometry)
            geometryGroup.Children.Add(QuantityTextHighlightGeometry)
            geometryGroup.Children.Add(LineGeometry)
        Else
            geometryGroup.Children.Add(GetArrowGeometry(False, New Point(FillBar, FillPrice)))
        End If
        Return geometryGroup.FillContains(point, 5, ToleranceType.Absolute)
    End Function
End Class



