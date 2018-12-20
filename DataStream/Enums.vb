Public Module DataStreamEnums

    Public Enum HistoricalDataType
        Trades
        Midpoint
        Bid
        Ask
        BidAsk
    End Enum
    Public Enum GenericTickType
        AuctionPrice = 225
        HistoricalVolatility = 104
        IndexFuturePremium = 162
        MarkPrice = 221
        MiscellaneousStats = 165
        OptionImpliedVolatility = 106
        OptionOpenInterest = 101
        OptionVolume = 100
        RealTimeVolume = 233
        Shortable = 236
        Undefined = 0
    End Enum
    Public Enum ErrorMessage
        NoValidId = -1
        Undefined = 0
        AlreadyConnected = 501
        ConnectFail = 502
        UpdateTws = 503
        NotConnected = 504
        UnknownId = 505
        FailSendRequestMarket = 510
        FailSendCancelMarket = 511
        FailSendOrder = 512
        FailSendAccountUpdate = 513
        FailSendExecution = 514
        FailSendCancelOrder = 515
        FailSendOpenOrder = 516
        UnknownContract = 517
        FailSendRequestContract = 518
        FailSendRequestMarketDepth = 519
        FailSendCancelMarketDepth = 520
        FailSendServerLogLevel = 521
        FailSendFARequest = 522
        FailSendFAReplace = 523
        FailSendRequestScanner = 524
        FailSendCancelScanner = 525
        FailSendRequestScannerParameters = 526
        FailSendRequestHistoricalData = 527
        FailSendCancelHistoricalData = 528
        FailSendRequestRealTimeBars = 529
        FailSendCancelRealTimeBars = 530
        FailSendRequestCurrentTime = 531
        FailSendRequestFundData = 532
        FailSendCancelFundData = 533
        FailSendReqCalcImpliedVolatility = 534
    End Enum
    Public Enum BarSize
        OneSecond = 1
        FiveSeconds = 2
        FifteenSeconds = 3
        ThirtySeconds = 4
        OneMinute = 5
        TwoMinutes = 6
        FiveMinutes = 7
        FifteenMinutes = 8
        ThirtyMinutes = 9
        OneHour = 10
        OneDay = 11
        OneWeek = 12
        OneMonth = 13
        OneYear = 14
    End Enum
    Public Enum TickType
        BidSize
        BidPrice
        AskPrice
        AskSize
        LastPrice
        LastSize
        High
        Low
        Volume
        Close
        BidOptComp
        AskOptComp
        LastOptComp
        ModelOption
        Open
        Low13Week
        High13Week
        Low26Week
        High26Week
        Low52Week
        High52Week
        AvgVolume
        OpenInterest
        OptionHistoricalVolatility
        OptionImpliedVolatility
        OptionBidExchStr
        OptionAskExchStr
        OptionCallOpenInterest
        OptionPutOpenInterest
        OptionCallVolume
        OptionPutVolume
        IndexFuturePremium
        BidExch
        AskExch
        AuctionVolume
        AuctionPrice
        AuctionImbalance
        MarkPrice
        BidEFP
        AskEFP
        LastEFP
        OpenEFP
        HighEFP
        LowEFP
        CloseEFP
        LastTimestamp
        Shortable
        Fundamentals
        RTVolume
        Halted
        BidYield
        AskYield
        LastYield
        CustOptComp
        Trades
        TradesPerMin
        VolumePerMin
        LastRTHTrade
        RTHistoricalVol
        IBDividends
        BondFactorMultiplier
        RegulatoryImbalance
        NewsTick
        ShortTermVolume3Min
        ShortTermVolume5Min
        ShortTermVolume10Min
        DelayedBid
        DelayedAsk
        DelayedLast
        DelayedBidSize
        DelayedAskSize
        DelayedLastSize
        DelayedHigh
        DelayedLow
        DelayedVolume
        DelayedClose
        DelayedOpen
        RTTrdVolume
        CreditmanMarkPrice
        CreditmanSlowMarkPrice
        DelayedBidOptComp
        DelayedAskOptComp
        DelayedLastOptComp
        DelayedModelOptComp
        LastExchange
        LastRegTime
        FuturesOpenInterest
        AvgOptVolume
        DelayedLastTimestamp
        Unknown
    End Enum
    Public Enum ActionSide
        BUY = 0
        SELL = 1
        Undefined = 2
        SShort = 3
        SShortX = 4
    End Enum
    Public Enum OrderType
        MKT = 0
        LMT = 2
        STP = 5
        None
    End Enum
    Public Enum OrderStatus
        PendingSubmit = 0
        PendingCancel = 1
        PreSubmitted = 2
        Submitted = 3
        Cancelled = 4
        Filled = 5
        Inactive = 6
        PartiallyFilled = 7
        ApiPending = 8
        ApiCancelled = 9
        [Error] = 10
        None = 11
    End Enum
    Public Enum SecurityType
        STK = 0
        OPT = 1
        FUT = 2
        IND = 3
        CASH = 5
        BAG = 6
        BOND = 7
        WAR = 8
        Undefined = 9
    End Enum
    Public Enum SecurityIdType
        None = 0
        ISIN = 1
        CUSIP = 2
        SEDOL = 3
        RIC = 4
    End Enum
    Public Enum RightType
        Put = 0
        [Call] = 1
        Undefined = 2
    End Enum
    Public Enum TimeInForce
        DAY = 0
        GTC = 1
        IOC = 2
        FOK = 3
        GTD = 4
        Undefined = 6
    End Enum
    Public Enum OcaType
        Undefined = 0
        CancelAll = 1
        ReduceWithBlock = 2
        ReduceWithNoBlock = 3
    End Enum

    Public Function ParseSecType(secType As String) As SecurityType
        Return If(secType IsNot Nothing, [Enum].Parse(GetType(SecurityType), secType), SecurityType.Undefined)
    End Function
    Public Function ParseSecIDType(secIDType As String) As SecurityIdType
        Return If(secIDType IsNot Nothing, [Enum].Parse(GetType(SecurityIdType), secIDType), SecurityIdType.None)
    End Function
    Public Function ParseRightType(rightType As String) As RightType
        Return If(rightType IsNot Nothing, [Enum].Parse(GetType(RightType), rightType), QuickTrader.RightType.Undefined)
    End Function
    Public Function ParseWhatToShowType(whattoShowType As String) As HistoricalDataType
        Return If(whattoShowType IsNot Nothing, [Enum].Parse(GetType(HistoricalDataType), whattoShowType), HistoricalDataType.Trades)
    End Function
    Public Function ParseOrderStatus(orderStatus As String) As OrderStatus
        Return If(orderStatus IsNot Nothing, [Enum].Parse(GetType(OrderStatus), orderStatus), QuickTrader.OrderStatus.None)
    End Function
    Public Function ParseOrderAction(orderAction As String) As ActionSide
        Return If(orderAction IsNot Nothing, [Enum].Parse(GetType(ActionSide), orderAction), ActionSide.Undefined)
    End Function
    Public Function ParseOrderType(orderType As String) As OrderType
        Return If(orderType IsNot Nothing, [Enum].Parse(GetType(OrderType), orderType), QuickTrader.OrderType.None)
    End Function
    Public Function ParseTimeInForce(timeInForce As String) As TimeInForce
        Return If(timeInForce IsNot Nothing, [Enum].Parse(GetType(TimeInForce), timeInForce), QuickTrader.TimeInForce.Undefined)
    End Function

    Public Function ParseSecType(secType As SecurityType) As String
        Return [Enum].GetName(GetType(SecurityType), secType)
    End Function
    Public Function ParseSecIDType(secIDType As SecurityIdType) As String
        Return [Enum].GetName(GetType(SecurityIdType), secIDType)
    End Function
    Public Function ParseRightType(rightType As RightType) As String
        Return [Enum].GetName(GetType(RightType), rightType)
    End Function
    Public Function ParseWhatToShowType(whattoShowType As HistoricalDataType) As String
        Return [Enum].GetName(GetType(HistoricalDataType), whattoShowType)
    End Function
    Public Function ParseOrderStatus(orderStatus As OrderStatus) As String
        Return [Enum].GetName(GetType(OrderStatus), orderStatus)
    End Function
    Public Function ParseOrderAction(orderAction As ActionSide) As String
        Return [Enum].GetName(GetType(ActionSide), orderAction)
    End Function
    Public Function ParseOrderType(orderType As OrderType) As String
        Return [Enum].GetName(GetType(OrderType), orderType)
    End Function
    Public Function ParseTimeInForce(timeInForce As TimeInForce) As String
        Return [Enum].GetName(GetType(TimeInForce), timeInForce)
    End Function
End Module
