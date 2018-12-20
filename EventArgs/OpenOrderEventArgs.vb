﻿' Copyright (C) 2018 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
' and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable.

Public Class OpenOrderEventArgs

    Property orderId As Integer

    Property contract As IBApi.Contract

    Property order As IBApi.Order

    Property orderState As IBApi.OrderState

End Class

