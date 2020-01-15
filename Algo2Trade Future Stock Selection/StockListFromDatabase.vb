'Imports System.Threading
'Imports Algo2TradeBLL
'Imports MySql.Data.MySqlClient
'Imports Utilities.Network
'Imports System.Net.Http
'Imports System.Text.RegularExpressions

'Public Class StockListFromDatabase
'    Implements IDisposable

'#Region "Events/Event handlers"
'    Public Event DocumentDownloadComplete()
'    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
'    Public Event Heartbeat(ByVal msg As String)
'    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
'    'The below functions are needed to allow the derived classes to raise the above two events
'    Protected Overridable Sub OnDocumentDownloadComplete()
'        RaiseEvent DocumentDownloadComplete()
'    End Sub
'    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
'        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
'    End Sub
'    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
'        RaiseEvent Heartbeat(msg)
'    End Sub
'    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
'        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
'    End Sub
'#End Region

'    Private ReadOnly _cts As CancellationTokenSource
'    Private ReadOnly _common As Common
'    Private _conn As MySqlConnection
'    Private ReadOnly ZerodhaEODHistoricalURL = "https://kitecharts-aws.zerodha.com/api/chart/{0}/day?api_key=kitefront&access_token=K&from={1}&to={2}"
'    Private ReadOnly ZerodhaIntradayHistoricalURL = "https://kitecharts-aws.zerodha.com/api/chart/{0}/minute?api_key=kitefront&access_token=K&from={1}&to={2}"
'    Private ReadOnly intradayTable As Common.DataBaseTable = Common.DataBaseTable.Intraday_Futures
'    Private ReadOnly eodTable As Common.DataBaseTable = Common.DataBaseTable.EOD_Futures
'    Private ReadOnly index As String = Nothing

'    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal intradayTbl As Common.DataBaseTable, ByVal eodTbl As Common.DataBaseTable, ByVal currentIndex As String)
'        _cts = canceller
'        intradayTable = intradayTbl
'        eodTable = eodTbl
'        index = currentIndex
'        _common = New Common(_cts)
'        AddHandler _common.Heartbeat, AddressOf OnHeartbeat
'    End Sub

'#Region "Private Class & Enum"
'    Private Class ActiveInstrumentData
'        Public Property Token As Integer
'        Public Property TradingSymbol As String
'        Public Property Expiry As Date
'        Public Property LastDayOpen As Decimal
'        Public Property LastDayLow As Decimal
'        Public Property LastDayHigh As Decimal
'        Public Property LastDayClose As Decimal
'        Public ReadOnly Property RawInstrumentName As String
'            Get
'                Return Me.TradingSymbol.Remove(Me.TradingSymbol.Count - 8)
'            End Get
'        End Property
'        Public Property CashInstrumentName As String
'        Public Property CashInstrumentToken As String
'    End Class

'    Enum TypeOfData
'        Intraday = 1
'        EOD
'    End Enum
'#End Region

'#Region "Private Functions"
'    Private Async Function GetHistoricalCandleStickAsync(ByVal instrumentToken As String, ByVal fromDate As Date, ByVal toDate As Date, ByVal historicalDataType As TypeOfData) As Task(Of Dictionary(Of String, Object))
'        Dim ret As Dictionary(Of String, Object) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim historicalDataURL As String = Nothing
'        Select Case historicalDataType
'            Case TypeOfData.Intraday
'                historicalDataURL = String.Format(ZerodhaIntradayHistoricalURL, instrumentToken, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))
'            Case TypeOfData.EOD
'                historicalDataURL = String.Format(ZerodhaEODHistoricalURL, instrumentToken, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))
'        End Select
'        OnHeartbeat(String.Format("Fetching historical Data: {0}", historicalDataURL))
'        'Using sr As New StreamReader(HttpWebRequest.Create(historicalDataURL).GetResponseAsync().Result.GetResponseStream)
'        '    Dim jsonString = Await sr.ReadToEndAsync.ConfigureAwait(False)
'        '    Dim retDictionary As Dictionary(Of String, Object) = StringManipulation.JsonDeserialize(jsonString)
'        '    Return retDictionary
'        'End Using
'        Dim proxyToBeUsed As HttpProxy = Nothing
'        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
'            AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
'            AddHandler browser.Heartbeat, AddressOf OnHeartbeat
'            AddHandler browser.WaitingFor, AddressOf OnWaitingFor
'            AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
'            'Get to the landing page first
'            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(historicalDataURL,
'                                                                                HttpMethod.Get,
'                                                                                Nothing,
'                                                                                True,
'                                                                                Nothing,
'                                                                                True,
'                                                                                "application/json").ConfigureAwait(False)
'            If l Is Nothing OrElse l.Item2 Is Nothing Then
'                Throw New ApplicationException(String.Format("No response while getting historical data for: {0}", historicalDataURL))
'            End If
'            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
'                ret = l.Item2
'            End If
'            RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
'            RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
'            RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
'            RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
'        End Using
'        Return ret
'    End Function

'    Private Async Function GetChartFromHistoricalAsync(ByVal historicalCandlesJSONDict As Dictionary(Of String, Object),
'                                                       ByVal tradingSymbol As String,
'                                                       ByVal tradingDate As Date) As Task(Of Dictionary(Of Date, Payload))
'        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of Date, Payload) = Nothing
'        If historicalCandlesJSONDict.ContainsKey("data") Then
'            Dim historicalCandlesDict As Dictionary(Of String, Object) = historicalCandlesJSONDict("data")
'            If historicalCandlesDict.ContainsKey("candles") AndAlso historicalCandlesDict("candles").count > 0 Then
'                Dim historicalCandles As ArrayList = historicalCandlesDict("candles")
'                If ret Is Nothing Then ret = New Dictionary(Of Date, Payload)
'                OnHeartbeat(String.Format("Generating Payload for {0} of {1}", tradingSymbol, tradingDate.ToShortDateString))
'                Dim previousPayload As Payload = Nothing
'                For Each historicalCandle In historicalCandles
'                    _cts.Token.ThrowIfCancellationRequested()
'                    Dim runningSnapshotTime As Date = Utilities.Time.GetDateTimeTillMinutes(historicalCandle(0))

'                    Dim runningPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
'                    With runningPayload
'                        .PayloadDate = Utilities.Time.GetDateTimeTillMinutes(historicalCandle(0))
'                        .TradingSymbol = tradingSymbol
'                        .Open = historicalCandle(1)
'                        .High = historicalCandle(2)
'                        .Low = historicalCandle(3)
'                        .Close = historicalCandle(4)
'                        .Volume = historicalCandle(5)
'                        .PreviousCandlePayload = previousPayload
'                    End With
'                    previousPayload = runningPayload
'                    ret.Add(runningSnapshotTime, runningPayload)
'                Next
'            End If
'        End If
'        Return ret
'    End Function
'#End Region

'#Region "Procedure to get stock"
'    Private Async Function GetATRBasedFOStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
'            _cts.Token.ThrowIfCancellationRequested()
'            _conn = _common.OpenDBConnection()
'        End If
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim previousTradingDay As Date = _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
'        If previousTradingDay <> Date.MinValue Then
'            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
'                _cts.Token.ThrowIfCancellationRequested()
'                _conn = _common.OpenDBConnection()
'            End If
'            _cts.Token.ThrowIfCancellationRequested()
'            OnHeartbeat("Fetching all future instrument")
'            Dim cm As MySqlCommand = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `AS_ON_DATE`=@sd AND `SEGMENT`='NFO-FUT'", _conn)
'            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim adapter As New MySqlDataAdapter(cm)
'            adapter.SelectCommand.CommandTimeout = 300
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim dt As DataTable = New DataTable
'            adapter.Fill(dt)
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim nfoInstruments As List(Of ActiveInstrumentData) = Nothing
'            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
'                For i = 0 To dt.Rows.Count - 1
'                    _cts.Token.ThrowIfCancellationRequested()
'                    Dim instrumentData As New ActiveInstrumentData With
'                    {.Token = dt.Rows(i).Item(0),
'                     .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
'                     .Expiry = dt.Rows(i).Item(2)}

