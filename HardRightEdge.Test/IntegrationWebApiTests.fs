module HardRightEdge.Test.IntegrationWebApiTests

open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open HardRightEdge
open HardRightEdge.Test
open HardRightEdge.Infrastructure.Serialization
open Xunit

// TODO: Add additional parameter "test" to API.
// If this is supplied, then wrap all work in a UnitOfWork.temp { ... },
// meaning it will go to the DB, but nothing will be persisted

let host = "http://127.0.0.1:8080"

[<Fact>]
let ``Web API should get portfolio`` () = 
  let response = Http.Request
                  ( host + "/portfolio",
                    headers = [ Accept HttpContentTypes.Json ] )
  
  Assert.True(response.StatusCode = 200)

[<Fact>]
let ``Serialization should deserialize Security.name`` () =
  // When we have more serialization tests, we can move them
  // to their own module
  let sec = Stubs.testShare None None None
  let secSer = Stubs.testShare |> toJson
  let secDeser : Domain.Security = fromJson secSer  
  Assert.Equal(sec.name, secDeser.name)

[<Fact>]
let ``Web API should create new securities`` () =
  let secList = Stubs.testShares ()
  let secListJson = secList |> toJson
  let response = Http.Request
                  ( host + "/portfolio",
                    httpMethod = "POST",
                    headers = [ Accept HttpContentTypes.Json ],
                    body = TextRequest secListJson )
  
  Assert.True(response.StatusCode = 200)