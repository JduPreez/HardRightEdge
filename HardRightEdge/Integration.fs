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

      let getSharePrices symbol (from: DateTime option) =
        
        while String.IsNullOrEmpty(Token.Cookie) || String.IsNullOrEmpty(Token.Crumb) do
          awaitPlainTask (Token.RefreshAsync()) |> ignore        

        let priceHistoryAsync = async {
                              let to' = DateTime.Now
                              let from' = if from.IsSome 
                                          then from.Value
                                          else DateTime.Now.AddYears(-1)
                                          
                              let! priceHistory = Historical.GetPriceAsync(symbol, from', to') |> Async.AwaitTask
                              return priceHistory
                            }

        let priceHistory = priceHistoryAsync |> Async.RunSynchronously

        { id = None;
          platforms = [| {  shareId = None;
                            symbol = symbol.ToUpper();
                            platform = Platform.Yahoo } |];
          name = symbol.ToUpper();
          previousName = None;
          currency = None;
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

let importsRoot = "Imports"

module Saxo =
  open ExcelPackageF
  open HardRightEdge.Infrastructure.FileSystem
  open HardRightEdge.Domain

  module SaxoFiles =
    trades = "Trades_*.xlsx"

  let getTradesByStatus isOpen = 
    match box (query {
      for fl in files (importsRoot +/ "Saxo") SaxoFiles.trades do
        select fl
        headOrDefault }) with
    | :? string as tradesXlsx -> 

      // TODO:
      // 1. Read file: TradeId, AccountID, Instrument, TradeTime, B/S, OpenClose, Amount, Price
      // 2. Split trades into open & closed transactions
      // 3. leftOuterJoin closed transactions onto open transactions
      //    on Instrument & Amount (make sure to * -1 negative amounts to make them positive)
      // 4. Rows where the closed transaction in the join is null, are the remaining ones

      seq { for row in tradesXlsx
                        |> Excel.getWorksheetByIndex 1
                        |> Excel.getContent -> }
      


    | null -> Seq.empty<Trade>