'                    Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
'                    If Regex.Matches(instrumentData.TradingSymbol, pattern).Count <= 1 Then
'                        If nfoInstruments Is Nothing Then nfoInstruments = New List(Of ActiveInstrumentData)
'                        nfoInstruments.Add(instrumentData)
'                    End If
'                Next
'            End If
'            If nfoInstruments IsNot Nothing AndAlso nfoInstruments.Count > 0 Then
'                Dim lastTradingDay As Date = Date.MinValue
'                Dim currentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
'                For Each runningInstrument In nfoInstruments
'                    If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
'                        Dim availableInstrument As IEnumerable(Of ActiveInstrumentData) = currentNFOInstruments.FindAll(Function(z)
'                                                                                                                            Return z.RawInstrumentName = runningInstrument.RawInstrumentName
'                                                                                                                        End Function)
'                        If availableInstrument IsNot Nothing AndAlso availableInstrument.Count > 0 Then
'                            Continue For
'                        End If
'                    End If
'                    Dim runningIntruments As IEnumerable(Of ActiveInstrumentData) = nfoInstruments.Where(Function(x)
'                                                                                                             Return x.RawInstrumentName = runningInstrument.RawInstrumentName
'                                                                                                         End Function)
'                    Dim minExpiry As Date = runningIntruments.Min(Function(x)
'                                                                      If x.Expiry.Date <= tradingDate.Date Then
'                                                                          Return Date.MaxValue
'                                                                      Else
'                                                                          Return x.Expiry
'                                                                      End If
'                                                                  End Function)
'                    Dim currentIntrument As ActiveInstrumentData = runningIntruments.ToList.Find(Function(y)
'                                                                                                     Return y.Expiry.Date = minExpiry.Date
'                                                                                                 End Function)
'                    If currentIntrument IsNot Nothing Then
'                        If currentNFOInstruments Is Nothing Then currentNFOInstruments = New List(Of ActiveInstrumentData)
'                        currentNFOInstruments.Add(currentIntrument)
'                    End If
'                Next
'                If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
'                    Dim priceFilterdCurrentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
'                    For Each runningInstrument In currentNFOInstruments
'                        _cts.Token.ThrowIfCancellationRequested()
'                        Dim previousDayPayloads As Dictionary(Of Date, Payload) = Nothing
'                        If eodTable = Common.DataBaseTable.EOD_Cash Then
'                            previousDayPayloads = _common.GetRawPayloadForSpecificTradingSymbol(eodTable, runningInstrument.RawInstrumentName, previousTradingDay.AddDays(-10), previousTradingDay)
'                        Else
'                            previousDayPayloads = _common.GetRawPayloadForSpecificTradingSymbol(eodTable, runningInstrument.TradingSymbol, previousTradingDay.AddDays(-10), previousTradingDay)
'                        End If
'                        Dim lastDayPayload As Payload = Nothing
'                        If previousDayPayloads IsNot Nothing AndAlso previousDayPayloads.Count > 0 Then
'                            lastDayPayload = previousDayPayloads.LastOrDefault.Value
'                        End If
'                        If lastDayPayload IsNot Nothing AndAlso lastDayPayload.Close >= My.Settings.MinClose AndAlso lastDayPayload.Close <= My.Settings.MaxClose Then
'                            Dim rawCashInstrument As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Cash, index, previousTradingDay, runningInstrument.RawInstrumentName)
'                            If rawCashInstrument IsNot Nothing Then
'                                runningInstrument.CashInstrumentToken = rawCashInstrument.Item1
'                                runningInstrument.CashInstrumentName = rawCashInstrument.Item2
'                                runningInstrument.LastDayOpen = lastDayPayload.Open
'                                runningInstrument.LastDayLow = lastDayPayload.Low
'                                runningInstrument.LastDayHigh = lastDayPayload.High
'                                runningInstrument.LastDayClose = lastDayPayload.Close
'                                If priceFilterdCurrentNFOInstruments Is Nothing Then priceFilterdCurrentNFOInstruments = New List(Of ActiveInstrumentData)
'                                priceFilterdCurrentNFOInstruments.Add(runningInstrument)
'                            End If
'                        End If
'                    Next
'                    Dim highATRStocks As Concurrent.ConcurrentDictionary(Of String, Decimal()) = Nothing
'                    Try
'                        If priceFilterdCurrentNFOInstruments IsNot Nothing AndAlso priceFilterdCurrentNFOInstruments.Count > 0 Then
'                            For i As Integer = 0 To priceFilterdCurrentNFOInstruments.Count - 1 Step 20
'                                Dim numberOfData As Integer = If(priceFilterdCurrentNFOInstruments.Count - i > 20, 20, priceFilterdCurrentNFOInstruments.Count - i)
'                                Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
'                                tasks = priceFilterdCurrentNFOInstruments.GetRange(i, numberOfData).Select(Async Function(x)
'                                                                                                               Try
'                                                                                                                   Dim rawCashInstrument As Tuple(Of String, String) = New Tuple(Of String, String)(x.CashInstrumentToken, x.CashInstrumentName)
'                                                                                                                   If rawCashInstrument IsNot Nothing Then
'                                                                                                                       Dim instrumentData As KeyValuePair(Of Integer, String) = New KeyValuePair(Of Integer, String)(rawCashInstrument.Item1, rawCashInstrument.Item2)
'                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                       Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Await GetHistoricalCandleStickAsync(instrumentData.Key, previousTradingDay.AddDays(-300), previousTradingDay, TypeOfData.EOD).ConfigureAwait(False)
'                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                       If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 Then
'                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                           Dim eodHistoricalData As Dictionary(Of Date, Payload) = Await GetChartFromHistoricalAsync(historicalCandlesJSONDict, instrumentData.Value, tradingDate).ConfigureAwait(False)
'                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                           If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 0 Then
'                                                                                                                               _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                               Dim ATRPayload As Dictionary(Of Date, Decimal) = Nothing
'                                                                                                                               Indicator.ATR.CalculateATR(14, eodHistoricalData, ATRPayload)
'                                                                                                                               _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                               If ATRPayload IsNot Nothing AndAlso ATRPayload.Count > 0 Then
'                                                                                                                                   Dim lastDayClosePrice As Decimal = eodHistoricalData.LastOrDefault.Value.Close
'                                                                                                                                   'lastTradingDay = eodHistoricalData.LastOrDefault.Key
'                                                                                                                                   'If lastDayClosePrice >= My.Settings.MinClose AndAlso lastDayClosePrice <= My.Settings.MaxClose Then
'                                                                                                                                   Dim atrPercentage As Decimal = (ATRPayload(eodHistoricalData.LastOrDefault.Key) / lastDayClosePrice) * 100
'                                                                                                                                   If atrPercentage >= My.Settings.ATRPercentage Then
'                                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                       Dim volumePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = eodHistoricalData.OrderByDescending(Function(z)
'                                                                                                                                                                                                                                                     Return z.Key
'                                                                                                                                                                                                                                                 End Function).Take(5)
'                                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                       If volumePayload IsNot Nothing AndAlso volumePayload.Count > 0 Then
'                                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                           Dim avgVolume As Decimal = volumePayload.Average(Function(z)
'                                                                                                                                                                                                Return z.Value.Volume
'                                                                                                                                                                                            End Function)
'                                                                                                                                           _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                           If avgVolume >= (My.Settings.PotentialAmount / 100) * lastDayClosePrice Then
'                                                                                                                                               If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentDictionary(Of String, Decimal())
'                                                                                                                                               highATRStocks.TryAdd(instrumentData.Value, {atrPercentage, ATRPayload(eodHistoricalData.LastOrDefault.Key), x.LastDayOpen, x.LastDayLow, x.LastDayHigh, x.LastDayClose})
'                                                                                                                                           End If
'                                                                                                                                       End If
'                                                                                                                                   End If
'                                                                                                                                   'End If
'                                                                                                                               End If
'                                                                                                                           End If
'                                                                                                                       End If
'                                                                                                                   End If
'                                                                                                               Catch ex As Exception
'                                                                                                                   Console.WriteLine(String.Format("{0}:{1}", x.TradingSymbol, ex.ToString))
'                                                                                                                   Throw ex
'                                                                                                               End Try
'                                                                                                               Return True
'                                                                                                           End Function)

'                                Dim mainTask As Task = Task.WhenAll(tasks)
'                                Await mainTask.ConfigureAwait(False)
'                                If mainTask.Exception IsNot Nothing Then
'                                    Throw mainTask.Exception
'                                End If
'                            Next
'                        End If
'                    Catch cex As TaskCanceledException
'                        Throw cex
'                    Catch aex As AggregateException
'                        Throw aex
'                    Catch ex As Exception
'                        Throw ex
'                    End Try


