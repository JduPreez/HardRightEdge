module HardRightEdge.Test.IntegrationRepositoriesTests

open HardRightEdge.Repositories
open HardRightEdge.Domain
open HardRightEdge.Infrastructure
open Xunit

let insertShare () = Securities.insert (Stubs.testShare None None None)

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
    | { id = Some sid } ->  match Securities.getById sid None with
                            | Some _  -> Assert.True(true)
                            | _       -> Assert.True(false)
    | _       -> Assert.True(false)
  }

[<Fact>]
let ``Securities should saveShare without prices`` () =
  UnitOfWork.temp {
    match Securities.save (Stubs.testShare None None None) with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Securities should saveShare with prices`` () =
  UnitOfWork.temp {
    match Securities.save { Stubs.testShare None None None
                            with prices = (Stubs.testSharePrices None) } with
    | { id = Some _ } -> Assert.True(true)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Securities should getBySymbol`` () = 
  UnitOfWork.temp {
    match Securities.save (Stubs.testShare None None None) with
    | { id = Some _; platforms = platfs } ->
      let yahoo = Seq.find (fun p -> p.platform = Platform.Yahoo)  platfs
      let security = Securities.getBySymbol yahoo.symbol Platform.Yahoo
      Assert.NotEqual(None, security)
    | _               -> Assert.True(false)
  }

[<Fact>]
let ``Securities should get all`` () =
  UnitOfWork.temp {
    let [sec1; sec2; sec3] as securities = [Securities.save (Stubs.testShare None None None);
                                            Securities.save (Stubs.testShare  (Some "Apple") 
                                                                              (Some "AAPL:xnas") 
                                                                              (Some "AAPL"));
                                            Securities.save (Stubs.testShare  (Some "Mastercard") 
                                                                              (Some "MA:xnas") 
                                                                              (Some "MA")) ]
    let securities2 = Securities.get None
    Assert.True(List.length(securities2) >= List.length(securities))
  }

[<Fact>]
let ``Securities should getBySymbol with prices`` () =
  UnitOfWork.temp {
    match Securities.save (Stubs.testShare None None None) with
    | { id = Some _; platforms = platfs; prices = { id = Some _ } :: _ } ->
      let yahoo = Seq.find (fun p -> p.platform = Platform.Yahoo)  platfs
      match Securities.getBySymbol yahoo.symbol Platform.Yahoo with
      | Some share  -> Assert.NotEmpty(share.prices)
      | _           -> Assert.True(false)
    | _ -> Assert.True(false)
  }