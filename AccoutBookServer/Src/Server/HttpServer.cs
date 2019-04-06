using AccoutBookServer.Dao;
using AccoutBookServer.Http;
using CSSamples.Common.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace AccoutBookServer.Server
{
    public class HttpServer : IDisposable
    {
        static Logger _logger = Logger.GetLogger(typeof(HttpServer));

        static TransactionDao _dao = new TransactionDao();

        public static HttpServer Create()
        {
            var result = new HttpServer();
            //result._listener.AddRequestHandler(new SimpleHttpGetHandler(@"/account/?$", "Res/Root/index.html", null));
            //result._listener.AddRequestHandler(new SimpleHttpGetHandler(@"/account/detail/(\d{4}/\d{2}/\d{2})/?$", "Res/Root/detail.html", result.ReturnDaylyDetail));
            //result._listener.AddRequestHandler(new SimpleHttpGetHandler(@"/account/detail/(\d{4}/\d{2})/?$", "Res/Root/index.html", result.ReturnMonthlyDetail));
            result._listener.AddRequestHandler(new SimpleHttpGetHandler(@"/account/edit/(\d*)/?$", "Res/Root/edit.html", result.ReturnEdit));
            result._listener.AddRequestHandler(new SimpleHttpPostHandler(@"/account/edit/(\d*)/?$", "Res/Root/detail.html", result.ReceiveEdit));
            result._listener.AddRequestHandler(new SimpleHttpGetHandler(@"/account/?$", "Res/Root/detail.html", result.ReturnDaylyDetail));
            return result;
        }

        private HttpServer()
        {
        }

        private HttpListenerWrapper _listener = new HttpListenerWrapper();

        private string _protocol = "http";
        private string _hostName = "192.168.2.201";
        private int _portNo = 8001;
        private string Prefix { get => $"{_protocol}://{_hostName}:{_portNo}/"; }

        public string Protocol { get => _protocol; set => _protocol = value; }
        public string HostName { get => _hostName; set => _hostName = value; }
        public int PortNo { get => _portNo; set => _portNo = value; }

        public void Start() => RunWithLog("Strat()", () =>
        {
            if (_listener.IsListening)
                throw new Exception("running");

            _listener.Start(Prefix);
        });

        public void Stop() => RunWithLog("Stop()", () =>
         {
             StopImpl();
         });

        public void Dispose() => RunWithLog("Dispose()", () =>
        {
            StopImpl();
        });

        void StopImpl()
        {
            if (_listener.IsListening)
                _listener.Stop();
        }

        void RunWithLog(string processName, Action process)
        {
            try
            {
                _logger.Info("{0} in", processName);
                process.Invoke();
            }
            finally
            {
                _logger.Info("{0} out", processName);
            }
        }

        Dictionary<string, string> ReturnDaylyDetail(SimpleHttpGetHandler sender, HttpListenerContext context)
        {
            var result = new Dictionary<string, string>();
            var dtos = _dao.SelectDaylyDetailPrices()
                .GroupBy(dto => dto.TransactionDate.Year)
                .OrderByDescending(groupByYear => groupByYear.Key)
                .Select(groupByYear => new YearGroupedRowModel
                {
                    Year = groupByYear.Key,
                    AddLinkUrl = $"{Prefix}account/edit/",
                    Months = groupByYear
                        .GroupBy(dto => dto.TransactionDate.Month)
                        .OrderByDescending(groupByMonth => groupByMonth.Key)
                        .Select(groupByMonth => new MonthGroupedRowModel
                        {
                            Year = groupByYear.Key,
                            Month = groupByMonth.Key,
                            AddLinkUrl = $"{Prefix}account/edit/",
                            Days = groupByMonth
                                .GroupBy(dto => dto.TransactionDate.Day)
                                .OrderByDescending(groupByDay => groupByDay.Key)
                                .Select(groupByDay => new DayGroupedRowModel
                                {
                                    Year = groupByYear.Key,
                                    Month = groupByMonth.Key,
                                    Day = groupByDay.Key,
                                    AddLinkUrl = $"{Prefix}account/edit/",
                                    Dailies = groupByDay
                                        .OrderByDescending(dto => dto.TransactionId)
                                        .Select(dto => new DailyDetailRowModel
                                        {
                                            TransactionId = dto.TransactionId,
                                            TransactionDate = dto.TransactionDate,
                                            ItemName = dto.ItemName,
                                            ItemPrice = dto.ItemPrice,
                                            ItemType = dto.ItemType,
                                            ShopName = dto.ShopName,
                                            Memo = dto.Memo,
                                            UserName = dto.UserName,
                                            EditLinkUrl = $"{Prefix}account/edit/{dto.TransactionId}"
                                        })
                                })
                        })
                });
            result.Add("body", string.Join("\n", dtos));
            return result;
        }

        Dictionary<string, string> ReturnEdit(SimpleHttpGetHandler sender, HttpListenerContext context)
        {
            var result = new Dictionary<string, string>();
            string transactionId = sender.UrlParameters[1];
            var dto = string.IsNullOrEmpty(transactionId)
                ? new TransactionDto { TransactionDate = DateTime.Now }
                : _dao.SelectDaylyDetailPrice(transactionId);
            result.Add("transactionDate", dto.TransactionDate.ToString("yyyy-MM-dd"));
            result.Add("itemName", dto.ItemName);
            result.Add("itemPrice", dto.ItemPrice.ToString());
            result.Add("itemType", dto.ItemType);
            result.Add("shopName", dto.ShopName);
            result.Add("memo", dto.Memo);
            result.Add("userName", dto.UserName);
            if (string.IsNullOrEmpty(transactionId))
                result.Add("style", @"style=""display:none""");
            return result;
        }

        private string ReceiveEdit(SimpleHttpPostHandler sender, HttpListenerContext context)
        {
            var parsed = HttpUtility.ParseQueryString(new StreamReader(context.Request.InputStream).ReadToEnd());
            _logger.Debug("parameter={0}", string.Join(", ", sender.UrlParameters));
            _logger.Debug("form parameter={0}", string.Join(", ", parsed.AllKeys.Select(key => parsed[key])));
            const int NOT_FOUND = -1;
            if (Array.IndexOf(parsed.AllKeys, "delete") != NOT_FOUND)
            {
                if (!string.IsNullOrEmpty(sender.UrlParameters[1]))
                    _dao.DeleteTransactionDto(int.Parse(sender.UrlParameters[1]));
            }
            else
            {
                var dto = new TransactionDto
                {
                    TransactionDate = DateTime.Parse(parsed["transactionDate"]),
                    ItemName = parsed["itemName"],
                    ItemPrice = int.Parse(parsed["itemPrice"]),
                    ItemType = parsed["itemType"],
                    ShopName = parsed["shopName"],
                    Memo = parsed["memo"],
                    UserName = parsed["userName"],
                };
                if (string.IsNullOrEmpty(sender.UrlParameters[1]))
                {
                    _dao.InsertTransactionDto(dto);
                }
                else
                {
                    dto.TransactionId = int.Parse(sender.UrlParameters[1]);
                    _dao.UpdateTransactionDto(dto);
                }

                _logger.Debug("dto={0}", dto);
            }

            return $"{Prefix}/account/";
        }

    }

    class YearGroupedRowModel
    {
        public int Year { get; set; }
        public string AddLinkUrl { get; set; }
        public IEnumerable<MonthGroupedRowModel> Months { get; set; }

        public int GetSum()
            => Months.Sum(month => month.GetSum());

        public override string ToString()
            => $"<tr>" +
                $"<td><a href=\"{AddLinkUrl}\">追加</a></td>" +
                $"<td><b>{new DateTime(Year, 1, 1).ToString("yyyy")}</b></td>" +
                $"<td><b>年間合計</b></td>" +
                $"<td align=\"right\"><b>{GetSum()}</b></td>" +
                $"</tr>\n" +
                string.Join("\n", Months);
    }

    class MonthGroupedRowModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string AddLinkUrl { get; set; }
        public IEnumerable<DayGroupedRowModel> Days { get; set; }

        public int GetSum()
            => Days.Sum(day => day.GetSum());

        public override string ToString()
            => $"<tr>" +
                $"<td><a href=\"{AddLinkUrl}\">追加</a></td>" +
                $"<td><b>{new DateTime(Year, Month, 1).ToString("yyyy/MM")}</b></td>" +
                $"<td><b>月間合計</b></td>" +
                $"<td align=\"right\"><b>{GetSum()}</b></td>" +
                $"</tr>\n" +
                string.Join("\n", Days);
    }

    class DayGroupedRowModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string AddLinkUrl { get; set; }
        public IEnumerable<DailyDetailRowModel> Dailies { get; set; }

        public int GetSum()
            => Dailies.Sum(dto => dto.ItemPrice);

        public override string ToString()
            => $"<tr>" +
                $"<td><a href=\"{AddLinkUrl}\">追加</a></td>" +
                $"<td><b>{new DateTime(Year, Month, Day).ToString("yyyy/MM/dd")}</b></td>" +
                $"<td><b>日間合計</b></td>" +
                $"<td align=\"right\"><b>{GetSum()}</b></td>" +
                $"</tr>\n" +
                string.Join("\n", Dailies);
    }

    class DailyDetailRowModel
    {
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ItemName { get; set; }
        public int ItemPrice { get; set; }
        public string ItemType { get; set; }
        public string ShopName { get; set; }
        public string Memo { get; set; }
        public string UserName { get; set; }
        public string EditLinkUrl { get; set; }

        public override string ToString()
        {
            return $"<tr>" +
                $"<td><a href=\"{EditLinkUrl}\">編集</a></td>" +
                $"<td style=\"display:none\">{TransactionId}</td>" +
                $"<td>{TransactionDate.ToString("yyyy/MM/dd")}</td>" +
                $"<td>{ItemName}</td>" +
                $"<td align=\"right\">{ItemPrice}</td>" +
                $"<td>{ItemType}</td>" +
                $"<td>{ShopName}</td>" +
                //$"<td>{Memo}</td>" +
                //$"<td>{UserName}</td>" +
                $"</tr>";
        }
    }

}
