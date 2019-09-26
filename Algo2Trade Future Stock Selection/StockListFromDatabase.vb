Imports System.Threading
Imports Algo2TradeBLL
Imports MySql.Data.MySqlClient
Imports Utilities.Network
Imports System.Net.Http

Public Class StockListFromDatabase
    Implements IDisposable

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

    Private ReadOnly _cts As CancellationTokenSource
    Private ReadOnly _common As Common
    Private _conn As MySqlConnection
    Private ReadOnly ZerodhaEODHistoricalURL = "https://kitecharts-aws.zerodha.com/api/chart/{0}/day?api_key=kitefront&access_token=K&from={1}&to={2}"
    Private ReadOnly ZerodhaIntradayHistoricalURL = "https://kitecharts-aws.zerodha.com/api/chart/{0}/minute?api_key=kitefront&access_token=K&from={1}&to={2}"

    Public Sub New(ByVal canceller As CancellationTokenSource)
        _cts = canceller
        _common = New Common(_cts)
        AddHandler _common.Heartbeat, AddressOf OnHeartbeat
    End Sub

#Region "Private Class & Enum"
    Private Class ActiveInstrumentData
        Public Property Token As Integer
        Public Property TradingSymbol As String
        Public Property Expiry As Date
        Public Property LastDayClose As Decimal
        Public ReadOnly Property RawInstrumentName As String
            Get
                Return Me.TradingSymbol.Remove(Me.TradingSymbol.Count - 8)
            End Get
        End Property
        Public Property CashInstrumentName As String
        Public Property CashInstrumentToken As String
    End Class

    Enum TypeOfData
        Intraday = 1
        EOD
    End Enum
#End Region

#Region "Private Functions"
    Private Async Function GetHistoricalCandleStickAsync(ByVal instrumentToken As String, ByVal fromDate As Date, ByVal toDate As Date, ByVal historicalDataType As TypeOfData) As Task(Of Dictionary(Of String, Object))
        Dim ret As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim historicalDataURL As String = Nothing
        Select Case historicalDataType
            Case TypeOfData.Intraday
                historicalDataURL = String.Format(ZerodhaIntradayHistoricalURL, instrumentToken, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))
            Case TypeOfData.EOD
                historicalDataURL = String.Format(ZerodhaEODHistoricalURL, instrumentToken, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))
        End Select
        OnHeartbeat(String.Format("Fetching historical Data: {0}", historicalDataURL))
        'Using sr As New StreamReader(HttpWebRequest.Create(historicalDataURL).GetResponseAsync().Result.GetResponseStream)
        '    Dim jsonString = Await sr.ReadToEndAsync.ConfigureAwait(False)
        '    Dim retDictionary As Dictionary(Of String, Object) = StringManipulation.JsonDeserialize(jsonString)
        '    Return retDictionary
        'End Using
        Dim proxyToBeUsed As HttpProxy = Nothing
        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            'Get to the landing page first
            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(historicalDataURL,
                                                                                HttpMethod.Get,
                                                                                Nothing,
                                                                                True,
                                                                                Nothing,
                                                                                True,
                                                                                "application/json").ConfigureAwait(False)
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response while getting historical data for: {0}", historicalDataURL))
            End If
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                ret = l.Item2
            End If
            RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
            RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
            RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        End Using
        Return ret
    End Function

    Private Async Function GetChartFromHistoricalAsync(ByVal historicalCandlesJSONDict As Dictionary(Of String, Object),
                                                       ByVal tradingSymbol As String,
                                                       ByVal tradingDate As Date) As Task(Of Dictionary(Of Date, Payload))
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        If historicalCandlesJSONDict.ContainsKey("data") Then
            Dim historicalCandlesDict As Dictionary(Of String, Object) = historicalCandlesJSONDict("data")
            If historicalCandlesDict.ContainsKey("candles") AndAlso historicalCandlesDict("candles").count > 0 Then
                Dim historicalCandles As ArrayList = historicalCandlesDict("candles")
                If ret Is Nothing Then ret = New Dictionary(Of Date, Payload)
                OnHeartbeat(String.Format("Generating Payload for {0} of {1}", tradingSymbol, tradingDate.ToShortDateString))
                Dim previousPayload As Payload = Nothing
                For Each historicalCandle In historicalCandles
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim runningSnapshotTime As Date = Utilities.Time.GetDateTimeTillMinutes(historicalCandle(0))

                    Dim runningPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
                    With runningPayload
                        .PayloadDate = Utilities.Time.GetDateTimeTillMinutes(historicalCandle(0))
                        .TradingSymbol = tradingSymbol
                        .Open = historicalCandle(1)
                        .High = historicalCandle(2)
                        .Low = historicalCandle(3)
                        .Close = historicalCandle(4)
                        .Volume = historicalCandle(5)
                        .PreviousCandlePayload = previousPayload
                    End With
                    previousPayload = runningPayload
                    ret.Add(runningSnapshotTime, runningPayload)
                Next
            End If
        End If
        Return ret
    End Function
