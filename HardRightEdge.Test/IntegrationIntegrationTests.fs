module HardRightEdge.IntegrationIntegrationTests

open Xunit
open HardRightEdge.Integration

module Yahoo =

  [<Fact>]
  let ``Yahoo should getSharePrices for the last year`` () =
    let share = Yahoo.getSharePrices "VEON" None
    Assert.True(List.length share.prices > 200) // Just use any reasonably large number of days less than 365, but more than 1
    Assert.True((List.head share.prices).close > 0.0)

module Saxo =

  [<Fact>]
  let ``Saxo should getTrades`` () =
    let trades = Saxo.getTrades ()
    Assert.NotEmpty(trades)