'                    If highATRStocks IsNot Nothing AndAlso highATRStocks.Count > 0 Then
'                        For Each runningStock In highATRStocks.OrderByDescending(Function(x)
'                                                                                     Return x.Value(0)
'                                                                                 End Function)
'                            _cts.Token.ThrowIfCancellationRequested()
'                            Dim currentTradingSymbol As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Futures, index, tradingDate, runningStock.Key)
'                            If currentTradingSymbol IsNot Nothing Then
'                                Dim lotSize As Integer = _common.GetLotSize(Common.DataBaseTable.EOD_Futures, currentTradingSymbol.Item2, tradingDate)
'                                If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                                ret.Add(runningStock.Key, New InstrumentDetails With {.ATRPercentage = runningStock.Value(0), .LotSize = lotSize, .DayATR = runningStock.Value(1), .PreviousDayOpen = runningStock.Value(2), .PreviousDayLow = runningStock.Value(3), .PreviousDayHigh = runningStock.Value(4), .PreviousDayClose = runningStock.Value(5)})
'                            End If
'                        Next
'                    End If
'                End If
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetATRBasedAllStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
'            _cts.Token.ThrowIfCancellationRequested()
'            _conn = _common.OpenDBConnection()
'        End If
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim previousTradingDay As Date = _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Cash, tradingDate)
'        If previousTradingDay <> Date.MinValue Then
'            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
'                _cts.Token.ThrowIfCancellationRequested()
'                _conn = _common.OpenDBConnection()
'            End If
'            _cts.Token.ThrowIfCancellationRequested()
'            OnHeartbeat("Fetching all cash instrument")
'            Dim cm As MySqlCommand = New MySqlCommand("SELECT DISTINCT(`INSTRUMENT_TOKEN`),`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_cash` WHERE `AS_ON_DATE`=@sd", _conn)
'            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim adapter As New MySqlDataAdapter(cm)
'            adapter.SelectCommand.CommandTimeout = 300
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim dt As DataTable = New DataTable
'            adapter.Fill(dt)
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim cashInstruments As List(Of ActiveInstrumentData) = Nothing
'            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
'                For i = 0 To dt.Rows.Count - 1
'                    _cts.Token.ThrowIfCancellationRequested()
'                    Dim instrumentData As New ActiveInstrumentData With
'                    {.Token = dt.Rows(i).Item(0),
'                     .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
'                     .Expiry = Date.MaxValue}

'                    Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
'                    If Regex.Matches(instrumentData.TradingSymbol, pattern).Count <= 1 Then
'                        If cashInstruments Is Nothing Then cashInstruments = New List(Of ActiveInstrumentData)
'                        cashInstruments.Add(instrumentData)
'                    End If
'                Next
'            End If
'            If cashInstruments IsNot Nothing AndAlso cashInstruments.Count > 0 Then
'                Dim lastTradingDay As Date = Date.MinValue
'                Dim currentInstruments As List(Of ActiveInstrumentData) = Nothing
'                'For Each runningInstrument In cashInstruments
'                '    If currentInstruments IsNot Nothing AndAlso currentInstruments.Count > 0 Then
'                '        Dim availableInstrument As IEnumerable(Of ActiveInstrumentData) = currentInstruments.FindAll(Function(z)
'                '                                                                                                         Return z.RawInstrumentName = runningInstrument.RawInstrumentName
'                '                                                                                                     End Function)
'                '        If availableInstrument IsNot Nothing AndAlso availableInstrument.Count > 0 Then
'                '            Continue For
'                '        End If
'                '    End If
'                '    Dim runningIntruments As IEnumerable(Of ActiveInstrumentData) = cashInstruments.Where(Function(x)
'                '                                                                                              Return x.RawInstrumentName = runningInstrument.RawInstrumentName
'                '                                                                                          End Function)
'                '    Dim minExpiry As Date = runningIntruments.Min(Function(x)
'                '                                                      If x.Expiry.Date <= tradingDate.Date Then
'                '                                                          Return Date.MaxValue
'                '                                                      Else
'                '                                                          Return x.Expiry
'                '                                                      End If
'                '                                                  End Function)
'                '    Dim currentIntrument As ActiveInstrumentData = runningIntruments.ToList.Find(Function(y)
'                '                                                                                     Return y.Expiry.Date = minExpiry.Date
'                '                                                                                 End Function)
'                '    If currentIntrument IsNot Nothing Then
'                '        If currentInstruments Is Nothing Then currentInstruments = New List(Of ActiveInstrumentData)
'                '        currentInstruments.Add(currentIntrument)
'                '    End If
'                'Next
'                currentInstruments = cashInstruments
'                If currentInstruments IsNot Nothing AndAlso currentInstruments.Count > 0 Then
'                    Dim priceFilterdCurrentInstruments As List(Of ActiveInstrumentData) = Nothing
'                    For Each runningInstrument In currentInstruments
'                        _cts.Token.ThrowIfCancellationRequested()
'                        Dim previousDayPayloads As Dictionary(Of Date, Payload) = Nothing
'                        If eodTable = Common.DataBaseTable.EOD_Cash Then
'                            previousDayPayloads = _common.GetRawPayloadForSpecificTradingSymbol(eodTable, runningInstrument.TradingSymbol, previousTradingDay.AddDays(-10), previousTradingDay)
'                        Else
'                            previousDayPayloads = _common.GetRawPayloadForSpecificTradingSymbol(eodTable, runningInstrument.TradingSymbol, previousTradingDay.AddDays(-10), previousTradingDay)
'                        End If
'                        Dim lastDayPayload As Payload = Nothing
'                        If previousDayPayloads IsNot Nothing AndAlso previousDayPayloads.Count > 0 Then
'                            lastDayPayload = previousDayPayloads.LastOrDefault.Value
'                        End If
'                        If lastDayPayload IsNot Nothing AndAlso lastDayPayload.Close >= My.Settings.MinClose AndAlso lastDayPayload.Close <= My.Settings.MaxClose Then
'                            Dim rawCashInstrument As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Cash, index, previousTradingDay, runningInstrument.TradingSymbol)
'                            If rawCashInstrument IsNot Nothing Then
'                                runningInstrument.CashInstrumentToken = rawCashInstrument.Item1
'                                runningInstrument.CashInstrumentName = rawCashInstrument.Item2
'                                runningInstrument.LastDayOpen = lastDayPayload.Open
'                                runningInstrument.LastDayLow = lastDayPayload.Low
'                                runningInstrument.LastDayHigh = lastDayPayload.High
'                                runningInstrument.LastDayClose = lastDayPayload.Close
'                                If priceFilterdCurrentInstruments Is Nothing Then priceFilterdCurrentInstruments = New List(Of ActiveInstrumentData)
'                                priceFilterdCurrentInstruments.Add(runningInstrument)
'                            End If
'                        End If
'                    Next
'                    Dim highATRStocks As Concurrent.ConcurrentDictionary(Of String, Decimal()) = Nothing
'                    Try
'                        If priceFilterdCurrentInstruments IsNot Nothing AndAlso priceFilterdCurrentInstruments.Count > 0 Then
'                            For i As Integer = 0 To priceFilterdCurrentInstruments.Count - 1 Step 20
'                                Dim numberOfData As Integer = If(priceFilterdCurrentInstruments.Count - i > 20, 20, priceFilterdCurrentInstruments.Count - i)
'                                Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
'                                tasks = priceFilterdCurrentInstruments.GetRange(i, numberOfData).Select(Async Function(x)
'                                                                                                            Try
'                                                                                                                Dim rawCashInstrument As Tuple(Of String, String) = New Tuple(Of String, String)(x.CashInstrumentToken, x.CashInstrumentName)
'                                                                                                                If rawCashInstrument IsNot Nothing Then
'                                                                                                                    Dim instrumentData As KeyValuePair(Of Integer, String) = New KeyValuePair(Of Integer, String)(rawCashInstrument.Item1, rawCashInstrument.Item2)
'                                                                                                                    _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                    Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Await GetHistoricalCandleStickAsync(instrumentData.Key, previousTradingDay.AddDays(-300), previousTradingDay, TypeOfData.EOD).ConfigureAwait(False)
'                                                                                                                    _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                    If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 Then
'                                                                                                                        _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                        Dim eodHistoricalData As Dictionary(Of Date, Payload) = Await GetChartFromHistoricalAsync(historicalCandlesJSONDict, instrumentData.Value, tradingDate).ConfigureAwait(False)
'                                                                                                                        _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                        If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 100 Then
'                                                                                                                            _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                            Dim ATRPayload As Dictionary(Of Date, Decimal) = Nothing
'                                                                                                                            Indicator.ATR.CalculateATR(14, eodHistoricalData, ATRPayload)
'                                                                                                                            _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                            If ATRPayload IsNot Nothing AndAlso ATRPayload.Count > 0 Then
'                                                                                                                                Dim lastDayClosePrice As Decimal = eodHistoricalData.LastOrDefault.Value.Close
'                                                                                                                                'lastTradingDay = eodHistoricalData.LastOrDefault.Key
'                                                                                                                                'If lastDayClosePrice >= My.Settings.MinClose AndAlso lastDayClosePrice <= My.Settings.MaxClose Then
'                                                                                                                                Dim atrPercentage As Decimal = (ATRPayload(eodHistoricalData.LastOrDefault.Key) / lastDayClosePrice) * 100
'                                                                                                                                If atrPercentage >= My.Settings.ATRPercentage Then
'                                                                                                                                    _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                    Dim volumePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = eodHistoricalData.OrderByDescending(Function(z)
'                                                                                                                                                                                                                                                  Return z.Key
'                                                                                                                                                                                                                                              End Function).Take(5)
'                                                                                                                                    _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                    If volumePayload IsNot Nothing AndAlso volumePayload.Count > 0 Then
'                                                                                                                                        _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                        Dim avgVolume As Decimal = volumePayload.Average(Function(z)
'                                                                                                                                                                                             Return z.Value.Volume
'                                                                                                                                                                                         End Function)
'                                                                                                                                        _cts.Token.ThrowIfCancellationRequested()
'                                                                                                                                        If avgVolume >= (My.Settings.PotentialAmount / 100) * lastDayClosePrice Then
'                                                                                                                                            If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentDictionary(Of String, Decimal())
'                                                                                                                                            highATRStocks.TryAdd(instrumentData.Value, {atrPercentage, ATRPayload(eodHistoricalData.LastOrDefault.Key), x.LastDayOpen, x.LastDayLow, x.LastDayHigh, x.LastDayClose})
'                                                                                                                                        End If
'                                                                                                                                    End If
'                                                                                                                                End If
'                                                                                                                                'End If
'                                                                                                                            End If
'                                                                                                                        End If
'                                                                                                                    End If
'                                                                                                                End If
'                                                                                                            Catch ex As Exception
'                                                                                                                Console.WriteLine(String.Format("{0}:{1}", x.TradingSymbol, ex.ToString))
'                                                                                                                Throw ex
'                                                                                                            End Try
'                                                                                                            Return True
'                                                                                                        End Function)

