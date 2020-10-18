namespace Sakaba.Domain
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class ColumnConfigAttribute : System.Attribute
    {
        [AddColumnType("int(1)", "NULL")]
        public int preferExcel;
        
        [AddColumnType("int(11)", "NOT NULL DEFAULT 100")]
        public int columnWidth = 100;
        
        [AddColumnType("varchar(50)", "NOT NULL")]
        public string sortLabel = "ラベル";
    }
}