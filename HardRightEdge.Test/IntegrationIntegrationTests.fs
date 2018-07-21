module HardRightEdge.IntegrationIntegrationTests

open Xunit
open HardRightEdge.Integration

module Yahoo =

  [<Fact>]
  let ``Yahoo should getSharePrices for the last year`` () =    
    match Yahoo.getShare "SPLK" None with
    | Some share -> 
      Assert.True(List.length share.prices > 200) // Just use any reasonably large number of days less than 365, but more than 1
      Assert.True((List.head share.prices).close > 0.0)
    | _ -> Assert.True(false)

module Saxo =

  [<Fact>]
  let ``Saxo should get trades`` () =
    let trades = Saxo.trades (fun _ -> true) |> Seq.toList // Just get all trades
    Assert.NotEmpty(trades)
    Assert.Equal(33, trades.Length)

  [<Fact>]
  let ``Saxo should get open trades`` () =
    let trades = Saxo.tradesOpen () |> Seq.toList
    Assert.NotEmpty(trades)

    // 3 * 2 because 3 securities were closed, but we're removing
    // both the open & closed transaction
    Assert.Equal(33-3*2, trades.Length)
