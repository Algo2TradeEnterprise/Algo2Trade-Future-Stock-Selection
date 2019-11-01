Imports System.Threading
Imports System.IO
Imports Algo2TradeBLL
Imports Utilities

Public Class frmMain


#Region "Common Delegates"
    Delegate Sub SetObjectEnableDisable_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectEnableDisable_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectEnableDisable_Delegate(AddressOf SetObjectEnableDisable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Enabled = [value]
        End If
    End Sub

    Delegate Sub SetObjectVisible_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectVisible_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectVisible_Delegate(AddressOf SetObjectVisible_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Visible = [value]
        End If
    End Sub

    Delegate Sub SetLabelText_Delegate(ByVal [label] As Label, ByVal [text] As String)
    Public Sub SetLabelText_ThreadSafe(ByVal [label] As Label, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelText_Delegate(AddressOf SetLabelText_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetLabelText_Delegate(ByVal [label] As Label) As String
    Public Function GetLabelText_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelText_Delegate(AddressOf GetLabelText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Sub SetLabelTag_Delegate(ByVal [label] As Label, ByVal [tag] As String)
    Public Sub SetLabelTag_ThreadSafe(ByVal [label] As Label, ByVal [tag] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelTag_Delegate(AddressOf SetLabelTag_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [tag]})
        Else
            [label].Tag = [tag]
        End If
    End Sub

    Delegate Function GetLabelTag_Delegate(ByVal [label] As Label) As String
    Public Function GetLabelTag_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelTag_Delegate(AddressOf GetLabelTag_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Tag
        End If
    End Function
    Delegate Sub SetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
    Public Sub SetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New SetToolStripLabel_Delegate(AddressOf SetToolStripLabel_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[toolStrip], [label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
    Public Function GetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New GetToolStripLabel_Delegate(AddressOf GetToolStripLabel_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[toolStrip], [label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Function GetDateTimePickerValue_Delegate(ByVal [dateTimePicker] As DateTimePicker) As Date
    Public Function GetDateTimePickerValue_ThreadSafe(ByVal [dateTimePicker] As DateTimePicker) As Date
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [dateTimePicker].InvokeRequired Then
            Dim MyDelegate As New GetDateTimePickerValue_Delegate(AddressOf GetDateTimePickerValue_ThreadSafe)
            Return Me.Invoke(MyDelegate, New DateTimePicker() {[dateTimePicker]})
        Else
            Return [dateTimePicker].Value
        End If
    End Function

    Delegate Function GetNumericUpDownValue_Delegate(ByVal [numericUpDown] As NumericUpDown) As Integer
    Public Function GetNumericUpDownValue_ThreadSafe(ByVal [numericUpDown] As NumericUpDown) As Integer
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [numericUpDown].InvokeRequired Then
            Dim MyDelegate As New GetNumericUpDownValue_Delegate(AddressOf GetNumericUpDownValue_ThreadSafe)
            Return Me.Invoke(MyDelegate, New NumericUpDown() {[numericUpDown]})
        Else
            Return [numericUpDown].Value
        End If
    End Function

    Delegate Function GetComboBoxIndex_Delegate(ByVal [combobox] As ComboBox) As Integer
    Public Function GetComboBoxIndex_ThreadSafe(ByVal [combobox] As ComboBox) As Integer
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [combobox].InvokeRequired Then
            Dim MyDelegate As New GetComboBoxIndex_Delegate(AddressOf GetComboBoxIndex_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[combobox]})
        Else
            Return [combobox].SelectedIndex
        End If
    End Function

    Delegate Function GetComboBoxItem_Delegate(ByVal [ComboBox] As ComboBox) As String
    Public Function GetComboBoxItem_ThreadSafe(ByVal [ComboBox] As ComboBox) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [ComboBox].InvokeRequired Then
            Dim MyDelegate As New GetComboBoxItem_Delegate(AddressOf GetComboBoxItem_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[ComboBox]})
        Else
            Return [ComboBox].SelectedItem.ToString
        End If
    End Function

    Delegate Function GetTextBoxText_Delegate(ByVal [textBox] As TextBox) As String
    Public Function GetTextBoxText_ThreadSafe(ByVal [textBox] As TextBox) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [textBox].InvokeRequired Then
            Dim MyDelegate As New GetTextBoxText_Delegate(AddressOf GetTextBoxText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[textBox]})
        Else
            Return [textBox].Text
        End If
    End Function

    Delegate Function GetCheckBoxChecked_Delegate(ByVal [checkBox] As CheckBox) As Boolean
    Public Function GetCheckBoxChecked_ThreadSafe(ByVal [checkBox] As CheckBox) As Boolean
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [checkBox].InvokeRequired Then
            Dim MyDelegate As New GetCheckBoxChecked_Delegate(AddressOf GetCheckBoxChecked_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[checkBox]})
        Else
            Return [checkBox].Checked
        End If
    End Function

    Delegate Function GetRadioButtonChecked_Delegate(ByVal [radioButton] As RadioButton) As Boolean
    Public Function GetRadioButtonChecked_ThreadSafe(ByVal [radioButton] As RadioButton) As Boolean
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [radioButton].InvokeRequired Then
            Dim MyDelegate As New GetRadioButtonChecked_Delegate(AddressOf GetRadioButtonChecked_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[radioButton]})
        Else
            Return [radioButton].Checked
        End If
    End Function

    Delegate Sub SetDatagridBindDatatable_Delegate(ByVal [datagrid] As DataGridView, ByVal [table] As DataTable)
    Public Sub SetDatagridBindDatatable_ThreadSafe(ByVal [datagrid] As DataGridView, ByVal [table] As DataTable)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [datagrid].InvokeRequired Then
            Dim MyDelegate As New SetDatagridBindDatatable_Delegate(AddressOf SetDatagridBindDatatable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[datagrid], [table]})
        Else
            [datagrid].DataSource = [table]
            [datagrid].Refresh()
        End If
    End Sub
#End Region

#Region "Event Handlers"
    Private _canceller As CancellationTokenSource
    Private Sub OnHeartbeat(message As String)
        SetLabelText_ThreadSafe(lblProgress, message)
    End Sub
    Private Sub OnHeartbeatMain(message As String)
        SetLabelText_ThreadSafe(lblProgress, message)
    End Sub
    Private Sub OnDocumentDownloadComplete()
        'OnHeartbeat("Document download compelete")
    End Sub
    Private Sub OnDocumentRetryStatus(currentTry As Integer, totalTries As Integer)
        OnHeartbeat(String.Format("Try #{0}/{1}: Connecting...", currentTry, totalTries))
    End Sub
    Public Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        OnHeartbeat(String.Format("{0}, waiting {1}/{2} secs", msg, elapsedSecs, totalSecs))
    End Sub
#End Region

    Private canceller As CancellationTokenSource
    Private _bannedStockFileName As String
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetObjectVisible_ThreadSafe(pnlInstrumentList, False)
        SetObjectEnableDisable_ThreadSafe(btnStop, False)
        SetObjectEnableDisable_ThreadSafe(btnExport, False)
        If My.Settings.StartDate <> Date.MinValue Then dtpckrFromDate.Value = My.Settings.StartDate
        If My.Settings.EndDate <> Date.MinValue Then dtpckrToDate.Value = My.Settings.EndDate
        cmbProcedure.SelectedIndex = My.Settings.ComboBoxIndex
        txtMaxBlankCandlePercentage.Text = My.Settings.MaxBlankCandlePercentage
        txtInstrumentList.Text = My.Settings.InstrumentList
        txtNumberOfStock.Text = My.Settings.NumberOfStockPerDay
        txtHigherLimitOfMaxBlankCandlePercentage.Text = My.Settings.HigherLimitOfMaxBlankCandlePercentage
        txtProcedureNumberOfStock.Text = My.Settings.ProcedureNumberOfRecords
        txtMinPrice.Text = My.Settings.MinClose
        txtMaxPrice.Text = My.Settings.MaxClose
        txtMinVolume.Text = My.Settings.PotentialAmount
        txtATRPercentage.Text = My.Settings.ATRPercentage
    End Sub

    Private Async Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        SetDatagridBindDatatable_ThreadSafe(dgrvMain, Nothing)
        dgrvMain.Refresh()
        SetObjectEnableDisable_ThreadSafe(btnStart, False)
        SetObjectEnableDisable_ThreadSafe(btnExport, False)
        SetObjectEnableDisable_ThreadSafe(btnStop, True)
        My.Settings.StartDate = dtpckrFromDate.Value
        My.Settings.EndDate = dtpckrToDate.Value
        My.Settings.ComboBoxIndex = cmbProcedure.SelectedIndex
        My.Settings.MaxBlankCandlePercentage = txtMaxBlankCandlePercentage.Text
        My.Settings.InstrumentList = txtInstrumentList.Text
        My.Settings.HigherLimitOfMaxBlankCandlePercentage = txtHigherLimitOfMaxBlankCandlePercentage.Text
        My.Settings.NumberOfStockPerDay = txtNumberOfStock.Text
        My.Settings.ProcedureNumberOfRecords = txtProcedureNumberOfStock.Text
        My.Settings.MinClose = txtMinPrice.Text
        My.Settings.MaxClose = txtMaxPrice.Text
        My.Settings.PotentialAmount = txtMinVolume.Text
        My.Settings.ATRPercentage = txtATRPercentage.Text
        My.Settings.Save()
        Dim dateFromFile As Boolean = GetCheckBoxChecked_ThreadSafe(chkbDatePicker)
        If dateFromFile Then
            Await Task.Run(AddressOf StartProcessingFromList).ConfigureAwait(False)
        Else
            Await Task.Run(AddressOf StartProcessing).ConfigureAwait(False)
        End If
    End Sub

    Private Async Function StartProcessing() As Task
        Dim mainDataTable As DataTable = Nothing
        Try
            canceller = New CancellationTokenSource
            mainDataTable = New DataTable
            mainDataTable.Columns.Add("Date")
            mainDataTable.Columns.Add("Trading Symbol")
            mainDataTable.Columns.Add("Lot Size")
            mainDataTable.Columns.Add("ATR %")
            mainDataTable.Columns.Add("Blank Candle %")
            mainDataTable.Columns.Add("Day ATR")
            mainDataTable.Columns.Add("Previous Day Open")
            mainDataTable.Columns.Add("Previous Day Low")
            mainDataTable.Columns.Add("Previous Day High")
            mainDataTable.Columns.Add("Previous Day Close")
            mainDataTable.Columns.Add("Supporting1")
            mainDataTable.Columns.Add("Supporting2")
            mainDataTable.Columns.Add("Supporting3")

            Dim startDate As Date = GetDateTimePickerValue_ThreadSafe(dtpckrFromDate)
            Dim endDate As Date = GetDateTimePickerValue_ThreadSafe(dtpckrToDate)
            Dim procedureToRun As Integer = GetComboBoxIndex_ThreadSafe(cmbProcedure)
            Dim maxBlankCandlePercentage As Decimal = GetTextBoxText_ThreadSafe(txtMaxBlankCandlePercentage)
            Dim higherLimitOfMaxBlankCandlePercentage As Decimal = GetTextBoxText_ThreadSafe(txtHigherLimitOfMaxBlankCandlePercentage)
            Dim numberOfStockPerDay As Decimal = GetTextBoxText_ThreadSafe(txtNumberOfStock)
            Dim imdtPrvsDay As Boolean = GetCheckBoxChecked_ThreadSafe(chbImmediatePreviousDay)
            If numberOfStockPerDay = 0 Then numberOfStockPerDay = 5000

            Dim instrumentNames As String = Nothing
            Dim instrumentList As Dictionary(Of String, Decimal()) = Nothing
            If procedureToRun = 2 Then
                instrumentNames = GetTextBoxText_ThreadSafe(txtInstrumentList)
                Dim instruments() As String = instrumentNames.Trim.Split(vbCrLf)
                For Each runningInstrument In instruments
                    Dim instrument As String = runningInstrument.Trim
                    If instrumentList Is Nothing Then instrumentList = New Dictionary(Of String, Decimal())
                    instrumentList.Add(instrument.Trim.ToUpper, {0, 0})
                Next
                If instrumentList Is Nothing OrElse instrumentList.Count = 0 Then
                    Throw New ApplicationException("No instrument available in user given list")
                End If
            End If

            Dim tradingDate As Date = startDate
            Using stockSelection As New StockListFromDatabase(canceller)
                AddHandler stockSelection.Heartbeat, AddressOf OnHeartbeat

                Select Case procedureToRun
                    Case 6
                        stockSelection.highVolumeInsideBarHLUserInputs = New StockListFromDatabase.HighVolumeInsideBarHLSettings With {
                            .CheckVolumeTillSignalTime = GetRadioButtonChecked_ThreadSafe(rdbSignalTime),
                            .CheckEODVolume = GetRadioButtonChecked_ThreadSafe(rdbEOD),
                            .Previous5DaysAvgVolumePercentage = GetTextBoxText_ThreadSafe(txtPreviousDaysVolumePercentage)
                        }
                End Select

                While tradingDate <= endDate
                    _bannedStockFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bannned Stocks {0}.csv", tradingDate.ToString("ddMMyyyy")))
                    For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Bannned Stocks *.csv")
                        If Not runningFile.Contains(tradingDate.ToString("ddMMyyyy")) Then File.Delete(runningFile)
                    Next
                    Dim bannedStockList As List(Of String) = Nothing
                    Using bannedStock As New BannedStockDataFetcher(_bannedStockFileName, canceller)
                        AddHandler bannedStock.Heartbeat, AddressOf OnHeartbeat
                        bannedStockList = Await bannedStock.GetBannedStocksData(tradingDate).ConfigureAwait(False)
                    End Using
                    Dim stockList As Dictionary(Of String, InstrumentDetails) = Await stockSelection.GetStockData(tradingDate, procedureToRun, bannedStockList, instrumentList).ConfigureAwait(False)
                    If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                        Dim activeInstrumentData As Dictionary(Of String, InstrumentDetails) = Await stockSelection.GetActiveInstrumentData(tradingDate, stockList).ConfigureAwait(False)
                        If activeInstrumentData IsNot Nothing AndAlso activeInstrumentData.Count > 0 Then
                            Dim filteredInstruments As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) = activeInstrumentData.Where(Function(x)
                                                                                                                                                   Return x.Value.IsTradable = True
                                                                                                                                               End Function)
                            If filteredInstruments IsNot Nothing AndAlso filteredInstruments.Count > 0 Then
                                For Each stockData In filteredInstruments
                                    canceller.Token.ThrowIfCancellationRequested()
                                    Dim stockPayload As Dictionary(Of Date, Payload) = Await stockSelection.GetStockPayload(tradingDate, stockData.Value, imdtPrvsDay).ConfigureAwait(False)
                                    If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                                        stockData.Value.BlankCandlePercentage = CalculateBlankVolumePercentage(stockPayload)
                                    Else
                                        If Not imdtPrvsDay Then Throw New ApplicationException(String.Format("Check volume checking for {0} on {1}", stockData.Key, tradingDate))
                                        stockData.Value.IsTradable = False
                                        stockData.Value.BlankCandlePercentage = Decimal.MinValue
                                    End If
                                Next
                                Dim stocksLessThanMaxBlankCandlePercentage As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) =
                                    filteredInstruments.Where(Function(x)
                                                                  Return x.Value.BlankCandlePercentage <> Decimal.MinValue AndAlso
                                                                  x.Value.BlankCandlePercentage <= maxBlankCandlePercentage AndAlso
                                                                  x.Value.IsTradable = True
                                                              End Function)
                                If stocksLessThanMaxBlankCandlePercentage IsNot Nothing AndAlso stocksLessThanMaxBlankCandlePercentage.Count > 0 Then
                                    Dim stockCounter As Integer = 0
                                    For Each stockData In stocksLessThanMaxBlankCandlePercentage
                                        Dim row As DataRow = mainDataTable.NewRow
                                        row("Date") = tradingDate.ToShortDateString
                                        row("Trading Symbol") = stockData.Value.TradingSymbol
                                        row("Lot Size") = stockData.Value.LotSize
                                        row("ATR %") = stockData.Value.ATRPercentage
                                        row("Blank Candle %") = stockData.Value.BlankCandlePercentage
                                        row("Day ATR") = stockData.Value.DayATR
                                        row("Previous Day Open") = stockData.Value.PreviousDayOpen
                                        row("Previous Day Low") = stockData.Value.PreviousDayLow
                                        row("Previous Day High") = stockData.Value.PreviousDayHigh
                                        row("Previous Day Close") = stockData.Value.PreviousDayClose
                                        row("Supporting1") = stockData.Value.Supporting1
                                        row("Supporting2") = stockData.Value.Supporting2
                                        row("Supporting3") = stockData.Value.Supporting3
                                        mainDataTable.Rows.Add(row)
                                        stockCounter += 1
                                        If stockCounter = numberOfStockPerDay Then Exit For
                                    Next
                                    If stockCounter < numberOfStockPerDay Then
                                        Dim stocksLessThanHigherLimitOfMaxBlankCandlePercentage As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) =
                                            filteredInstruments.Where(Function(x)
                                                                          Return x.Value.BlankCandlePercentage > maxBlankCandlePercentage AndAlso
                                                                          x.Value.BlankCandlePercentage <= higherLimitOfMaxBlankCandlePercentage AndAlso
                                                                          x.Value.IsTradable = True
                                                                      End Function)
                                        If stocksLessThanHigherLimitOfMaxBlankCandlePercentage IsNot Nothing AndAlso stocksLessThanHigherLimitOfMaxBlankCandlePercentage.Count > 0 Then
                                            For Each stockData In stocksLessThanHigherLimitOfMaxBlankCandlePercentage.OrderBy(Function(y)
                                                                                                                                  Return y.Value.BlankCandlePercentage
                                                                                                                              End Function)
                                                Dim row As DataRow = mainDataTable.NewRow
                                                row("Date") = tradingDate.ToShortDateString
                                                row("Trading Symbol") = stockData.Value.TradingSymbol
                                                row("Lot Size") = stockData.Value.LotSize
                                                row("ATR %") = stockData.Value.ATRPercentage
                                                row("Blank Candle %") = stockData.Value.BlankCandlePercentage
                                                row("Supporting1") = stockData.Value.Supporting1
                                                row("Supporting2") = stockData.Value.Supporting2
                                                row("Supporting3") = stockData.Value.Supporting3
                                                mainDataTable.Rows.Add(row)
                                                stockCounter += 1
                                                If stockCounter = numberOfStockPerDay Then Exit For
                                            Next
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    tradingDate = tradingDate.AddDays(1)
                End While
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Future Stock List Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            SetDatagridBindDatatable_ThreadSafe(dgrvMain, mainDataTable)
            SetObjectEnableDisable_ThreadSafe(btnExport, True)
            SetObjectEnableDisable_ThreadSafe(btnStart, True)
            SetObjectEnableDisable_ThreadSafe(btnStop, False)
            OnHeartbeat(String.Format("Process Complete. Number of records: {0}", dgrvMain.Rows.Count))
        End Try
    End Function
    Private Async Function StartProcessingFromList() As Task
        Dim mainDataTable As DataTable = Nothing
        Try
            canceller = New CancellationTokenSource
            mainDataTable = New DataTable
            mainDataTable.Columns.Add("Date")
            mainDataTable.Columns.Add("Trading Symbol")
            mainDataTable.Columns.Add("Lot Size")
            mainDataTable.Columns.Add("ATR %")
            mainDataTable.Columns.Add("Blank Candle %")
            mainDataTable.Columns.Add("Supporting1")
            mainDataTable.Columns.Add("Supporting2")

            Dim dateListFilePath As String = Path.Combine(My.Application.Info.DirectoryPath, "Date List.txt")
            Dim allDates As String() = File.ReadAllLines(dateListFilePath)

            Dim procedureToRun As Integer = GetComboBoxIndex_ThreadSafe(cmbProcedure)
            Dim maxBlankCandlePercentage As Decimal = GetTextBoxText_ThreadSafe(txtMaxBlankCandlePercentage)
            Dim higherLimitOfMaxBlankCandlePercentage As Decimal = GetTextBoxText_ThreadSafe(txtHigherLimitOfMaxBlankCandlePercentage)
            Dim numberOfStockPerDay As Decimal = GetTextBoxText_ThreadSafe(txtNumberOfStock)
            Dim imdtPrvsDay As Boolean = GetCheckBoxChecked_ThreadSafe(chbImmediatePreviousDay)
            If numberOfStockPerDay = 0 Then numberOfStockPerDay = 5000

            Dim instrumentNames As String = Nothing
            Dim instrumentList As Dictionary(Of String, Decimal()) = Nothing
            If procedureToRun = 2 Then
                instrumentNames = GetTextBoxText_ThreadSafe(txtInstrumentList)
                Dim instruments() As String = instrumentNames.Trim.Split(vbCrLf)
                For Each runningInstrument In instruments
                    Dim instrument As String = runningInstrument.Trim
                    If instrumentList Is Nothing Then instrumentList = New Dictionary(Of String, Decimal())
                    instrumentList.Add(instrument.Trim.ToUpper, {0, 0})
                Next
                If instrumentList Is Nothing OrElse instrumentList.Count = 0 Then
                    Throw New ApplicationException("No instrument available in user given list")
                End If
            End If

            If allDates IsNot Nothing AndAlso allDates.Count > 0 Then
                Using stockSelection As New StockListFromDatabase(canceller)
                    AddHandler stockSelection.Heartbeat, AddressOf OnHeartbeat
                    For Each runningDate In allDates
                        Dim tradingDate As Date = Date.Parse(runningDate.Trim.Split(vbTab)(0))
                        _bannedStockFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bannned Stocks {0}.csv", tradingDate.ToString("ddMMyyyy")))
                        For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Bannned Stocks *.csv")
                            If Not runningFile.Contains(tradingDate.ToString("ddMMyyyy")) Then File.Delete(runningFile)
                        Next
                        Dim bannedStockList As List(Of String) = Nothing
                        Using bannedStock As New BannedStockDataFetcher(_bannedStockFileName, canceller)
                            AddHandler bannedStock.Heartbeat, AddressOf OnHeartbeat
                            bannedStockList = Await bannedStock.GetBannedStocksData(tradingDate).ConfigureAwait(False)
                        End Using
                        Dim stockList As Dictionary(Of String, InstrumentDetails) = Await stockSelection.GetStockData(tradingDate, procedureToRun, bannedStockList, instrumentList).ConfigureAwait(False)
                        If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                            Dim activeInstrumentData As Dictionary(Of String, InstrumentDetails) = Await stockSelection.GetActiveInstrumentData(tradingDate, stockList).ConfigureAwait(False)
                            If activeInstrumentData IsNot Nothing AndAlso activeInstrumentData.Count > 0 Then
                                Dim filteredInstruments As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) = activeInstrumentData.Where(Function(x)
                                                                                                                                                       Return x.Value.IsTradable = True
                                                                                                                                                   End Function)
                                If filteredInstruments IsNot Nothing AndAlso filteredInstruments.Count > 0 Then
                                    For Each stockData In filteredInstruments
                                        canceller.Token.ThrowIfCancellationRequested()
                                        Dim stockPayload As Dictionary(Of Date, Payload) = Await stockSelection.GetStockPayload(tradingDate, stockData.Value, imdtPrvsDay).ConfigureAwait(False)
                                        If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                                            stockData.Value.BlankCandlePercentage = CalculateBlankVolumePercentage(stockPayload)
                                        Else
                                            If Not imdtPrvsDay Then Throw New ApplicationException(String.Format("Check volume checking for {0} on {1}", stockData.Key, tradingDate))
                                            stockData.Value.IsTradable = False
                                            stockData.Value.BlankCandlePercentage = Decimal.MinValue
                                        End If
                                    Next
                                    Dim stocksLessThanMaxBlankCandlePercentage As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) =
                                    filteredInstruments.Where(Function(x)
                                                                  Return x.Value.BlankCandlePercentage <> Decimal.MinValue AndAlso
                                                                  x.Value.BlankCandlePercentage <= maxBlankCandlePercentage AndAlso
                                                                  x.Value.IsTradable = True
                                                              End Function)
                                    If stocksLessThanMaxBlankCandlePercentage IsNot Nothing AndAlso stocksLessThanMaxBlankCandlePercentage.Count > 0 Then
                                        Dim stockCounter As Integer = 0
                                        For Each stockData In stocksLessThanMaxBlankCandlePercentage
                                            Dim row As DataRow = mainDataTable.NewRow
                                            row("Date") = tradingDate.ToShortDateString
                                            row("Trading Symbol") = stockData.Value.TradingSymbol
                                            row("Lot Size") = stockData.Value.LotSize
                                            row("ATR %") = stockData.Value.ATRPercentage
                                            row("Blank Candle %") = stockData.Value.BlankCandlePercentage
                                            row("Supporting1") = stockData.Value.Supporting1
                                            row("Supporting2") = stockData.Value.Supporting2
                                            mainDataTable.Rows.Add(row)
                                            stockCounter += 1
                                            If stockCounter = numberOfStockPerDay Then Exit For
                                        Next
                                        If stockCounter < numberOfStockPerDay Then
                                            Dim stocksLessThanHigherLimitOfMaxBlankCandlePercentage As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) =
                                            filteredInstruments.Where(Function(x)
                                                                          Return x.Value.BlankCandlePercentage > maxBlankCandlePercentage AndAlso
                                                                          x.Value.BlankCandlePercentage <= higherLimitOfMaxBlankCandlePercentage AndAlso
                                                                          x.Value.IsTradable = True
                                                                      End Function)
                                            If stocksLessThanHigherLimitOfMaxBlankCandlePercentage IsNot Nothing AndAlso stocksLessThanHigherLimitOfMaxBlankCandlePercentage.Count > 0 Then
                                                For Each stockData In stocksLessThanHigherLimitOfMaxBlankCandlePercentage.OrderBy(Function(y)
                                                                                                                                      Return y.Value.BlankCandlePercentage
                                                                                                                                  End Function)
                                                    Dim row As DataRow = mainDataTable.NewRow
                                                    row("Date") = tradingDate.ToShortDateString
                                                    row("Trading Symbol") = stockData.Value.TradingSymbol
                                                    row("Lot Size") = stockData.Value.LotSize
                                                    row("ATR %") = stockData.Value.ATRPercentage
                                                    row("Blank Candle %") = stockData.Value.BlankCandlePercentage
                                                    row("Supporting1") = stockData.Value.Supporting1
                                                    row("Supporting2") = stockData.Value.Supporting2
                                                    mainDataTable.Rows.Add(row)
                                                    stockCounter += 1
                                                    If stockCounter = numberOfStockPerDay Then Exit For
                                                Next
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                End Using
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Future Stock List Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            SetDatagridBindDatatable_ThreadSafe(dgrvMain, mainDataTable)
            SetObjectEnableDisable_ThreadSafe(btnExport, True)
            SetObjectEnableDisable_ThreadSafe(btnStart, True)
            SetObjectEnableDisable_ThreadSafe(btnStop, False)
            OnHeartbeat(String.Format("Process Complete. Number of records: {0}", dgrvMain.Rows.Count))
        End Try
    End Function
    Private Function CalculateBlankVolumePercentage(ByVal inputPayload As Dictionary(Of Date, Payload)) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim blankCandlePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Where(Function(x)
                                                                                                              Return x.Value.Open = x.Value.Low AndAlso
                                                                                                              x.Value.Low = x.Value.High AndAlso
                                                                                                              x.Value.High = x.Value.Close
                                                                                                          End Function)
            If blankCandlePayload IsNot Nothing AndAlso blankCandlePayload.Count > 0 Then
                ret = Math.Round((blankCandlePayload.Count / inputPayload.Count) * 100, 2)
            Else
                ret = 0
            End If
        End If
        Return ret
    End Function

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        canceller.Cancel()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If dgrvMain IsNot Nothing AndAlso dgrvMain.Rows.Count > 0 Then
            saveFile.AddExtension = True
            saveFile.FileName = String.Format("Future Stock List with volume filter {0} to {1}.csv", GetDateTimePickerValue_ThreadSafe(dtpckrFromDate).ToString("dd_MM_yy"), GetDateTimePickerValue_ThreadSafe(dtpckrToDate).ToString("dd_MM_yy"))
            saveFile.Filter = "CSV (*.csv)|*.csv"
            saveFile.ShowDialog()
        Else
            MessageBox.Show("Empty DataGrid. Nothing to export.", "Future Stock CSV File", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub saveFile_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles saveFile.FileOk
        Using export As New DAL.CSVHelper(saveFile.FileName, ",", canceller)
            export.GetCSVFromDataGrid(dgrvMain)
        End Using
        If MessageBox.Show("Do you want to open file?", "Future Stock List CSV File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Process.Start(saveFile.FileName)
        End If
    End Sub

    Private Sub cmbProcedure_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbProcedure.SelectedIndexChanged
        Dim index As Integer = GetComboBoxIndex_ThreadSafe(cmbProcedure)
        If index = 0 Then
            SetObjectVisible_ThreadSafe(pnlHighVolumeInsidebatHLSettings, False)
            SetObjectVisible_ThreadSafe(pnlInstrumentList, True)
            Dim btnLocation As Point = New Point(512, 192)
            pnlBtn.Location = btnLocation
        ElseIf index = 6 Then
            rdbSignalTime.Checked = True
            txtPreviousDaysVolumePercentage.Text = 100
            SetObjectVisible_ThreadSafe(pnlHighVolumeInsidebatHLSettings, True)
            SetObjectVisible_ThreadSafe(pnlInstrumentList, False)
            Dim btnLocation As Point = New Point(512, 128)
            pnlBtn.Location = btnLocation
        Else
            SetObjectVisible_ThreadSafe(pnlHighVolumeInsidebatHLSettings, False)
            SetObjectVisible_ThreadSafe(pnlInstrumentList, False)
            Dim btnLocation As Point = New Point(512, 55)
            pnlBtn.Location = btnLocation
        End If
    End Sub
End Class
