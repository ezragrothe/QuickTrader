Public Class SymbolInfoWindow
    Private contractDetails As IBApi.ContractDetails
    Public Sub New(ByVal contractDetails As IBApi.ContractDetails)
        Me.contractDetails = contractDetails
        InitializeComponent()
    End Sub

    Private Sub SymbolInfoWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        SaveSetting("QuickTrader", "SymbolInfoWindow", "ShowRelevantData", chkShowRelevant.IsChecked)
    End Sub
    Private Sub SymbolInfoWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Dim s As IBApi.Contract = contractDetails.Contract
        Dim c As IBApi.ContractDetails = contractDetails
        symbol.Text = s.Symbol
        localSymbol.Text = s.LocalSymbol
        marketName.Text = c.MarketName
        longName.Text = c.LongName
        contractID.Text = s.ConId
        securityType.Text = s.SecType
        exchange.Text = s.Exchange
        primaryExchange.Text = s.PrimaryExch
        expiry.Text = "[deprecated]"
        contractMonth.Text = c.ContractMonth
        multiplier.Text = s.Multiplier
        right.Text = s.Right
        strike.Text = s.Strike
        minTick.Text = c.MinTick
        category.Text = c.Category
        subcategory.Text = c.Subcategory
        industry.Text = c.Industry
        timeZone.Text = c.TimeZoneId
        tradingHours.Text = c.TradingHours
        liquidHours.Text = c.LiquidHours
        priceMagnifier.Text = c.PriceMagnifier

        bondType.Text = c.BondType
        callable.Text = c.Callable
        convertible.Text = c.Convertible
        coupon.Text = c.Coupon
        couponType.Text = c.CouponType
        cusip.Text = c.Cusip
        description.Text = c.DescAppend
        issueDate.Text = c.IssueDate
        maturity.Text = c.Maturity
        nextOptionDate.Text = c.NextOptionDate
        nextOptionPartial.Text = c.NextOptionPartial
        nextOptionType.Text = c.NextOptionType
        notes.Text = c.Notes
        putable.Text = c.Putable
        ratings.Text = c.Ratings

        chkShowRelevant.IsChecked = GetSetting("QuickTrader", "SymbolInfoWindow", "ShowRelevantData", "False")
    End Sub

    Private Sub btnOK_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub chkShowRelevant_CheckedChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If chkShowRelevant.IsChecked Then
            For i = 0 To grdSymbol.Children.Count - 1
                If TypeOf grdSymbol.Children(i) Is TextBox AndAlso CType(grdSymbol.Children(i), TextBox).Text = "" Then
                    grdSymbol.Children(i - 1).Visibility = Windows.Visibility.Collapsed
                    grdSymbol.Children(i).Visibility = Windows.Visibility.Collapsed
                End If
            Next
            If ParseSecType(contractDetails.Contract.SecType) <> QuickTrader.SecurityType.Bond Then
                grpBonds.Visibility = Windows.Visibility.Collapsed
            Else
                grpBonds.Visibility = Windows.Visibility.Visible
                For i = 0 To grdBonds.Children.Count - 1
                    If TypeOf grdBonds.Children(i) Is TextBox AndAlso CType(grdBonds.Children(i), TextBox).Text = "" Then
                        grdBonds.Children(i - 1).Visibility = Windows.Visibility.Collapsed
                        grdBonds.Children(i).Visibility = Windows.Visibility.Collapsed
                    End If
                Next
            End If
        Else
            grpBonds.Visibility = Windows.Visibility.Visible
            For i = 0 To grdSymbol.Children.Count - 1
                If grdSymbol.Children(i).Visibility = Windows.Visibility.Collapsed Then grdSymbol.Children(i).Visibility = Windows.Visibility.Visible
            Next
            For i = 0 To grdBonds.Children.Count - 1
                If grdBonds.Children(i).Visibility = Windows.Visibility.Collapsed Then grdBonds.Children(i).Visibility = Windows.Visibility.Visible
            Next
        End If
    End Sub

End Class
