Imports System.Threading
Imports Algo2TradeBLL

Public Class HighATRStocks
    Inherits StockSelection

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal tradingDate As Date,
                   ByVal bannedStocks As List(Of String))
        MyBase.New(canceller, cmn, stockType, tradingDate, bannedStocks)
    End Sub

    Public Overrides Async Function GetStockDataAsync() As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Lot Size")
        ret.Columns.Add("ATR %")
        ret.Columns.Add("Blank Candle %")
        ret.Columns.Add("Day ATR")
        ret.Columns.Add("Previous Day Open")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Slab")

        Using atrStock As New ATRStockSelection(_canceller)
            AddHandler atrStock.Heartbeat, AddressOf OnHeartbeat

            Dim atrStockList As Dictionary(Of String, InstrumentDetails) = Await atrStock.GetATRStockData(_eodTable, _tradingDate, _bannedStocksList, False).ConfigureAwait(False)
            If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 Then
                Dim stockCounter As Integer = 0
                For Each runningStock In atrStockList
                    Dim row As DataRow = ret.NewRow
                    row("Date") = _tradingDate.ToString("dd-MM-yyyy")
                    row("Trading Symbol") = runningStock.Value.TradingSymbol
                    row("Lot Size") = runningStock.Value.LotSize
                    row("ATR %") = Math.Round(runningStock.Value.ATRPercentage, 4)
                    row("Blank Candle %") = runningStock.Value.BlankCandlePercentage
                    row("Day ATR") = Math.Round(runningStock.Value.DayATR, 4)
                    row("Previous Day Open") = runningStock.Value.PreviousDayOpen
                    row("Previous Day Low") = runningStock.Value.PreviousDayLow
                    row("Previous Day High") = runningStock.Value.PreviousDayHigh
                    row("Previous Day Close") = runningStock.Value.PreviousDayClose
                    row("Slab") = runningStock.Value.Slab
                    ret.Rows.Add(row)
                    stockCounter += 1
                    If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                Next
            End If
        End Using
        Return ret
    End Function
End Class