'                                Dim mainTask As Task = Task.WhenAll(tasks)
'                                Await mainTask.ConfigureAwait(False)
'                                If mainTask.Exception IsNot Nothing Then
'                                    Throw mainTask.Exception
'                                End If
'                            Next
'                        End If
'                    Catch cex As TaskCanceledException
'                        Throw cex
'                    Catch aex As AggregateException
'                        Throw aex
'                    Catch ex As Exception
'                        Throw ex
'                    End Try


'                    If highATRStocks IsNot Nothing AndAlso highATRStocks.Count > 0 Then
'                        For Each runningStock In highATRStocks.OrderByDescending(Function(x)
'                                                                                     Return x.Value(0)
'                                                                                 End Function)
'                            _cts.Token.ThrowIfCancellationRequested()
'                            Dim currentTradingSymbol As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Cash, index, tradingDate, runningStock.Key)
'                            If currentTradingSymbol IsNot Nothing Then
'                                Dim lotSize As Integer = 1
'                                If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                                ret.Add(runningStock.Key, New InstrumentDetails With {.ATRPercentage = runningStock.Value(0), .LotSize = lotSize, .DayATR = runningStock.Value(1), .PreviousDayOpen = runningStock.Value(2), .PreviousDayLow = runningStock.Value(3), .PreviousDayHigh = runningStock.Value(4), .PreviousDayClose = runningStock.Value(5)})
'                            End If
'                        Next
'                    End If
'                End If
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetPreMarketStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
'                _cts.Token.ThrowIfCancellationRequested()
'                _conn = _common.OpenDBConnection()
'            End If
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim cm As MySqlCommand = New MySqlCommand("SELECT * FROM `v_pre_market` WHERE `APPLICABLE_DATE`=@sd", _conn)
'            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim adapter As New MySqlDataAdapter(cm)
'            adapter.SelectCommand.CommandTimeout = 300
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim dt As DataTable = New DataTable
'            adapter.Fill(dt)
'            _cts.Token.ThrowIfCancellationRequested()
'            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
'                Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
'                For i = 0 To dt.Rows.Count - 1
'                    _cts.Token.ThrowIfCancellationRequested()
'                    Dim instrumentName As String = dt.Rows(i).Item(1).ToString.ToUpper
'                    Dim changePer As String = Math.Round(((dt.Rows(i).Item(4) / dt.Rows(i).Item(7)) - 1) * 100, 2)
'                    If highATRStockList.ContainsKey(instrumentName) Then
'                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
'                        tempStockList.Add(instrumentName, {changePer, dt.Rows(i).Item(7)})
'                    End If
'                Next
'                If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                    For Each runningStock In tempStockList.OrderByDescending(Function(x)
'                                                                                 Return Math.Abs(x.Value(0))
'                                                                             End Function)
'                        If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                        highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                        highATRStockList(runningStock.Key).Supporting2 = runningStock.Value(1)
'                        ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                    Next
'                End If
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetIntradayVolumeSpikeStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        If intradayVolumeSpikeUserInputs Is Nothing Then Throw New ApplicationException("IntradayVolumeSpike Settings not implemented properly")
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, runningStock, tradingDate.AddDays(-15), tradingDate)
'                If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
'                    Dim signalCheckStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
'                    Dim signalCheckEndTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day,
'                                                              intradayVolumeSpikeUserInputs.CheckingTime.Hour,
'                                                              intradayVolumeSpikeUserInputs.CheckingTime.Minute,
'                                                              intradayVolumeSpikeUserInputs.CheckingTime.Second)
'                    Dim currentDayVolumeSum As Long = 0
'                    Dim previousDaysVolumeSum As Long = 0
'                    Dim counter As Integer = 0
'                    Dim lastCalculatedDate As Date = Date.MinValue
'                    For Each runningPayload In intradayPayload.Keys.OrderByDescending(Function(x)
'                                                                                          Return x
'                                                                                      End Function)
'                        Dim signalStart As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, signalCheckStartTime.Hour, signalCheckStartTime.Minute, signalCheckStartTime.Second)
'                        Dim signalEnd As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, signalCheckEndTime.Hour, signalCheckEndTime.Minute, signalCheckEndTime.Second)
'                        If runningPayload.Date = tradingDate.Date Then
'                            If runningPayload >= signalStart AndAlso runningPayload <= signalEnd Then
'                                currentDayVolumeSum += intradayPayload(runningPayload).Volume
'                            End If
'                        ElseIf runningPayload.Date < tradingDate.Date Then
'                            If runningPayload >= signalStart AndAlso runningPayload <= signalEnd Then
'                                If lastCalculatedDate.Date <> runningPayload.Date Then
'                                    lastCalculatedDate = runningPayload
'                                    counter += 1
'                                    If counter = 5 + 1 Then Exit For
'                                End If
'                                previousDaysVolumeSum += intradayPayload(runningPayload).Volume
'                            End If
'                        End If
'                    Next
'                    If currentDayVolumeSum <> 0 AndAlso previousDaysVolumeSum <> 0 Then
'                        Dim changePer As Decimal = ((currentDayVolumeSum / (previousDaysVolumeSum / 5)) - 1) * 100
'                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
'                        tempStockList.Add(runningStock, {changePer})
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList.OrderByDescending(Function(x)
'                                                                             Return x.Value(0)
'                                                                         End Function)
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetOHLATRStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, String()) = Nothing
'            Dim previousTradingDay As Date = _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim eodPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Cash, runningStock, previousTradingDay, previousTradingDay)
'                If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
'                    Dim open As Decimal = eodPayload.FirstOrDefault.Value.Open
'                    Dim low As Decimal = eodPayload.FirstOrDefault.Value.Low
'                    Dim high As Decimal = eodPayload.FirstOrDefault.Value.High
'                    Dim direction As String = Nothing
'                    If open = low Then
'                        direction = "BUY"
'                    ElseIf open = high Then
'                        direction = "SELL"
'                    End If
'                    If direction IsNot Nothing Then
'                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
'                        tempStockList.Add(runningStock, {direction})
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetTouchPreviousDayLastCandleStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim currentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(intradayTable, index, tradingDate, runningStock)
'                If currentSymbolToken IsNot Nothing Then
'                    Dim tradingSymbol As String = currentSymbolToken.Item2
'                    Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, tradingSymbol, tradingDate.AddDays(-15), tradingDate)
'                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
'                        Dim currentDayFirstCandle As Payload = Nothing
'                        Dim firstPayloadTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
'                        If intradayPayload.ContainsKey(firstPayloadTime) Then
'                            currentDayFirstCandle = intradayPayload(firstPayloadTime)
'                        End If
'                        If currentDayFirstCandle IsNot Nothing AndAlso currentDayFirstCandle.PreviousCandlePayload IsNot Nothing Then
'                            Dim buffer As Decimal = CalculateBuffer(currentDayFirstCandle.Open, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
'                            Dim atrPer As Decimal = highATRStockList(runningStock).ATRPercentage
'                            'Dim slab As Decimal = CalculateSlab(currentDayFirstCandle.Open, atrPer)
'                            If currentDayFirstCandle.Open < currentDayFirstCandle.PreviousCandlePayload.Close Then
'                                If currentDayFirstCandle.High + buffer >= currentDayFirstCandle.PreviousCandlePayload.Low Then
'                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
'                                    'tempStockList.Add(runningStock, {slab})
'                                    tempStockList.Add(runningStock, {0})
'                                End If
'                            ElseIf currentDayFirstCandle.Open >= currentDayFirstCandle.PreviousCandlePayload.Close Then
'                                If currentDayFirstCandle.Low - buffer <= currentDayFirstCandle.PreviousCandlePayload.High Then
'                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
'                                    'tempStockList.Add(runningStock, {slab})
'                                    tempStockList.Add(runningStock, {0})
'                                End If
'                            End If
'                        End If
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    'highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetTopGainerTopLosserStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        If topGainerTopLosserUserInputs Is Nothing Then Throw New ApplicationException("Top Gainer Top Losser Settings not implemented properly")
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim payloadTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day,
'                                                topGainerTopLosserUserInputs.CheckingTime.Hour,
'                                                topGainerTopLosserUserInputs.CheckingTime.Minute, 0)
'            Dim niftyGainLossPercentage As Decimal = Decimal.MinValue
'            Dim niftyCurrentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.Intraday_Futures, Nothing, tradingDate, "NIFTY")
'            If niftyCurrentSymbolToken IsNot Nothing Then
'                Dim niftyTradingSymbol As String = niftyCurrentSymbolToken.Item2
'                Dim niftyEODPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures, niftyTradingSymbol, tradingDate.AddDays(-15), tradingDate)
'                If niftyEODPayload IsNot Nothing AndAlso niftyEODPayload.Count > 0 AndAlso niftyEODPayload.LastOrDefault.Key.Date = tradingDate.Date Then
'                    If niftyEODPayload.LastOrDefault.Value.PreviousCandlePayload IsNot Nothing Then
'                        Dim previousDayPayload As Payload = niftyEODPayload.LastOrDefault.Value.PreviousCandlePayload
'                        Dim niftyIntradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures, niftyTradingSymbol, tradingDate.AddDays(-15), tradingDate)
'                        If niftyIntradayPayload IsNot Nothing AndAlso niftyIntradayPayload.Count > 0 Then
'                            If topGainerTopLosserUserInputs.NiftyChangePercentage = 0 Then
'                                Dim candleToCheck As Payload = Nothing
'                                If niftyIntradayPayload.ContainsKey(payloadTime) Then
'                                    candleToCheck = niftyIntradayPayload(payloadTime)
'                                End If
'                                If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
'                                    niftyGainLossPercentage = Math.Round(((candleToCheck.Close - previousDayPayload.Close) / previousDayPayload.Close) * 100, 4)
'                                End If
'                            Else
'                                For Each runningPaylod In niftyIntradayPayload.Keys
'                                    If runningPaylod >= payloadTime Then
'                                        Dim candleToCheck As Payload = niftyIntradayPayload(runningPaylod)
'                                        If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
'                                            niftyGainLossPercentage = Math.Round(((candleToCheck.Close - previousDayPayload.Close) / previousDayPayload.Close) * 100, 4)
'                                            If Math.Abs(niftyGainLossPercentage) >= topGainerTopLosserUserInputs.NiftyChangePercentage Then
'                                                payloadTime = runningPaylod
'                                                Exit For
'                                            End If
'                                        End If
'                                    End If
'                                Next
'                            End If
'                        End If
'                    End If
'                End If
'            End If
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, String()) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim currentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(intradayTable, index, tradingDate, runningStock)
'                If currentSymbolToken IsNot Nothing Then
'                    Dim tradingSymbol As String = currentSymbolToken.Item2
'                    Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, tradingSymbol, tradingDate.AddDays(-15), tradingDate)
'                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
'                        Dim candleToCheck As Payload = Nothing
'                        If intradayPayload.ContainsKey(payloadTime) Then
'                            candleToCheck = intradayPayload(payloadTime)
'                        End If
'                        If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
'                            Dim previousClose As Decimal = highATRStockList(runningStock).PreviousDayClose
'                            Dim gainLossPercentage As Decimal = ((candleToCheck.Close - previousClose) / previousClose) * 100
'                            'Dim slab As Decimal = CalculateSlab(candleToCheck.Close, highATRStockList(runningStock).ATRPercentage)
'                            'Dim dayATR As Decimal = highATRStockList(runningStock).DayATR
'                            'Dim targetPoint As Decimal = Utilities.Numbers.ConvertFloorCeling(dayATR / 2, 0.05, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
'                            'Dim slPoint As Decimal = Utilities.Numbers.ConvertFloorCeling(dayATR / 4, 0.05, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
'                            'Dim eodTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 15, 15, 0)
'                            'Dim plPoint As Decimal = 0
'                            'Dim time As Date = tradingDate.Date
'                            'For Each runningPayload In intradayPayload.Values
'                            '    If runningPayload.PayloadDate > payloadTime Then
'                            '        If gainLossPercentage > 0 Then
'                            '            If runningPayload.PayloadDate = eodTime Then
'                            '                plPoint = runningPayload.Open - candleToCheck.Close
'                            '                time = runningPayload.PayloadDate
'                            '                Exit For
'                            '            ElseIf runningPayload.High - candleToCheck.Close >= targetPoint Then
'                            '                plPoint = targetPoint
'                            '                time = runningPayload.PayloadDate
'                            '                Exit For
'                            '            ElseIf runningPayload.Low - candleToCheck.Close <= slPoint * -1 Then
'                            '                plPoint = slPoint * -1
'                            '                time = runningPayload.PayloadDate
'                            '                Exit For
'                            '            End If
'                            '        ElseIf gainLossPercentage < 0 Then
'                            '            If runningPayload.PayloadDate = eodTime Then
'                            '                plPoint = candleToCheck.Close - runningPayload.Open
'                            '                time = runningPayload.PayloadDate
'                            '                Exit For
'                            '            ElseIf candleToCheck.Close - runningPayload.Low >= targetPoint Then
'                            '                plPoint = targetPoint
'                            '                time = runningPayload.PayloadDate
'                            '                Exit For
'                            '            ElseIf candleToCheck.Close - runningPayload.High <= slPoint * -1 Then
'                            '                plPoint = slPoint * -1
'                            '                time = runningPayload.PayloadDate
'                            '                Exit For
'                            '            End If
'                            '        End If
'                            '    End If
'                            'Next
'                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
'                            tempStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), niftyGainLossPercentage, payloadTime.ToString("HH:mm:ss")})
'                            'tempStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), plPoint, time.ToString("HH:mm:ss")})
'                        End If
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList.OrderByDescending(Function(x)
'                                                                             Return CDec(x.Value(0))
'                                                                         End Function)
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    highATRStockList(runningStock.Key).Supporting2 = runningStock.Value(1)
'                    highATRStockList(runningStock.Key).Supporting3 = runningStock.Value(2)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetHighLowGapStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim currentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(intradayTable, index, tradingDate, runningStock)
'                If currentSymbolToken IsNot Nothing Then
'                    Dim tradingSymbol As String = currentSymbolToken.Item2
'                    Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, tradingSymbol, tradingDate.AddDays(-15), tradingDate)
'                    Dim eodPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(eodTable, tradingSymbol, tradingDate.AddDays(-15), tradingDate)
'                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
'                        Dim signalStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
'                        Dim signalEndTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 20, 0)
'                        Dim signalPayload As IEnumerable(Of Payload) = intradayPayload.Values.Where(Function(x)
'                                                                                                        Return x.PayloadDate >= signalStartTime AndAlso
'                                                                                                               x.PayloadDate < signalEndTime
'                                                                                                    End Function)
'                        If signalPayload IsNot Nothing AndAlso signalPayload.Count > 0 Then
'                            Dim open As Decimal = signalPayload.FirstOrDefault.Open
'                            Dim high As Decimal = signalPayload.Max(Function(x)
'                                                                        Return x.High
'                                                                    End Function)
'                            Dim low As Decimal = signalPayload.Min(Function(x)
'                                                                       Return x.Low
'                                                                   End Function)
'                            If open > eodPayload.LastOrDefault.Value.PreviousCandlePayload.High Then
'                                If low > eodPayload.LastOrDefault.Value.PreviousCandlePayload.High Then
'                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
'                                    tempStockList.Add(runningStock, {1})
'                                End If
'                            ElseIf open < eodPayload.LastOrDefault.Value.PreviousCandlePayload.Low Then
'                                If high < eodPayload.LastOrDefault.Value.PreviousCandlePayload.Low Then
'                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
'                                    tempStockList.Add(runningStock, {-1})
'                                End If
'                            End If
'                        End If
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetSpotFutureArbritrageStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        If spotFutureArbritrageUserInputs Is Nothing Then Throw New ApplicationException("Spot Future Arbritrage Settings not implemented properly")
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, String()) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim currentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.Intraday_Futures, index, tradingDate, runningStock)
'                If currentSymbolToken IsNot Nothing Then
'                    Dim tradingSymbol As String = currentSymbolToken.Item2
'                    Dim instrumentName As String = tradingSymbol.Remove(tradingSymbol.Count - 8)
'                    Dim futureIntradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures, tradingSymbol, tradingDate, tradingDate)
'                    Dim cashIntradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, instrumentName, tradingDate, tradingDate)
'                    If futureIntradayPayload IsNot Nothing AndAlso futureIntradayPayload.Count > 0 Then
'                        Dim diff As String = ""
'                        Dim time As String = ""
'                        For Each runningPayload In futureIntradayPayload
'                            If cashIntradayPayload.ContainsKey(runningPayload.Key) Then
'                                Dim diffPer As Decimal = ((runningPayload.Value.Close / cashIntradayPayload(runningPayload.Key).Close) - 1) * 100
'                                If diffPer >= spotFutureArbritrageUserInputs.DifferencePercentage Then
'                                    diff = String.Format("{0};{1}", diff, Math.Round(diffPer, 2))
'                                    time = String.Format("{0};{1}", time, runningPayload.Key.ToString("dd-MM-yyyy HH:mm:ss"))
'                                End If
'                            End If
'                        Next
'                        If diff <> "" AndAlso time <> "" Then
'                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
'                            tempStockList.Add(runningStock, {diff.Substring(1), time.Substring(1)})
'                        End If
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    highATRStockList(runningStock.Key).Supporting2 = runningStock.Value(1)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetHighTurnoverStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim tempStockList As Dictionary(Of String, String()) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim currentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.Intraday_Cash, index, tradingDate, runningStock)
'                If currentSymbolToken IsNot Nothing Then
'                    Dim tradingSymbol As String = currentSymbolToken.Item2
'                    Dim cashEODPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Cash, tradingSymbol, tradingDate.AddDays(-15), tradingDate.AddDays(-1))
'                    If cashEODPayload IsNot Nothing AndAlso cashEODPayload.Count > 0 Then
'                        Dim dayCtr As Integer = 0
'                        Dim turnover As Decimal = 0
'                        For Each runningPayload In cashEODPayload.OrderByDescending(Function(x)
'                                                                                        Return x.Key
'                                                                                    End Function)
'                            turnover += runningPayload.Value.Close * runningPayload.Value.Volume
'                            dayCtr += 1
'                            If dayCtr = 5 Then Exit For
'                        Next

