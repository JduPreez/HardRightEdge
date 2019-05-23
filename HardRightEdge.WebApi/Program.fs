open Suave
open HardRightEdge.WebApi.Restful
open HardRightEdge.Application

[<EntryPoint>]
let main _ =
  let api = choose [  rest "portfolio/file"
                        [
                          Create portfolioFile
                        ];
                      rest "portfolio" 
                        [
                          GetAll portfolio
                          Update savePortfolio
                          UpdateById saveSecurityById
                        ]]
  startWebServer defaultConfig api
  0 // return an integer exit code*)
