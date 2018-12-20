﻿' Copyright (C) 2018 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
' and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable.


Public Class TickReqParamsEventArgs

    Property tickerId As Integer

    Property minTick As Double

    Property bboExchange As String

    Property snapshotPermissions As Integer

End Class
