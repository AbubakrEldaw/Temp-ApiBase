
namespace APIBase.Models.Enums
{
    public enum TransactionStatus
    {
        //[ArabicName("قيد الانتظار")]
        Pending,

        //[ArabicName("مدفوع")]
        Paid,

        //[ArabicName("مسترد")]
        Refund,

        //[ArabicName("تجديد فاشل")]
        FailedRenewal
    }
}
