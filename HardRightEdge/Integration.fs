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
                        yield { id        = None
                                shareId   = None
                                date      = price.Date
                                openp     = price.Open
                                high      = price.High
                                low       = price.Low
                                close     = price.Close 
                                volume    = price.Volume |> int64
                                adjClose  = Some price.AdjClose } ] }

let importsRoot = "Imports"

module Saxo =
  open ExcelPackageF
  open HardRightEdge.Infrastructure.Common
  open HardRightEdge.Infrastructure.FileSystem
  open HardRightEdge.Domain

  module Trades =
    let filePattern = "Trades_*.xlsx"
    let datePattern = "dd/MM/yyyy"

  let accountCurrency (accountId: string) = 
    currency (accountId.ToUpper().Substring(accountId.Length - 3)) |> Some

  let shareCurrency (symbol: string) =
    match symbol.ToLower().Split([|':'|]).[1] with
    | "xnas" | "xnys" -> Currency.USD |> Some
    | "xlon"          -> Currency.GBP |> Some
    | "xses"          -> Currency.SGD |> Some
    | "xetr"          -> Currency.EUR |> Some
    | "xcse"          -> Currency.DKK |> Some

  let toTrade (row: string seq) =
    match row |> List.ofSeq with
    | [ tradeId'; 
        accountId;
        instrument; 
        tradeTime; 
        buyOrSell; 
        openOrClose; 
        amount; 
        price'; 
        tradedVal; 
        spreadCosts; 
        bookedAmount;
        symbol';
        _ ] -> Some { 
                      id          = None
                      tradeId     = tradeId'
                      account     = accountId
                      type'       = match buyOrSell.ToLower() with
                                    | "bought"  -> TradeType.Bought
                                    | "sold"    -> TradeType.Sold
                                    | _         -> TradeType.TransferIn
                      isOpen      = openOrClose.ToLower() = "open"
                      commission  = None
                      share       = { id            = None
                                      name          = instrument 
                                      previousName  = None
                                      prices        = []
                                      platforms     = seq [ { shareId   = None; 
                                                              platform  = Platform.Saxo; 
                                                              symbol    = symbol' } ]
                                      currency      = symbol' |> shareCurrency }
                      transaction   = { id              = None
                                        quantity        = Some(int64 amount)
                                        date            = tradeTime |> toDateTime Trades.datePattern
                                        valueDate       = None
                                        settlementDate  = None
                                        price           = float price'
                                        amount          = float bookedAmount
                                        type'           = Security(Security.Share)
                                        currency        = accountId |> accountCurrency }}
    | [] -> None

  let getTrades () = 
    match box (query {
      for fl in files (importsRoot +/ "Saxo") Trades.filePattern do
        select fl
        headOrDefault }) with
    | :? string as tradesFile -> 

      // TODO:
      // 1. Read file: TradeId, AccountID, Instrument, TradeTime, B/S, OpenClose, Amount, Price
      // 2. Split trades into open & closed transactions
      // 3. leftOuterJoin closed transactions onto open transactions
      //    on Instrument & Amount (make sure to * -1 negative amounts to make them positive)
      // 4. Rows where the closed transaction in the join is null, are the remaining ones      
      let worksheet   = Excel.getWorksheetByIndex 2 tradesFile // Trades with additional info
      let maxRow      = Excel.getMaxRowNumber worksheet

      seq { for row in 1 .. maxRow ->
              worksheet 
              |> Excel.getRow row
              |> toTrade }
              
    | null -> Seq.empty<Trade option>