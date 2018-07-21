module HardRightEdge.IntegrationRepositoriesTests

open System
open Xunit
open HardRightEdge.Repositories
open HardRightEdge.Domain
open HardRightEdge.Infrastructure

module Common =
  let random = Random()

  let testShare = { id = None
                    name = "GlaxoSmithKline Plc"
                    previousName = None
                    prices = [ for i in 1..10 ->      
                                  { id        = None
                                    shareId   = None 
                                    date      = DateTime.Now.AddDays(-1.0)
                                    openp     = random.NextDouble() * float(random.Next(100))
                                    high      = random.NextDouble() * float(random.Next(100))
                                    low       = random.NextDouble() * float(random.Next(100)) 
                                    close     = random.NextDouble() * float(random.Next(100)) 
                                    adjClose  = None
                                    volume    = int64(random.Next()) }]
                    currency = None
                    platforms = seq { yield { shareId = None 
                                              platform = Platform.Saxo
                                              symbol = "GSK:xlon" }
                                              
                                      yield { shareId = None
                                              platform = Platform.Yahoo
                                              symbol = "GSK.L" } } }

  let insertShare () = Shares.insert testShare

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
let ``Shares should get share`` () =
  UnitOfWork.temp {
    match Common.insertShare () with
    | { id = Some sid } ->  match Shares.get sid None with
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

[<Fact>]
let ``Shares should getBySymbol with prices`` () =
  UnitOfWork.temp {
    match Shares.save Common.testShare with
    | { id = Some _; platforms = platfs; prices = { id = Some _ } :: _ } ->
      let yahoo = Seq.find (fun p -> p.platform = Platform.Yahoo)  platfs
      match Shares.getBySymbol yahoo.symbol Platform.Yahoo with
      | Some share  -> Assert.NotEmpty(share.prices)
      | _           -> Assert.True(false)
    | _ -> Assert.True(false)
  }