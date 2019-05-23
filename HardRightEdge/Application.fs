module HardRightEdge.Application

open HardRightEdge.Repositories
open HardRightEdge.Integration
open HardRightEdge.Infrastructure
open HardRightEdge.Domain

let getShareBySymbol = Securities.getBySymbol

let getShareByDate = Yahoo.getShare

let saveSecurity = Securities.save

let saveSecurityById id sec = saveSecurity { sec with id = Some id }

let getSecurity = Domain.Services.getSyncSecurity getShareBySymbol getShareByDate saveSecurity

let portfolio () = 
  let tradesOpen =  Saxo.worksheetFromFile "Saxo" Saxo.Trades.filePattern
                    |> Saxo.tradesOpen
  Domain.Services.portfolio tradesOpen getSecurity Platform.Yahoo Platform.Saxo

let portfolioFile (docs : Document<byte array, Security list> list) = 
  [{  content = 
        match docs with
        | { file = Some f } :: _ ->
          let tradesOpen =  Saxo.worksheetFromBin f
                            |> Saxo.tradesOpen
          Domain.Services.portfolio tradesOpen getSecurity Platform.Yahoo Platform.Saxo |> Some
        | _  -> None 
      file    = None }]

let savePortfolio (securities: Security list) =
  UnitOfWork.durable {
    return! [ for sec in securities ->
                saveSecurity sec ]
  }