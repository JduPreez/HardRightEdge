module HardRightEdge.Application

open HardRightEdge.Repositories
open HardRightEdge.Integration
open Domain

let getShareBySymbol = Shares.getBySymbol
let getShareByDate = Yahoo.getShare
let saveShare = Shares.save

let getShare = Domain.Services.getSyncShare getShareBySymbol getShareByDate saveShare

let portfolio = Domain.Services.portfolio Saxo.tradesOpen getShare Platform.Yahoo