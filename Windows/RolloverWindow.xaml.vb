Imports System.Windows.Media.Animation
Imports System.Collections.ObjectModel

Public Class RolloverWindow
    Private chart As Chart
    Public Sub New(chart As Chart)
        InitializeComponent()
        Me.chart = chart
    End Sub
    Private _buttonOKPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _buttonOKPressed
        End Get
    End Property
    Private id As Integer = 100000
    Private marketDataOnce As Boolean
    Private details As IBApi.ContractDetails
    Private Sub btnOK_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        If lstExpiries.SelectedItem IsNot Nothing Then
            Dim contractDetails As IBApi.ContractDetails = lstExpiries.SelectedItem.Tag
            Dim c As IBApi.Contract = contractDetails.Contract
            chart.IB.Contract(c.ConId) = c
            marketDataOnce = True
            details = contractDetails
            id = c.ConId
            chart.IB.RequestMarketData(c.ConId)
            AddHandler chart.IB.MarketPriceData, AddressOf MarketData
            AddHandler chart.IB.Error,
                Sub(s As Object, a As QuickTrader.ErrMsgEventArgs)
                    If a.id = Me.id Then
                        ShowInfoBox(a.errorMsg, Me)
                    End If
                End Sub
            Cursor = Cursors.Wait
        End If
    End Sub
    Private Sub MarketData(tickType As TickType, bar As BarData, tickerID As Integer)
        If tickerID = Me.id AndAlso tickType = TickType.LastPrice And marketDataOnce Then
            Dispatcher.Invoke(Sub(p As Decimal) Rollover(p), bar.Close)
            marketDataOnce = False
        End If
        chart.IB.CancelMarketData(tickerID)
    End Sub
    Private Sub Rollover(price As Decimal)
        Dim priceDif As Decimal = price - chart.Price
        Dim converter As New BarDataConverter
        Dim originalCacheFileName As String = chart.IB.GetCacheFileName(chart.TickerID)
        'Dim originalIndexFileName As String = chart.IB.GetIndexFileName(chart.TickerID)
        Dim c As IBApi.Contract = details.Contract
        chart.IB.Contract(chart.TickerID) = c
        chart.IB.MinTick(chart.TickerID) = details.MinTick
        Dim newCacheFileName As String = chart.IB.GetCacheFileName(chart.TickerID)
        'Dim newIndexFileName As String = chart.IB.GetIndexFileName(chart.TickerID)
        File.WriteAllText(newCacheFileName, "")
        Using writer As New StreamWriter(newCacheFileName)
            Using reader As New StreamReader(originalCacheFileName)
                While reader.Peek <> -1
                    Dim line As String = reader.ReadLine
                    If line <> "" Then
                        Dim barData As BarData = converter.ConvertFromString(line)
                        Dim newBarData As New BarData(barData.Open + priceDif, barData.High + priceDif, barData.Low + priceDif, barData.Close + priceDif, barData.Date, barData.Duration)
                        writer.WriteLine(converter.ConvertToString(newBarData))
                    End If
                End While
            End Using
        End Using
        'File.WriteAllText(newIndexFileName, File.ReadAllText(originalIndexFileName))
        Cursor = Cursors.Arrow
        _buttonOKPressed = True
        Close()
    End Sub
    Private Sub btnCancel_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub BaseWindow_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs)
        btnOK.IsEnabled = False
        lstExpiries.IsEnabled = False
        btnRefresh_Click(Nothing, Nothing)
    End Sub
    Private Sub IB_ContractDetailsEnd(ByVal sender As Object, ByVal e As ContractDetailsEndEventArgs)
        Dispatcher.Invoke(Sub() Cursor = Cursors.Arrow)
    End Sub
    Private Sub IB_ContractDetails(ByVal sender As Object, ByVal e As ContractDetailsEventArgs)
        If e.reqId >= 10000 Then
            Dispatcher.Invoke(Sub() ProcessSymbol(e.contractDetails))
        End If
    End Sub
    Private Sub ProcessSymbol(ByVal contractDetails As IBApi.ContractDetails)
        Cursor = Cursors.Wait
        If ParseSecType(contractDetails.Contract.SecType) = SecurityType.FUT And chart.IB.Contract(chart.TickerID).Exchange = contractDetails.Contract.Exchange Then
            'Dim o = chart.IB.Contract(chart.TickerID)
            Dim listBoxItem As New ListBoxItem
            listBoxItem.Content = GetContractMonthFormattedString(contractDetails.Contract.LastTradeDateOrContractMonth)
            listBoxItem.Tag = contractDetails
            AddHandler listBoxItem.MouseDoubleClick,
                Sub(sender As Object, e As EventArgs)
                    Dim win As New SymbolInfoWindow(sender.Tag)
                    win.Owner = Me
                    win.Show()
                End Sub
            Dim insertionPoint As Integer = 0
            For i = 0 To lstExpiries.Items.Count - 1
                If GetContractMonthAsDate(contractDetails.Contract.LastTradeDateOrContractMonth) > GetContractMonthAsDate(CType(lstExpiries.Items(i).Tag, IBApi.ContractDetails).Contract.LastTradeDateOrContractMonth) Then
                    insertionPoint = i + 1
                End If
            Next
            lstExpiries.Items.Insert(insertionPoint, listBoxItem)
            btnOK.IsEnabled = True
            lstExpiries.IsEnabled = True
        End If
    End Sub



    Private Sub btnRefresh_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        lstExpiries.Items.Clear()
        txtHeader.Text = "Expiry dates for " & chart.IB.Contract(chart.TickerID).Symbol
        RemoveHandler chart.IB.EventSources.ContractDetails, AddressOf IB_ContractDetails
        AddHandler chart.IB.EventSources.ContractDetails, AddressOf IB_ContractDetails
        RemoveHandler chart.IB.EventSources.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        AddHandler chart.IB.EventSources.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        chart.IB.reqContractDetails(10000, New IBApi.Contract With {.Symbol = chart.IB.Contract(chart.TickerID).Symbol, .SecType = "Future"})
    End Sub
End Class
