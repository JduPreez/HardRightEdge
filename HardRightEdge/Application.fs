module HardRightEdge.Application

open HardRightEdge.Repositories
open HardRightEdge.Integration
open Domain

let getShareBySymbol = Securities.getBySymbol
let getShareByDate = Yahoo.getShare

let saveSecurity = Securities.save

let saveSecurityById id sec = saveSecurity { sec with id = Some id }

let getSecurity = Domain.Services.getSyncSecurity getShareBySymbol getShareByDate saveSecurity

let portfolio = Domain.Services.portfolio Saxo.tradesOpen getSecurity Platform.Yahoo Platform.Saxo
