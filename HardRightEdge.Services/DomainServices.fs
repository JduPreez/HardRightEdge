namespace HardRightEdge.Services

open System
open HardRightEdge.Services.Domain
open HardRightEdge.Services.Repositories
open HardRightEdge.Services.Infrastructure
open HardRightEdge.Services.Infrastructure.Common

module DomainServices =

  open HardRightEdge.Services.Integration.Yahoo

  let getFinancialSecurity (sec: obj) =

    match
      match sec with
      | :? string as name -> Some (getFinancialSecurity name (default' priceStartDate) priceEndDate)
      | :? int64 as id ->
        // 1. Get the security's info & last price from the DB
        match StockRepository.get id with
        | Some stock ->
          // 2. Get all stock prices from Yahoo Finance, 
          // from the last DB's price until yesterday (the last closing price we have).
          Some { stock with prices = (getFinancialSecurity stock.name stock.prices.Head.date priceEndDate).prices } 
        | _ -> None

      | _ -> None with

    // 3. Save the financial security
    | Some stock -> Some (StockRepository.save(stock))
    | _ -> None

  let saveFinancialSecurity x = 
    StockRepository.save x

  module DataProviderStock =
    
    let get dataProvider stock = Seq.tryFind (fun dps -> dps.dataProvider = dataProvider) stock.dataProviders

    // Find the stockId of the DataProviderStock with the specified DataProvider type.
    let stockId stock = 
      match stock with
      | Some s -> toNullable s.stockId
      | _ -> new Nullable<int64>()

    let symbol stock =
      match stock with
      | Some s -> s.symbol
      | _ -> null