namespace HardRightEdge.Tests

open System
open NUnit.Framework
open FsUnit
open HardRightEdge
open HardRightEdge.Services.Domain
open HardRightEdge.Services.DomainServices
open HardRightEdge.Portfolio.Controller
open HardRightEdge.Portfolio.Model
open HardRightEdge.Portfolio.View

module IntegrationTests = 
  
  [<Test>]
  let ``DomainServices.saveFinancialSecurity should update stock`` () =
    let stock =  {  id = Some 1L;
                name = "GSK.L2";
                previousName = None;
                prices = [];
                dataProviders = Seq.empty }
        
    let reslt = saveFinancialSecurity stock
    
    reslt |> should not' (throw typeof<Exception>)

  [<Test>]
  let ``Portfolio MVC should draw charts`` () =
    let r = Random ()
    let stocks = [ for i in 1 .. 4 ->
                    { id = Some (int64 i);
                      name = "GSK.L2"
                      previousName = None
                      prices = [for x in 1L .. 100L ->
                                  { id = None
                                    stockId = Some x
                                    date = DateTime.Now
                                    openp = 0.0
                                    high = 0.0
                                    low = 0.0
                                    close = double (r.Next(1, 100))
                                    adjClose = 0.0
                                    volume = 0L }]
                      dataProviders = Seq.empty } ]

    // TODO: Make this work
    show scene << with' <| stocks