#End Region

#Region "Procedure to get stock"
    Private Async Function GetATRBasedAllStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, String()))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
            _cts.Token.ThrowIfCancellationRequested()
            _conn = _common.OpenDBConnection()
        End If
        Dim ret As Dictionary(Of String, String()) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim previousTradingDay As Date = _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
        If previousTradingDay <> Date.MinValue Then
            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
                _cts.Token.ThrowIfCancellationRequested()
                _conn = _common.OpenDBConnection()
            End If
            _cts.Token.ThrowIfCancellationRequested()
            Dim cm As MySqlCommand = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `AS_ON_DATE`=@sd", _conn)
            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
            _cts.Token.ThrowIfCancellationRequested()
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            _cts.Token.ThrowIfCancellationRequested()
            Dim dt As DataTable = New DataTable
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
            Dim nfoInstruments As List(Of ActiveInstrumentData) = Nothing
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For i = 0 To dt.Rows.Count - 1
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim instrumentData As New ActiveInstrumentData With
                    {.Token = dt.Rows(i).Item(0),
                     .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                     .Expiry = dt.Rows(i).Item(2)}
                    If nfoInstruments Is Nothing Then nfoInstruments = New List(Of ActiveInstrumentData)
                    nfoInstruments.Add(instrumentData)
                Next
            End If
            If nfoInstruments IsNot Nothing AndAlso nfoInstruments.Count > 0 Then
                Dim lastTradingDay As Date = Date.MinValue
                Dim currentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
                For Each runningInstrument In nfoInstruments
                    If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                        Dim availableInstrument As IEnumerable(Of ActiveInstrumentData) = currentNFOInstruments.FindAll(Function(z)
                                                                                                                            Return z.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                                        End Function)
                        If availableInstrument IsNot Nothing AndAlso availableInstrument.Count > 0 Then
                            Continue For
                        End If
                    End If
                    Dim runningIntruments As IEnumerable(Of ActiveInstrumentData) = nfoInstruments.Where(Function(x)
                                                                                                             Return x.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                         End Function)
                    Dim minExpiry As Date = runningIntruments.Min(Function(x)
                                                                      If x.Expiry.Date = Now.Date Then
                                                                          Return Date.MaxValue
                                                                      Else
                                                                          Return x.Expiry
                                                                      End If
                                                                  End Function)
                    Dim currentIntrument As ActiveInstrumentData = runningIntruments.ToList.Find(Function(y)
                                                                                                     Return y.Expiry.Date = minExpiry.Date
                                                                                                 End Function)
                    If currentIntrument IsNot Nothing Then
                        If currentNFOInstruments Is Nothing Then currentNFOInstruments = New List(Of ActiveInstrumentData)
                        currentNFOInstruments.Add(currentIntrument)
                    End If
                Next
                If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                    Dim priceFilterdCurrentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
                    For Each runningInstrument In currentNFOInstruments
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim previousDayPayloads As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures, runningInstrument.TradingSymbol, previousTradingDay.AddDays(-10), previousTradingDay)
                        Dim lastDayPayload As Payload = Nothing
                        If previousDayPayloads IsNot Nothing AndAlso previousDayPayloads.Count > 0 Then
                            lastDayPayload = previousDayPayloads.LastOrDefault.Value
                        End If
                        If lastDayPayload IsNot Nothing AndAlso lastDayPayload.Close >= My.Settings.MinClose AndAlso lastDayPayload.Close <= My.Settings.MaxClose Then
                            Dim rawCashInstrument As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Cash, previousTradingDay, runningInstrument.RawInstrumentName)
                            If rawCashInstrument IsNot Nothing Then
                                runningInstrument.CashInstrumentToken = rawCashInstrument.Item1
                                runningInstrument.CashInstrumentName = rawCashInstrument.Item2
                                runningInstrument.LastDayClose = lastDayPayload.Close
                                If priceFilterdCurrentNFOInstruments Is Nothing Then priceFilterdCurrentNFOInstruments = New List(Of ActiveInstrumentData)
                                priceFilterdCurrentNFOInstruments.Add(runningInstrument)
                            End If
                        End If
                    Next
                    Dim highATRStocks As Concurrent.ConcurrentDictionary(Of String, Decimal()) = Nothing
                    Try
                        If priceFilterdCurrentNFOInstruments IsNot Nothing AndAlso priceFilterdCurrentNFOInstruments.Count > 0 Then
                            For i As Integer = 0 To priceFilterdCurrentNFOInstruments.Count - 1 Step 20
                                Dim numberOfData As Integer = If(priceFilterdCurrentNFOInstruments.Count - i > 20, 20, priceFilterdCurrentNFOInstruments.Count - i)
                                Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
                                tasks = priceFilterdCurrentNFOInstruments.GetRange(i, numberOfData).Select(Async Function(x)
                                                                                                               Try
                                                                                                                   Dim rawCashInstrument As Tuple(Of String, String) = New Tuple(Of String, String)(x.CashInstrumentToken, x.CashInstrumentName)
                                                                                                                   If rawCashInstrument IsNot Nothing Then
                                                                                                                       Dim instrumentData As KeyValuePair(Of Integer, String) = New KeyValuePair(Of Integer, String)(rawCashInstrument.Item1, rawCashInstrument.Item2)
                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                       Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Await GetHistoricalCandleStickAsync(instrumentData.Key, previousTradingDay.AddDays(-300), previousTradingDay, TypeOfData.EOD).ConfigureAwait(False)
                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                       If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 Then
                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                                           Dim eodHistoricalData As Dictionary(Of Date, Payload) = Await GetChartFromHistoricalAsync(historicalCandlesJSONDict, instrumentData.Value, tradingDate).ConfigureAwait(False)
                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                                           If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 0 Then
                                                                                                                               _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               Dim ATRPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                                                                               Indicator.ATR.CalculateATR(14, eodHistoricalData, ATRPayload)
                                                                                                                               _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               If ATRPayload IsNot Nothing AndAlso ATRPayload.Count > 0 Then
                                                                                                                                   Dim lastDayClosePrice As Decimal = eodHistoricalData.LastOrDefault.Value.Close
                                                                                                                                   'lastTradingDay = eodHistoricalData.LastOrDefault.Key
                                                                                                                                   'If lastDayClosePrice >= My.Settings.MinClose AndAlso lastDayClosePrice <= My.Settings.MaxClose Then
                                                                                                                                   Dim atrPercentage As Decimal = (ATRPayload(eodHistoricalData.LastOrDefault.Key) / lastDayClosePrice) * 100
                                                                                                                                   If atrPercentage >= My.Settings.ATRPercentage Then
                                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                                       Dim volumePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = eodHistoricalData.OrderByDescending(Function(z)
                                                                                                                                                                                                                                                     Return z.Key
                                                                                                                                                                                                                                                 End Function).Take(5)
                                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                                       If volumePayload IsNot Nothing AndAlso volumePayload.Count > 0 Then
                                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                                                           Dim avgVolume As Decimal = volumePayload.Average(Function(z)
                                                                                                                                                                                                Return z.Value.Volume
                                                                                                                                                                                            End Function)
                                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                                                           If avgVolume >= (My.Settings.PotentialAmount / 100) * lastDayClosePrice Then
                                                                                                                                               If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentDictionary(Of String, Decimal())
                                                                                                                                               highATRStocks.TryAdd(instrumentData.Value, {atrPercentage, x.LastDayClose, ATRPayload(eodHistoricalData.LastOrDefault.Key)})
                                                                                                                                           End If
                                                                                                                                       End If
                                                                                                                                   End If
                                                                                                                                   'End If
                                                                                                                               End If
                                                                                                                           End If
                                                                                                                       End If
                                                                                                                   End If
                                                                                                               Catch ex As Exception
                                                                                                                   Console.WriteLine(String.Format("{0}:{1}", x.TradingSymbol, ex.ToString))
                                                                                                                   Throw ex
                                                                                                               End Try
                                                                                                               Return True
                                                                                                           End Function)

                                Dim mainTask As Task = Task.WhenAll(tasks)
                                Await mainTask.ConfigureAwait(False)
                                If mainTask.Exception IsNot Nothing Then
                                    Throw mainTask.Exception
                                End If
                            Next
                        End If
                    Catch cex As TaskCanceledException
                        Throw cex
                    Catch aex As AggregateException
                        Throw aex
                    Catch ex As Exception
                        Throw ex
                    End Try


                    If highATRStocks IsNot Nothing AndAlso highATRStocks.Count > 0 Then
                        For Each runningStock In highATRStocks.OrderByDescending(Function(x)
                                                                                     Return x.Value(0)
                                                                                 End Function)
                            _cts.Token.ThrowIfCancellationRequested()
                            Dim currentTradingSymbol As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Futures, tradingDate, runningStock.Key)
                            If currentTradingSymbol IsNot Nothing Then
                                Dim lotSize As Integer = _common.GetLotSize(Common.DataBaseTable.EOD_Futures, currentTradingSymbol.Item2, tradingDate)
                                If ret Is Nothing Then ret = New Dictionary(Of String, String())
                                ret.Add(runningStock.Key, {runningStock.Value(0), lotSize, runningStock.Value(1), runningStock.Value(2)})
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Async Function GetTrendingStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, String()))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
            _cts.Token.ThrowIfCancellationRequested()
            _conn = _common.OpenDBConnection()
        End If
        Dim ret As Dictionary(Of String, String()) = Nothing
        Dim dt As DataTable = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        If _conn.State = ConnectionState.Open Then
            _cts.Token.ThrowIfCancellationRequested()
            OnHeartbeat(String.Format("Fetching Trending Stock Data for {0}", tradingDate.ToShortDateString))
            Dim cmd As New MySqlCommand("GET_TRENDING_STOCKS_FROM_PRE_MARKET_DATA_ATR_VOLUME_ALL_DATES", _conn)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@startDate", tradingDate)
            cmd.Parameters.AddWithValue("@endDate", tradingDate)
            cmd.Parameters.AddWithValue("@numberOfRecords", My.Settings.ProcedureNumberOfRecords)
            cmd.Parameters.AddWithValue("@minClose", My.Settings.MinClose)
            cmd.Parameters.AddWithValue("@maxClose", My.Settings.MaxClose)
            cmd.Parameters.AddWithValue("@atrPercentage", My.Settings.ATRPercentage)
            cmd.Parameters.AddWithValue("@potentialAmount", My.Settings.PotentialAmount)
            cmd.Parameters.AddWithValue("@minCapPercentage", My.Settings.MinCapPercentage)
            cmd.Parameters.AddWithValue("@sortColumn", "")

            Dim adapter As New MySqlDataAdapter(cmd)
            adapter.SelectCommand.CommandTimeout = 3000
            dt = New DataTable
            _cts.Token.ThrowIfCancellationRequested()
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
        End If
        _cts.Token.ThrowIfCancellationRequested()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            For i = 0 To dt.Rows.Count - 1
                _cts.Token.ThrowIfCancellationRequested()
                If Not IsDBNull(dt.Rows(i).Item(1)) AndAlso Not IsDBNull(dt.Rows(i).Item(10)) AndAlso
                    Not IsDBNull(dt.Rows(i).Item(11)) AndAlso Not IsDBNull(dt.Rows(i).Item(15)) Then
                    If ret Is Nothing Then ret = New Dictionary(Of String, String())
                    ret.Add(dt.Rows(i).Item(1), {dt.Rows(i).Item(10), dt.Rows(i).Item(11), dt.Rows(i).Item(15)})
                End If
            Next
        End If
        Return ret
    End Function

    Private Async Function GetPreMarketStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, String()))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of String, String()) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim highATRStockList As Dictionary(Of String, String()) = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
        _cts.Token.ThrowIfCancellationRequested()
        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
                _cts.Token.ThrowIfCancellationRequested()
                _conn = _common.OpenDBConnection()
            End If
            _cts.Token.ThrowIfCancellationRequested()
            Dim cm As MySqlCommand = New MySqlCommand("SELECT * FROM `v_pre_market` WHERE `APPLICABLE_DATE`=@sd", _conn)
            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
            _cts.Token.ThrowIfCancellationRequested()
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            _cts.Token.ThrowIfCancellationRequested()
            Dim dt As DataTable = New DataTable
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
                For i = 0 To dt.Rows.Count - 1
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim instrumentName As String = dt.Rows(i).Item(1).ToString.ToUpper
                    Dim changePer As String = Math.Round(((dt.Rows(i).Item(4) / dt.Rows(i).Item(7)) - 1) * 100, 2)
                    If highATRStockList.ContainsKey(instrumentName) Then
                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                        tempStockList.Add(instrumentName, {highATRStockList(instrumentName)(0), highATRStockList(instrumentName)(1), changePer, dt.Rows(i).Item(7)})
                    End If
                Next
                If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                    For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                 Return Math.Abs(x.Value(2))
                                                                             End Function)
                        If ret Is Nothing Then ret = New Dictionary(Of String, String())
                        ret.Add(runningStock.Key, {runningStock.Value(0), runningStock.Value(1), runningStock.Value(2), runningStock.Value(3)})
                    Next
                End If
            End If
        End If
        Return ret
    End Function

    Private Async Function GetIntradayVolumeSpikeStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, String()))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of String, String()) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim highATRStockList As Dictionary(Of String, String()) = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
        _cts.Token.ThrowIfCancellationRequested()
        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
            For Each runningStock In highATRStockList.Keys
                _cts.Token.ThrowIfCancellationRequested()
                Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, runningStock, tradingDate.AddDays(-15), tradingDate)
                If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                    Dim currentDayVolumeSum As Long = 0
                    Dim previousDaysVolumeSum As Long = 0
                    Dim counter As Integer = 0
                    For Each runningPayload In intradayPayload.Keys.OrderByDescending(Function(x)
                                                                                          Return x
                                                                                      End Function)
                        Dim firstCandle As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, 9, 15, 0)
                        Dim secondCandle As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, 9, 16, 0)
                        If runningPayload.Date = tradingDate.Date Then
                            If runningPayload = firstCandle OrElse runningPayload = secondCandle Then
                                currentDayVolumeSum += intradayPayload(runningPayload).Volume
                            End If
                        ElseIf runningPayload.Date < tradingDate.Date Then
                            If runningPayload = firstCandle OrElse runningPayload = secondCandle Then
                                previousDaysVolumeSum += intradayPayload(runningPayload).Volume
                                counter += 1
                                If counter = 10 Then Exit For
                            End If
                        End If
                    Next
                    If currentDayVolumeSum <> 0 AndAlso previousDaysVolumeSum <> 0 Then
                        Dim changePer As Decimal = ((currentDayVolumeSum / (previousDaysVolumeSum / 5)) - 1) * 100
                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                        tempStockList.Add(runningStock, {highATRStockList(runningStock)(0), highATRStockList(runningStock)(1), changePer, highATRStockList(runningStock)(3)})
                    End If
                End If
            Next
            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                             Return x.Value(2)
                                                                         End Function)
                    If ret Is Nothing Then ret = New Dictionary(Of String, String())
                    ret.Add(runningStock.Key, {runningStock.Value(0), runningStock.Value(1), runningStock.Value(2), runningStock.Value(3)})
                Next
            End If
        End If
        Return ret
    End Function

    Private Async Function GetHighLowATRStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, String()))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of String, String()) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim highATRStockList As Dictionary(Of String, String()) = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
        _cts.Token.ThrowIfCancellationRequested()
        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempStockList As Dictionary(Of String, String()) = Nothing
            For Each runningStock In highATRStockList.Keys
                _cts.Token.ThrowIfCancellationRequested()
                Dim tradingSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.Intraday_Futures, tradingDate, runningStock)
                If tradingSymbolToken IsNot Nothing Then
                    Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures, tradingSymbolToken.Item2, tradingDate.AddDays(-15), tradingDate)
                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                        Dim previousDayATR As Decimal = highATRStockList(runningStock)(3)
                        For Each runningPayload In intradayPayload.Keys
                            If runningPayload.Date = tradingDate.Date Then
                                Dim dayHigh As Decimal = intradayPayload.Values.Max(Function(x)
                                                                                        If x.PayloadDate.Date = tradingDate.Date AndAlso x.PayloadDate <= runningPayload Then
                                                                                            Return x.High
                                                                                        Else
                                                                                            Return Decimal.MinValue
                                                                                        End If
                                                                                    End Function)
                                Dim dayLow As Decimal = intradayPayload.Values.Min(Function(x)
                                                                                       If x.PayloadDate.Date = tradingDate.Date AndAlso x.PayloadDate <= runningPayload Then
                                                                                           Return x.Low
                                                                                       Else
                                                                                           Return Decimal.MaxValue
                                                                                       End If
                                                                                   End Function)
                                If dayHigh - dayLow >= previousDayATR * 85 / 100 Then
                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                    tempStockList.Add(runningStock, {highATRStockList(runningStock)(0), highATRStockList(runningStock)(1), runningPayload.ToShortTimeString})
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End If
            Next
            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                             Return x.Value(0)
                                                                         End Function)
                    If ret Is Nothing Then ret = New Dictionary(Of String, String())
                    ret.Add(runningStock.Key, runningStock.Value)
                Next
            End If
        End If
        Return ret
    End Function

    'Private Async Function GetIntradayVolumeSpikeWithLowSLStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, String()))
    '    Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
    '    Dim ret As Dictionary(Of String, String()) = Nothing
    '    _cts.Token.ThrowIfCancellationRequested()
    '    Dim highATRStockList As Dictionary(Of String, String()) = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
    '    _cts.Token.ThrowIfCancellationRequested()
    '    If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
    '        _cts.Token.ThrowIfCancellationRequested()
    '        Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
    '        For Each runningStock In highATRStockList.Keys
    '            _cts.Token.ThrowIfCancellationRequested()
    '            Dim dayATR As Decimal = highATRStockList(runningStock)(3)
    '            Dim close As Decimal = highATRStockList(runningStock)(2)
    '            Dim lotSize As Integer = highATRStockList(runningStock)(1)
    '            Dim capital As Decimal = close * lotSize / 30
    '            Dim quanity As Integer = lotSize
    '            If capital < 10000 Then quanity = 2 * lotSize
    '            Dim stoploss As Decimal = CalculatorStoploss(close, quanity, 1000)
    '            Dim slPoint As Decimal = close - stoploss
    '            If capital <= 25000 AndAlso slPoint * 4 <= dayATR / 2 Then
    '                Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, runningStock, tradingDate.AddDays(-15), tradingDate)
    '                If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
    '                    Dim currentDayVolumeSum As Long = 0
    '                    Dim previousDaysVolumeSum As Long = 0
    '                    Dim counter As Integer = 0
    '                    For Each runningPayload In intradayPayload.Keys.OrderByDescending(Function(x)
    '                                                                                          Return x
    '                                                                                      End Function)
    '                        Dim firstCandle As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, 9, 15, 0)
    '                        Dim secondCandle As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, 9, 16, 0)
    '                        If runningPayload.Date = tradingDate.Date Then
    '                            If runningPayload = firstCandle OrElse runningPayload = secondCandle Then
    '                                currentDayVolumeSum += intradayPayload(runningPayload).Volume
    '                            End If
    '                        ElseIf runningPayload.Date < tradingDate.Date Then
    '                            If runningPayload = firstCandle OrElse runningPayload = secondCandle Then
    '                                previousDaysVolumeSum += intradayPayload(runningPayload).Volume
    '                                counter += 1
    '                                If counter = 10 Then Exit For
    '                            End If
    '                        End If
    '                    Next
    '                    If currentDayVolumeSum <> 0 AndAlso previousDaysVolumeSum <> 0 Then
    '                        Dim changePer As Decimal = ((currentDayVolumeSum / (previousDaysVolumeSum / 5)) - 1) * 100
    '                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
    '                        tempStockList.Add(runningStock, {highATRStockList(runningStock)(0), highATRStockList(runningStock)(1), changePer, slPoint, dayATR})
    '                    End If
    '                End If
    '            End If
    '        Next
    '        If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
    '            For Each runningStock In tempStockList.OrderByDescending(Function(x)
    '                                                                         Return x.Value(2)
    '                                                                     End Function)
    '                If ret Is Nothing Then ret = New Dictionary(Of String, String())
    '                ret.Add(runningStock.Key, {runningStock.Value(0), runningStock.Value(1), runningStock.Value(2), runningStock.Value(3), runningStock.Value(4)})
    '            Next
    '        End If
    '    End If
    '    Return ret
    'End Function
