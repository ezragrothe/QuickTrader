Imports System.Math
Imports System.Windows.Markup
Imports System.ComponentModel
Imports System.Windows.Threading
Imports System.IO
Imports System.Threading
Imports IBApi
Imports System.Collections.ObjectModel


Public Class Order
    Private WithEvents IB As IBDataStream
    Private isExistingOrder As Boolean
    Private existingContract As Contract
    Private chart As Chart
    Public Sub New(ByVal chart As Chart, price As Decimal)
        IB = chart.IB
        _price = price
        Me.chart = chart
        AddHandler IB.EventSources.NextValidId, AddressOf IB_NextValidID
        AddHandler IB.EventSources.OrderStatus, AddressOf IB_OrderStatus
        If UseRandom() Then
            AddHandler chart.RAND.MarketPriceData, AddressOf RandPriceChange
            'AddHandler chart.RAND.HistoricalDataBarRecieved, AddressOf RandPriceChange
        End If
    End Sub
    Public Sub New(ByVal chart As Chart, price As Decimal, ByVal id As Integer, ByVal state As OrderState, ByVal contract As Contract)
        Me.New(chart, price)
        _orderID = id
        _orderStatus = ParseOrderStatus(state.Status)
        isExistingOrder = True
        existingContract = contract
    End Sub

    Private _price As Double
    Public Property Price As Double
        Get
            Return If(chart IsNot Nothing, RoundTo(_price, chart.GetMinTick), _price)
        End Get
        Set(ByVal value As Double)
            'If value <> _price Then _isUpdateNeeded = True
            _price = value
            Resend()
        End Set
    End Property
    Private _orderAction As ActionSide = ActionSide.BUY
    Public Property OrderAction As ActionSide
        Get
            Return _orderAction
        End Get
        Set(ByVal value As ActionSide)
            _orderAction = value
            Resend()
        End Set
    End Property
    Private _orderType As OrderType = OrderType.LMT
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
    Private _isOCA As Double
    Public Property IsOCA As Double
        Get
            Return _isOCA
        End Get
        Set(ByVal value As Double)
            _isOCA = value
            Resend()
        End Set
    End Property
    Private _orderStatus As OrderStatus = OrderStatus.Inactive
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
    Private Sub RandPriceChange(ByVal tickType As TickType, bar As BarData, ByVal id As Integer)
        If tickType = TickType.LastPrice And OrderStatus = OrderStatus.Submitted And IsUpdateNeeded = False Then
            Dim fill As Boolean
            If OrderAction = ActionSide.BUY Then
                If OrderType = OrderType.LMT Then
                    If Me.Price >= bar.Close Then fill = True
                ElseIf OrderType = OrderType.STP Then
                    If Me.Price = bar.Close Then fill = True
                End If
            Else
                If OrderType = OrderType.LMT Then
                    If Me.Price <= bar.Close Then fill = True
                ElseIf OrderType = OrderType.STP Then
                    If Me.Price = bar.Close Then fill = True
                End If
            End If
            If fill Then IB_OrderStatus(Me, New OrderStatusEventArgs With {.orderId = OrderID, .status = "Filled", .filled = Quantity, .avgFillPrice = chart.Price, .lastFillPrice = chart.Price})
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
    Public Sub TransmitOrder()
        If IsUpdateNeeded Then
            Resend()
        Else
            Transmit()
        End If
    End Sub
    Private Sub Cancel()
        If OrderType <> OrderType.MKT Then
            If OrderStatus = OrderStatus.Submitted Or OrderStatus = OrderStatus.PreSubmitted Then
                _orderStatus = OrderStatus.ApiCancelled
                If UseRandom() Then
                    IB_OrderStatus(Me, New OrderStatusEventArgs With {.orderId = OrderID, .status = "Canceled"})
                Else
                    IB_OrderStatus(Me, New OrderStatusEventArgs With {.orderId = OrderID, .status = "ApiCancelled"})
                    IB.cancelOrder(OrderID)
                End If
            End If
        End If
    End Sub
    Private Sub Transmit()
        'MsgBox("transmit")
        If OrderStatus <> OrderStatus.Submitted And OrderStatus <> OrderStatus.Filled Then
            getNewID = True
            If UseRandom() Then
                Send()
            Else
                IB.reqIds(1)
            End If
        End If
    End Sub
    Private Sub Filled()
        _transmittedPriceBeforeMove = 0
        _exitedShares = Quantity
        'If chart IsNot Nothing Then chart.CurrentQuantity = 1
    End Sub
    Private Sub Send()
        _transmittedPriceBeforeMove = Price
        Dim order As New IBApi.Order
        order.Action = ParseOrderAction(OrderAction)
        order.TotalQuantity = Quantity
        order.OrderType = ParseOrderType(OrderType)
        order.LmtPrice = Price
        order.AuxPrice = Price
        order.OutsideRth = True
        order.Tif = ParseTimeInForce(TimeInForce.GTC)
        order.OcaType = If(IsOCA, OcaType.CancelAll, OcaType.Undefined)

        If UseRandom() Then
            If OrderType = OrderType.MKT Then
                IB_OrderStatus(Me, New OrderStatusEventArgs With {.orderId = OrderID, .status = "Filled", .filled = Quantity, .remaining = 0, .lastFillPrice = chart.Price, .avgFillPrice = chart.Price})
            Else
                IB_OrderStatus(Me, New OrderStatusEventArgs With {.orderId = OrderID, .status = "Submitted"})
            End If
            RandPriceChange(TickType.LastPrice, chart.CurrentBar, 0)
        Else
            IB_OrderStatus(Me, New OrderStatusEventArgs With {.orderId = OrderID, .status = "ApiPending"})
            IB.placeOrder(OrderID, If(isExistingOrder, existingContract, IB.Contract(chart.TickerID)), order)
        End If
        _isUpdateNeeded = False
        'If chart IsNot Nothing Then chart.CurrentQuantity = 1
    End Sub
    Public Sub Resend()
        If chart IsNot Nothing And OrderStatus <> OrderStatus.Filled And OrderStatus <> OrderStatus.Inactive Then
            Send()
        End If
    End Sub

    Private getNewID As Boolean
    Private Sub IB_OrderStatus(ByVal sender As Object, ByVal e As OrderStatusEventArgs)
        If OrderID = e.orderId And chart IsNot Nothing Then
            'Log("Order " & e.OrderId & " is " & Spacify([Enum].GetName(GetType(OrderStatus), e.Status).ToLower) & ". Order #" & OrderID & ".")
            _orderStatus = ParseOrderStatus(e.status)
            'Cursor = Cursors.SizeNS
            Select Case OrderStatus
                Case OrderStatus.Filled
                    _fillBar = chart.BarIndex
                    _fillPrice = e.lastFillPrice
                    _fillDate = Now
                    Filled()
            End Select
        End If
    End Sub
    Private Sub IB_NextValidID(ByVal sender As Object, ByVal e As NextValidIdEventArgs)
        If getNewID Then
            _orderID = e.Id
            getNewID = False
            Send()
        End If
    End Sub

    Private Function GetOrderStatusIsSubmitted() As Boolean
        Return OrderStatus = OrderStatus.PendingSubmit Or OrderStatus = OrderStatus.Submitted Or OrderStatus = OrderStatus.PreSubmitted
    End Function
End Class
