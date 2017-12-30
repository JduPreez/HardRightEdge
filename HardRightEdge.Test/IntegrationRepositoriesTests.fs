module HardRightEdge.IntegrationRepositoriesTests

open System
open Xunit
open HardRightEdge.Repositories
open HardRightEdge.Domain

module Common =
  let insertShare () =
    ShareRepository.insertShare { id = None
                                  name = "GSK"
                                  previousName = None
                                  prices = []
                                  platforms = Seq.empty<SharePlatform> }

[<Fact>]
let ``ShareRepository should insertShare`` () =
  match Common.insertShare () with
  | Some id -> Assert.True(id > 0L)
  | _       -> Assert.True(false)

[<Fact>]
let ``ShareRepository should getShare`` () =
  match Common.insertShare () with
  | Some id ->  match (ShareRepository.getShare id) with
                | Some _  -> Assert.True(true)
                | _       -> Assert.True(false)
  | _       -> Assert.True(false)