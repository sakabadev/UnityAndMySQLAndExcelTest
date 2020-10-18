namespace MD2DBFromExcel {
    partial class Ribbon1 : Microsoft.Office.Tools.Ribbon.RibbonBase {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon1()
            : base(Globals.Factory.GetRibbonFactory()) {
            InitializeComponent();
        }

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.Export2DBButton = this.Factory.CreateRibbonButton();
            this.ExportSelectedSheetButton = this.Factory.CreateRibbonButton();
            this.ImportSelectedSheetButton = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Label = "Sakaba";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Items.Add(this.Export2DBButton);
            this.group1.Items.Add(this.ExportSelectedSheetButton);
            this.group1.Items.Add(this.ImportSelectedSheetButton);
            this.group1.Label = "同期";
            this.group1.Name = "group1";
            // 
            // Export2DBButton
            // 
            this.Export2DBButton.Label = "Bookを保存";
            this.Export2DBButton.Name = "Export2DBButton";
            this.Export2DBButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Export2DBButton_Click);
            // 
            // ExportSelectedSheetButton
            // 
            this.ExportSelectedSheetButton.Label = "選択中のシートを保存";
            this.ExportSelectedSheetButton.Name = "ExportSelectedSheetButton";
            this.ExportSelectedSheetButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ExportSelectedSheetButton_Click);
            // 
            // ImportSelectedSheetButton
            // 
            this.ImportSelectedSheetButton.Label = "選択中のシートを読み込む";
            this.ImportSelectedSheetButton.Name = "ImportSelectedSheetButton";
            this.ImportSelectedSheetButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ImportSelectedSheetButton_Click);
            // 
            // Ribbon1
            // 
            this.Name = "Ribbon1";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton Export2DBButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ImportSelectedSheetButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ExportSelectedSheetButton;
    }

    partial class ThisRibbonCollection {
        internal Ribbon1 Ribbon1 {
            get { return this.GetRibbon<Ribbon1>(); }
        }
    }
}
