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

  // TODO 1: Make this work
  let getShare symbol platform =
    // 1. getShareBySymbol, with lastSharePrice
    // 2. Yahoo.getSharePrices symbol (lastSharePrice+1 day)
    // 3. Save share prices returned by Yahoo to DB
    // 4. Now fetch all share prices for the last X years
    // 5. Return share with shares prices from 4.
    ()
  
  // TODO 2: Make this work
  let portfolio () =
    // 1. Get open trades
    // 2. Group by security
    // 3. Get shares prices for each security. If online, synch share prices for each security
    // 4. List grouped securities, with share prices from DB

    ()