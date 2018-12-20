Public Class FormatOrderWindow
    Private _okPressed As Boolean
    Private trader As OrderControl
    Public Sub New(ByVal chartTrader As OrderControl)
        InitializeComponent()
        trader = chartTrader
        txtQuantity.Text = trader.Quantity
        cboOrderAction.ItemsSource = [Enum].GetNames(GetType(ActionSide))
        cboOrderType.ItemsSource = {"Limit", "Market", "Stop"} ' [Enum].GetNames(GetType(OrderType))
        cboOrderAction.SelectedItem = [Enum].GetName(GetType(ActionSide), trader.OrderAction)
        cboOrderType.SelectedItem = [Enum].GetName(GetType(OrderType), trader.OrderType)
        txtQuantity.Focus()
        txtQuantity.SelectionStart = 0
        txtQuantity.SelectionLength = txtQuantity.Text.Length
        Owner = chartTrader.Parent.DesktopWindow
        ShowDialog()
    End Sub

    Public ReadOnly Property Quantity As Integer
        Get
            Return txtQuantity.Text
        End Get
    End Property
    Public ReadOnly Property OrderAction As ActionSide
        Get
            Return ParseOrderAction(cboOrderAction.SelectedItem)
        End Get
    End Property
    Public ReadOnly Property OrderType As OrderType
        Get
            Return ParseOrderType(cboOrderType.SelectedItem)
        End Get
    End Property
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If Not Integer.TryParse(txtQuantity.Text, 0) OrElse Integer.Parse(txtQuantity.Text) < 0 Then
            ShowInfoBox("Please enter positive, integral data in the 'Quantity' textbox.", Me)
            txtQuantity.Focus()
        ElseIf cboOrderAction.SelectedIndex = -1 Then
            ShowInfoBox("Please choose an order action.", Me)
            cboOrderAction.Focus()
        ElseIf cboOrderType.SelectedIndex = -1 Then
            ShowInfoBox("Please choose an order action.", Me)
            cboOrderType.Focus()
        Else
            trader.Quantity = Quantity
            trader.OrderAction = OrderAction
            trader.OrderType = OrderType
            _okPressed = True
            Close()
        End If
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
End Class
