module HardRightEdge.Domain

open System

let default' (deflt: unit -> 't) = deflt()

let priceStartDate () = DateTime.Now.AddMonths(-4)

let priceEndDate () = DateTime.Now.AddDays(-1.0)
  
type SecurityPrice = {
  id:         int64 option
  securityId: int64 option
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
  | "EUR" -> Currency.EUR
  | "GBP" -> Currency.GBP
  | "SGD" -> Currency.SGD
  | "DKK" -> Currency.DKK
  | _ -> Currency.USD
 
type Platform =
| Yahoo   = 1
| Google  = 2
| Saxo    = 3

type SecurityPlatform = {
  securityId: int64 option
  platform:   Platform
  symbol:     string }

type Security = {  
  id:           int64 option
  name:         string
  previousName: string option
  prices:       SecurityPrice list
  platforms:    SecurityPlatform seq
  currency:     CurrencyType option }

type SecurityTransaction =
| Equity = 1

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
| SecurityTransaction of SecurityTransaction
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
  security:     Security
  transaction:  Transaction
  commission:   Transaction option }

module Services =

  type getSecurity            = string * Platform -> string * Platform -> Security option
  type getSecurityByPlatform  = string    -> Platform        -> Security option
  type getSecurityByDate      = string    -> DateTime option -> Security option  
  type saveSecurity           = Security  -> Security
  type tradesOpen             = unit      -> Trade seq

  let getSyncSecurity (getShare: getSecurityByPlatform) (getShareFromDataFeed: getSecurityByDate) (saveShare: saveSecurity) (tradePlatfrm: string * Platform) (feedSymPlatfrm: string * Platform) =

    let (symbol, platform) = tradePlatfrm
    let (feedSymbl, feedPlatfrm) = feedSymPlatfrm

    let securityNotFound = sprintf "Security '%s (%s)' wasn't found on platform '%O (%O)" symbol feedSymbl platform feedPlatfrm

    let share = match getShare symbol platform with // TODO: Change this to not call getShare, & just match on a passed in Share record
                | Some ({ prices = head :: _ } as s) ->
                  // We have an existing share, with at least 1 historic share price,
                  // so just update the prices to latest
                  match getShareFromDataFeed feedSymbl (Some head.date) with
                  | Some { prices = prcs } -> { s with prices = s.prices @ prcs }
                  | _ -> failwith securityNotFound

                | Some ({ prices = [] } as s) ->
                  // We have an existing share, but without any historic prices
                  match getShareFromDataFeed feedSymbl None with
                  | Some { prices = prcs } -> { s with prices = prcs }
                  | _ -> failwith securityNotFound

                | None -> 
                  // Completely new share, not stored locally yet, so just use whatever
                  // the platform gives us
                  match getShareFromDataFeed feedSymbl None with
                  | Some s -> s
                  | _ -> failwith securityNotFound

    // Save share prices returned by data feed platform to DB
    Some(saveShare share)
  
  let portfolio (tradesOpen: tradesOpen) (getSecurity: getSecurity) (dataFeedPlatfrm: Platform) (tradePlatfrm: Platform) =    
    let p () =
      (tradesOpen())                                    // In this version, we just show some graphs
      |> Seq.groupBy  (fun t -> t.security.name)        // for each unique share in the list of trades,
      |> Seq.map      (fun x ->  x |> snd |> Seq.head)  // threrefore just pick 1 trade, because the share
                                                        // info will be the same for each one in the groupBy.                                                      
      |> Seq.map      (fun x ->
                        let pfs = x.security.platforms 
                                  |> Seq.map (fun sp -> sp.symbol, sp.platform)

                        match Seq.tryFind (fun p -> snd p = dataFeedPlatfrm) pfs with
                        | Some dataPlatfrm ->
                          // If the share doesn't have a symbol for the data platform,
                          // then don't synch it to the local database.
                          getSecurity  (Seq.find (fun p -> snd p = tradePlatfrm) pfs)
                                        dataPlatfrm
                        | _ -> Some x.security) // No data platform, therefore only return the Trade's Share, without synching
                        
      |> Seq.filter   (fun x -> x.IsSome)
      |> Seq.map      (fun x -> x.Value)
      |> Seq.toList      
    p