#End Region

#Region "Main Public Function"
    Public Async Function GetStockData(ByVal tradingDate As Date,
                                       ByVal procedureToRun As Integer,
                                       ByVal bannedStocks As List(Of String),
                                       ByVal userGivenInstrumentList As Dictionary(Of String, Decimal())) As Task(Of Dictionary(Of String, InstrumentDetails))
        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
        Dim stockList As Dictionary(Of String, String()) = Nothing
        Dim isTradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
        If isTradingDay OrElse tradingDate.Date = Now.Date Then
            Select Case procedureToRun
                Case 0
                    stockList = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
                Case 1
                    stockList = Await GetTrendingStockDataAsync(tradingDate).ConfigureAwait(False)
                Case 2
                    If userGivenInstrumentList IsNot Nothing AndAlso userGivenInstrumentList.Count > 0 Then
                        For Each ruuningUserGivenStock In userGivenInstrumentList
                            _cts.Token.ThrowIfCancellationRequested()
                            Dim currentTradingSymbol As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Futures, tradingDate, ruuningUserGivenStock.Key)
                            If currentTradingSymbol IsNot Nothing Then
                                Dim lotSize As Integer = _common.GetLotSize(Common.DataBaseTable.EOD_Futures, currentTradingSymbol.Item2, tradingDate)
                                If stockList Is Nothing Then stockList = New Dictionary(Of String, String())
                                stockList.Add(ruuningUserGivenStock.Key, {0, lotSize})
                            End If
                        Next
                    End If
                Case 3
                    stockList = Await GetPreMarketStockDataAsync(tradingDate).ConfigureAwait(False)
                Case 4
                    stockList = Await GetIntradayVolumeSpikeStockDataAsync(tradingDate).ConfigureAwait(False)
                Case 5
                    stockList = Await GetHighLowATRStockDataAsync(tradingDate).ConfigureAwait(False)
            End Select
            _cts.Token.ThrowIfCancellationRequested()

            If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                For Each stock In stockList.Keys
                    _cts.Token.ThrowIfCancellationRequested()
                    If bannedStocks Is Nothing OrElse
                    (bannedStocks IsNot Nothing AndAlso bannedStocks.Count > 0 AndAlso Not bannedStocks.Contains(stock.ToUpper)) Then
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim instrumentDetails As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.Intraday_Futures, tradingDate, stock)
                        If instrumentDetails IsNot Nothing Then
                            Dim tradingSymbol As String = instrumentDetails.Item2
                            _cts.Token.ThrowIfCancellationRequested()
                            If tradingSymbol IsNot Nothing AndAlso tradingSymbol <> "" Then
                                _cts.Token.ThrowIfCancellationRequested()
                                If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
                                Dim instrumentData As New InstrumentDetails With
                                {.TradingSymbol = tradingSymbol,
                                 .ATRPercentage = stockList(stock)(0),
                                 .LotSize = stockList(stock)(1),
                                 .Supporting1 = If(stockList(stock).Count > 2, stockList(stock)(2), 0),
                                 .Supporting2 = If(stockList(stock).Count > 3, stockList(stock)(3), 0),
                                 .Supporting3 = If(stockList(stock).Count > 4, stockList(stock)(4), 0)}
                                ret.Add(tradingSymbol, instrumentData)
                            End If
                        End If
                    End If
                Next
            End If
        End If
        Return ret
    End Function
