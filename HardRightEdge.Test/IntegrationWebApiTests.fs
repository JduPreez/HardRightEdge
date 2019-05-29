module HardRightEdge.Test.IntegrationWebApiTests

open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open HardRightEdge
open HardRightEdge.Domain
open HardRightEdge.Test
open HardRightEdge.Infrastructure.Serialization
open HardRightEdge.Integration
open System.IO
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
  let secSer = Stubs.testShare None None None |> toJson
  let secDeser : Domain.Security = fromJson secSer
  Assert.Equal(sec.name, secDeser.name)

[<Fact>]
let ``Web API should create new securities`` () =
  let secList = Stubs.testShares ()
  let secListJson = secList |> toJson
  let response = Http.Request
                  ( host + "/portfolio",
                    httpMethod = "PUT",
                    headers = [ Accept HttpContentTypes.Json ],
                    body = TextRequest secListJson )
  
  Assert.True(response.StatusCode = 200)

[<Fact>]
let ``Web API should respond to OPTIONS`` () =
  let response = Http.Request
                  ( host + "/portfolio",
                    httpMethod = "OPTIONS",
                    headers = [ "Access-Control-Request-Headers", "content-type";
                                "Access-Control-Request-Method", "PUT"] )

  Assert.True(response.StatusCode = 200)

[<Fact>]
let ``Web API should upload new portfolio file`` () =
  let filePath = tradesFile importsRoot Saxo.folder Saxo.filePattern
  let docJson = [{ file = Some(File.ReadAllBytes(filePath)); content = None }] |> toJson
  let response = Http.Request
                  ( host + "/portfolio/file",
                    httpMethod = "POST",
                    headers = [ Accept HttpContentTypes.Json ],
                    body = TextRequest docJson )

  Assert.True(response.StatusCode = 200)