'                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
'                        tempStockList.Add(runningStock, {Math.Round(turnover / 5)})
'                    End If
'                End If
'            Next
'            If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
'                For Each runningStock In tempStockList.OrderByDescending(Function(x)
'                                                                             Return CDec(x.Value(0))
'                                                                         End Function)
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function

'    Private Async Function GetTopGainerTopLosserEveryMinuteStockDataAsync(ByVal tradingDate As Date) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        Dim highATRStockList As Dictionary(Of String, InstrumentDetails) = Await GetATRBasedAllStockDataAsync(tradingDate).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If highATRStockList IsNot Nothing AndAlso highATRStockList.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim startTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 19, 0)
'            Dim endTime As Date = startTime.AddHours(2)
'            Dim payloadTime As Date = startTime
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim stockData As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
'            For Each runningStock In highATRStockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim currentSymbolToken As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(intradayTable, index, tradingDate, runningStock)
'                If currentSymbolToken IsNot Nothing Then
'                    Dim tradingSymbol As String = currentSymbolToken.Item2
'                    Dim intradayPayload As Dictionary(Of Date, Payload) = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, tradingSymbol, tradingDate, tradingDate)
'                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
'                        If stockData Is Nothing Then stockData = New Dictionary(Of String, Dictionary(Of Date, Payload))
'                        stockData.Add(runningStock, intradayPayload)
'                    End If
'                End If
'            Next
'            Dim topGainerLosserStockList As Dictionary(Of String, String()) = Nothing
'            If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
'                While payloadTime <= endTime
'                    Dim tempCloseStockList As Dictionary(Of String, String()) = Nothing
'                    Dim tempOpenStockList As Dictionary(Of String, String()) = Nothing
'                    For Each runningStock In highATRStockList.Keys
'                        _cts.Token.ThrowIfCancellationRequested()
'                        If stockData.ContainsKey(runningStock) Then
'                            Dim intradayPayload As Dictionary(Of Date, Payload) = stockData(runningStock)
'                            If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
'                                Dim candleToCheck As Payload = Nothing
'                                If intradayPayload.ContainsKey(payloadTime) Then
'                                    candleToCheck = intradayPayload(payloadTime)
'                                End If
'                                If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
'                                    Dim previousClose As Decimal = highATRStockList(runningStock).PreviousDayClose
'                                    Dim gainLossPercentage As Decimal = ((candleToCheck.Close - previousClose) / previousClose) * 100
'                                    If tempCloseStockList Is Nothing Then tempCloseStockList = New Dictionary(Of String, String())
'                                    tempCloseStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), payloadTime.ToString("HH:mm:ss"), "Previous Close"})
'                                End If
'                                If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
'                                    Dim currentOpen As Decimal = intradayPayload.FirstOrDefault.Value.Open
'                                    Dim gainLossPercentage As Decimal = ((candleToCheck.Close - currentOpen) / currentOpen) * 100
'                                    If tempOpenStockList Is Nothing Then tempOpenStockList = New Dictionary(Of String, String())
'                                    tempOpenStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), payloadTime.ToString("HH:mm:ss"), "Current Open"})
'                                End If
'                            End If
'                        End If
'                    Next
'                    If tempCloseStockList IsNot Nothing AndAlso tempCloseStockList.Count > 0 Then
'                        Dim topGainer As KeyValuePair(Of String, String()) = tempCloseStockList.OrderByDescending(Function(x)
'                                                                                                                      Return CDec(x.Value(0))
'                                                                                                                  End Function).FirstOrDefault
'                        Dim topLosser As KeyValuePair(Of String, String()) = tempCloseStockList.OrderByDescending(Function(x)
'                                                                                                                      Return CDec(x.Value(0))
'                                                                                                                  End Function).LastOrDefault
'                        If topGainerLosserStockList Is Nothing Then topGainerLosserStockList = New Dictionary(Of String, String())
'                        If Not topGainerLosserStockList.ContainsKey(topGainer.Key) Then topGainerLosserStockList.Add(topGainer.Key, topGainer.Value)
'                        If Not topGainerLosserStockList.ContainsKey(topLosser.Key) Then topGainerLosserStockList.Add(topLosser.Key, topLosser.Value)
'                    End If
'                    If tempOpenStockList IsNot Nothing AndAlso tempOpenStockList.Count > 0 Then
'                        Dim topGainer As KeyValuePair(Of String, String()) = tempOpenStockList.OrderByDescending(Function(x)
'                                                                                                                     Return CDec(x.Value(0))
'                                                                                                                 End Function).FirstOrDefault
'                        Dim topLosser As KeyValuePair(Of String, String()) = tempOpenStockList.OrderByDescending(Function(x)
'                                                                                                                     Return CDec(x.Value(0))
'                                                                                                                 End Function).LastOrDefault
'                        If topGainerLosserStockList Is Nothing Then topGainerLosserStockList = New Dictionary(Of String, String())
'                        If Not topGainerLosserStockList.ContainsKey(topGainer.Key) Then
'                            topGainerLosserStockList.Add(topGainer.Key, topGainer.Value)
'                        Else
'                            'topGainerLosserStockList(topGainer.Key)(2) = "Previous Close + Current Open"
'                        End If
'                        If Not topGainerLosserStockList.ContainsKey(topLosser.Key) Then
'                            topGainerLosserStockList.Add(topLosser.Key, topLosser.Value)
'                        Else
'                            'topGainerLosserStockList(topLosser.Key)(2) = "Previous Close + Current Open"
'                        End If
'                    End If

