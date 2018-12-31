open Suave
open Suave.Web
open Suave.Successful
open Suave.Filters
open Suave.Operators
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

  (*let portfolioWebPart =
    choose
        [ GET >=> choose
            [ path "/" >=> OK "Index"
              path "/portfolio" >=> OK "Hello!" ]
          POST >=> choose
            [ path "/portfolio" >=> request (getResourceFromReq >> saveSecurity >> JSON); OK "Hello POST!" ] ]*)

  startWebServer defaultConfig portfolioWebPart
  0 // return an integer exit code*)
