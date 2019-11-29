﻿Imports System.IO
Imports System.Net
Imports System.Threading
Imports MySql.Data.MySqlClient
Imports Utilities.DAL
Imports Utilities.Strings

Public Class Common
    Implements IDisposable
    Dim conn As MySqlConnection
    Private _canceller As CancellationTokenSource
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

#Region "Constructor"
    Public Sub New(calceller As CancellationTokenSource)
        _canceller = calceller
    End Sub
#End Region

#Region "Enum"
    Public Enum DataBaseTable
        Intraday_Cash = 1
        Intraday_Commodity
        Intraday_Currency
        Intraday_Futures
        EOD_Cash
        EOD_Commodity
        EOD_Currency
        EOD_Futures
    End Enum
#End Region

#Region "Public Methods"
    Public Function GetPayloadAtPositionOrPositionMinus1(ByVal beforeThisTime As DateTime, ByVal inputPayload As Dictionary(Of Date, Decimal)) As KeyValuePair(Of DateTime, Decimal)
        Dim ret As KeyValuePair(Of DateTime, Decimal) = Nothing
        If inputPayload IsNot Nothing Then
            Dim tempret = inputPayload.Where(Function(x)
                                                 Return x.Key < beforeThisTime
                                             End Function)
            If tempret IsNot Nothing Then
                ret = tempret.LastOrDefault
            End If
        End If
        Return ret
    End Function
    Public Shared Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, Decimal),
                                     ByVal beforeThisTime As DateTime,
                                      ByVal numberOfItemsToRetrive As Integer,
                                      ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of DateTime, Decimal))
        Dim ret As List(Of KeyValuePair(Of DateTime, Decimal)) = Nothing
        If inputPayload IsNot Nothing Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = -1
            Dim loopTerminatedOnCondition As Boolean = False
            For Each item In inputPayload
                firstIndexOfKey += 1
                If item.Key >= beforeThisTime Then
                    loopTerminatedOnCondition = True
                    Exit For
                End If
            Next
            If loopTerminatedOnCondition Then 'Specially useful for only 1 count of item is there
                If Not includeTimePassedAsOneOftheItems Then
                    firstIndexOfKey -= 1
                End If
            End If
            If firstIndexOfKey >= 0 Then
                Dim startIndex As Integer = Math.Max((firstIndexOfKey - numberOfItemsToRetrive) + 1, 0)
                Dim revisedNumberOfItemsToRetrieve As Integer = Math.Min(numberOfItemsToRetrive, (firstIndexOfKey - startIndex) + 1)
                Dim referencePayLoadAsList = inputPayload.ToList
                ret = referencePayLoadAsList.GetRange(startIndex, revisedNumberOfItemsToRetrieve)
            End If
        End If
        Return ret
    End Function
    Public Shared Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, Payload),
                                     ByVal beforeThisTime As DateTime,
                                      ByVal numberOfItemsToRetrive As Integer,
                                      ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of DateTime, Payload))
        Dim ret As List(Of KeyValuePair(Of DateTime, Payload)) = Nothing
        If inputPayload IsNot Nothing Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = -1
            Dim loopTerminatedOnCondition As Boolean = False
            For Each item In inputPayload

                firstIndexOfKey += 1
                If item.Key >= beforeThisTime Then
                    loopTerminatedOnCondition = True
                    Exit For
                End If
            Next
            If loopTerminatedOnCondition Then 'Specially useful for only 1 count of item is there
                If Not includeTimePassedAsOneOftheItems Then
                    firstIndexOfKey -= 1
                End If
            End If
            If firstIndexOfKey >= 0 Then
                Dim startIndex As Integer = Math.Max((firstIndexOfKey - numberOfItemsToRetrive) + 1, 0)
                Dim revisedNumberOfItemsToRetrieve As Integer = Math.Min(numberOfItemsToRetrive, (firstIndexOfKey - startIndex) + 1)
                Dim referencePayLoadAsList = inputPayload.ToList
                ret = referencePayLoadAsList.GetRange(startIndex, revisedNumberOfItemsToRetrieve)
            End If
        End If
        Return ret
    End Function
    Public Shared Function GetPayloadAt(ByVal inputPayload As Dictionary(Of Date, Payload),
                                        ByVal currentTime As Date,
                                        ByVal positionToRetrive As Integer) As KeyValuePair(Of Date, Payload)?
        Dim ret As KeyValuePair(Of Date, Payload)? = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 AndAlso inputPayload.ContainsKey(currentTime) Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = Array.IndexOf(inputPayload.Keys.ToArray, currentTime)
            Dim indexToRetrive As Integer = firstIndexOfKey + positionToRetrive
            If positionToRetrive > 0 Then
                indexToRetrive = indexToRetrive - 1
            ElseIf positionToRetrive < 0 Then
                indexToRetrive = indexToRetrive + 1
            End If
            If indexToRetrive >= 0 AndAlso indexToRetrive < inputPayload.Count Then
                Dim retrivedDate As Date = inputPayload.Keys.ToArray(indexToRetrive)
                ret = New KeyValuePair(Of Date, Payload)(retrivedDate, inputPayload(retrivedDate))
            End If
        End If
        Return ret
    End Function
    Public Shared Function GetPayloadAt(ByVal inputPayload As Dictionary(Of Date, Decimal),
                                        ByVal currentTime As Date,
                                        ByVal positionToRetrive As Integer) As KeyValuePair(Of Date, Decimal)?
        Dim ret As KeyValuePair(Of Date, Decimal)? = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 AndAlso inputPayload.ContainsKey(currentTime) Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = Array.IndexOf(inputPayload.Keys.ToArray, currentTime)
            Dim indexToRetrive As Integer = firstIndexOfKey + positionToRetrive
            If positionToRetrive > 0 Then
                indexToRetrive = indexToRetrive - 1
            ElseIf positionToRetrive < 0 Then
                indexToRetrive = indexToRetrive + 1
            End If
            If indexToRetrive >= 0 AndAlso indexToRetrive < inputPayload.Count Then
                Dim retrivedDate As Date = inputPayload.Keys.ToArray(indexToRetrive)
                ret = New KeyValuePair(Of Date, Decimal)(retrivedDate, inputPayload(retrivedDate))
            End If
        End If
        Return ret
    End Function
    Public Function ConvertPayloadsToXMinutes(ByVal payloads As Dictionary(Of Date, Payload), ByVal minute As Integer) As Dictionary(Of Date, Payload)
        Dim XMinutePayloads As Dictionary(Of Date, Payload) = Nothing
        If payloads IsNot Nothing AndAlso payloads.Count > 0 Then
            Dim newCandleStarted As Boolean = True
            Dim runningOutputPayload As Payload = Nothing
            Dim startTime As Date = DateTime.MaxValue
            Dim endTime As Date = Date.MaxValue
            For Each payload In payloads.Values

                If payload.PayloadDate >= endTime Then
                    newCandleStarted = True
                    If runningOutputPayload IsNot Nothing Then
                        If XMinutePayloads Is Nothing Then XMinutePayloads = New Dictionary(Of Date, Payload)
                        XMinutePayloads.Add(runningOutputPayload.PayloadDate, runningOutputPayload)
                    End If
                End If
                If newCandleStarted Then
                    newCandleStarted = False
                    startTime = payload.PayloadDate
                    endTime = payload.PayloadDate.AddMinutes(minute)
                    Dim prevPayload As Payload = runningOutputPayload
                    runningOutputPayload = New Payload(Payload.CandleDataSource.Calculated)
                    runningOutputPayload.PayloadDate = startTime
                    runningOutputPayload.Open = payload.Open
                    runningOutputPayload.High = payload.High
                    runningOutputPayload.Low = payload.Low
                    runningOutputPayload.Close = payload.Close
                    runningOutputPayload.Volume = payload.Volume
                    runningOutputPayload.TradingSymbol = payload.TradingSymbol
                    runningOutputPayload.PreviousCandlePayload = prevPayload
                Else
                    runningOutputPayload.High = Math.Max(runningOutputPayload.High, payload.High)
                    runningOutputPayload.Low = Math.Min(runningOutputPayload.Low, payload.Low)
                    runningOutputPayload.Close = payload.Close
                    runningOutputPayload.Volume = runningOutputPayload.Volume + payload.Volume
                End If
                If (runningOutputPayload.PreviousCandlePayload IsNot Nothing AndAlso runningOutputPayload.PayloadDate.Date <> runningOutputPayload.PreviousCandlePayload.PayloadDate.Date) Then
                    runningOutputPayload.CumulativeVolume = runningOutputPayload.Volume
                ElseIf (runningOutputPayload.PreviousCandlePayload Is Nothing) Then
                    runningOutputPayload.CumulativeVolume = runningOutputPayload.Volume
                ElseIf (runningOutputPayload.PreviousCandlePayload IsNot Nothing AndAlso runningOutputPayload.PayloadDate.Date = runningOutputPayload.PreviousCandlePayload.PayloadDate.Date) Then
                    runningOutputPayload.CumulativeVolume = runningOutputPayload.PreviousCandlePayload.CumulativeVolume + runningOutputPayload.Volume
                End If
            Next
            If runningOutputPayload IsNot Nothing Then
                If XMinutePayloads Is Nothing Then XMinutePayloads = New Dictionary(Of Date, Payload)
                XMinutePayloads.Add(runningOutputPayload.PayloadDate, runningOutputPayload)
            End If
        End If
        Return XMinutePayloads
    End Function
    Public Function ConvetDecimalToPayload(ByVal targetfield As Payload.PayloadFields, ByVal inputpayload As Dictionary(Of Date, Decimal), ByRef outputpayload As Dictionary(Of Date, Payload))
        Dim output As Payload
        outputpayload = New Dictionary(Of Date, Payload)
        For Each runningitem In inputpayload
            output = New Payload(Payload.CandleDataSource.Chart)
            output.PayloadDate = runningitem.Key
            Select Case targetfield
                Case Payload.PayloadFields.Close
                    output.Close = runningitem.Value
                Case Payload.PayloadFields.C_AVG_HL
                    output.C_AVG_HL = runningitem.Value
                Case Payload.PayloadFields.High
                    output.High = runningitem.Value
                Case Payload.PayloadFields.H_L
                    output.H_L = runningitem.Value
                Case Payload.PayloadFields.Low
                    output.Low = runningitem.Value
                Case Payload.PayloadFields.Open
                    output.Open = runningitem.Value
                Case Payload.PayloadFields.Volume
                    output.Volume = runningitem.Value
                Case Payload.PayloadFields.SMI_EMA
                    output.SMI_EMA = runningitem.Value
                Case Payload.PayloadFields.Additional_Field
                    output.Additional_Field = runningitem.Value
            End Select
            outputpayload.Add(runningitem.Key, output)
        Next
        Return Nothing
    End Function
    Public Function ConvertDataTableToPayload(ByVal dt As DataTable,
                                              ByVal openColumnIndex As Integer,
                                              ByVal lowColumnIndex As Integer,
                                              ByVal highColumnIndex As Integer,
                                              ByVal closeColumnIndex As Integer,
                                              ByVal volumeColumnIndex As Integer,
                                              ByVal dateColumnIndex As Integer,
                                              ByVal tradingSymbolColumnIndex As Integer) As Dictionary(Of Date, Payload)

        Dim inputpayload As Dictionary(Of Date, Payload) = Nothing

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            Dim cur_cum_vol As Long = Nothing
            inputpayload = New Dictionary(Of Date, Payload)
            Dim tempPreCandle As Payload = Nothing
            While Not i = dt.Rows.Count()

                Dim tempPayload As Payload
                tempPayload = New Payload(Payload.CandleDataSource.Chart)
                tempPayload.PreviousCandlePayload = tempPreCandle
                tempPayload.Open = dt.Rows(i).Item(openColumnIndex)
                tempPayload.Low = dt.Rows(i).Item(lowColumnIndex)
                tempPayload.High = dt.Rows(i).Item(highColumnIndex)
                tempPayload.Close = dt.Rows(i).Item(closeColumnIndex)
                tempPayload.PayloadDate = dt.Rows(i).Item(dateColumnIndex)
                tempPayload.TradingSymbol = dt.Rows(i).Item(tradingSymbolColumnIndex)
                If tempPayload.PreviousCandlePayload IsNot Nothing Then
                    If tempPayload.PayloadDate.Date = tempPayload.PreviousCandlePayload.PayloadDate.Date Then
                        tempPayload.CumulativeVolume = tempPayload.PreviousCandlePayload.CumulativeVolume + dt.Rows(i).Item(volumeColumnIndex)
                    Else
                        tempPayload.CumulativeVolume = dt.Rows(i).Item(volumeColumnIndex)
                    End If
                Else
                    tempPayload.CumulativeVolume = dt.Rows(i).Item(volumeColumnIndex)
                End If
                tempPreCandle = tempPayload
                inputpayload.Add(dt.Rows(i).Item(dateColumnIndex), tempPayload)
                i += 1
            End While
        End If

        Return inputpayload

    End Function
    Public Function GetRawPayload(ByVal tableName As DataBaseTable, ByVal instrumentName As String, ByVal startDate As Date, ByVal endDate As Date) As Dictionary(Of Date, Payload)
        Dim trade As String = Nothing
        Dim dt As DataTable = Nothing
        Dim inputpayload As Dictionary(Of Date, Payload) = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cmd As MySqlCommand = Nothing
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CASH", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.Intraday_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.Intraday_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.Intraday_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.EOD_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CASH", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.EOD_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.EOD_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
            Case DataBaseTable.EOD_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
                Dim input As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
                cm = New MySqlCommand(input, conn)
        End Select

        OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", instrumentName, endDate.ToShortDateString))

        cmd.CommandType = CommandType.StoredProcedure
        cmd.Parameters.AddWithValue("@userDate", endDate.ToString("yyyy-MM-dd"))
        cmd.Parameters.AddWithValue("@allData", 0)
        cmd.Parameters.AddWithValue("@tableName", "")
        cmd.Parameters.AddWithValue("@instrumentName", instrumentName)
        cmd.Parameters.Add("@currentTradingSymbol", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output
        cmd.ExecuteNonQuery()
        trade = cmd.Parameters(4).Value.ToString()

        If trade IsNot Nothing Then
            cm.Parameters.AddWithValue("@trd", trade)
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                inputpayload = ConvertDataTableToPayload(dt, 0, 1, 2, 3, 4, 5, 6)
            End If
        End If
        Return inputpayload
    End Function
    Public Function GetCurrentTradingSymbol(ByVal tableName As DataBaseTable, ByVal instrumentName As String, ByVal userDate As Date) As String
        Dim ret As String = Nothing
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cmd As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CASH", conn)
            Case DataBaseTable.Intraday_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
            Case DataBaseTable.Intraday_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
            Case DataBaseTable.Intraday_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
            Case DataBaseTable.EOD_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CASH", conn)
            Case DataBaseTable.EOD_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
            Case DataBaseTable.EOD_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
            Case DataBaseTable.EOD_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
        End Select

        OnHeartbeat(String.Format("Fetching Trading Symbol for {0} on {1}", instrumentName, userDate.ToShortDateString))

        cmd.CommandType = CommandType.StoredProcedure
        cmd.Parameters.AddWithValue("@userDate", userDate.ToString("yyyy-MM-dd"))
        cmd.Parameters.AddWithValue("@allData", 0)
        cmd.Parameters.AddWithValue("@tableName", "")
        cmd.Parameters.AddWithValue("@instrumentName", instrumentName)
        cmd.Parameters.Add("@currentTradingSymbol", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output
        cmd.ExecuteNonQuery()
        ret = cmd.Parameters(4).Value.ToString()

        Return ret
    End Function
    Public Function GetRawPayloadForSpecificTradingSymbol(ByVal tableName As DataBaseTable, ByVal tradingSymbol As String, ByVal startDate As Date, ByVal endDate As Date) As Dictionary(Of Date, Payload)
        Dim trade As String = Nothing
        Dim dt As DataTable = Nothing
        Dim inputpayload As Dictionary(Of Date, Payload) = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", tradingSymbol, endDate.ToShortDateString))

        trade = tradingSymbol

        If trade IsNot Nothing Then
            cm.Parameters.AddWithValue("@trd", trade)
            cm.Parameters.AddWithValue("@ed", endDate.ToString("yyyy-MM-dd"))
            cm.Parameters.AddWithValue("@sd", startDate.ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                inputpayload = ConvertDataTableToPayload(dt, 0, 1, 2, 3, 4, 5, 6)
            End If
        End If
        Return inputpayload
    End Function
    Public Function GetPreviousTradingDay(ByVal tableName As DataBaseTable, ByVal instrumentName As String, ByVal currentDate As Date) As Date
        Dim trade As String = Nothing
        Dim dt As DataTable = Nothing
        Dim previousTradingDate As Date = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cmd As MySqlCommand = Nothing
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CASH", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CASH", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", instrumentName, currentDate.ToShortDateString))

        cmd.CommandType = CommandType.StoredProcedure
        cmd.Parameters.AddWithValue("@userDate", currentDate.ToString("yyyy-MM-dd"))
        cmd.Parameters.AddWithValue("@allData", 0)
        cmd.Parameters.AddWithValue("@tableName", "")
        cmd.Parameters.AddWithValue("@instrumentName", instrumentName)
        cmd.Parameters.Add("@currentTradingSymbol", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output
        cmd.ExecuteNonQuery()
        trade = cmd.Parameters(4).Value.ToString()

        If trade IsNot Nothing Then
            cm.Parameters.AddWithValue("@trd", trade)
            cm.Parameters.AddWithValue("@ed", currentDate.ToString("yyyy-MM-dd"))
            cm.Parameters.AddWithValue("@sd", currentDate.Date.AddDays(-15).ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                previousTradingDate = dt.Rows(0).Item(0)
            End If
        End If
        Return previousTradingDate
    End Function
    Public Function GetPreviousTradingDayOfAnInstrument(ByVal tableName As DataBaseTable, ByVal tradingSymbol As String, ByVal currentDate As Date) As Date
        Dim dt As DataTable = Nothing
        Dim previousTradingDate As Date = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", tradingSymbol, currentDate.ToShortDateString))
        cm.Parameters.AddWithValue("@trd", tradingSymbol)
        cm.Parameters.AddWithValue("@ed", currentDate.ToString("yyyy-MM-dd"))
        cm.Parameters.AddWithValue("@sd", currentDate.Date.AddDays(-15).ToString("yyyy-MM-dd"))
        Dim adapter As New MySqlDataAdapter(cm)
        adapter.SelectCommand.CommandTimeout = 300
        dt = New DataTable()
        adapter.Fill(dt)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            previousTradingDate = dt.Rows(0).Item(0)
        End If
        Return previousTradingDate
    End Function
    Public Function GetPreviousTradingDay(ByVal tableName As DataBaseTable, ByVal currentDate As Date) As Date
        Dim dt As DataTable = Nothing
        Dim previousTradingDate As Date = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_cash` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_currency` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_commodity` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_futures` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_cash` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_currency` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_commodity` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_futures` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Fetching Previous Trading Date from DataBase for {0}", currentDate.ToShortDateString))

        cm.Parameters.AddWithValue("@ed", currentDate.Date.ToString("yyyy-MM-dd"))
        cm.Parameters.AddWithValue("@sd", currentDate.Date.AddDays(-15).ToString("yyyy-MM-dd"))
        Dim adapter As New MySqlDataAdapter(cm)
        adapter.SelectCommand.CommandTimeout = 300
        dt = New DataTable()
        adapter.Fill(dt)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            previousTradingDate = dt.Rows(0).Item(0)
        End If

        Return previousTradingDate
    End Function
    Public Function GetAppropiateLotSize(ByVal tableName As DataBaseTable, ByVal instrumentName As String, ByVal currentDate As Date) As Integer
        Dim trade As String = Nothing
        Dim dt As DataTable = Nothing
        Dim lotSize As Integer = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cmd As MySqlCommand = Nothing
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash, DataBaseTable.EOD_Cash
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`<=@ed AND `AS_ON_DATE`>=@sd ORDER BY `AS_ON_DATE` DESC LIMIT 1", conn)
            Case DataBaseTable.Intraday_Currency, DataBaseTable.EOD_Currency
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_CURRENCY", conn)
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`<=@ed AND `AS_ON_DATE`>=@sd ORDER BY `AS_ON_DATE` DESC LIMIT 1", conn)
            Case DataBaseTable.Intraday_Commodity, DataBaseTable.EOD_Commodity
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_COMMODITY", conn)
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`<=@ed AND `AS_ON_DATE`>=@sd ORDER BY `AS_ON_DATE` DESC LIMIT 1", conn)
            Case DataBaseTable.Intraday_Futures, DataBaseTable.EOD_Futures
                cmd = New MySqlCommand("CURRENT_TRADINGSYMBOL_FUTURE", conn)
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`<=@ed AND `AS_ON_DATE`>=@sd ORDER BY `AS_ON_DATE` DESC LIMIT 1", conn)
        End Select

        OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", instrumentName, currentDate.ToShortDateString))

        cmd.CommandType = CommandType.StoredProcedure
        cmd.Parameters.AddWithValue("@userDate", currentDate.Date.ToString("yyyy-MM-dd"))
        cmd.Parameters.AddWithValue("@allData", 0)
        cmd.Parameters.AddWithValue("@tableName", "")
        cmd.Parameters.AddWithValue("@instrumentName", instrumentName)
        cmd.Parameters.Add("@currentTradingSymbol", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output
        cmd.ExecuteNonQuery()
        trade = cmd.Parameters(4).Value.ToString()

        If trade IsNot Nothing Then
            cm.Parameters.AddWithValue("@trd", trade)
            cm.Parameters.AddWithValue("@ed", currentDate.Date.ToString("yyyy-MM-dd"))
            cm.Parameters.AddWithValue("@sd", currentDate.Date.AddDays(-30).ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                lotSize = dt.Rows(0).Item(0)
            End If
        End If
        Return lotSize
    End Function
    Public Function GetCurrentTradingSymbolWithInstrumentToken(ByVal tableName As DataBaseTable, ByVal index As String, ByVal tradingDate As Date, ByVal rawInstrumentName As String) As Tuple(Of String, String)
        Dim ret As Tuple(Of String, String) = Nothing
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing
        Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing

        If index Is Nothing OrElse index.ToUpper = "NONE" OrElse IsInstrumentRelatedToCurrentIndex(index, tradingDate, rawInstrumentName) Then
            Select Case tableName
                Case DataBaseTable.Intraday_Cash, DataBaseTable.EOD_Cash
                    cm = New MySqlCommand("SELECT DISTINCT(`INSTRUMENT_TOKEN`),`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL` = @trd AND `AS_ON_DATE`=@sd", conn)
                    cm.Parameters.AddWithValue("@trd", String.Format("{0}", rawInstrumentName))
                Case DataBaseTable.Intraday_Currency, DataBaseTable.EOD_Currency
                    cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL` LIKE @trd AND `AS_ON_DATE`=@sd", conn)
                    cm.Parameters.AddWithValue("@trd", String.Format("{0}%", rawInstrumentName))
                Case DataBaseTable.Intraday_Commodity, DataBaseTable.EOD_Commodity
                    cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL` LIKE @trd AND `AS_ON_DATE`=@sd", conn)
                    cm.Parameters.AddWithValue("@trd", String.Format("{0}%", rawInstrumentName))
                Case DataBaseTable.Intraday_Futures, DataBaseTable.EOD_Futures
                    cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` LIKE @trd AND `AS_ON_DATE`=@sd", conn)
                    cm.Parameters.AddWithValue("@trd", String.Format("{0}%", rawInstrumentName))
            End Select

            OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", rawInstrumentName, tradingDate.ToShortDateString))

            cm.Parameters.AddWithValue("@sd", tradingDate.Date.ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For i = 0 To dt.Rows.Count - 1
                    Dim instrumentData As New ActiveInstrumentData With
                        {.Token = dt.Rows(i).Item(0),
                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}
                    If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                    activeInstruments.Add(instrumentData)
                Next
            End If
            If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                Dim minExipry As Date = activeInstruments.Min(Function(x)
                                                                  If x.Expiry.Date <= tradingDate.Date Then
                                                                      Return Date.MaxValue
                                                                  Else
                                                                      Return x.Expiry
                                                                  End If
                                                              End Function)
                Dim currentInstrument As ActiveInstrumentData = activeInstruments.Find(Function(x)
                                                                                           Return x.Expiry = minExipry
                                                                                       End Function)
                If currentInstrument IsNot Nothing Then
                    ret = New Tuple(Of String, String)(currentInstrument.Token, currentInstrument.TradingSymbol)
                End If
            End If
        End If
        Return ret
    End Function

    Public Function IsInstrumentRelatedToCurrentIndex(ByVal currentIndex As String, ByVal tradingDate As Date, ByVal rawInstrumentName As String) As Boolean
        Dim ret As Boolean = False
        If currentIndex IsNot Nothing Then
            Dim dt As DataTable = Nothing
            Dim conn As MySqlConnection = OpenDBConnection()
            Dim cm As MySqlCommand = Nothing

            cm = New MySqlCommand(String.Format("SELECT COUNT(1) FROM `active_instruments_cash` WHERE `TRADING_SYMBOL` = @trd AND `AS_ON_DATE`=@sd and `{0}`=1", currentIndex), conn)
            cm.Parameters.AddWithValue("@trd", String.Format("{0}", rawInstrumentName))

            OnHeartbeat(String.Format("Fetching required data from DataBase for {0} on {1}", rawInstrumentName, tradingDate.ToShortDateString))

            cm.Parameters.AddWithValue("@sd", tradingDate.Date.ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                If Not IsDBNull(dt.Rows(0).Item(0)) Then
                    If dt.Rows(0).Item(0) = 1 Then ret = True
                End If
            End If
        End If
        Return ret
    End Function

    Public Async Function GetHistoricalData(ByVal tableName As DataBaseTable, ByVal rawInstrumentName As String, ByVal currentDate As Date) As Task(Of Dictionary(Of Date, Payload))
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        Dim instrumentToken As String = Nothing
        Dim tradingSymbol As String = Nothing
        Dim ZerodhaHistoricalURL As String = "https://kitecharts-aws.zerodha.com/api/chart/{0}/minute?api_key=kitefront&access_token=K&from={1}&to={2}"
        Dim instrument As Tuple(Of String, String) = GetCurrentTradingSymbolWithInstrumentToken(tableName, Nothing, currentDate, rawInstrumentName)
        If instrument IsNot Nothing Then
            instrumentToken = instrument.Item1
            tradingSymbol = instrument.Item2
        End If
        If instrumentToken IsNot Nothing AndAlso instrumentToken <> "" Then
            Dim historicalDataURL As String = String.Format(ZerodhaHistoricalURL, instrumentToken, currentDate.AddDays(-7).ToString("yyyy-MM-dd"), currentDate.ToString("yyyy-MM-dd"))
            OnHeartbeat(String.Format("Fetching historical Data: {0}", historicalDataURL))
            Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Nothing
            Using sr As New StreamReader(HttpWebRequest.Create(historicalDataURL).GetResponseAsync().Result.GetResponseStream)
                Dim jsonString = Await sr.ReadToEndAsync.ConfigureAwait(False)
                historicalCandlesJSONDict = StringManipulation.JsonDeserialize(jsonString)
            End Using
            If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 AndAlso
                historicalCandlesJSONDict.ContainsKey("data") Then
                Dim historicalCandlesDict As Dictionary(Of String, Object) = historicalCandlesJSONDict("data")
                If historicalCandlesDict.ContainsKey("candles") AndAlso historicalCandlesDict("candles").count > 0 Then
                    Dim historicalCandles As ArrayList = historicalCandlesDict("candles")
                    If ret Is Nothing Then ret = New Dictionary(Of Date, Payload)
                    OnHeartbeat(String.Format("Generating Payload for {0}", tradingSymbol))
                    Dim previousPayload As Payload = Nothing
                    For Each historicalCandle In historicalCandles
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
        End If
        Return ret
    End Function

    Public Function CalculateStandardDeviation(ByVal inputPayload As Dictionary(Of Date, Decimal)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim sum As Double = 0
            For Each runningPayload In inputPayload.Keys
                sum = sum + inputPayload(runningPayload)
            Next
            Dim mean As Double = sum / inputPayload.Count
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance = sumVariance + Math.Pow((inputPayload(runningPayload) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (inputPayload.Count - 1)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return Math.Round(ret, 4)
    End Function
    Public Function CalculateStandardDeviation(ParamArray numbers() As Double) As Double
        Dim ret As Double = Nothing
        If numbers.Count > 0 Then
            Dim sum As Double = 0
            For i = 0 To numbers.Count - 1
                sum = sum + numbers(i)
            Next
            Dim mean As Double = sum / numbers.Count
            Dim sumVariance As Double = 0
            For j = 0 To numbers.Count - 1
                sumVariance = sumVariance + Math.Pow((numbers(j) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (numbers.Count - 1)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return Math.Round(ret, 4)
    End Function
    Public Function CalculateStandardDeviationPA(ByVal inputPayload As Dictionary(Of Date, Decimal)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim sum As Double = 0
            For Each runningPayload In inputPayload.Keys
                sum = sum + inputPayload(runningPayload)
            Next
            Dim mean As Double = sum / inputPayload.Count
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance = sumVariance + Math.Pow((inputPayload(runningPayload) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (inputPayload.Count)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return Math.Round(ret, 4)
    End Function
    Public Function GetLotSize(ByVal tableName As DataBaseTable, ByVal tradingSymbol As String, ByVal currentDate As Date) As Integer
        Dim ret As Integer = 0
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Getting Lot Size from DataBase for {0} on {1}", tradingSymbol, currentDate.ToShortDateString))

        If tradingSymbol IsNot Nothing Then
            cm.Parameters.AddWithValue("@trd", tradingSymbol)
            cm.Parameters.AddWithValue("@sd", currentDate.ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                ret = dt.Rows(0).Item(0)
            End If
        End If
        Return ret
    End Function

    Public Function IsTradingDay(ByVal tableName As DataBaseTable, ByVal currentDate As Date) As Boolean
        Dim ret As Boolean = False
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash, DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT COUNT(1) FROM `eod_prices_cash` WHERE `SnapshotDate`=@sd", conn)
            Case DataBaseTable.Intraday_Currency, DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT COUNT(1) FROM `eod_prices_currency` WHERE `SnapshotDate`=@sd", conn)
            Case DataBaseTable.Intraday_Commodity, DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT COUNT(1) FROM `eod_prices_commodity` WHERE `SnapshotDate`=@sd", conn)
            Case DataBaseTable.Intraday_Futures, DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT COUNT(1) FROM `eod_prices_futures` WHERE `SnapshotDate`=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Checking trading day from DataBase for {0}", currentDate.ToShortDateString))

        cm.Parameters.AddWithValue("@sd", currentDate.ToString("yyyy-MM-dd"))
        Dim adapter As New MySqlDataAdapter(cm)
        adapter.SelectCommand.CommandTimeout = 300
        dt = New DataTable()
        adapter.Fill(dt)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            If Val(dt.Rows(0).Item(0)) > 0 Then
                ret = True
            End If
        End If
        Return ret
    End Function

    Public Function PassedVolumeFilter(ByVal tradingSymbol As String, ByVal tradingDate As Date, ByVal maximumBenchmark As Decimal) As Boolean
        Dim ret As Boolean = False
        Dim instrumentName As String = Nothing
        If tradingSymbol IsNot Nothing AndAlso tradingSymbol.Contains("FUT") Then
            instrumentName = tradingSymbol.Remove(tradingSymbol.Count - 8)
        Else
            Throw New ApplicationException(String.Format("{0} is a future instrument", tradingSymbol))
        End If
        Dim inputPayload As Dictionary(Of Date, Payload) = GetRawPayload(DataBaseTable.Intraday_Cash, instrumentName, tradingDate.AddDays(-8), tradingDate)
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim tradingDatePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Where(Function(x)
                                                                                                              Return x.Value.PayloadDate.Date = tradingDate.Date
                                                                                                          End Function)
            If tradingDatePayload IsNot Nothing AndAlso tradingDatePayload.Count > 0 Then
                Dim thirdPreviousTradingDate As Date = Date.MinValue
                Dim thirdPreviousTradingDatePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = Nothing
                Dim thirdPreviousTradingDateLotSize As Integer = 0
                Dim secondPreviousTradingDate As Date = Date.MinValue
                Dim secondPreviousTradingDatePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = Nothing
                Dim secondPreviousTradingDateLotSize As Integer = 0
                Dim firstPreviousTradingDate As Date = Date.MinValue
                Dim firstPreviousTradingDatePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = Nothing
                Dim firstPreviousTradingDateLotSize As Integer = 0
                If tradingDatePayload.FirstOrDefault.Value.PreviousCandlePayload IsNot Nothing Then
                    thirdPreviousTradingDate = tradingDatePayload.FirstOrDefault.Value.PreviousCandlePayload.PayloadDate.Date
                    thirdPreviousTradingDatePayload = inputPayload.Where(Function(x)
                                                                             Return x.Value.PayloadDate.Date = thirdPreviousTradingDate.Date
                                                                         End Function)
                    thirdPreviousTradingDateLotSize = GetLotSize(DataBaseTable.Intraday_Futures, tradingSymbol, thirdPreviousTradingDate)
                End If
                If thirdPreviousTradingDatePayload.FirstOrDefault.Value.PreviousCandlePayload IsNot Nothing Then
                    secondPreviousTradingDate = thirdPreviousTradingDatePayload.FirstOrDefault.Value.PreviousCandlePayload.PayloadDate.Date
                    secondPreviousTradingDatePayload = inputPayload.Where(Function(x)
                                                                              Return x.Value.PayloadDate.Date = secondPreviousTradingDate.Date
                                                                          End Function)
                    secondPreviousTradingDateLotSize = GetLotSize(DataBaseTable.Intraday_Futures, tradingSymbol, secondPreviousTradingDate)
                End If
                If secondPreviousTradingDatePayload.FirstOrDefault.Value.PreviousCandlePayload IsNot Nothing Then
                    firstPreviousTradingDate = secondPreviousTradingDatePayload.FirstOrDefault.Value.PreviousCandlePayload.PayloadDate.Date
                    firstPreviousTradingDatePayload = inputPayload.Where(Function(x)
                                                                             Return x.Value.PayloadDate.Date = firstPreviousTradingDate.Date
                                                                         End Function)
                    firstPreviousTradingDateLotSize = GetLotSize(DataBaseTable.Intraday_Futures, tradingSymbol, firstPreviousTradingDate)
                End If
                If thirdPreviousTradingDate = Date.MinValue OrElse secondPreviousTradingDate = Date.MinValue OrElse firstPreviousTradingDate = Date.MinValue Then
                    Throw New ApplicationException("Sufficient data isnot available")
                End If
                Dim blankCandlePayloadInThirdPreviousDay As IEnumerable(Of KeyValuePair(Of Date, Payload)) = Nothing
                Dim blankCandlePayloadInSecondPreviousDay As IEnumerable(Of KeyValuePair(Of Date, Payload)) = Nothing
                Dim blankCandlePayloadInFirstPreviousDay As IEnumerable(Of KeyValuePair(Of Date, Payload)) = Nothing

                blankCandlePayloadInThirdPreviousDay = thirdPreviousTradingDatePayload.Where(Function(x)
                                                                                                 Return x.Value.Volume < thirdPreviousTradingDateLotSize
                                                                                             End Function)
                blankCandlePayloadInSecondPreviousDay = secondPreviousTradingDatePayload.Where(Function(x)
                                                                                                   Return x.Value.Volume < secondPreviousTradingDateLotSize
                                                                                               End Function)
                blankCandlePayloadInFirstPreviousDay = firstPreviousTradingDatePayload.Where(Function(x)
                                                                                                 Return x.Value.Volume < firstPreviousTradingDateLotSize
                                                                                             End Function)
                Dim totalNumberOfCandle As Integer = thirdPreviousTradingDatePayload.Count + secondPreviousTradingDatePayload.Count + firstPreviousTradingDatePayload.Count
                Dim totalNumberOfBlankCandle As Integer = 0
                If blankCandlePayloadInThirdPreviousDay IsNot Nothing AndAlso blankCandlePayloadInThirdPreviousDay.Count > 0 Then
                    totalNumberOfBlankCandle += blankCandlePayloadInThirdPreviousDay.Count
                End If
                If blankCandlePayloadInSecondPreviousDay IsNot Nothing AndAlso blankCandlePayloadInSecondPreviousDay.Count > 0 Then
                    totalNumberOfBlankCandle += blankCandlePayloadInSecondPreviousDay.Count
                End If
                If blankCandlePayloadInFirstPreviousDay IsNot Nothing AndAlso blankCandlePayloadInFirstPreviousDay.Count > 0 Then
                    totalNumberOfBlankCandle += blankCandlePayloadInFirstPreviousDay.Count
                End If
                If (totalNumberOfBlankCandle / totalNumberOfCandle) * 100 <= maximumBenchmark Then
                    ret = True
                End If
            End If
        End If
        Return ret
    End Function
    Public Function CalculateCamarillaPivotPoints(ByVal high As Decimal, ByVal low As Decimal, ByVal close As Decimal) As CamarillaPivotPoints
        Dim ret As CamarillaPivotPoints = Nothing
        ret = New CamarillaPivotPoints With
        {
            .H4 = close + ((high - low) * 0.55),
            .H3 = close + ((high - low) * 0.275),
            .H2 = close + ((high - low) * 0.183),
            .H1 = close + ((high - low) * 0.0916),
            .L4 = close - ((high - low) * 0.55),
            .L3 = close - ((high - low) * 0.275),
            .L2 = close - ((high - low) * 0.183),
            .L1 = close - ((high - low) * 0.0916)
        }
        Return ret
    End Function
    Public Class CamarillaPivotPoints
        Public Property H1 As Decimal
        Public Property H2 As Decimal
        Public Property H3 As Decimal
        Public Property H4 As Decimal
        Public Property L1 As Decimal
        Public Property L2 As Decimal
        Public Property L3 As Decimal
        Public Property L4 As Decimal
    End Class

    Private Class ActiveInstrumentData
        Public Token As String
        Public TradingSymbol As String
        Public Expiry As Date
    End Class

#End Region

#Region "DB Connection"
    Public Function OpenDBConnection() As MySqlConnection
        If conn Is Nothing OrElse conn.State <> ConnectionState.Open Then
            OnHeartbeat("Connecting Database")
            Try
                conn = New MySqlConnection(My.Settings.dbConnectionLocal)
                conn.Open()
            Catch ex1 As MySqlException
                Try
                    conn = New MySqlConnection(My.Settings.dbConnectionLocalNetwork)
                    conn.Open()
                Catch ex3 As Exception
                    conn = New MySqlConnection(My.Settings.dbConnectionRemote)
                    conn.Open()
                End Try
            End Try
        End If
        Return conn
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
