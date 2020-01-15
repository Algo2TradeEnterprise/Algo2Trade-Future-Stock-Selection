<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CommonSettingsUC
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.cmbStockType = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtMaxBlankCandlePercentage = New System.Windows.Forms.TextBox()
        Me.lblMaxBlankCandlePercentage = New System.Windows.Forms.Label()
        Me.dtpckrToDate = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrFromDate = New System.Windows.Forms.DateTimePicker()
        Me.lblToDate = New System.Windows.Forms.Label()
        Me.lblFromDate = New System.Windows.Forms.Label()
        Me.txtATRPercentage = New System.Windows.Forms.TextBox()
        Me.txtNumberOfStock = New System.Windows.Forms.TextBox()
        Me.lblATR = New System.Windows.Forms.Label()
        Me.lblNumberOfStock = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'cmbStockType
        '
        Me.cmbStockType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbStockType.FormattingEnabled = True
        Me.cmbStockType.Items.AddRange(New Object() {"Cash", "Commodity", "Currency", "Futures"})
        Me.cmbStockType.Location = New System.Drawing.Point(475, 10)
        Me.cmbStockType.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbStockType.Name = "cmbStockType"
        Me.cmbStockType.Size = New System.Drawing.Size(120, 24)
        Me.cmbStockType.TabIndex = 68
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(388, 13)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(83, 17)
        Me.Label2.TabIndex = 67
        Me.Label2.Text = "Stock Type:"
        '
        'txtMaxBlankCandlePercentage
        '
        Me.txtMaxBlankCandlePercentage.Location = New System.Drawing.Point(758, 10)
        Me.txtMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxBlankCandlePercentage.Name = "txtMaxBlankCandlePercentage"
        Me.txtMaxBlankCandlePercentage.Size = New System.Drawing.Size(114, 22)
        Me.txtMaxBlankCandlePercentage.TabIndex = 63
        '
        'lblMaxBlankCandlePercentage
        '
        Me.lblMaxBlankCandlePercentage.AutoSize = True
        Me.lblMaxBlankCandlePercentage.Location = New System.Drawing.Point(617, 14)
        Me.lblMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxBlankCandlePercentage.Name = "lblMaxBlankCandlePercentage"
        Me.lblMaxBlankCandlePercentage.Size = New System.Drawing.Size(140, 17)
        Me.lblMaxBlankCandlePercentage.TabIndex = 62
        Me.lblMaxBlankCandlePercentage.Text = "Max Blank Candle %:"
        '
        'dtpckrToDate
        '
        Me.dtpckrToDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrToDate.Location = New System.Drawing.Point(265, 9)
        Me.dtpckrToDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrToDate.Name = "dtpckrToDate"
        Me.dtpckrToDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrToDate.TabIndex = 61
        '
        'dtpckrFromDate
        '
        Me.dtpckrFromDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrFromDate.Location = New System.Drawing.Point(81, 9)
        Me.dtpckrFromDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrFromDate.Name = "dtpckrFromDate"
        Me.dtpckrFromDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrFromDate.TabIndex = 60
        '
        'lblToDate
        '
        Me.lblToDate.AutoSize = True
        Me.lblToDate.Location = New System.Drawing.Point(199, 13)
        Me.lblToDate.Name = "lblToDate"
        Me.lblToDate.Size = New System.Drawing.Size(63, 17)
        Me.lblToDate.TabIndex = 59
        Me.lblToDate.Text = "To Date:"
        '
        'lblFromDate
        '
        Me.lblFromDate.AutoSize = True
        Me.lblFromDate.Location = New System.Drawing.Point(3, 13)
        Me.lblFromDate.Name = "lblFromDate"
        Me.lblFromDate.Size = New System.Drawing.Size(78, 17)
        Me.lblFromDate.TabIndex = 58
        Me.lblFromDate.Text = "From Date:"
        '
        'txtATRPercentage
        '
        Me.txtATRPercentage.Location = New System.Drawing.Point(475, 46)
        Me.txtATRPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtATRPercentage.Name = "txtATRPercentage"
        Me.txtATRPercentage.Size = New System.Drawing.Size(113, 22)
        Me.txtATRPercentage.TabIndex = 73
        Me.txtATRPercentage.Tag = "ATR %"
        '
        'txtNumberOfStock
        '
        Me.txtNumberOfStock.Location = New System.Drawing.Point(776, 41)
        Me.txtNumberOfStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStock.Name = "txtNumberOfStock"
        Me.txtNumberOfStock.Size = New System.Drawing.Size(96, 22)
        Me.txtNumberOfStock.TabIndex = 78
        '
        'lblATR
        '
        Me.lblATR.AutoSize = True
        Me.lblATR.Location = New System.Drawing.Point(411, 50)
        Me.lblATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATR.Name = "lblATR"
        Me.lblATR.Size = New System.Drawing.Size(56, 17)
        Me.lblATR.TabIndex = 74
        Me.lblATR.Text = "ATR %:"
        '
        'lblNumberOfStock
        '
        Me.lblNumberOfStock.AutoSize = True
        Me.lblNumberOfStock.Location = New System.Drawing.Point(598, 45)
        Me.lblNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStock.Name = "lblNumberOfStock"
        Me.lblNumberOfStock.Size = New System.Drawing.Size(175, 17)
        Me.lblNumberOfStock.TabIndex = 77
        Me.lblNumberOfStock.Text = "Number Of Stock Per Day:"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(283, 45)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMaxPrice.TabIndex = 71
        Me.txtMaxPrice.Tag = "Max Price"
        '
        'lblMaxPrice
        '
        Me.lblMaxPrice.AutoSize = True
        Me.lblMaxPrice.Location = New System.Drawing.Point(206, 48)
        Me.lblMaxPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxPrice.Name = "lblMaxPrice"
        Me.lblMaxPrice.Size = New System.Drawing.Size(73, 17)
        Me.lblMaxPrice.TabIndex = 72
        Me.lblMaxPrice.Text = "Max Price:"
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(81, 45)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMinPrice.TabIndex = 69
        Me.txtMinPrice.Tag = "Min Price"
        '
        'lblMinPrice
        '
        Me.lblMinPrice.AutoSize = True
        Me.lblMinPrice.Location = New System.Drawing.Point(3, 48)
        Me.lblMinPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinPrice.Name = "lblMinPrice"
        Me.lblMinPrice.Size = New System.Drawing.Size(70, 17)
        Me.lblMinPrice.TabIndex = 70
        Me.lblMinPrice.Text = "Min Price:"
        '
        'CommonSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.txtATRPercentage)
        Me.Controls.Add(Me.txtNumberOfStock)
        Me.Controls.Add(Me.lblATR)
        Me.Controls.Add(Me.lblNumberOfStock)
        Me.Controls.Add(Me.txtMaxPrice)
        Me.Controls.Add(Me.lblMaxPrice)
        Me.Controls.Add(Me.txtMinPrice)
        Me.Controls.Add(Me.lblMinPrice)
        Me.Controls.Add(Me.cmbStockType)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtMaxBlankCandlePercentage)
        Me.Controls.Add(Me.lblMaxBlankCandlePercentage)
        Me.Controls.Add(Me.dtpckrToDate)
        Me.Controls.Add(Me.dtpckrFromDate)
        Me.Controls.Add(Me.lblToDate)
        Me.Controls.Add(Me.lblFromDate)
        Me.Name = "CommonSettings"
        Me.Size = New System.Drawing.Size(883, 79)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents cmbStockType As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtMaxBlankCandlePercentage As TextBox
    Friend WithEvents lblMaxBlankCandlePercentage As Label
    Friend WithEvents dtpckrToDate As DateTimePicker
    Friend WithEvents dtpckrFromDate As DateTimePicker
    Friend WithEvents lblToDate As Label
    Friend WithEvents lblFromDate As Label
    Friend WithEvents txtATRPercentage As TextBox
    Friend WithEvents txtNumberOfStock As TextBox
    Friend WithEvents lblATR As Label
    Friend WithEvents lblNumberOfStock As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents lblMinPrice As Label
End Class