#End Region

#Region "Supporting Public Function"
    Public Async Function GetActiveInstrumentData(ByVal tradingDate As Date, ByVal stockList As Dictionary(Of String, InstrumentDetails)) As Task(Of Dictionary(Of String, InstrumentDetails))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
        If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
            For Each stock In stockList.Keys
                _cts.Token.ThrowIfCancellationRequested()
                If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
                    _cts.Token.ThrowIfCancellationRequested()
                    _conn = _common.OpenDBConnection()
                End If
                _cts.Token.ThrowIfCancellationRequested()
                Dim rawInstrumentName As String = stock
                If stock.ToUpper.Contains("FUT") Then
                    rawInstrumentName = stock.Remove(stock.Count - 8)
                End If
                _cts.Token.ThrowIfCancellationRequested()
                Dim cm As MySqlCommand = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` LIKE @trd AND `AS_ON_DATE`=@sd", _conn)
                cm.Parameters.AddWithValue("@trd", String.Format("{0}%", rawInstrumentName))
                cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
                _cts.Token.ThrowIfCancellationRequested()
                Dim adapter As New MySqlDataAdapter(cm)
                adapter.SelectCommand.CommandTimeout = 300
                Dim dt As New DataTable
                _cts.Token.ThrowIfCancellationRequested()
                adapter.Fill(dt)
                _cts.Token.ThrowIfCancellationRequested()
                Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing
                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    For i = 0 To dt.Rows.Count - 1
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim instrumentData As New ActiveInstrumentData With
                        {.Token = dt.Rows(i).Item(0),
                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                         .Expiry = dt.Rows(i).Item(2)}
                        If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                        activeInstruments.Add(instrumentData)
                        If instrumentData.TradingSymbol = stock Then
                            stockList(stock).InstrumentIdentifier = instrumentData.Token
                            stockList(stock).CurrentContractExpiry = instrumentData.Expiry
                        End If
                    Next
                End If
                If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim minExpiry As Date = activeInstruments.Min(Function(x)
                                                                      Return x.Expiry
                                                                  End Function)
                    If minExpiry.Date = stockList(stock).CurrentContractExpiry.Date Then
                        stockList(stock).IsTradable = True
                        _cts.Token.ThrowIfCancellationRequested()
                    Else
                        _cts.Token.ThrowIfCancellationRequested()
                        If minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date > tradingDate.Date Then
                            _cts.Token.ThrowIfCancellationRequested()
                            Throw New ApplicationException(String.Format("Check stock {0} on {1}", stockList(stock).TradingSymbol, tradingDate))
                            stockList(stock).IsTradable = False
                            stockList(stock).PreviousContractTradingSymbol = activeInstruments.Find(Function(x)
                                                                                                        Return x.Expiry.Date = minExpiry.Date
                                                                                                    End Function).TradingSymbol
                            stockList(stock).PreviousContractExpiry = minExpiry
                        ElseIf minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date = tradingDate.Date Then
                            _cts.Token.ThrowIfCancellationRequested()
                            stockList(stock).IsTradable = True
                            stockList(stock).PreviousContractTradingSymbol = activeInstruments.Find(Function(x)
                                                                                                        Return x.Expiry.Date = minExpiry.Date
                                                                                                    End Function).TradingSymbol
                            stockList(stock).PreviousContractExpiry = minExpiry
                        End If
                    End If
                    ret = stockList
                End If
            Next
        End If
        Return ret
    End Function

    Public Async Function GetStockPayload(ByVal tradingDate As Date, ByVal instrumentData As InstrumentDetails, ByVal immediatePreviousDay As Boolean) As Task(Of Dictionary(Of Date, Payload))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        _cts.Token.ThrowIfCancellationRequested()
        If instrumentData IsNot Nothing AndAlso instrumentData.IsTradable Then
            If instrumentData.PreviousContractTradingSymbol Is Nothing Then
                Dim previousTradingDate As Date = Date.MinValue
                If immediatePreviousDay Then
                    previousTradingDate = _common.GetPreviousTradingDay(Common.DataBaseTable.Intraday_Futures, tradingDate)
                Else
                    previousTradingDate = _common.GetPreviousTradingDayOfAnInstrument(Common.DataBaseTable.Intraday_Futures, instrumentData.TradingSymbol, tradingDate)
                End If
                If previousTradingDate <> Date.MinValue Then
                    ret = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures, instrumentData.TradingSymbol, previousTradingDate, previousTradingDate)
                End If
            Else
                Dim previousTradingDate As Date = Date.MinValue
                If immediatePreviousDay Then
                    previousTradingDate = _common.GetPreviousTradingDay(Common.DataBaseTable.Intraday_Futures, tradingDate)
                Else
                    previousTradingDate = _common.GetPreviousTradingDayOfAnInstrument(Common.DataBaseTable.Intraday_Futures, instrumentData.PreviousContractTradingSymbol, tradingDate)
                End If
                If previousTradingDate <> Date.MinValue Then
                    ret = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures, instrumentData.PreviousContractTradingSymbol, previousTradingDate, previousTradingDate)
                End If
            End If
        End If
        Return ret
    End Function

    Public Function GetPreviousTradingDay(ByVal tradingDate As Date) As Date
        Return _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
    End Function

    Public Async Function IsTradableDay(ByVal tradingDate As Date) As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Await GetHistoricalCandleStickAsync("1723649", tradingDate, tradingDate, TypeOfData.Intraday).ConfigureAwait(False)
        _cts.Token.ThrowIfCancellationRequested()
        If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 Then
            _cts.Token.ThrowIfCancellationRequested()
            Dim intradayHistoricalData As Dictionary(Of Date, Payload) = Await GetChartFromHistoricalAsync(historicalCandlesJSONDict, "JINDALSTEL", tradingDate).ConfigureAwait(False)
            If intradayHistoricalData IsNot Nothing AndAlso intradayHistoricalData.Count > 0 Then
                ret = True
            End If
        End If
        Return ret
    End Function
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class