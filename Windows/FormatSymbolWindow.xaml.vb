Imports System.Windows.Media.Animation
Imports System.Collections.ObjectModel

Public Class FormatSymbolWindow
    Private IB As IBDataStream
    'Private RND As New RandomDataStream
    Private chart As Chart
    Public Sub New(ByVal chart As Chart)
        InitializeComponent()
        Me.IB = chart.IB
        Me.chart = chart
        txtContractID.Text = IB.Contract(chart.TickerID).ConId
        txtSymbol.Text = IB.Contract(chart.TickerID).Symbol
        txtCurrency.Text = IB.Contract(chart.TickerID).Currency
        txtExchange.Text = IB.Contract(chart.TickerID).Exchange
        txtMultiplier.Text = IB.Contract(chart.TickerID).Multiplier
        txtPrimaryExchange.Text = IB.Contract(chart.TickerID).PrimaryExch
        txtStrike.Text = IB.Contract(chart.TickerID).Strike
        chkUseRegular.IsChecked = IB.UseRegularTradingHours(chart.TickerID)
        chkExpired.IsChecked = IB.Contract(chart.TickerID).IncludeExpired
        txtMinTick.Text = IB.MinTick(chart.TickerID)
        contractMonth.SelectedDate = GetContractMonthAsDate(IB.Contract(chart.TickerID).LastTradeDateOrContractMonth)
        'If IB.Contract(chart.TickerID).Expiry <> "" Then
        '    Expiry = IB.Contract(chart.TickerID).Expiry
        'End If
        For Each item In [Enum].GetNames(GetType(HistoricalDataType))
            cboWhatToShow.Items.Add(item)
        Next
        cboWhatToShow.SelectedItem = [Enum].GetName(GetType(HistoricalDataType), IB.WhatToShow(chart.TickerID))
        For Each item In [Enum].GetNames(GetType(SecurityType))
            cboType.Items.Add(item)
            cboSearchType.Items.Add(item)
        Next
        cboSearchType.SelectedItem = [Enum].GetName(GetType(SecurityType), SecurityType.FUT)
        cboType.SelectedItem = IB.Contract(chart.TickerID).SecType
        txtLookUpSymbol.Text = IB.Contract(chart.TickerID).Symbol
    End Sub

    Private _okPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property

    Public ReadOnly Property Symbol As String
        Get
            Return txtSymbol.Text
        End Get
    End Property
    Public ReadOnly Property ContractID As Integer
        Get
            Return txtContractID.Text
        End Get
    End Property
    Public Property Expiry As String
        Get
            'If cboExpiry.Visibility = Windows.Visibility.Visible Then
            '    If cboExpiry.SelectedItem IsNot Nothing Then
            '        Return GetExpiryStringFromDate(cboExpiry.SelectedItem.Tag)
            '    Else
            '        Return ""
            '    End If
            'Else
            'If expiryDate.SelectedDate.HasValue Then
            '    Return GetExpiryStringFromDate(expiryDate.SelectedDate)
            'Else
            '    Return GetExpiryStringFromDate(Now)
            'End If
            'End If
            Return "[deprecated]"
        End Get
        Set(ByVal value As String)
            'expiryDate.Visibility = Windows.Visibility.Visible
            'cboExpiry.Visibility = Windows.Visibility.Collapsed
            'expiryDate.SelectedDate = GetExpiryAsDate(value)
        End Set
    End Property
    Public ReadOnly Property Strike As Double
        Get
            If IsNumeric(txtStrike.Text) Then Return txtStrike.Text
            Return 0
        End Get
    End Property
    Public ReadOnly Property MinTick As Double
        Get
            If IsNumeric(txtMinTick.Text) Then
                Return txtMinTick.Text
            Else
                ShowInfoBox("The code should not get here!", Me)
                Return 0.25
            End If
        End Get
    End Property
    Private LocalSymbol As String
    Public ReadOnly Property Multiplier As String
        Get
            Return txtMultiplier.Text
        End Get
    End Property
    Public ReadOnly Property Exchange As String
        Get
            Return txtExchange.Text
        End Get
    End Property
    Public ReadOnly Property PrimaryExchange As String
        Get
            Return txtPrimaryExchange.Text
        End Get
    End Property
    Public ReadOnly Property Currency As String
        Get
            Return txtCurrency.Text
        End Get
    End Property
    Public ReadOnly Property IncludeExpired As Boolean
        Get
            Return chkExpired.IsChecked
        End Get
    End Property
    Public ReadOnly Property UseRegularTradingHours As Boolean
        Get
            Return chkUseRegular.IsChecked
        End Get
    End Property
    Public ReadOnly Property WhatToShow As HistoricalDataType
        Get
            Return ParseWhatToShowType(cboWhatToShow.SelectedItem)
        End Get
    End Property
    Public ReadOnly Property Type As SecurityType
        Get
            Return ParseSecType(cboType.SelectedItem)
        End Get
    End Property

    Private Sub cboType_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)

    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        If (txtContractID.Text = "" Or Not IsNumeric(txtContractID.Text)) Then
            ShowInfoBox("Please enter a valid contract ID.", Me)
        ElseIf txtSymbol.Text = "" Then
            ShowInfoBox("Please enter a symbol.", Me)
        ElseIf txtMinTick.Text = "" Or Not IsNumeric(txtMinTick.Text) Then
            ShowInfoBox("Please enter a numeric min tick.", Me)
        ElseIf cboType.SelectedIndex = -1 Then
            ShowInfoBox("Please select a symbol type.", Me)
        ElseIf (txtStrike.Text <> "" And Not IsNumeric(txtStrike.Text)) Then
            ShowInfoBox("Please enter a valid strike price.", Me)
        ElseIf (txtMultiplier.Text <> "" And Not IsNumeric(txtMultiplier.Text)) Then
            ShowInfoBox("Please enter a valid multiplier value.", Me)
        ElseIf txtExchange.Text = "" Then
            ShowInfoBox("Please enter an exchange.", Me)
        ElseIf txtCurrency.Text = "" Then
            ShowInfoBox("Please enter a currency.", Me)
        ElseIf cboWhatToShow.SelectedIndex = -1 Then
            ShowInfoBox("Please select what to show.", Me)

        Else
            _okPressed = True
            If chkDefault.IsChecked Then
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.Currency", Currency)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.Exchange", Exchange)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.LastTradeDateOrContractMonth", GetContractMonthStringFromDate(contractMonth.SelectedDate))
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.ContractID", ContractID)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.IncludeExpired", IncludeExpired)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Multiplier", Multiplier)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.PrimaryExchange", PrimaryExchange)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.Strike", Strike)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.Symbol", Symbol)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.UseRegularTradingHours", UseRegularTradingHours)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.MinTick", MinTick)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.WhatToShow", WhatToShow)
                WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.IB.Contract.SecurityType", Type)
            End If


            IB.Contract(chart.TickerID).Currency = Currency
            IB.Contract(chart.TickerID).Exchange = Exchange
            'If [Enum].Parse(GetType(SecurityType), cboType.SelectedItem) <> SecurityType.Stock Then
            '    IB.Contract(chart.TickerID).Expiry = Expiry
            'Else
            '    IB.Contract(chart.TickerID).Expiry = ""
            'End If
            IB.Contract(chart.TickerID).LastTradeDateOrContractMonth = GetContractMonthStringFromDate(contractMonth.SelectedDate)
            IB.Contract(chart.TickerID).ConId = ContractID
            IB.Contract(chart.TickerID).IncludeExpired = IncludeExpired
            IB.Contract(chart.TickerID).Multiplier = 0
            IB.Contract(chart.TickerID).PrimaryExch = PrimaryExchange
            IB.Contract(chart.TickerID).Strike = Strike
            IB.Contract(chart.TickerID).Symbol = Symbol
            IB.Contract(chart.TickerID).LocalSymbol = LocalSymbol
            IB.UseRegularTradingHours(chart.TickerID) = UseRegularTradingHours
            IB.WhatToShow(chart.TickerID) = WhatToShow
            IB.Contract(chart.TickerID).SecType = ParseSecType(Type)
            IB.MinTick(chart.TickerID) = MinTick
            IB.Contract(chart.TickerID).Multiplier = Multiplier
            Close()
        End If
    End Sub

    Private Sub txtSymbol_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        'If IB.Connected Then
        '    txtContractID.Items.Clear()
        '    txtCurrency.Items.Clear()
        '    txtExchange.Items.Clear()
        '    txtMinTick.Items.Clear()
        '    txtMultiplier.Items.Clear()
        '    txtPrimaryExchange.Items.Clear()
        '    txtStrike.Items.Clear()
        '    cboExpiry.Visibility = Windows.Visibility.Visible
        '    expiryDate.Visibility = Windows.Visibility.Collapsed
        '    cboExpiry.Items.Add("Custom...")
        '    RemoveHandler IB.ContractDetails, AddressOf IB_ContractDetails
        '    AddHandler IB.ContractDetails, AddressOf IB_ContractDetails
        '    RemoveHandler IB.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        '    AddHandler IB.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        '    'For i = 0 To 8
        '    'If i <> 4 And i <> 1 Then ' option and future option
        '    IB.RequestContractDetails(10000, New Contract With {.Symbol = txtSymbol.Text, .SecurityType = SecurityType.Stock})
        '    IB.RequestContractDetails(10002, New Contract With {.Symbol = txtSymbol.Text, .SecurityType = SecurityType.Future})
        '    'End If
        '    'Next
        'End If
    End Sub

    Private Sub btnLookUpSymbol_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        treeSymbols.Items.Clear()
        RemoveHandler IB.EventSources.ContractDetails, AddressOf IB_ContractDetails
        AddHandler IB.EventSources.ContractDetails, AddressOf IB_ContractDetails
        RemoveHandler IB.EventSources.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        AddHandler IB.EventSources.ContractDetailsEnd, AddressOf IB_ContractDetailsEnd
        'For i = 0 To 8
        'If i <> 4 And i <> 1 Then ' option and future option
        IB.reqContractDetails(100000, New IBApi.Contract With {.Symbol = txtLookUpSymbol.Text, .SecType = cboSearchType.SelectedItem})
        '    End If
        'Next
    End Sub
    Private Sub IB_ContractDetailsEnd(ByVal sender As Object, ByVal e As ContractDetailsEndEventArgs)
        Dispatcher.Invoke(
            Sub()
                Cursor = Cursors.Arrow
            End Sub)
        'ShowInfoBox("Finished loading symbol data for '" & [Enum].GetName(GetType(SecurityType), e.RequestId - 10000) & "'.")
    End Sub
    Private Sub IB_ContractDetails(ByVal sender As Object, ByVal e As ContractDetailsEventArgs)
        If e.reqId >= 10000 Then
            Dispatcher.Invoke(Sub() ProcessSymbol(e.contractDetails))
        End If
    End Sub
    Private Sub ProcessSymbol(ByVal contractDetails As IBApi.ContractDetails)
        Cursor = Cursors.Wait
        'Dim c = contractDetails
        'Dim s = c.Summary
        'If contractDetails.Summary.Symbol = txtSymbol.Text Then
        '    txtContractID.Items.Add(s.ContractId)
        '    txtCurrency.Items.Add(s.Currency)
        '    txtExchange.Items.Add(s.Exchange)
        '    txtMinTick.Items.Add(c.MinTick)
        '    txtMultiplier.Items.Add(s.Multiplier)
        '    txtPrimaryExchange.Items.Add(s.PrimaryExchange)
        '    txtStrike.Items.Add(s.Strike)
        '    cboExpiry.Items.Add(New Controls.Label With {.Content = GetExpiryFormattedString(s.Expiry), .Tag = GetExpiryAsDate(s.Expiry)})

        'End If
        Dim contractName As String = contractDetails.LongName,
           exchange As String = contractDetails.Contract.Exchange,
           securityType As String = contractDetails.Contract.SecType

        'Dim contractNameItem As TreeViewItem = GetTreeViewItem(contractName, treeSymbols.Items),
        '    exchangeItem As TreeViewItem = GetTreeViewItem(exchange, contractNameItem.Items),
        '    securityTypeItem As TreeViewItem = GetTreeViewItem(securityType, exchangeItem.Items)

        Dim symbolTreeItem As New TreeViewItem
        Select Case ParseSecType(contractDetails.Contract.SecType)
            Case QuickTrader.SecurityType.FUT
                symbolTreeItem.Header = String.Format("{0} {1} {2}",
                   contractDetails.Contract.Symbol, GetContractMonthFormattedString(contractDetails.Contract.LastTradeDateOrContractMonth), contractDetails.Contract.Exchange)
            Case QuickTrader.SecurityType.STK
                symbolTreeItem.Header = contractDetails.Contract.Symbol & " " & contractDetails.Contract.Exchange
            Case QuickTrader.SecurityType.CASH
                symbolTreeItem.Header = contractDetails.Contract.LocalSymbol
            Case Else
                symbolTreeItem.Header = contractDetails.Contract.Symbol
        End Select
        Dim grd As New Grid
        'grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        'grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
        'grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        Dim detailsLink As New Hyperlink
        detailsLink.Inlines.Add("Details")
        Dim useLink As New Hyperlink
        useLink.Inlines.Add("Use Symbol")
        Dim links As New TextBlock
        links.Inlines.Add(detailsLink)
        links.Inlines.Add("  ")
        links.Inlines.Add(useLink)
        AddHandler detailsLink.Click,
            Sub()
                Dim win As New SymbolInfoWindow(contractDetails)
                win.Show()
            End Sub
        AddHandler useLink.Click, Sub() UseContract(contractDetails)
        symbolTreeItem.Items.Add(links)
        symbolTreeItem.Tag = contractDetails
        Dim insertionPoint As Integer = treeSymbols.Items.Count
        For i = 0 To treeSymbols.Items.Count - 1
            If ParseSecType(CType(treeSymbols.Items(i).Tag, IBApi.ContractDetails).Contract.SecType) <> QuickTrader.SecurityType.FUT Or GetContractMonthAsDate(CType(treeSymbols.Items(i).Tag, IBApi.ContractDetails).Contract.LastTradeDateOrContractMonth) > GetContractMonthAsDate(contractDetails.Contract.LastTradeDateOrContractMonth) Then
                insertionPoint = i
                Exit For
                'ElseIf contractDetails.Summary.SecurityType <> SecurityType.Future Then
                '    insertionPoint = treeSymbols.Items.Count
                '    Exit For
            End If
        Next
        treeSymbols.Items.Insert(insertionPoint, symbolTreeItem)
    End Sub
    Private Sub UseContract(contractDetails As IBApi.ContractDetails)
        Dim c As IBApi.Contract = contractDetails.Contract
        Dim cd As IBApi.ContractDetails = contractDetails
        txtContractID.Text = c.ConId
        txtCurrency.Text = c.Currency
        txtExchange.Text = c.Exchange
        txtMinTick.Text = cd.MinTick
        txtMultiplier.Text = c.Multiplier
        txtPrimaryExchange.Text = c.PrimaryExch
        txtStrike.Text = c.Strike
        txtSymbol.Text = c.Symbol
        LocalSymbol = c.LocalSymbol
        cboType.SelectedItem = c.SecType
        contractMonth.SelectedDate = GetContractMonthAsDate(c.LastTradeDateOrContractMonth)
        'If c.Expiry <> "" Then
        '    expiryDate.SelectedDate = New Date(c.Expiry.Substring(0, 4), c.Expiry.Substring(4, 2), If(c.Expiry.Length = 8, c.Expiry.Substring(6, 2), ""))
        'End If
    End Sub
    Private Function GetTreeViewItem(ByVal header As String, ByRef items As ItemCollection) As TreeViewItem
        Dim contains As Boolean = False
        For Each item As TreeViewItem In items
            If header = item.Header Then
                Return item
                contains = True
            End If
        Next
        If Not contains Then
            Dim newItem As New TreeViewItem With {.Header = header}
            items.Add(newItem)
            Return newItem
        End If
        Return Nothing
    End Function

    'Private Sub IB_ContractDetailsFinished(ByVal sender As Object, ByVal e As ContractDetailsEndEventArgs)
    '    If e.RequestId = 10008 Then
    '        loadingInfoBox.Close()
    '        For i = 0 To 8
    '            Dim contractDetailsList As List(Of ContractDetails) = details(i)
    '            If contractDetailsList.Count > 0 Then
    '                Dim securityTypeTreeItem As New TreeViewItem With {.Header = [Enum].GetName(GetType(SecurityType), i)}
    '                treeSymbols.Items.Add(securityTypeTreeItem)

    '                For Each contractDetails In contractDetailsList
    '                    Dim contract As Contract = contractDetails.Summary
    '                    If Not exchanges.ContainsKey(contract.Exchange) Then
    '                        Dim exchangeTreeItem As New TreeViewItem With {.Header = contract.Exchange}
    '                        exchanges.Add(contract.Exchange, exchangeTreeItem)
    '                        securityTypeTreeItem.Items.Add(exchangeTreeItem)
    '                        exchangeTreeItem.Items.Add(String.Format("{0} {1} {2}", contract.Symbol, contractDetails.ContractMonth, contract.Expiry))
    '                    Else
    '                        exchanges(contract.Exchange).Items.Add(String.Format("{0} {1} {2}", contract.Symbol, contractDetails.ContractMonth, contract.Expiry))
    '                    End If
    '                Next
    '            End If
    '        Next
    '    End If
    'End Sub

    Private Sub FormatSymbolWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        btnLookUpSymbol_Click(Nothing, Nothing)
    End Sub
