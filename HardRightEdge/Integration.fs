module HardRightEdge.Integration

open System
open System.Threading
open System.Threading.Tasks
open YahooFinanceAPI
open HardRightEdge.Domain
open HardRightEdge.Infrastructure.Concurrent

module Yahoo = 

      module Fields =
        let shortName = "shortName"
        let startMonth = "startMonth"
        let startDay = "startDay"
        let startYear = "startYear"
        let endMonth = "endMonth"
        let endDay = "endDay"
        let endYear = "endYear"

      type QueryDate = {  month: string;
                          day: int }
      
      let private queryDate (date: DateTime) =
        {   month = (date.Month - 1).ToString().PadLeft(2, '0');
            day = date.Day }

      let private fromDte date = queryDate date

      let private toDte = queryDate DateTime.Now

      let getSharePrices symbol (from: DateTime) (to': unit -> DateTime) =
        
        while String.IsNullOrEmpty(Token.Cookie) || String.IsNullOrEmpty(Token.Crumb) do
          awaitPlainTask (Token.RefreshAsync()) |> ignore        

        let priceHistoryAsync = async {
                              let toVal = to'()
                              let! priceHistory = Historical.GetPriceAsync(symbol, from, toVal) |> Async.AwaitTask
                              return priceHistory
                            }

        let priceHistory = priceHistoryAsync |> Async.RunSynchronously

        { id = None;
          platforms = [| {  shareId = None;
                            symbol = symbol.ToUpper();
                            platform = Platform.Yahoo } |];
          name = symbol.ToUpper();
          previousName = None;
          prices = [ for price in priceHistory do
                        yield { id = None;
                                shareId = None;
                                date = price.Date;
                                openp = price.Open;
                                high = price.High;
                                low = price.Low;
                                close = price.Close; 
                                volume = price.Volume |> int64;
                                adjClose = Some price.AdjClose } ] }
