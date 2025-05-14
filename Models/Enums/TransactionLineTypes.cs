namespace APIBase.Models.Enums
{
    public enum TransactionLineTypes
    {
        Plan,
        License,
        Feature,

        /// <summary>
        /// Should be MarketPlaceApp, but the column length allows nvarchar(10)
        /// </summary>
        App,
    }
}
