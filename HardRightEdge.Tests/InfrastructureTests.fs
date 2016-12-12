namespace HardRightEdge.Tests

open System
open NUnit.Framework
open FsUnit
open HardRightEdge.Services.Infrastructure.Common

module InfrastructureTests = 
  
  [<Test>]
  let ``unwrap should return value if object is Some object`` () =
    unwrap (Some obj) |> should not' (be null)

  [<Test>]
  let ``unwrap should return null if object is None`` () =
    unwrap None |> should be null
  