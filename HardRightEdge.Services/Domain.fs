namespace HardRightEdge.Services

open System

module Domain =

  let default' (deflt: unit -> 't) = deflt()

  let priceStartDate () = DateTime.Now.AddMonths(-4)

  let priceEndDate () = DateTime.Now.AddDays(-1.0)
    
  type StockPrice = {
    id:         int64 option;
    stockId:    int64 option;
    date:       DateTime; 
    openp:      decimal;
    high:       decimal; 
    low:        decimal;
    close:      decimal;
    adjClose:   decimal;
    volume:     int64 }

  type DataProvider =
  | Yahoo = 1
  | Google = 2

  type DataProviderStock = {
    stockId: int64 option; 
    dataProvider: DataProvider; 
    symbol: string }

  type Stock = {  
    id: int64 option; 
    name: string; 
    previousName: string option;
    prices: StockPrice list;
    dataProviders: DataProviderStock seq; }