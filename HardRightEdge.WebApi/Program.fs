open Suave
open Suave.Web
open Suave.Successful
open HardRightEdge.WebApi.Restful
open HardRightEdge.Application


[<EntryPoint>]
let main _ = 
  let portfolioWebPart = rest "portfolio" {
    GetAll      = Some portfolio
    GetById     = None
    IsExists    = None
    Create      = Some saveSecurity
    Update      = None
    UpdateById  = Some saveSecurityById
    Delete      = None
  }

  startWebServer defaultConfig portfolioWebPart
  0 // return an integer exit code*)
