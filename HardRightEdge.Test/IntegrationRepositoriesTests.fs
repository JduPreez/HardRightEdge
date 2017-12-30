module HardRightEdge.IntegrationRepositoriesTests

open System
open Xunit
open HardRightEdge.Repositories
open HardRightEdge.Domain

[<Fact>]
let ``ShareRepository should insertShare`` () =
  ShareRepository.insertShare { id = None
                                name = "GSK"
                                previousName = None
                                prices = []
                                dataProviders = Seq.empty<DataProviderShare> }