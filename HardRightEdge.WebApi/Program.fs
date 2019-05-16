open Suave
open Suave.Web
open Suave.Successful
open Suave.Filters
open Suave.Operators
open HardRightEdge.Domain
open HardRightEdge.Repositories
open HardRightEdge.WebApi.Restful
open HardRightEdge.Application

[<EntryPoint>]
let main _ =
  let portfolioWebPart = rest "portfolio" 
                          [
                            GetAll portfolio
                            Update savePortfolio
                            UpdateById saveSecurityById
                          ]
  startWebServer defaultConfig portfolioWebPart
  0 // return an integer exit code*)
