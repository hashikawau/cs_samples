using CSSamples.Common.Logger;
using System.Collections.Generic;
using System.Data.SQLite;

namespace AccoutBookServer.Dao
{
    class TransactionDao
    {
        static Logger _logger = Logger.GetLogger(typeof(TransactionDao));

        SQLiteConnectionStringBuilder sqlConnectionSb;

        public TransactionDao()
        {
            sqlConnectionSb = new SQLiteConnectionStringBuilder
            {
                DataSource = "C:/Users/hashikawa/sqlite/account_book.db"
            };
        }

        public List<TransactionDto> SelectDaylyDetailPrices()
        {
            using (var conn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = $@"
select
    account.transaction_id as 'id'
    , account.transaction_date as 'date_yyyymmdd'
    , account.item_name as 'name' 
    , account.item_price as 'price' 
    , account.item_type as 'type' 
    , account.shop_name as 'shop' 
    , account.memo as 'memo' 
    , account.user_name as 'user' 
  from
    Account as account 
  order by
    account.transaction_date
";

                    var result = new List<TransactionDto>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new TransactionDto
                            {
                                TransactionId = reader.GetInt32(0),
                                TransactionDate = reader.GetDateTime(1),
                                ItemName = reader.GetString(2),
                                ItemPrice = reader.GetInt32(3),
                                ItemType = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                ShopName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Memo = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                UserName = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                    return result;
                }
            }
        }

        public TransactionDto SelectDaylyDetailPrice(string transactionId)
        {
            using (var conn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = $@"
select
    account.transaction_id as 'id'
    , account.transaction_date as 'date_yyyymmdd'
    , account.item_name as 'name' 
    , account.item_price as 'price' 
    , account.item_type as 'type' 
    , account.shop_name as 'shop' 
    , account.memo as 'memo' 
    , account.user_name as 'user' 
  from
    Account as account 
  where
    account.transaction_id = {transactionId}
";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TransactionDto
                            {
                                TransactionId = reader.GetInt32(0),
                                TransactionDate = reader.GetDateTime(1),
                                ItemName = reader.GetString(2),
                                ItemPrice = reader.GetInt32(3),
                                ItemType = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                ShopName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Memo = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                UserName = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public void InsertTransactionDto(TransactionDto dto)
        {
            using (var conn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = $@"
insert 
  into Account( 
    transaction_date
    , item_name
    , item_price
    , item_type
    , shop_name
    , memo
    , user_name
  ) 
  values (
    '{dto.TransactionDate.ToString("yyyy-MM-dd")}'
    , '{dto.ItemName}'
    , {dto.ItemPrice}
    , '{dto.ItemType}'
    , '{dto.ShopName}'
    , '{dto.Memo}'
    , '{dto.UserName}')
";

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTransactionDto(TransactionDto dto)
        {
            using (var conn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = $@"
update Account 
  set
    transaction_date = '{dto.TransactionDate.ToString("yyyy-MM-dd")}'
    , item_name = '{dto.ItemName}'
    , item_price = {dto.ItemPrice}
    , item_type = '{dto.ItemType}'
    , shop_name = '{dto.ShopName}'
    , memo = '{dto.Memo}'
    , user_name = '{dto.UserName}' 
  where
    transaction_id = {dto.TransactionId}
";

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTransactionDto(int transactionId)
        {
            using (var conn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = $@"
delete 
  from
    Account 
  where
    transaction_id = {transactionId}
";

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}