'                    payloadTime = payloadTime.AddMinutes(1)
'                End While
'            End If
'            If topGainerLosserStockList IsNot Nothing AndAlso topGainerLosserStockList.Count > 0 Then
'                For Each runningStock In topGainerLosserStockList
'                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                    highATRStockList(runningStock.Key).Supporting1 = runningStock.Value(0)
'                    highATRStockList(runningStock.Key).Supporting2 = runningStock.Value(1)
'                    highATRStockList(runningStock.Key).Supporting3 = runningStock.Value(2)
'                    ret.Add(runningStock.Key, highATRStockList(runningStock.Key))
'                Next
'            End If
'        End If
'        Return ret
'    End Function
'#End Region

'#Region "Main Public Function"
'    Public Async Function GetStockData(ByVal tradingDate As Date,
'                                       ByVal procedureToRun As Integer,
'                                       ByVal bannedStocks As List(Of String),
'                                       ByVal userGivenInstrumentList As Dictionary(Of String, Decimal())) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        Dim stockList As Dictionary(Of String, InstrumentDetails) = Nothing
'        Dim isTradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
'        If isTradingDay OrElse tradingDate.Date = Now.Date Then
'            Select Case procedureToRun
'                Case 0
'                    If userGivenInstrumentList IsNot Nothing AndAlso userGivenInstrumentList.Count > 0 Then
'                        For Each ruuningUserGivenStock In userGivenInstrumentList
'                            _cts.Token.ThrowIfCancellationRequested()
'                            Dim currentTradingSymbol As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(eodTable, Nothing, tradingDate, ruuningUserGivenStock.Key)
'                            If currentTradingSymbol IsNot Nothing Then
'                                Dim lotSize As Integer = _common.GetLotSize(eodTable, currentTradingSymbol.Item2, tradingDate)
'                                If stockList Is Nothing Then stockList = New Dictionary(Of String, InstrumentDetails)
'                                stockList.Add(ruuningUserGivenStock.Key, New InstrumentDetails With {.ATRPercentage = 0, .LotSize = lotSize, .DayATR = 0, .PreviousDayOpen = 0, .PreviousDayLow = 0, .PreviousDayHigh = 0, .PreviousDayClose = 0})
'                            End If
'                        Next
'                    End If
'                Case 1
'                    stockList = Await GetATRBasedFOStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 2
'                    stockList = Await GetPreMarketStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 3
'                    stockList = Await GetIntradayVolumeSpikeStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 4
'                    stockList = Await GetOHLATRStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 5
'                    stockList = Await GetTouchPreviousDayLastCandleStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 6
'                    stockList = Await GetTopGainerTopLosserStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 7
'                    stockList = Await GetHighLowGapStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 8
'                    stockList = Await GetSpotFutureArbritrageStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 9
'                    stockList = Await GetHighTurnoverStockDataAsync(tradingDate).ConfigureAwait(False)
'                Case 10
'                    stockList = Await GetTopGainerTopLosserEveryMinuteStockDataAsync(tradingDate).ConfigureAwait(False)
'            End Select
'            _cts.Token.ThrowIfCancellationRequested()

