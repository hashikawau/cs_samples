using System;

namespace AccoutBookServer.Dao
{
    class TransactionDto
    {
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ItemName { get; set; }
        public int ItemPrice { get; set; }
        public string ItemType { get; set; }
        public string ShopName { get; set; }
        public string Memo { get; set; }
        public string UserName { get; set; }

        public override string ToString()
        {
            return $"TransactionDto{{TransactionId={TransactionId}, TransactionDate={TransactionDate}, ItemName={ItemName}, ItemPrice={ItemPrice}}}";
        }
    }
}