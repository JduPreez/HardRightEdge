namespace HardRightEdge.Tests

open System
open NUnit.Framework
open FsUnit
open HardRightEdge.Services.Domain
open HardRightEdge.Services.DomainServices

module IntegrationTests = 
  
  [<Test>]
  let ``DomainServices.saveFinancialSecurity should update stock`` () =
    let x =  {  id = Some 1L;
                name = "GSK.L2";
                previousName = None;
                prices = [];
                dataProviders = Seq.empty }
        
    let y = saveFinancialSecurity x
    
    y |> should not' (throw typeof<Exception>)