'            If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
'                For Each stock In stockList.Keys
'                    _cts.Token.ThrowIfCancellationRequested()
'                    If bannedStocks Is Nothing OrElse
'                    (bannedStocks IsNot Nothing AndAlso bannedStocks.Count > 0 AndAlso Not bannedStocks.Contains(stock.ToUpper)) Then
'                        _cts.Token.ThrowIfCancellationRequested()
'                        Dim instrumentDetails As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(intradayTable, index, tradingDate, stock)
'                        If instrumentDetails IsNot Nothing Then
'                            Dim tradingSymbol As String = instrumentDetails.Item2
'                            _cts.Token.ThrowIfCancellationRequested()
'                            If tradingSymbol IsNot Nothing AndAlso tradingSymbol <> "" Then
'                                _cts.Token.ThrowIfCancellationRequested()
'                                If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
'                                Dim instrumentData As New InstrumentDetails With
'                                {.TradingSymbol = tradingSymbol,
'                                 .ATRPercentage = stockList(stock).ATRPercentage,
'                                 .LotSize = stockList(stock).LotSize,
'                                 .DayATR = stockList(stock).DayATR,
'                                 .PreviousDayOpen = stockList(stock).PreviousDayOpen,
'                                 .PreviousDayLow = stockList(stock).PreviousDayLow,
'                                 .PreviousDayHigh = stockList(stock).PreviousDayHigh,
'                                 .PreviousDayClose = stockList(stock).PreviousDayClose,
'                                 .Slab = CalculateSlab(.PreviousDayClose, .ATRPercentage),
'                                 .Supporting1 = If(stockList(stock).Supporting1 IsNot Nothing, stockList(stock).Supporting1, 0),
'                                 .Supporting2 = If(stockList(stock).Supporting2 IsNot Nothing, stockList(stock).Supporting2, 0),
'                                 .Supporting3 = If(stockList(stock).Supporting3 IsNot Nothing, stockList(stock).Supporting3, 0),
'                                 .Supporting4 = If(stockList(stock).Supporting4 IsNot Nothing, stockList(stock).Supporting4, 0),
'                                 .Supporting5 = If(stockList(stock).Supporting5 IsNot Nothing, stockList(stock).Supporting5, 0)}
'                                ret.Add(tradingSymbol, instrumentData)
'                            End If
'                        End If
'                    End If
'                Next
'            End If
'        End If
'        Return ret
'    End Function
'#End Region

'#Region "Supporting Public Function"
'    Public Async Function GetActiveInstrumentData(ByVal tradingDate As Date, ByVal stockList As Dictionary(Of String, InstrumentDetails)) As Task(Of Dictionary(Of String, InstrumentDetails))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
'        If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
'            For Each stock In stockList.Keys
'                _cts.Token.ThrowIfCancellationRequested()
'                If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
'                    _cts.Token.ThrowIfCancellationRequested()
'                    _conn = _common.OpenDBConnection()
'                End If
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim rawInstrumentName As String = stock
'                If stock.ToUpper.Contains("FUT") Then
'                    rawInstrumentName = stock.Remove(stock.Count - 8)
'                End If
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim cm As MySqlCommand = Nothing
'                Select Case eodTable
'                    Case Common.DataBaseTable.EOD_Cash
'                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL` LIKE @trd AND `AS_ON_DATE`=@sd", _conn)
'                        cm.Parameters.AddWithValue("@trd", String.Format("{0}", rawInstrumentName))
'                    Case Common.DataBaseTable.EOD_Commodity
'                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='MCX' AND `AS_ON_DATE`=@sd", _conn)
'                        cm.Parameters.AddWithValue("@trd", String.Format("{0}[0-9][0-9]*", rawInstrumentName))
'                    Case Common.DataBaseTable.EOD_Currency
'                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='CDS-FUT' AND `AS_ON_DATE`=@sd", _conn)
'                        cm.Parameters.AddWithValue("@trd", String.Format("{0}[0-9][0-9]*", rawInstrumentName))
'                    Case Common.DataBaseTable.EOD_Futures
'                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='NFO-FUT' AND `AS_ON_DATE`=@sd", _conn)
'                        cm.Parameters.AddWithValue("@trd", String.Format("{0}[0-9][0-9]*", rawInstrumentName))
'                End Select
'                cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim adapter As New MySqlDataAdapter(cm)
'                adapter.SelectCommand.CommandTimeout = 300
'                Dim dt As New DataTable
'                _cts.Token.ThrowIfCancellationRequested()
'                adapter.Fill(dt)
'                _cts.Token.ThrowIfCancellationRequested()
'                Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing
'                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
'                    For i = 0 To dt.Rows.Count - 1
'                        _cts.Token.ThrowIfCancellationRequested()
'                        Dim instrumentData As New ActiveInstrumentData With
'                        {.Token = dt.Rows(i).Item(0),
'                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
'                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}

'                        Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
'                        If Regex.Matches(instrumentData.TradingSymbol, pattern).Count <= 1 Then
'                            If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
'                            activeInstruments.Add(instrumentData)
'                            If instrumentData.TradingSymbol = stock Then
'                                stockList(stock).InstrumentIdentifier = instrumentData.Token
'                                stockList(stock).CurrentContractExpiry = instrumentData.Expiry
'                            End If
'                        End If
'                    Next
'                End If
'                If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
'                    _cts.Token.ThrowIfCancellationRequested()
'                    Dim minExpiry As Date = activeInstruments.Min(Function(x)
'                                                                      Return x.Expiry
'                                                                  End Function)
'                    If minExpiry.Date = stockList(stock).CurrentContractExpiry.Date Then
'                        stockList(stock).IsTradable = True
'                        _cts.Token.ThrowIfCancellationRequested()
'                    Else
'                        _cts.Token.ThrowIfCancellationRequested()
'                        If minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date > tradingDate.Date Then
'                            _cts.Token.ThrowIfCancellationRequested()
'                            Throw New ApplicationException(String.Format("Check stock {0} on {1}", stockList(stock).TradingSymbol, tradingDate))
'                            stockList(stock).IsTradable = False
'                            stockList(stock).PreviousContractTradingSymbol = activeInstruments.Find(Function(x)
'                                                                                                        Return x.Expiry.Date = minExpiry.Date
'                                                                                                    End Function).TradingSymbol
'                            stockList(stock).PreviousContractExpiry = minExpiry
'                        ElseIf minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date = tradingDate.Date Then
'                            _cts.Token.ThrowIfCancellationRequested()
'                            stockList(stock).IsTradable = True
'                            stockList(stock).PreviousContractTradingSymbol = activeInstruments.Find(Function(x)
'                                                                                                        Return x.Expiry.Date = minExpiry.Date
'                                                                                                    End Function).TradingSymbol
'                            stockList(stock).PreviousContractExpiry = minExpiry
'                        ElseIf minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date < tradingDate.Date Then
'                            stockList(stock).IsTradable = True
'                            _cts.Token.ThrowIfCancellationRequested()
'                        End If
'                    End If
'                    ret = stockList
'                End If
'            Next
'        End If
'        Return ret
'    End Function

