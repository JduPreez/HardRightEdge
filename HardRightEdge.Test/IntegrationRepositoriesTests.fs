module HardRightEdge.Test.IntegrationRepositoriesTests

open HardRightEdge.Repositories
open HardRightEdge.Domain
open HardRightEdge.Infrastructure
open Xunit

let insertShare () = Securities.insert Stubs.testShare

[<Fact>]
let ``Securities should insertShare`` () =
  UnitOfWork.temp {
    match insertShare () with
    | { id = Some sid } -> Assert.True(sid > 0L)
    | _                 -> Assert.True(false)
  }

[<Fact>]
let ``Securities should update share`` () =
  UnitOfWork.temp {
    let share = insertShare ()

    match (Securities.update { share with previousName = Some "test" }) with
    | { id = Some sid } -> Assert.True(true)
    | _                 -> Assert.True(false)
  }

[<Fact>]
let ``Securities should get share`` () =
  UnitOfWork.temp {
    match insertShare () with
    | { id = Some sid } ->  match Securities.get sid None with
                            | Some _  -> Assert.True(true)
                            | _       -> Assert.True(false)
    | _       -> Assert.True(false)
  }

[<Fact>]
let ``Securities should saveShare without prices`` () =
  UnitOfWork.temp {
    match Securities.save Stubs.testShare with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Securities should saveShare with prices`` () =
  UnitOfWork.temp {
    match Securities.save { Stubs.testShare 
                            with prices = (Stubs.testSharePrices None) } with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Securities should getBySymbol`` () = 
  UnitOfWork.temp {
    match Securities.save Stubs.testShare with
    | { id = Some _; platforms = platfs } ->
      let yahoo = Seq.find (fun p -> p.platform = Platform.Yahoo)  platfs
      let share = Securities.getBySymbol yahoo.symbol Platform.Yahoo
      Assert.NotEqual(None, share)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Securities should getBySymbol with prices`` () =
  UnitOfWork.temp {
    match Securities.save Stubs.testShare with
    | { id = Some _; platforms = platfs; prices = { id = Some _ } :: _ } ->
      let yahoo = Seq.find (fun p -> p.platform = Platform.Yahoo)  platfs
      match Securities.getBySymbol yahoo.symbol Platform.Yahoo with
      | Some share  -> Assert.NotEmpty(share.prices)
      | _           -> Assert.True(false)
    | _ -> Assert.True(false)
  }