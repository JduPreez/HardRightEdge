namespace HardRightEdge.WebApi

module App =   
  open Suave.Web
  open HardRightEdge.WebApi.Restful
  open HardRightEdge.Application
  open Suave

  [<EntryPoint>]
  let main _ = 
    let shareWebPart = rest "shares" {
      GetAll      = Some getShares
      GetById     = None
      IsExists    = None
      Create      = None 
      Update      = None 
      UpdateById  = None
      Delete      = None
    }
    0 // return an integer exit code
