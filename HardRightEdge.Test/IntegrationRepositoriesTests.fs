module HardRightEdge.IntegrationRepositoriesTests

open System
open Xunit
open HardRightEdge.Repositories
open HardRightEdge.Domain
open HardRightEdge.Infrastructure

module Common =
  let testShare = { id = None
                    name = "GlaxoSmithKline Plc"
                    previousName = None
                    prices = []
                    currency = None
                    platforms = seq { yield { shareId = None 
                                              platform = Platform.Saxo
                                              symbol = "GSK:xlon" }
                                              
                                      yield { shareId = None
                                              platform = Platform.Yahoo
                                              symbol = "GSK.L" } } }

  let insertShare () = Shares.insert testShare

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
let ``Shares should insertShare`` () =
  UnitOfWork.temp {
    match Common.insertShare () with
    | { id = Some sid } -> Assert.True(sid > 0L)
    | _                 -> Assert.True(false)
  }

[<Fact>]
let ``Shares should update share`` () =
  UnitOfWork.temp {
    let share = Common.insertShare () 

    match (Shares.update { share with previousName = Some "test" }) with
    | { id = Some sid } -> Assert.True(true)
    | _                 -> Assert.True(false)
  }

[<Fact>]
let ``ShareRepository should get share`` () =
  UnitOfWork.temp {
    match Common.insertShare () with
    | { id = Some sid } ->  match (Shares.get sid) with
                            | Some _  -> Assert.True(true)
                            | _       -> Assert.True(false)
    | _       -> Assert.True(false)
  }

[<Fact>]
let ``Shares should saveShare without prices`` () =
  UnitOfWork.temp {
    match Shares.save Common.testShare with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Shares should saveShare with prices`` () =
  UnitOfWork.temp {
    match Shares.save { Common.testShare 
                                        with prices = (Common.testSharePrices None) } with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Shares should getBySymbol`` () = 
  UnitOfWork.temp {
    match Shares.save Common.testShare with
    | { id = Some _; platforms = platfs } ->
      let yahoo = Seq.find (fun p -> p.platform = Platform.Yahoo)  platfs
      let share = Shares.getBySymbol yahoo.symbol Platform.Yahoo
      Assert.NotEqual(None, share)
    | _               -> Assert.True(false)
  }