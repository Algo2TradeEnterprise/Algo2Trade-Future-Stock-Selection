<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.dtpckrToDate = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrFromDate = New System.Windows.Forms.DateTimePicker()
        Me.lblToDate = New System.Windows.Forms.Label()
        Me.lblFromDate = New System.Windows.Forms.Label()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.dgrvMain = New System.Windows.Forms.DataGridView()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.saveFile = New System.Windows.Forms.SaveFileDialog()
        Me.lblProcedure = New System.Windows.Forms.Label()
        Me.cmbProcedure = New System.Windows.Forms.ComboBox()
        Me.lblMaxBlankCandlePercentage = New System.Windows.Forms.Label()
        Me.txtMaxBlankCandlePercentage = New System.Windows.Forms.TextBox()
        Me.pnlInstrumentList = New System.Windows.Forms.Panel()
        Me.txtInstrumentList = New System.Windows.Forms.TextBox()
        Me.lblInstrumentList = New System.Windows.Forms.Label()
        Me.txtNumberOfStock = New System.Windows.Forms.TextBox()
        Me.lblNumberOfStock = New System.Windows.Forms.Label()
        Me.pnlBtn = New System.Windows.Forms.Panel()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.txtProcedureNumberOfStock = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtMinVolume = New System.Windows.Forms.TextBox()
        Me.lblMinVolume = New System.Windows.Forms.Label()
        Me.txtATRPercentage = New System.Windows.Forms.TextBox()
        Me.lblATR = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.chbImmediatePreviousDay = New System.Windows.Forms.CheckBox()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.pnlIntradayVolumeSpikeSettings = New System.Windows.Forms.Panel()
        Me.dtpkrVolumeSpikeChkTime = New System.Windows.Forms.DateTimePicker()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmbStockType = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.pnlTopGainerLooserSettings = New System.Windows.Forms.Panel()
        Me.dtpkrTopGainerLosserChkTime = New System.Windows.Forms.DateTimePicker()
        Me.Label4 = New System.Windows.Forms.Label()
        CType(Me.dgrvMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlInstrumentList.SuspendLayout()
        Me.pnlBtn.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.pnlIntradayVolumeSpikeSettings.SuspendLayout()
        Me.pnlTopGainerLooserSettings.SuspendLayout()
        Me.SuspendLayout()
        '
        'dtpckrToDate
        '
        Me.dtpckrToDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrToDate.Location = New System.Drawing.Point(269, 12)
        Me.dtpckrToDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrToDate.Name = "dtpckrToDate"
        Me.dtpckrToDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrToDate.TabIndex = 30
        '
        'dtpckrFromDate
        '
        Me.dtpckrFromDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrFromDate.Location = New System.Drawing.Point(85, 12)
        Me.dtpckrFromDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrFromDate.Name = "dtpckrFromDate"
        Me.dtpckrFromDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrFromDate.TabIndex = 29
        '
        'lblToDate
        '
        Me.lblToDate.AutoSize = True
        Me.lblToDate.Location = New System.Drawing.Point(203, 16)
        Me.lblToDate.Name = "lblToDate"
        Me.lblToDate.Size = New System.Drawing.Size(63, 17)
        Me.lblToDate.TabIndex = 28
        Me.lblToDate.Text = "To Date:"
        '
        'lblFromDate
        '
        Me.lblFromDate.AutoSize = True
        Me.lblFromDate.Location = New System.Drawing.Point(7, 16)
        Me.lblFromDate.Name = "lblFromDate"
        Me.lblFromDate.Size = New System.Drawing.Size(78, 17)
        Me.lblFromDate.TabIndex = 27
        Me.lblFromDate.Text = "From Date:"
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.Location = New System.Drawing.Point(5, 579)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(1203, 52)
        Me.lblProgress.TabIndex = 31
        Me.lblProgress.Text = "Progress Status"
        '
        'dgrvMain
        '
        Me.dgrvMain.AllowUserToAddRows = False
        Me.dgrvMain.AllowUserToDeleteRows = False
        Me.dgrvMain.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgrvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgrvMain.Location = New System.Drawing.Point(3, 170)
        Me.dgrvMain.Margin = New System.Windows.Forms.Padding(4)
        Me.dgrvMain.Name = "dgrvMain"
        Me.dgrvMain.ReadOnly = True
        Me.dgrvMain.RowHeadersVisible = False
        Me.dgrvMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgrvMain.Size = New System.Drawing.Size(1205, 350)
        Me.dgrvMain.TabIndex = 32
        '
        'btnStart
        '
        Me.btnStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStart.Location = New System.Drawing.Point(11, 4)
        Me.btnStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(119, 38)
        Me.btnStart.TabIndex = 33
        Me.btnStart.Text = "Start"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnExport
        '
        Me.btnExport.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnExport.Location = New System.Drawing.Point(29, 49)
        Me.btnExport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(205, 39)
        Me.btnExport.TabIndex = 34
        Me.btnExport.Text = "Export CSV"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnStop
        '
        Me.btnStop.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStop.Location = New System.Drawing.Point(132, 2)
        Me.btnStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(117, 39)
        Me.btnStop.TabIndex = 35
        Me.btnStop.Text = "Stop"
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'saveFile
        '
        '
        'lblProcedure
        '
        Me.lblProcedure.AutoSize = True
        Me.lblProcedure.Location = New System.Drawing.Point(686, 15)
        Me.lblProcedure.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProcedure.Name = "lblProcedure"
        Me.lblProcedure.Size = New System.Drawing.Size(78, 17)
        Me.lblProcedure.TabIndex = 36
        Me.lblProcedure.Text = "Procedure:"
        '
        'cmbProcedure
        '
        Me.cmbProcedure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbProcedure.FormattingEnabled = True
        Me.cmbProcedure.Items.AddRange(New Object() {"User Given", "ATR Based All Stock", "Pre Market Stock", "Intraday Volume Spike Stock", "OHL ATR Stock", "Touch Previous Day Last Candle", "Top Gainer Top Looser"})
        Me.cmbProcedure.Location = New System.Drawing.Point(765, 11)
        Me.cmbProcedure.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbProcedure.Name = "cmbProcedure"
        Me.cmbProcedure.Size = New System.Drawing.Size(433, 24)
        Me.cmbProcedure.TabIndex = 37
        '
        'lblMaxBlankCandlePercentage
        '
        Me.lblMaxBlankCandlePercentage.AutoSize = True
        Me.lblMaxBlankCandlePercentage.Location = New System.Drawing.Point(6, 50)
        Me.lblMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxBlankCandlePercentage.Name = "lblMaxBlankCandlePercentage"
        Me.lblMaxBlankCandlePercentage.Size = New System.Drawing.Size(140, 17)
        Me.lblMaxBlankCandlePercentage.TabIndex = 38
        Me.lblMaxBlankCandlePercentage.Text = "Max Blank Candle %:"
        '
        'txtMaxBlankCandlePercentage
        '
        Me.txtMaxBlankCandlePercentage.Location = New System.Drawing.Point(147, 46)
        Me.txtMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxBlankCandlePercentage.Name = "txtMaxBlankCandlePercentage"
        Me.txtMaxBlankCandlePercentage.Size = New System.Drawing.Size(57, 22)
        Me.txtMaxBlankCandlePercentage.TabIndex = 39
        '
        'pnlInstrumentList
        '
        Me.pnlInstrumentList.Controls.Add(Me.txtInstrumentList)
        Me.pnlInstrumentList.Controls.Add(Me.lblInstrumentList)
        Me.pnlInstrumentList.Location = New System.Drawing.Point(255, 188)
        Me.pnlInstrumentList.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlInstrumentList.Name = "pnlInstrumentList"
        Me.pnlInstrumentList.Size = New System.Drawing.Size(257, 113)
        Me.pnlInstrumentList.TabIndex = 42
        '
        'txtInstrumentList
        '
        Me.txtInstrumentList.Location = New System.Drawing.Point(8, 28)
        Me.txtInstrumentList.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentList.Multiline = True
        Me.txtInstrumentList.Name = "txtInstrumentList"
        Me.txtInstrumentList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtInstrumentList.Size = New System.Drawing.Size(244, 72)
        Me.txtInstrumentList.TabIndex = 41
        '
        'lblInstrumentList
        '
        Me.lblInstrumentList.AutoSize = True
        Me.lblInstrumentList.Location = New System.Drawing.Point(4, 1)
        Me.lblInstrumentList.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentList.Name = "lblInstrumentList"
        Me.lblInstrumentList.Size = New System.Drawing.Size(104, 17)
        Me.lblInstrumentList.TabIndex = 40
        Me.lblInstrumentList.Text = "Instrument List:"
        '
        'txtNumberOfStock
        '
        Me.txtNumberOfStock.Location = New System.Drawing.Point(577, 12)
        Me.txtNumberOfStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStock.Name = "txtNumberOfStock"
        Me.txtNumberOfStock.Size = New System.Drawing.Size(96, 22)
        Me.txtNumberOfStock.TabIndex = 44
        '
        'lblNumberOfStock
        '
        Me.lblNumberOfStock.AutoSize = True
        Me.lblNumberOfStock.Location = New System.Drawing.Point(399, 16)
        Me.lblNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStock.Name = "lblNumberOfStock"
        Me.lblNumberOfStock.Size = New System.Drawing.Size(175, 17)
        Me.lblNumberOfStock.TabIndex = 43
        Me.lblNumberOfStock.Text = "Number Of Stock Per Day:"
        '
        'pnlBtn
        '
        Me.pnlBtn.Controls.Add(Me.btnStart)
        Me.pnlBtn.Controls.Add(Me.btnExport)
        Me.pnlBtn.Controls.Add(Me.btnStop)
        Me.pnlBtn.Location = New System.Drawing.Point(949, 56)
        Me.pnlBtn.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlBtn.Name = "pnlBtn"
        Me.pnlBtn.Size = New System.Drawing.Size(259, 94)
        Me.pnlBtn.TabIndex = 47
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.txtProcedureNumberOfStock)
        Me.GroupBox3.Controls.Add(Me.Label1)
        Me.GroupBox3.Controls.Add(Me.txtMinVolume)
        Me.GroupBox3.Controls.Add(Me.lblMinVolume)
        Me.GroupBox3.Controls.Add(Me.txtATRPercentage)
        Me.GroupBox3.Controls.Add(Me.lblATR)
        Me.GroupBox3.Controls.Add(Me.txtMaxPrice)
        Me.GroupBox3.Controls.Add(Me.lblMaxPrice)
        Me.GroupBox3.Controls.Add(Me.txtMinPrice)
        Me.GroupBox3.Controls.Add(Me.lblMinPrice)
        Me.GroupBox3.Controls.Add(Me.chbImmediatePreviousDay)
        Me.GroupBox3.Location = New System.Drawing.Point(3, 76)
        Me.GroupBox3.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox3.Size = New System.Drawing.Size(672, 89)
        Me.GroupBox3.TabIndex = 49
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Stock Selection Settings"
        '
        'txtProcedureNumberOfStock
        '
        Me.txtProcedureNumberOfStock.Location = New System.Drawing.Point(401, 57)
        Me.txtProcedureNumberOfStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProcedureNumberOfStock.Name = "txtProcedureNumberOfStock"
        Me.txtProcedureNumberOfStock.Size = New System.Drawing.Size(69, 22)
        Me.txtProcedureNumberOfStock.TabIndex = 42
        Me.txtProcedureNumberOfStock.Tag = "Number Of Stock"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(212, 59)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(186, 17)
        Me.Label1.TabIndex = 43
        Me.Label1.Text = "Procedure Number Of Stock"
        '
        'txtMinVolume
        '
        Me.txtMinVolume.Location = New System.Drawing.Point(93, 57)
        Me.txtMinVolume.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinVolume.Name = "txtMinVolume"
        Me.txtMinVolume.Size = New System.Drawing.Size(111, 22)
        Me.txtMinVolume.TabIndex = 40
        Me.txtMinVolume.Tag = "Min Volume"
        '
        'lblMinVolume
        '
        Me.lblMinVolume.AutoSize = True
        Me.lblMinVolume.Location = New System.Drawing.Point(5, 59)
        Me.lblMinVolume.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinVolume.Name = "lblMinVolume"
        Me.lblMinVolume.Size = New System.Drawing.Size(81, 17)
        Me.lblMinVolume.TabIndex = 41
        Me.lblMinVolume.Text = "Min Volume"
        '
        'txtATRPercentage
        '
        Me.txtATRPercentage.Location = New System.Drawing.Point(477, 26)
        Me.txtATRPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtATRPercentage.Name = "txtATRPercentage"
        Me.txtATRPercentage.Size = New System.Drawing.Size(113, 22)
        Me.txtATRPercentage.TabIndex = 38
        Me.txtATRPercentage.Tag = "ATR %"
        '
        'lblATR
        '
        Me.lblATR.AutoSize = True
        Me.lblATR.Location = New System.Drawing.Point(413, 30)
        Me.lblATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATR.Name = "lblATR"
        Me.lblATR.Size = New System.Drawing.Size(52, 17)
        Me.lblATR.TabIndex = 39
        Me.lblATR.Text = "ATR %"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(285, 25)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMaxPrice.TabIndex = 36
        Me.txtMaxPrice.Tag = "Max Price"
        '
        'lblMaxPrice
        '
        Me.lblMaxPrice.AutoSize = True
        Me.lblMaxPrice.Location = New System.Drawing.Point(208, 28)
        Me.lblMaxPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxPrice.Name = "lblMaxPrice"
        Me.lblMaxPrice.Size = New System.Drawing.Size(69, 17)
        Me.lblMaxPrice.TabIndex = 37
        Me.lblMaxPrice.Text = "Max Price"
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(83, 25)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMinPrice.TabIndex = 34
        Me.txtMinPrice.Tag = "Min Price"
        '
        'lblMinPrice
        '
        Me.lblMinPrice.AutoSize = True
        Me.lblMinPrice.Location = New System.Drawing.Point(5, 28)
        Me.lblMinPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinPrice.Name = "lblMinPrice"
        Me.lblMinPrice.Size = New System.Drawing.Size(66, 17)
        Me.lblMinPrice.TabIndex = 35
        Me.lblMinPrice.Text = "Min Price"
        '
        'chbImmediatePreviousDay
        '
        Me.chbImmediatePreviousDay.AutoSize = True
        Me.chbImmediatePreviousDay.Location = New System.Drawing.Point(483, 59)
        Me.chbImmediatePreviousDay.Margin = New System.Windows.Forms.Padding(4)
        Me.chbImmediatePreviousDay.Name = "chbImmediatePreviousDay"
        Me.chbImmediatePreviousDay.Size = New System.Drawing.Size(182, 21)
        Me.chbImmediatePreviousDay.TabIndex = 44
        Me.chbImmediatePreviousDay.Text = "Immediate Previous Day"
        Me.chbImmediatePreviousDay.UseVisualStyleBackColor = True
        '
        'lblDescription
        '
        Me.lblDescription.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblDescription.Location = New System.Drawing.Point(5, 524)
        Me.lblDescription.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDescription.Name = "lblDescription"
        Me.lblDescription.Size = New System.Drawing.Size(1203, 52)
        Me.lblDescription.TabIndex = 50
        Me.lblDescription.Text = "Description"
        '
        'pnlIntradayVolumeSpikeSettings
        '
        Me.pnlIntradayVolumeSpikeSettings.Controls.Add(Me.dtpkrVolumeSpikeChkTime)
        Me.pnlIntradayVolumeSpikeSettings.Controls.Add(Me.Label3)
        Me.pnlIntradayVolumeSpikeSettings.Location = New System.Drawing.Point(519, 189)
        Me.pnlIntradayVolumeSpikeSettings.Name = "pnlIntradayVolumeSpikeSettings"
        Me.pnlIntradayVolumeSpikeSettings.Size = New System.Drawing.Size(257, 91)
        Me.pnlIntradayVolumeSpikeSettings.TabIndex = 52
        '
        'dtpkrVolumeSpikeChkTime
        '
        Me.dtpkrVolumeSpikeChkTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpkrVolumeSpikeChkTime.Location = New System.Drawing.Point(118, 11)
        Me.dtpkrVolumeSpikeChkTime.Name = "dtpkrVolumeSpikeChkTime"
        Me.dtpkrVolumeSpikeChkTime.Size = New System.Drawing.Size(110, 22)
        Me.dtpkrVolumeSpikeChkTime.TabIndex = 5
        Me.dtpkrVolumeSpikeChkTime.Value = New Date(2019, 12, 8, 0, 0, 0, 0)
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(8, 12)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(108, 17)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Check Till Time:"
        '
        'cmbStockType
        '
        Me.cmbStockType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbStockType.FormattingEnabled = True
        Me.cmbStockType.Items.AddRange(New Object() {"Cash", "Commodity", "Currency", "Futures"})
        Me.cmbStockType.Location = New System.Drawing.Point(314, 47)
        Me.cmbStockType.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbStockType.Name = "cmbStockType"
        Me.cmbStockType.Size = New System.Drawing.Size(120, 24)
        Me.cmbStockType.TabIndex = 57
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(227, 50)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(83, 17)
        Me.Label2.TabIndex = 56
        Me.Label2.Text = "Stock Type:"
        '
        'pnlTopGainerLooserSettings
        '
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.dtpkrTopGainerLosserChkTime)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.Label4)
        Me.pnlTopGainerLooserSettings.Location = New System.Drawing.Point(519, 286)
        Me.pnlTopGainerLooserSettings.Name = "pnlTopGainerLooserSettings"
        Me.pnlTopGainerLooserSettings.Size = New System.Drawing.Size(257, 91)
        Me.pnlTopGainerLooserSettings.TabIndex = 58
        '
        'dtpkrTopGainerLosserChkTime
        '
        Me.dtpkrTopGainerLosserChkTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpkrTopGainerLosserChkTime.Location = New System.Drawing.Point(118, 11)
        Me.dtpkrTopGainerLosserChkTime.Name = "dtpkrTopGainerLosserChkTime"
        Me.dtpkrTopGainerLosserChkTime.Size = New System.Drawing.Size(110, 22)
        Me.dtpkrTopGainerLosserChkTime.TabIndex = 5
        Me.dtpkrTopGainerLosserChkTime.Value = New Date(2019, 12, 8, 0, 0, 0, 0)
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(8, 12)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(108, 17)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Check Till Time:"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1213, 636)
        Me.Controls.Add(Me.pnlTopGainerLooserSettings)
        Me.Controls.Add(Me.cmbStockType)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.pnlIntradayVolumeSpikeSettings)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.lblDescription)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.pnlBtn)
        Me.Controls.Add(Me.txtNumberOfStock)
        Me.Controls.Add(Me.lblNumberOfStock)
        Me.Controls.Add(Me.pnlInstrumentList)
        Me.Controls.Add(Me.txtMaxBlankCandlePercentage)
        Me.Controls.Add(Me.lblMaxBlankCandlePercentage)
        Me.Controls.Add(Me.cmbProcedure)
        Me.Controls.Add(Me.lblProcedure)
        Me.Controls.Add(Me.dgrvMain)
        Me.Controls.Add(Me.dtpckrToDate)
        Me.Controls.Add(Me.dtpckrFromDate)
        Me.Controls.Add(Me.lblToDate)
        Me.Controls.Add(Me.lblFromDate)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Future Stock List"
        CType(Me.dgrvMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlInstrumentList.ResumeLayout(False)
        Me.pnlInstrumentList.PerformLayout()
        Me.pnlBtn.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.pnlIntradayVolumeSpikeSettings.ResumeLayout(False)
        Me.pnlIntradayVolumeSpikeSettings.PerformLayout()
        Me.pnlTopGainerLooserSettings.ResumeLayout(False)
        Me.pnlTopGainerLooserSettings.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents dtpckrToDate As DateTimePicker
    Friend WithEvents dtpckrFromDate As DateTimePicker
    Friend WithEvents lblToDate As Label
    Friend WithEvents lblFromDate As Label
    Friend WithEvents lblProgress As Label
    Friend WithEvents dgrvMain As DataGridView
    Friend WithEvents btnStart As Button
    Friend WithEvents btnExport As Button
    Friend WithEvents btnStop As Button
    Friend WithEvents saveFile As SaveFileDialog
    Friend WithEvents lblProcedure As Label
    Friend WithEvents cmbProcedure As ComboBox
    Friend WithEvents lblMaxBlankCandlePercentage As Label
    Friend WithEvents txtMaxBlankCandlePercentage As TextBox
    Friend WithEvents pnlInstrumentList As Panel
    Friend WithEvents txtInstrumentList As TextBox
    Friend WithEvents lblInstrumentList As Label
    Friend WithEvents txtNumberOfStock As TextBox
    Friend WithEvents lblNumberOfStock As Label
    Friend WithEvents pnlBtn As Panel
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents txtProcedureNumberOfStock As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents txtMinVolume As TextBox
    Friend WithEvents lblMinVolume As Label
    Friend WithEvents txtATRPercentage As TextBox
    Friend WithEvents lblATR As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents lblMinPrice As Label
    Friend WithEvents chbImmediatePreviousDay As CheckBox
    Friend WithEvents lblDescription As Label
    Friend WithEvents pnlIntradayVolumeSpikeSettings As Panel
    Friend WithEvents dtpkrVolumeSpikeChkTime As DateTimePicker
    Friend WithEvents Label3 As Label
    Friend WithEvents cmbStockType As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents pnlTopGainerLooserSettings As Panel
    Friend WithEvents dtpkrTopGainerLosserChkTime As DateTimePicker
    Friend WithEvents Label4 As Label
End Class
