Imports System.Windows.Media.Animation
Imports System.Collections.ObjectModel
Imports System.Math
Public Class LoadSymbolGroupWindow
    Dim IB As IBDataStream
    Public Sub New(ib As IBDataStream)
        InitializeComponent()
        Me.IB = ib
    End Sub

    Private Sub LoadSymbolGroupWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        For Each item In [Enum].GetNames(GetType(SymbolGroupTimeFrame))
            lstTimeFrame.Items.Add(item)
        Next
        For Each item In [Enum].GetNames(GetType(SecurityType))
            cboSearchType.Items.Add(item)
        Next
        cboSearchType.SelectedItem = "Future"
        Dim thread As New System.Threading.Thread(New System.Threading.ThreadStart(
                Sub()
                    Try
                        IB.Connect(0)
                        Log("sucessfully connected to IB")
                    Catch ex As Exception
                        Dispatcher.Invoke(
                            Sub()
                                Log("could not connect to IB")
                            End Sub)
                    End Try
                End Sub))

        If IB.IsConnected = False Then thread.Start()
    End Sub

    Private Sub btnLookUpSymbol_Click(sender As Object, e As RoutedEventArgs) Handles btnLookUpSymbol.Click
        treeSymbols.Items.Clear()
        RemoveHandler IB.EventSources.ContractDetails, AddressOf IB_ContractDetails
        AddHandler IB.EventSources.ContractDetails, AddressOf IB_ContractDetails
        RemoveHandler IB.EventSources.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        AddHandler IB.EventSources.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        IB.reqContractDetails(100000, New IBApi.Contract With {.Symbol = txtLookUpSymbol.Text, .SecType = cboSearchType.SelectedItem})

    End Sub
    Private Sub IB_ContractDetailsEnd(ByVal sender As Object, ByVal e As ContractDetailsEndEventArgs)
        Dispatcher.Invoke(
            Sub()
                Cursor = Cursors.Arrow
            End Sub)
    End Sub
    Private Sub IB_ContractDetails(ByVal sender As Object, ByVal e As ContractDetailsEventArgs)
        If e.reqId >= 10000 Then
            Dispatcher.Invoke(Sub() ProcessSymbol(e.contractDetails))
        End If
    End Sub
    Private Sub ProcessSymbol(ByVal contractDetails As IBApi.ContractDetails)
        Cursor = Cursors.Wait
        Dim contractName As String = contractDetails.LongName,
           exchange As String = contractDetails.Contract.Exchange,
           securityType As String = ParseSecType(contractDetails.Contract.SecType)

        Dim symbolTreeItem As New TreeViewItem
        Select Case ParseSecType(contractDetails.Contract.SecType)
            Case QuickTrader.SecurityType.FUT
                symbolTreeItem.Header = String.Format("{0} ({1}) {2} {3}",
                   contractDetails.Contract.Symbol, contractDetails.LongName, GetContractMonthFormattedString(contractDetails.ContractMonth))
            Case QuickTrader.SecurityType.STK
                symbolTreeItem.Header = contractDetails.Contract.Symbol & " " & contractDetails.Contract.Exchange
            Case Else
                symbolTreeItem.Header = contractDetails.Contract.Symbol
        End Select
        AddHandler symbolTreeItem.Selected,
            Sub(sender As Object, e As EventArgs)
                Dim s As IBApi.Contract = CType(sender.Tag, IBApi.ContractDetails).Contract
                Dim c As IBApi.ContractDetails = contractDetails
                symbol.Text = s.Symbol
                localSymbol.Text = s.LocalSymbol
                marketName.Text = c.MarketName
                longName.Text = c.LongName
                contractID.Text = s.ConId
                Me.securityType.Text = s.SecType
                Me.exchange.Text = s.Exchange
                primaryExchange.Text = s.PrimaryExch
                expiry.Text = "[deprecated]"
                contractMonth.Text = c.ContractMonth
                multiplier.Text = s.Multiplier
                right.Text = [Enum].GetName(GetType(RightType), s.Right)
                strike.Text = s.Strike
                minTick.Text = c.MinTick
                category.Text = c.Category
                subcategory.Text = c.Subcategory
                industry.Text = c.Industry
                timeZone.Text = c.TimeZoneId
                tradingHours.Text = c.TradingHours
                liquidHours.Text = c.LiquidHours
                priceMagnifier.Text = c.PriceMagnifier
            End Sub
        symbolTreeItem.Tag = contractDetails
        Dim insertionPoint As Integer = treeSymbols.Items.Count
        For i = 0 To treeSymbols.Items.Count - 1
            If ParseSecType(CType(treeSymbols.Items(i).Tag, IBApi.ContractDetails).Contract.SecType) <> QuickTrader.SecurityType.FUT Or GetContractMonthAsDate(CType(treeSymbols.Items(i).Tag, IBApi.ContractDetails).Contract.LastTradeDateOrContractMonth) > GetContractMonthAsDate(contractDetails.Contract.LastTradeDateOrContractMonth) Then
                insertionPoint = i
                Exit For
            End If
        Next
        treeSymbols.Items.Insert(insertionPoint, symbolTreeItem)
    End Sub

    Private Sub treeSymbols_SelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
        If treeSymbols.SelectedItem Is Nothing Then
            symbol.Text = ""
            localSymbol.Text = ""
            marketName.Text = ""
            longName.Text = ""
            contractID.Text = ""
            Me.securityType.Text = ""
            Me.exchange.Text = ""
            primaryExchange.Text = ""
            expiry.Text = ""
            contractMonth.Text = ""
            multiplier.Text = ""
            right.Text = ""
            strike.Text = ""
            minTick.Text = ""
            category.Text = ""
            subcategory.Text = ""
            industry.Text = ""
            timeZone.Text = ""
            tradingHours.Text = ""
            liquidHours.Text = ""
            priceMagnifier.Text = ""
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Close()
    End Sub
    Public buttonOKPressed As Boolean = False
    Public SelectedContractDetails As IBApi.ContractDetails
    Public SelectedGroupTimeFrame As SymbolGroupTimeFrame
    Enum SymbolGroupTimeFrame
        _300Days
        _60Days
    End Enum
    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        If treeSymbols.SelectedItem Is Nothing Then
            ShowInfoBox("Please select a symbol.", Me)
        ElseIf lstTimeFrame.SelectedItem Is Nothing Then
            ShowInfoBox("Please select a timeframe.", Me)
        ElseIf IB.isConnected = False Then
            ShowInfoBox("Not connected.", Me)
        Else
            SelectedContractDetails = treeSymbols.SelectedItem.Tag
            SelectedGroupTimeFrame = [Enum].Parse(GetType(SymbolGroupTimeFrame), lstTimeFrame.SelectedItem)
            buttonOKPressed = True
            Close()
        End If

    End Sub

End Class
