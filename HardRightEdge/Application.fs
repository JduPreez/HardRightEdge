module HardRightEdge.Application

open HardRightEdge.Repositories
open HardRightEdge.Integration
open HardRightEdge.Infrastructure
open HardRightEdge.Domain

let getShareBySymbol = Securities.getBySymbol

let getSecurities () = Securities.get None

let getShareByDate = Yahoo.getShare

let saveSecurity = Securities.save

let saveSecurityById id sec = saveSecurity { sec with id = Some id }

let getSecurity = Domain.Services.getSyncSecurity getShareBySymbol getShareByDate saveSecurity

let portfolio () = getSecurities ()

let portfolioFile () = 
  let tradesOpen =  Saxo.worksheetFromFile Saxo.folder Saxo.filePattern
                    |> Saxo.tradesOpen
  Domain.Services.portfolio tradesOpen getSecurity Platform.Yahoo Platform.Saxo

let portfolioFileBin (file : byte array) = 
  let tradesOpen =  Saxo.worksheetFromBin file
                    |> Saxo.tradesOpen
  Domain.Services.portfolio tradesOpen getSecurity Platform.Yahoo Platform.Saxo

let savePortfolio (securities: Security list) =
  UnitOfWork.durable {
    return! [for sec in securities ->
              saveSecurity sec]
  }