'    Public Async Function GetStockPayload(ByVal tradingDate As Date, ByVal instrumentData As InstrumentDetails, ByVal immediatePreviousDay As Boolean) As Task(Of Dictionary(Of Date, Payload))
'        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
'        Dim ret As Dictionary(Of Date, Payload) = Nothing
'        _cts.Token.ThrowIfCancellationRequested()
'        _cts.Token.ThrowIfCancellationRequested()
'        If instrumentData IsNot Nothing AndAlso instrumentData.IsTradable Then
'            If instrumentData.PreviousContractTradingSymbol Is Nothing Then
'                Dim previousTradingDate As Date = Date.MinValue
'                If immediatePreviousDay Then
'                    previousTradingDate = _common.GetPreviousTradingDay(intradayTable, tradingDate)
'                Else
'                    previousTradingDate = _common.GetPreviousTradingDayOfAnInstrument(intradayTable, instrumentData.TradingSymbol, tradingDate)
'                End If
'                If previousTradingDate <> Date.MinValue Then
'                    ret = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, instrumentData.TradingSymbol, previousTradingDate, previousTradingDate)
'                End If
'            Else
'                Dim previousTradingDate As Date = Date.MinValue
'                If immediatePreviousDay Then
'                    previousTradingDate = _common.GetPreviousTradingDay(intradayTable, tradingDate)
'                Else
'                    previousTradingDate = _common.GetPreviousTradingDayOfAnInstrument(intradayTable, instrumentData.PreviousContractTradingSymbol, tradingDate)
'                End If
'                If previousTradingDate <> Date.MinValue Then
'                    ret = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, instrumentData.PreviousContractTradingSymbol, previousTradingDate, previousTradingDate)
'                End If
'            End If
'        End If
'        Return ret
'    End Function

'    Public Async Function IsTradableDay(ByVal tradingDate As Date) As Task(Of Boolean)
'        Dim ret As Boolean = False
'        Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Await GetHistoricalCandleStickAsync("1723649", tradingDate, tradingDate, TypeOfData.Intraday).ConfigureAwait(False)
'        _cts.Token.ThrowIfCancellationRequested()
'        If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 Then
'            _cts.Token.ThrowIfCancellationRequested()
'            Dim intradayHistoricalData As Dictionary(Of Date, Payload) = Await GetChartFromHistoricalAsync(historicalCandlesJSONDict, "JINDALSTEL", tradingDate).ConfigureAwait(False)
'            If intradayHistoricalData IsNot Nothing AndAlso intradayHistoricalData.Count > 0 Then
'                ret = True
'            End If
'        End If
'        Return ret
'    End Function
'#End Region

'#Region "Supporting Private Functions"
'    Private Function CalculatePL(ByVal stockName As String, ByVal buyPrice As Decimal, ByVal sellPrice As Decimal, ByVal quantity As Integer, ByVal lotSize As Integer) As Decimal
'        Dim potentialBrokerage As New Calculator.BrokerageAttributes
'        Dim calculator As New Calculator.BrokerageCalculator(_cts)

'        'calculator.Intraday_Equity(buyPrice, sellPrice, quantity, potentialBrokerage)
'        'calculator.Currency_Futures(buyPrice, sellPrice, quantity / lotSize, potentialBrokerage)
'        'calculator.Commodity_MCX(stockName, buyPrice, sellPrice, quantity / lotSize, potentialBrokerage)
'        calculator.FO_Futures(buyPrice, sellPrice, quantity, potentialBrokerage)

'        Return potentialBrokerage.NetProfitLoss
'    End Function

'    Private Function CalculateBuffer(ByVal price As Decimal, ByVal floorOrCeiling As Utilities.Numbers.RoundOfType) As Decimal
'        Dim bufferPrice As Decimal = Nothing
'        'Assuming 1% target, we can afford to have buffer as 2.5% of that 1% target
'        bufferPrice = Utilities.Numbers.NumberManipulation.ConvertFloorCeling(price * 0.01 * 0.025, 0.05, floorOrCeiling)
'        Return bufferPrice
'    End Function

'    Private Function CalculateQuantityFromInvestment(ByVal lotSize As Integer, ByVal totalInvestment As Decimal, ByVal stockPrice As Decimal, ByVal allowIncreaseCapital As Boolean) As Integer
'        Dim quantity As Integer = lotSize
'        Dim quantityMultiplier As Integer = 1
'        If allowIncreaseCapital Then
'            quantityMultiplier = Math.Ceiling(totalInvestment / (quantity * stockPrice / 30))
'        Else
'            quantityMultiplier = Math.Floor(totalInvestment / (quantity * stockPrice / 30))
'        End If
'        If quantityMultiplier = 0 Then quantityMultiplier = 1
'        Return quantity * quantityMultiplier
'    End Function

'    Private Function IsInsideBar(ByVal candle As Payload) As Boolean
'        Dim ret As Boolean = False
'        If candle IsNot Nothing AndAlso candle.PreviousCandlePayload IsNot Nothing Then
'            If candle.High <= candle.PreviousCandlePayload.High AndAlso candle.Low >= candle.PreviousCandlePayload.Low Then
'                ret = True
'            End If
'        End If
'        Return ret
'    End Function

'    Private Function CalculateSlab(ByVal price As Decimal, ByVal atrPer As Decimal) As Decimal
'        Dim ret As Decimal = 0.25
'        Dim slabList As List(Of Decimal) = New List(Of Decimal) From {0.25, 0.5, 1, 2.5, 5, 10, 15}
'        Dim atr As Decimal = (atrPer / 100) * price
'        Dim supportedSlabList As List(Of Decimal) = slabList.FindAll(Function(x)
'                                                                         Return x <= atr / 8
'                                                                     End Function)
'        If supportedSlabList IsNot Nothing AndAlso supportedSlabList.Count > 0 Then
'            ret = supportedSlabList.Max
'            If price * 1 / 100 < ret Then
'                Dim newSupportedSlabList As List(Of Decimal) = supportedSlabList.FindAll(Function(x)
'                                                                                             Return x <= price * 1 / 100
'                                                                                         End Function)
'                If newSupportedSlabList IsNot Nothing AndAlso newSupportedSlabList.Count > 0 Then
'                    ret = newSupportedSlabList.Max
'                End If
'            End If
'        End If
'        Return ret
'    End Function
'#End Region

'#Region "Supporting Public Class"
'    'Public highVolumeInsideBarHLUserInputs As HighVolumeInsideBarHLSettings = Nothing
'    'Public Class HighVolumeInsideBarHLSettings
'    '    Public CheckVolumeTillSignalTime As Boolean
'    '    Public CheckEODVolume As Boolean
'    '    Public Previous5DaysAvgVolumePercentage As Decimal
'    'End Class

'    Public intradayVolumeSpikeUserInputs As IntradayVolumeSpikeSettings = Nothing
'    Public Class IntradayVolumeSpikeSettings
'        Public CheckingTime As Date
'    End Class

'    Public topGainerTopLosserUserInputs As TopGainerTopLosserSettings = Nothing
'    Public Class TopGainerTopLosserSettings
'        Public CheckingTime As Date
'        Public NiftyChangePercentage As Decimal
'    End Class

'    Public spotFutureArbritrageUserInputs As SpotFutureArbritrageSettings = Nothing
'    Public Class SpotFutureArbritrageSettings
'        Public DifferencePercentage As Decimal
'    End Class
'#End Region

'#Region "IDisposable Support"
'    Private disposedValue As Boolean ' To detect redundant calls

'    ' IDisposable
'    Protected Overridable Sub Dispose(disposing As Boolean)
'        If Not disposedValue Then
'            If disposing Then
'                ' TODO: dispose managed state (managed objects).
'            End If

'            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
'            ' TODO: set large fields to null.
'        End If
'        disposedValue = True
'    End Sub

'    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
'    'Protected Overrides Sub Finalize()
'    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'    '    Dispose(False)
'    '    MyBase.Finalize()
'    'End Sub

'    ' This code added by Visual Basic to correctly implement the disposable pattern.
'    Public Sub Dispose() Implements IDisposable.Dispose
'        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'        Dispose(True)
'        ' TODO: uncomment the following line if Finalize() is overridden above.
'        ' GC.SuppressFinalize(Me)
'    End Sub
'#End Region
'End Class