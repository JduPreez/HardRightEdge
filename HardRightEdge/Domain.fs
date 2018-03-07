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


type Currency =
| SGD of string

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
  currency:     Currency option }

type Security =
| Shares = 1

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
  currency:       Currency option }

type TradeType =
| Bought = 1
| Sold = 2
| TransferIn = 3

type Trade = {
  id:           int64 option
  tradeId:      string
  accountId:    string
  type':        TradeType
  isOpen:       bool
  shareId:      int64
  transaction:  Transaction
  commission:   Transaction option }