module Api exposing (..)

import Domain exposing (..)
import Json.Decode as JsonD
import Json.Encode as JsonE
import Http

baseUrl : String
baseUrl = "http://localhost:8081"

sharesDecoder : JsonD.Decoder (List Share)
sharesDecoder = JsonD.list shareDecoder

shareDecoder : JsonD.Decoder Share
shareDecoder =
    JsonD.map2 Share
        (JsonD.maybe <| JsonD.field "id"  JsonD.int)
        (JsonD.field "name"  JsonD.string)
        --(JsonD.field "albumArtistId"  JsonD.int)
        --(JsonD.field "albumTracks" <| JsonD.list trackDecoder)

getPortfolio : (Result Http.Error (List Share) -> msg) -> Cmd msg
getPortfolio msg =
    Http.get (baseUrl ++ "/portfolio") sharesDecoder
    |> Http.send msg