open Suave
open HardRightEdge.WebApi.Restful
open HardRightEdge.Application

[<EntryPoint>]
let main _ =
  // TODO:
  // 1. Upload a file to an existing portfolio & allow user to delete securities before saving & merging the
  //    new securities with the existing ones
  // 2. Import other data points for portfolio performance report
  // 3. Reskin front end
  let api = choose [  rest "portfolio/file"
                        [
                          Upload portfolioFileBin
                        ];
                      rest "portfolio" 
                        [
                          GetAll portfolio
                          Update savePortfolio
                          UpdateById saveSecurityById
                        ] ]
  startWebServer defaultConfig api
  0 // return an integer exit code*)