End Class

Public Class TreeCategory
    Private _subcategories As New List(Of TreeCategory)

    Public Sub New()
    End Sub
    Public Sub New(ByVal header As Object)
        Me.Header = header
    End Sub
    Public Property Header As Object
    Public ReadOnly Property Subcategories As ReadOnlyCollection(Of TreeCategory)
        Get
            Return New ReadOnlyCollection(Of TreeCategory)(_subcategories)
        End Get
    End Property

    Private _parentCategory As TreeCategory = Nothing
    Public ReadOnly Property ParentCategory As TreeCategory
        Get
            Return _parentCategory
        End Get
    End Property

    <DebuggerStepThrough()>
    Public Function AddSubcategory(ByVal header As Object) As TreeCategory
        If Not ContainsSubcategory(header) Then
            Dim cat As New TreeCategory(header)
            cat._parentCategory = Me
            _subcategories.Add(cat)
            Return cat
        Else
            For Each category In Subcategories
                If category.Header.Equals(header) Then
                    Return category
                End If
            Next
        End If
        Return Nothing
    End Function
    Public Sub RemoveSubcategory(ByVal header As Object)
        Dim itemToRemove As TreeCategory = Nothing
        For Each cat In Subcategories
            If cat.Header.Equals(header) Then
                itemToRemove = cat
                Exit For
            End If
        Next
        _subcategories.Remove(itemToRemove)
    End Sub
    Public Function ContainsSubcategory(ByVal header As Object) As Boolean
        Dim contains As Boolean
        For Each category In Subcategories
            If category.Header.Equals(header) Then
                contains = True
                Exit For
            End If
        Next
        Return contains
    End Function
    Public Function FindSubcategory(ByVal header As Object) As TreeCategory
        Return FindSubcategory(header, Me)
    End Function
    Private Function FindSubcategory(ByVal header As Object, ByVal category As TreeCategory) As TreeCategory
        For Each category In category.Subcategories
            If category.Header.Equals(header) Then
                Return category
            End If
        Next
        For Each category In category.Subcategories
            Dim result As TreeCategory = FindSubcategory(category.Header, category)
            If result IsNot Nothing Then
                Return result
            End If
        Next
        Return Nothing
    End Function
End Class
