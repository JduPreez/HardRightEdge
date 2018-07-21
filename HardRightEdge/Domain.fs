module HardRightEdge.Domain

open System

let default' (deflt: unit -> 't) = deflt()

let priceStartDate () = DateTime.Now.AddMonths(-4)

let priceEndDate () = DateTime.Now.AddDays(-1.0)
  
type SharePrice = {
  id:         int64 option
  shareId:    int64 option
  date:       DateTime
  openp:      float
  high:       float
  low:        float
  close:      float
  adjClose:   float option
  volume:     int64 }

let idIs (symbol: string) (id: int) = 
  let mapToId id' = 
    if symbol.ToUpper() = symbol then Some id' else None
  mapToId

type CurrencyType =
| USD of int * string 
| EUR of int * string
| GBP of int * string
| SGD of int * string
| DKK of int * string

module Currency =
  let USD = CurrencyType.USD(1, "USD")
  let EUR = CurrencyType.EUR(2, "EUR")
  let GBP = CurrencyType.GBP(3, "GBP")
  let SGD = CurrencyType.SGD(4, "SGD")
  let DKK = CurrencyType.DKK(5, "DKK")

let currency symbol =
  match symbol with
  | "USD" as x -> Currency.USD
  | "EUR" as x -> Currency.EUR
  | "GBP" as x -> Currency.GBP
  | "SGD" as x -> Currency.SGD
  | "DKK" as x -> Currency.DKK
 
type Platform =
| Yahoo   = 1
| Google  = 2
| Saxo    = 3

type SharePlatform = {
  shareId: int64 option
  platform: Platform
  symbol: string }

type Share = {  
  id:           int64 option
  name:         string
  previousName: string option
  prices:       SharePrice list
  platforms:    SharePlatform seq
  currency:     CurrencyType option }

type Security =
| Share = 1

type CorporateAction =
| DividendCash = 2
| DividendReinvestment = 3
| Fractions = 4

type Expense =
| StockExchangeFee = 5
| Commission = 6

type Tax =
| WithholdingTax = 7

type TransactionType =
| Security of Security
| CorporateAction of CorporateAction
| Expense of Expense
| Tax of Tax

type Transaction = {
  id:             int64 option
  quantity:       int64 option
  date:           DateTime // Trade date
  valueDate:      DateTime option
  settlementDate: DateTime option
  price:          float
  amount:         float
  type':          TransactionType
  currency:       CurrencyType option }

type TradeType =
| Bought = 1
| Sold = 2
| TransferIn = 3

type Trade = {
  id:           int64 option
  tradeId:      string
  account:      string
  type':        TradeType
  isOpen:       bool
  share:        Share
  transaction:  Transaction
  commission:   Transaction option }

module Services =

  type getShare             = string * Platform -> string * Platform -> Share option
  type getShareByPlatform   = string  -> Platform        -> Share option
  type getShareByDate       = string  -> DateTime option -> Share option  
  type saveShare            = Share   -> Share
  type tradesOpen           = unit    -> Trade seq

  let getSyncShare (getShare: getShareByPlatform) (getShareFromDataFeed: getShareByDate) (saveShare: saveShare) (symblPlatfrm: string * Platform) (feedSymPlatfrm: string * Platform) =

    let (symbol, platform) = symblPlatfrm
    let (feedSymbl, feedPlatfrm) = feedSymPlatfrm

    let shareNotFound = sprintf "Share '%s (%s)' wasn't found on platform '%O (%O)" symbol feedSymbl platform feedPlatfrm

    let share = match getShare symbol platform with // TODO: Change this to not call getShare, & just match on a passed in Share record
                | Some ({ prices = head :: _ } as s) ->
                  // We have an existing share, with at least 1 historic share price,
                  // so just update the prices to latest
                  match getShareFromDataFeed feedSymbl (Some head.date) with
                  | Some { prices = prcs } -> { s with prices = s.prices @ prcs }
                  | _ -> failwith shareNotFound

                | Some ({ prices = [] } as s) ->
                  // We have an existing share, but without any historic prices
                  match getShareFromDataFeed feedSymbl None with
                  | Some { prices = prcs } -> { s with prices = prcs }
                  | _ -> failwith shareNotFound

                | None -> 
                  // Completely new share, not stored locally yet, so just use whatever
                  // the platform gives us
                  match getShareFromDataFeed feedSymbl None with
                  | Some s -> s
                  | _ -> failwith shareNotFound

    // Save share prices returned by data feed platform to DB
    Some(saveShare share)
  
  let portfolio (tradesOpen: tradesOpen) (getSyncShare: getShare) (dataFeedPlatfrm: Platform) (platform: Platform) =
    
    (tradesOpen())                                    // In this version, we just show some graphs
    |> Seq.groupBy  (fun t -> t.share.name)           // for each unique share in the list of trades,
    |> Seq.map      (fun x ->  x |> snd |> Seq.head)  // threrefore just pick 1 trade, because the share
                                                      // info will be the same for each one in the groupBy.                                                      
    |> Seq.map      (fun x ->
                      let pfs = x.share.platforms 
                                |> Seq.map (fun sp -> sp.symbol, sp.platform)

                      getSyncShare  (Seq.find (fun p -> snd p = platform) pfs) 
                                    (Seq.find (fun p -> snd p = dataFeedPlatfrm) pfs))
    |> Seq.filter   (fun x -> x.IsSome)
    |> Seq.map      (fun x -> x.Value)
    |> Seq.toList