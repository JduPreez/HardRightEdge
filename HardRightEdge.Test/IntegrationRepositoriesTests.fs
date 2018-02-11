module HardRightEdge.IntegrationRepositoriesTests

open System
open Xunit
open HardRightEdge.Repositories
open HardRightEdge.Domain
open HardRightEdge.Infrastructure

module Common =
  let testShare = { id = None
                    name = "GSK"
                    previousName = None
                    prices = []
                    platforms = Seq.empty<SharePlatform> }

  let insertShare () = ShareRepository.insertShare testShare

  let random = Random()

  let testSharePrices shrId = seq { for x in 1 .. 5 -> 
                                      { id        = None  
                                        shareId   = shrId 
                                        date      = DateTime.Now 
                                        openp     = double (random.Next())
                                        high      = double (random.Next()) 
                                        low       = double (random.Next()) 
                                        close     = double (random.Next()) 
                                        adjClose  = None
                                        volume    = int64 (random.Next()) } } |> Seq.toList

[<Fact>]
let ``ShareRepository should insertShare`` () =
  UnitOfWork.temp {
    match Common.insertShare () with
    | { id = Some sid } -> Assert.True(sid > 0L)
    | _                 -> Assert.True(false)
  }

[<Fact>]
let ``ShareRepository should updateShare`` () =
  UnitOfWork.temp {
    let share = Common.insertShare () 

    match (ShareRepository.updateShare { share with previousName = Some "test" }) with
    | { id = Some sid } -> Assert.True(true)
    | _                 -> Assert.True(false)
  }


[<Fact>]
let ``ShareRepository should getShare`` () =
  UnitOfWork.temp {
    match Common.insertShare () with
    | { id = Some sid } ->  match (ShareRepository.getShare sid) with
                          | Some _  -> Assert.True(true)
                          | _       -> Assert.True(false)
    | _       -> Assert.True(false)
  }

[<Fact>]
let ``ShareRepository should saveShare without prices`` () =
  UnitOfWork.temp {
    match ShareRepository.saveShare Common.testShare with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``ShareRepository should saveShare with prices`` () =
  UnitOfWork.temp {
    match ShareRepository.saveShare { Common.testShare 
                                        with prices = (Common.testSharePrices None) } with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

// TODO: Wrap in SQL transactions, to rollback data