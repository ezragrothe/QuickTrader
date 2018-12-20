Imports System.Collections.ObjectModel
Imports System.Math
Public Class FormatWindow
    Private Chart As Chart
    Private _okPressed As Boolean
    Private _colorProps As New Dictionary(Of String, Color)

    Public Property RefreshChart As Boolean

    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property

    Private Sub LoadSettings()
        For Each item In Chart.Settings.Settings
            If item.IsColorSetting Then Settings.AddSetting(New Setting(item.Name, item.Value))
        Next

        Settings.AddSetting(New Setting("BarType", Chart.Settings("BarType").Value))
        Settings.AddSetting(New Setting("UseRandom", Chart.Settings("UseRandom").Value))
        Settings.AddSetting(New Setting("UseReplayMode", Chart.DataStream.UseReplayMode(Chart.TickerID)))
        Settings.AddSetting(New Setting("RangeValue", Chart.Settings("RangeValue").Value))
        Settings.AddSetting(New Setting("XMargin", Chart.Settings("XMargin").Value))
        Settings.AddSetting(New Setting("YMargin", Chart.Settings("YMargin").Value))
        Settings.AddSetting(New Setting("RestrictMovement", Chart.Settings("RestrictMovement").Value))
        Settings.AddSetting(New Setting("PriceBarTextInterval", Chart.Settings("PriceBarTextInterval").Value))
        Settings.AddSetting(New Setting("TimeBarTextInterval", Chart.Settings("TimeBarTextInterval").Value))
        Settings.AddSetting(New Setting("RAND.RandomMaxMove", Chart.RAND.RandomMaxMove))
        Settings.AddSetting(New Setting("RAND.RandomMinMove", Chart.RAND.RandomMinMove))
        Settings.AddSetting(New Setting("RAND.RandomSpeed", Chart.RAND.RandomSpeed))
        Settings.AddSetting(New Setting("ShowGrid", Chart.Settings("ShowGrid").Value))
        Settings.AddSetting(New Setting("GridWidth", Chart.Settings("GridWidth").Value))
        Settings.AddSetting(New Setting("CenterScalingOffsetHeight", Chart.Settings("CenterScalingOffsetHeight").Value))
        Settings.AddSetting(New Setting("BarSize", Chart.DataStream.BarSize(Chart.TickerID)))
        Settings.AddSetting(New Setting("DaysBack", Chart.DataStream.DaysBack(Chart.TickerID)))
        'Settings.AddSetting(New Setting("EndDaysBack", Chart.DataStream.EndDaysBack(Chart.TickerID)))
        Settings.AddSetting(New Setting("DesiredBarCount", Chart.Settings("DesiredBarCount").Value))
        Settings.AddSetting(New Setting("AutoScale", Chart.Settings("AutoScale").Value))
        Settings.AddSetting(New Setting("DecimalPlaces", CInt(Chart.Settings("DecimalPlaces").Value)))
        Settings.AddSetting(New Setting("StatusTextLocation", CType(Chart.Settings("StatusTextLocation").Value, HorizontalAlignment)))
        Settings.AddSetting(New Setting("DefaultOrderQuantity", CInt(Chart.Settings("DefaultOrderQuantity").Value)))
        Settings.AddSetting(New Setting("UpdateScalingBarInterval", CInt(Chart.Settings("UpdateScalingBarInterval").Value)))
        Settings.AddSetting(New Setting("UpdateAnalysisTechniquesEveryTick", Chart.Settings("UpdateAnalysisTechniquesEveryTick").Value))
        originalContract = New IBApi.Contract()
        SetContractToContract(originalContract, Chart.IB.Contract(Chart.TickerID))

        For Each item In Settings.Settings
            originalSettings.AddSetting(New Setting(item.Name, item.Value))
        Next
    End Sub
    Private _settings As SettingList
    Public Function Settings() As SettingList
        Return _settings
    End Function
    Private originalContract As IBApi.Contract
    Private originalSettings As New SettingList
    Private _changedSettingNames As New List(Of String)
    Public ReadOnly Property ChangedSettingNames As ReadOnlyCollection(Of String)
        Get
            Return New ReadOnlyCollection(Of String)(_changedSettingNames)
        End Get
    End Property
    Private Sub CheckIBConnection()
        'btnConnect.IsEnabled = Not Chart.IB.Connected
        'ShowInfoBox("")
    End Sub
    Dim noChart As Boolean
    Public Sub New()
        InitializeComponent()
        noChart = True
        For Each item In My.Application.ThemeNames
            lstThemes.Items.Add(item)
        Next
        lstThemes.SelectedItem = ReadSetting(GLOBAL_CONFIG_FILE, "Theme", "Dark")
        chkShowMousePopups.IsChecked = My.Application.ShowMousePopups
        txtTickWidth.Text = My.Application.TickWidth * 100
        txtBarWidth.Text = My.Application.BarWidth
        txtFineResizeValue.Text = My.Application.FineChartResizeValue
        txtCoarseResizeValue.Text = My.Application.CoarseChartResizeValue
        txtBorderWidth.Text = My.Application.ChartBorderWidth
        chkHideLogWindow.IsChecked = My.Application.HideLogWindow
        sldScalingFactor.Value = My.Application.DesktopScaleFactor
        For Each p In GetType(Colors).GetProperties
            cboWorkspaceBackground.Items.Add(p.Name)
        Next
        AddHandler cboWorkspaceBackground.SelectionChanged,
            Sub()
                cboWorkspaceBackground.Text = cboWorkspaceBackground.SelectedItem
            End Sub
        cboWorkspaceBackground.Text = (New ColorConverter).ConvertToString(My.Application.WorkspaceBackgroundColor)
    End Sub
    Private connectionTimer As System.Threading.Timer
    Public Sub New(ByVal chart As Chart)
        Me.New()
        noChart = False
        Me.Chart = chart

        _settings = New SettingList
        txtDaysBack.Text = chart.IB.DaysBack(chart.TickerID)
        'txtEndDaysBack.Text = chart.IB.EndDaysBack(chart.TickerID)
        txtDesiredBarCount.Text = chart.Settings("DesiredBarCount").Value
        txtSymbol.Text = chart.IB.Contract(chart.TickerID).Symbol
        chkReplay.IsChecked = chart.IB.EnableReplayMode(chart.TickerID)
        txtRange.Text = chart.Settings("RangeValue").Value
        txtDefaultQuantity.Text = chart.Settings("DefaultOrderQuantity").Value

        For Each item In [Enum].GetNames(GetType(BarSize))
            cboInterval.Items.Add(item)
        Next

        If chart.Settings("UseRandom").Value Then
            cboInterval.SelectedItem = [Enum].GetName(GetType(BarSize), chart.RAND.BarSize(chart.TickerID))
        Else
            cboInterval.SelectedItem = [Enum].GetName(GetType(BarSize), chart.IB.BarSize(chart.TickerID))
        End If

        txtXMargin.Text = chart.Settings("XMargin").Value * 100
        txtYMargin.Text = chart.Settings("YMargin").Value * 100
        txtPriceInterval.Text = chart.Settings("PriceBarTextInterval").Value
        txtTimeInterval.Text = chart.Settings("TimeBarTextInterval").Value
        chkFreeMovement.IsChecked = Not chart.Settings("RestrictMovement").Value
        chkUseRandom.IsChecked = chart.Settings("UseRandom").Value
        chkShowGrid.IsChecked = chart.Settings("ShowGrid").Value
        txtTickerID.Text = chart.TickerID
        txtGridWidth.Text = chart.Settings("GridWidth").Value
        chkAutoScale.IsChecked = chart.Settings("AutoScale").Value
        txtPriceTextDecimalCount.Text = chart.Settings("DecimalPlaces").Value
        chkUseCache.IsChecked = chart.IB.UseCachedData(chart.TickerID)
        txtCenterOffset.Text = chart.Settings("CenterScalingOffsetHeight").Value * 100
        txtUpdateBarInterval.Text = chart.Settings("UpdateScalingBarInterval").Value
        chkUpdateEveryTick.IsChecked = chart.Settings("UpdateAnalysisTechniquesEveryTick").Value

        cboStatusTextLocation.ItemsSource = {"Left", "Center", "Right"}
        cboStatusTextLocation.SelectedIndex = CInt(chart.Settings("StatusTextLocation").Value)
        If chart.IB.Contract(chart.TickerID).SecType = ParseSecType(SecurityType.FUT) Then
            btnRollover.Visibility = Windows.Visibility.Visible
        Else
            btnRollover.Visibility = Windows.Visibility.Collapsed
        End If
        'If chart.Settings("IsSlaveChart").Value Then
        '    lstBarType.Items.Add("Slave Bars")
        '    lstBarType.SelectedIndex = 0
        '    cboInterval.IsEnabled = False
        '    'txtDaysBack.IsEnabled = False
        '    'chkRefresh.IsEnabled = False
        '    grpBarTypeSettings.Visibility = Windows.Visibility.Collapsed
        'Else
        For Each item In [Enum].GetNames(GetType(BarTypes))
            lstBarType.Items.Add(Spacify(item))
        Next
        lstBarType.SelectedIndex = chart.Settings("BarType").Value
        'End If
    End Sub
    Private Sub FormatChartWindow_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Closed
        If Not noChart Then
            SaveSetting("QuickTrader", "FormatChartWindow", "SelectedIndex", tabMain.SelectedIndex)
        End If
    End Sub
    Private Sub FormatChartWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        If Not noChart Then
            LoadSettings()
            For Each item In Settings.Settings
                If item.IsColorSetting Then lstProperty.Items.Add(item.Name)
            Next
            tabMain.SelectedIndex = GetSetting("QuickTrader", "FormatChartWindow", "SelectedIndex", 0)
            lstInterval_SelectionChanged(lstBarType, Nothing)
            TextBox_TextChanged(Nothing, Nothing)
            sldMax.Value = Chart.RAND.RandomMaxMove
            sldMin.Value = Chart.RAND.RandomMinMove
            sldSpeed.Value = Chart.RAND.RandomSpeed
            connectionTimer = New System.Threading.Timer(New System.Threading.TimerCallback(AddressOf CheckIBConnection), Nothing, 0, 100)
            Dim thread As New System.Threading.Thread(New System.Threading.ThreadStart(
                Sub()
                    Try
                        If Not Chart.IB.IsConnected Then
                            Chart.IB.Connect(Chart.TickerID)
                            If Chart.IB.IsConnected Then
                                Log("sucessfully connected to IB")
                            Else
                                Dispatcher.Invoke(
                                 Sub()
                                     btnConnect.IsEnabled = True
                                     Log("could not connect to IB")
                                 End Sub)
                            End If
                        End If
                    Catch ex As Exception
                        'ShowInfoBox("Couldn't connect to TWS. Confirm that 'Enable ActiveX and Socket Clients' is enabled on the TWS 'Configure->API' menu.")
                        Dispatcher.Invoke(
                            Sub()
                                btnConnect.IsEnabled = True
                                Log("could not connect to IB")
                            End Sub)
                    End Try
                End Sub))
            If Chart.IB.IsConnected = False Then thread.Start()
            'AddHandler Chart.IB.ConnectionClosed,
            '            Sub()
            '                Dispatcher.Invoke(
            '                        Sub()
            '                            btnConnect.IsEnabled = True
            '                            Log("lost connection to IB")
            '                        End Sub)
            '            End Sub
            UpdateCacheDataAvailability()
        End If
    End Sub
    Private Sub UpdateCacheDataAvailability()
        Dim firstDate As Date = Date.MinValue
        Dim lastDate As Date = Date.MinValue
        Dim cacheReader As New StreamReader(Chart.IB.GetCacheFileName(Chart.TickerID))
        'Dim indexReader As New StreamReader(Chart.IB.GetIndexFileName(Chart.TickerID))
        If cacheReader.Peek <> -1 Then firstDate = CType((New BarDataConverter).ConvertFromString(cacheReader.ReadLine()), BarData).Date
        'If indexReader.Peek <> -1 Then
        '    lastDate = StringToDate(indexReader.ReadLine(), True)
        'Else
        Dim lastLine As String = ""
        While cacheReader.Peek <> -1
            lastLine = cacheReader.ReadLine
        End While
        lastDate = If(lastLine <> "", CType((New BarDataConverter).ConvertFromString(lastLine), BarData).Date, Date.MinValue)
        'End If
        cacheReader.Close()
        'indexReader.Close()
        If firstDate <> Date.MinValue And chkUseRandom.IsChecked = False Then
            chkUseCache.Content = "Use Cache Data If Possible (oldest date " & Floor((Now - firstDate).TotalDays * 100) / 100 & " day(s) back; most recent date " & Floor((Now - lastDate).TotalDays * 100) / 100 & " day(s) back)"
        Else
            chkUseCache.Content = "Use Cache Data If Possible (0 day(s) back)"
        End If
    End Sub

    Private Sub lstBarType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstBarType.SelectionChanged
        CheckDirtiness()
    End Sub
    Private Sub lstInterval_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If IsLoaded Then
            If lstBarType.SelectedIndex = BarTypes.RangeBars Or lstBarType.SelectedIndex = BarTypes.VerticalSwingBars Then
                grpBarTypeSettings.Visibility = Windows.Visibility.Visible
                grdTick.Visibility = Windows.Visibility.Visible
            Else
                grpBarTypeSettings.Visibility = Windows.Visibility.Collapsed
                grdTick.Visibility = Windows.Visibility.Collapsed
            End If
        End If
        CheckDirtiness()
    End Sub
    Private textBoxTextChanged As Boolean
    Private Sub sldSpeed_ValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of System.Double))
        If Not textBoxTextChanged Then
            txtSpeed.Text = Math.Round(sender.Value, 1)
        End If
        textBoxTextChanged = False
        CheckDirtiness()
    End Sub
    Private Sub sldMin_ValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of System.Double))
        If Not textBoxTextChanged Then txtMinMove.Text = Math.Round(sender.Value, 2)
        If IsLoaded AndAlso sldMin.Value > sldMax.Value Then sldMax.Value = sldMin.Value
        textBoxTextChanged = False
        CheckDirtiness()
    End Sub
    Private Sub sldMax_ValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of System.Double))
        If Not textBoxTextChanged Then txtMaxMove.Text = Math.Round(sender.Value, 2)
        If IsLoaded AndAlso sldMin.Value > sldMax.Value Then sldMin.Value = sldMax.Value
        textBoxTextChanged = False
        CheckDirtiness()
    End Sub
    Private Sub TextBox_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        textBoxTextChanged = True
        If IsLoaded Then
            If IsNumeric(txtMaxMove.Text) Then sldMax.Value = txtMaxMove.Text
            If IsNumeric(txtMinMove.Text) Then sldMin.Value = txtMinMove.Text
            If IsNumeric(txtSpeed.Text) Then sldSpeed.Value = txtSpeed.Text
        End If
        CheckDirtiness()
    End Sub
    Private Sub lstProperty_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstProperty.SelectedItem IsNot Nothing Then
            lstColors.AreColorsVisible = True
            lstColors.Color = Settings(lstProperty.SelectedItem).Value
        Else
            lstColors.AreColorsVisible = False
        End If
    End Sub
    Private Sub lstColors_ColorChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles lstColors.ColorChanged
        If lstColors.HasColor Then
            If lstProperty.SelectedItem IsNot Nothing Then
                Settings(lstProperty.SelectedItem).Value = lstColors.Color
            Else
                lstColors.AreColorsVisible = False
            End If
        End If
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim close As Boolean
        If Not noChart Then
            If lstBarType.SelectedIndex = -1 Then
                ShowInfoBox("Please select a bar type.", Me)
                tabMain.SelectedIndex = 0
                lstBarType.Focus()
            ElseIf (Not IsNumeric(txtDefaultQuantity.Text) OrElse txtDefaultQuantity.Text <= 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Default Order Quantity' textbox.", Me)
                tabMain.SelectedIndex = 0
                txtDefaultQuantity.Focus()
            ElseIf (Not IsNumeric(txtRange.Text) OrElse txtRange.Text <= 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Range' textbox.", Me)
                tabMain.SelectedIndex = 0
                txtRange.Focus()
            ElseIf (Not IsNumeric(txtDaysBack.Text) OrElse txtDaysBack.Text < 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Days Back' textbox.", Me)
                tabMain.SelectedIndex = 0
                txtDaysBack.Focus()
                'ElseIf chkReplay.IsChecked AndAlso (Not IsNumeric(txtEndDaysBack.Text) OrElse txtEndDaysBack.Text < 0 OrElse CDec(txtEndDaysBack.Text) > CDec(txtDaysBack.Text)) Then
                '    ShowInfoBox("Please type positive, numerical data in the 'End Date Days Back' textbox.", Me)
                '    tabMain.SelectedIndex = 0
                '    txtEndDaysBack.Focus()
            ElseIf cboInterval.SelectedItem Is Nothing Then
                ShowInfoBox("Please choose a historical data interval.", Me)
                tabMain.SelectedIndex = 0
                cboInterval.Focus() ' Tab item 1
            ElseIf (Not IsNumeric(txtXMargin.Text) OrElse txtXMargin.Text < 0 OrElse txtXMargin.Text > 100) Then
                ShowInfoBox("Please type positive, numerical data in the 'Horizontal Margin' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtXMargin.Focus()
            ElseIf (Not IsNumeric(txtYMargin.Text) OrElse txtYMargin.Text < 0 OrElse txtYMargin.Text > 50) Then
                ShowInfoBox("Please type positive, numerical data in the 'Vertical Margin' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtYMargin.Focus()
            ElseIf (Not IsNumeric(txtPriceInterval.Text) OrElse txtPriceInterval.Text < 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Price Bar Text Interval' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtPriceInterval.Focus()
            ElseIf (Not IsNumeric(txtTimeInterval.Text) OrElse txtTimeInterval.Text < 0 OrElse txtTimeInterval.Text.Contains(".")) Then
                ShowInfoBox("Please type positive, integral data in the 'Time Bar Text Interval' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtTimeInterval.Focus()
            ElseIf (Not IsNumeric(txtPriceTextDecimalCount.Text) OrElse txtPriceTextDecimalCount.Text < 0 OrElse txtPriceTextDecimalCount.Text.Contains(".")) Then
                ShowInfoBox("Please type positive, integral data in the 'Decimal Places' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtPriceTextDecimalCount.Focus()
            ElseIf cboStatusTextLocation.SelectedIndex = -1 Then
                ShowInfoBox("Please select a status text location.", Me)
                tabMain.SelectedIndex = 1
                cboStatusTextLocation.Focus()
            ElseIf (Not IsNumeric(txtGridWidth.Text) OrElse txtGridWidth.Text < 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Grid Width' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtGridWidth.Focus()
            ElseIf (Not IsNumeric(txtCenterOffset.Text) OrElse txtCenterOffset.Text > 100) Then
                ShowInfoBox("Please type numerical data in the 'Center Offset' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtCenterOffset.Focus()
            ElseIf (Not IsNumeric(txtUpdateBarInterval.Text) OrElse txtUpdateBarInterval.Text > 100) Then
                ShowInfoBox("Please type numerical data in the 'Update Interval' textbox.", Me)
                tabMain.SelectedIndex = 1
                txtUpdateBarInterval.Focus()
            ElseIf (Not IsNumeric(txtTickWidth.Text) OrElse txtTickWidth.Text < 0 OrElse txtTickWidth.Text > 100) Then
                ShowInfoBox("Please type positive, numerical data below 100 in the 'Tick Width' textbox.", Me)
                tabMain.SelectedIndex = 3
                txtTickWidth.Focus()
            ElseIf (Not IsNumeric(txtBarWidth.Text) OrElse txtBarWidth.Text < 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Bar Width' textbox.", Me)
                tabMain.SelectedIndex = 3
                txtBarWidth.Focus()
            ElseIf (Not IsNumeric(txtFineResizeValue.Text) OrElse txtFineResizeValue.Text < 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Chart Resizing Fine Adjustment' textbox.", Me)
                tabMain.SelectedIndex = 3
                txtFineResizeValue.Focus()
            ElseIf (Not IsNumeric(txtCoarseResizeValue.Text) OrElse txtCoarseResizeValue.Text < 0) Then
                ShowInfoBox("Please type positive, numerical data in the 'Chart Resizing Coarse Adjustment' textbox.", Me)
                tabMain.SelectedIndex = 3
                txtCoarseResizeValue.Focus()
            ElseIf (Not IsNumeric(txtBorderWidth.Text) OrElse txtBorderWidth.Text < 0 OrElse txtBorderWidth.Text > 20) Then
                ShowInfoBox("Please type positive, numerical data below 20 in the 'Chart Border Width' textbox.", Me)
                tabMain.SelectedIndex = 3
                txtBorderWidth.Focus()
            ElseIf (Not (New ColorConverter).IsValid(cboWorkspaceBackground.Text)) Then
                ShowInfoBox("Please enter a valid color in the 'Workspace Background Color' combo box.", Me)
                tabMain.SelectedIndex = 3
                cboWorkspaceBackground.Focus()
            Else ' Tab item 2
                'For Each item In Settings.Settings
                '    If item.IsColorSetting AndAlso item.Value = Color.FromArgb(0, 0, 0, 0) Then
                '        tabMain.SelectedIndex = 2
                '        ShowInfoBox("Please select a color for '" & item.Name & "'.", Me)
                '        lstProperty.SelectedItem = item.Name
                '        Exit Sub
                '    End If
                'Next
                'If (chkUseRandom.IsChecked = False And Not Chart.IB.Connected And chkRefresh.IsChecked) AndAlso
                'ShowInfoBox("QuickTrader is not connected to TWS. Would you like to load whatever is available from the cache?", Me, "Yes", "No") = 0 Then
                RefreshChangedSettings()
                If chkDefault.IsChecked Then
                    For Each item In Settings.Settings
                        WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle." & item.Name, item.StringValue)
                    Next
                    WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.ChartName", "")
                    WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.Bounds", (New BoundsConverter).ConvertToString(Chart.Bounds))
                    'WriteSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle.ChartRect", (New RectConverter).ConvertToString(New Rect(Canvas.GetLeft(Chart), Canvas.GetTop(Chart), Chart.ActualWidth, Chart.ActualHeight)))
                End If
                For Each a In Chart.AnalysisTechniques
                    If TypeOf a.AnalysisTechnique Is AnalysisTechniques.Statistics Then
                        Chart.dontDrawBarVisuals = True
                        Exit For
                    End If
                Next


                _okPressed = True
                RefreshChart = chkRefresh.IsChecked
                close = True
            End If
        Else
            close = True
        End If
        If close Then
            If RefreshChart Then
                Chart.IB.ClearMemoryCache(Chart.TickerID)
            End If
            My.Application.ChartBorderWidth = txtBorderWidth.Text
            My.Application.BarWidth = txtBarWidth.Text
            My.Application.TickWidth = txtTickWidth.Text / 100
            My.Application.FineChartResizeValue = txtFineResizeValue.Text
            My.Application.CoarseChartResizeValue = txtCoarseResizeValue.Text
            My.Application.ShowMousePopups = chkShowMousePopups.IsChecked
            My.Application.WorkspaceBackgroundColor = ColorConverter.ConvertFromString(cboWorkspaceBackground.Text)
            My.Application.HideLogWindow = chkHideLogWindow.IsChecked
            Me.Close()
        End If
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
    Private Sub btnSymbolSettings_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim IBWin As New FormatSymbolWindow(Chart)
        IBWin.Owner = Me
        IBWin.ShowDialog()
        If IBWin.ButtonOKPressed Then
            txtSymbol.Text = IBWin.Symbol
            'chkRefresh.IsChecked = True
            UpdateCacheDataAvailability()
        End If
    End Sub
    Private Sub btnConnect_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If Not Chart.IB.IsConnected Then
            Try
                Chart.IB.Connect(Chart.TickerID)
                btnConnect.IsEnabled = False
            Catch ex As Exception
                ShowInfoBox("Couldn't connect to TWS. Confirm that 'Enable ActiveX and Socket Clients' is enabled on the TWS 'Configure->API' menu.", Me)
                btnConnect.IsEnabled = True
            End Try
        End If
    End Sub

    Private Sub btnApplyTheme_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        For Each item In My.Application.Windows
            If TypeOf item Is BaseWindow Then
                CType(item, BaseWindow).ApplyTheme(lstThemes.SelectedItem)
            End If
        Next
        WriteSetting(GLOBAL_CONFIG_FILE, "Theme", lstThemes.SelectedItem)
        btnApplyTheme.IsEnabled = False
    End Sub
    Private Sub lstThemes_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        btnApplyTheme.IsEnabled = lstThemes.SelectedItem IsNot Nothing
    End Sub

    Private Sub CheckDirtiness()
        RefreshChangedSettings()
        If IsLoaded Then
            chkRefresh.IsChecked = (ChangedSettingNames.Contains("DaysBack") Or
                ChangedSettingNames.Contains("DesiredBarCount") Or
                ChangedSettingNames.Contains("BarSize") Or
                ChangedSettingNames.Contains("RangeValue") Or
                ChangedSettingNames.Contains("UseRandom") Or
                ChangedSettingNames.Contains("BarType") Or
                (ChangedSettingNames.Contains("EndDaysBack") And chkReplay.IsChecked) Or
                RefreshChart
                )
            UpdateCacheDataAvailability()
        End If
    End Sub
    Private Sub RefreshChangedSettings()
        If IsLoaded Then
            If lstBarType.SelectedIndex <> -1 Then Settings("BarType").Value = lstBarType.SelectedIndex
            Settings("UseRandom").Value = chkUseRandom.IsChecked
            Settings("UseReplayMode").Value = chkReplay.IsChecked
            If IsNumeric(txtRange.Text) Then Settings("RangeValue").Value = txtRange.Text
            If IsNumeric(txtXMargin.Text) Then Settings("XMargin").Value = txtXMargin.Text / 100
            If IsNumeric(txtYMargin.Text) Then Settings("YMargin").Value = txtYMargin.Text / 100
            Settings("RestrictMovement").Value = Not chkFreeMovement.IsChecked
            If IsNumeric(txtPriceInterval.Text) Then Settings("PriceBarTextInterval").Value = txtPriceInterval.Text
            If IsNumeric(txtTimeInterval.Text) Then Settings("TimeBarTextInterval").Value = txtTimeInterval.Text
            Settings("RAND.RandomMaxMove").Value = sldMax.Value
            Settings("RAND.RandomMinMove").Value = sldMin.Value
            Settings("RAND.RandomSpeed").Value = sldSpeed.Value
            Settings("ShowGrid").Value = chkShowGrid.IsChecked
            If IsNumeric(txtGridWidth.Text) Then Settings("GridWidth").Value = txtGridWidth.Text
            If IsNumeric(txtCenterOffset.Text) Then Settings("CenterScalingOffsetHeight").Value = txtCenterOffset.Text / 100
            If IsNumeric(txtUpdateBarInterval.Text) Then Settings("UpdateScalingBarInterval").Value = txtUpdateBarInterval.Text
            If cboInterval.SelectedItem IsNot Nothing Then Settings("BarSize").Value = [Enum].Parse(GetType(BarSize), cboInterval.SelectedItem)
            If IsNumeric(txtDaysBack.Text) Then Settings("DaysBack").Value = txtDaysBack.Text
            'If IsNumeric(txtEndDaysBack.Text) Then Settings("EndDaysBack").Value = txtEndDaysBack.Text
            If IsNumeric(txtDesiredBarCount.Text) Then Settings("DesiredBarCount").Value = txtDesiredBarCount.Text
            Settings("UpdateAnalysisTechniquesEveryTick").Value = chkUpdateEveryTick.IsChecked
            Settings("AutoScale").Value = chkAutoScale.IsChecked
            Chart.IB.UseCachedData(Chart.TickerID) = chkUseCache.IsChecked
            If IsNumeric(txtPriceTextDecimalCount.Text) Then Settings("DecimalPlaces").Value = txtPriceTextDecimalCount.Text
            If cboStatusTextLocation.SelectedIndex <> -1 Then Settings("StatusTextLocation").Value = cboStatusTextLocation.SelectedIndex
            If IsNumeric(txtDefaultQuantity.Text) Then Settings("DefaultOrderQuantity").Value = txtDefaultQuantity.Text
            Chart.DataStream.DaysBack(Chart.TickerID) = Settings("DaysBack").Value
            'Chart.DataStream.EndDaysBack(Chart.TickerID) = Settings("EndDaysBack").Value
            Chart.DataStream.UseReplayMode(Chart.TickerID) = Settings("UseReplayMode").Value
            _changedSettingNames = New List(Of String)
            For Each item In Settings.Settings
                If item.StringValue <> originalSettings(item.Name).StringValue Then
                    _changedSettingNames.Add(item.Name)
                End If
            Next
        End If
    End Sub

    Private Sub chkUseRandom_CheckedChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        CheckDirtiness()
    End Sub

    Private Sub cboInterval_SelectionChanged(sender As System.Object, e As System.Windows.Controls.SelectionChangedEventArgs)
        If IsLoaded Then
            Chart.IB.BarSize(Chart.TickerID) = [Enum].Parse(GetType(BarSize), cboInterval.SelectedItem)
        End If
        CheckDirtiness()
    End Sub

    Private Sub btnRollover_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles btnRollover.Click
        If Chart.Price = 0 Then
            ShowInfoBox("You must load data on the chart before updating this symbol.", Me)
        Else
            Dim win As New RolloverWindow(Chart)
            win.Owner = Me
            win.ShowDialog()
            If win.ButtonOKPressed Then
                chkRefresh.IsChecked = True
                UpdateCacheDataAvailability()
                ShowInfoBox("Rollover complete. You are now using " & Chart.IB.Contract(Chart.TickerID).Symbol & " " & GetContractMonthFormattedString(Chart.IB.Contract(Chart.TickerID).LastTradeDateOrContractMonth) & " data.", Me)
            End If
        End If
    End Sub

    Private Sub btnCopyColors_Click(sender As Object, e As RoutedEventArgs)
        For Each ch In My.Application.Charts
            If ch IsNot Chart Then
                For Each colSetting In Settings.Settings
                    If colSetting.IsColorSetting Then
                        For Each setting In ch.Settings.Settings
                            If setting.Name = colSetting.Name Then
                                setting.Value = colSetting.Value
                            End If
                        Next
                    End If
                Next
            End If
        Next
    End Sub

    Private Sub btnSymbolMaster_Click(sender As Object, e As RoutedEventArgs)
        For Each c In Chart.Parent.Charts
            If c IsNot Chart Then
                SetContractToContract(c.IB.Contract(c.TickerID), Chart.IB.Contract(Chart.TickerID))
                c.Settings("IsSymbolMaster").Value = False
            End If
        Next
        Chart.Settings("IsSymbolMaster").Value = True
        ShowInfoBox("Done. All charts in the current workspace now have the same symbol settings.", Me)
    End Sub

    Private Sub sldScalingFactor_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldScalingFactor.ValueChanged
        If Me.IsLoaded Then
            My.Application.DesktopScaleFactor = sldScalingFactor.Value
        End If
    End Sub
End Class

