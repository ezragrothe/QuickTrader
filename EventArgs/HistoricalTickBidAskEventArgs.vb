﻿' Copyright (C) 2018 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
' and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable.

Imports IBApi

Public Class HistoricalTicksBidAskEventArgs

    Property reqId As Integer

    Property ticks As HistoricalTickBidAsk()

End Class
