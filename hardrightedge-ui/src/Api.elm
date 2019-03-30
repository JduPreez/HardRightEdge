module Api exposing (..)

import Domain exposing (..)
import Json.Decode as JsonD
import Json.Encode as JsonE
import Http exposing (header)

baseUrl : String
baseUrl = "http://localhost:8080"

platformDecoder : JsonD.Decoder (Maybe Platform)
platformDecoder =
  JsonD.int
  |> JsonD.andThen (\id -> JsonD.succeed (platform id))

securityPlatformDecoder: JsonD.Decoder SecurityPlatform
securityPlatformDecoder =
  JsonD.map3 SecurityPlatform
    (JsonD.maybe <| JsonD.field "shareId"  JsonD.int)
    (JsonD.field "platform" platformDecoder)
    (JsonD.field "symbol" JsonD.string)

securityPlatformsDecoder : JsonD.Decoder (List SecurityPlatform)
securityPlatformsDecoder = JsonD.list securityPlatformDecoder

securitiesDecoder : JsonD.Decoder (List Security)
securitiesDecoder = JsonD.list securityDecoder

securityDecoder : JsonD.Decoder Security
securityDecoder =
  JsonD.map3 Security
    (JsonD.maybe <| JsonD.field "id"  JsonD.int)
    (JsonD.field "name"  JsonD.string)
    (JsonD.field "platforms" <| securityPlatformsDecoder)

encodeSecurityPlatform : SecurityPlatform -> JsonE.Value
encodeSecurityPlatform securityPlatform =
  JsonE.object
    [ ("securityId",  (case securityPlatform.securityId of
                        Nothing -> JsonE.null
                        Just secId -> JsonE.int secId)),
      ("platform",    (case securityPlatform.platform of
                        Nothing -> JsonE.null
                        Just platform -> JsonE.int (platformId platform))),
      ("symbol",      JsonE.string securityPlatform.symbol) ]

encodeSecurity : Security -> JsonE.Value
encodeSecurity security =  
  JsonE.object 
    [ ("id",          (case security.id of
                        Nothing -> JsonE.null
                        Just id -> JsonE.int id)),
      ("name",          JsonE.string security.name),
      ("previousName",  JsonE.null),
      ("prices",        JsonE.list []),
      ("platforms",     JsonE.list <| List.map encodeSecurityPlatform security.platforms),
      ("currency",      JsonE.null) ]

encodeSecurities : List Security -> String
encodeSecurities securities =  
    securities
    |> List.map encodeSecurity
    |> JsonE.list
    |> JsonE.encode 0

getPortfolio : (Result Http.Error (List Security) -> msg) -> Cmd msg
getPortfolio msg =
  Http.get (baseUrl ++ "/portfolio") securitiesDecoder
  |> Http.send msg

-- TODO: On the back-end it only saves and/or returns the 1st item !!!!!
saveSecurities : (List Security) -> (Result Http.Error (List Security) -> msg) -> Cmd msg
saveSecurities securities msg =
  let x = encodeSecurities (List.take 1 securities)
      _ = Debug.log "JSON" x
  in
    Http.request
      { method  = "PUT",
        headers = [ header "Accept" "application/json" ],
        url     = baseUrl ++ "/portfolio",
        body    = Http.stringBody "application/json" <| encodeSecurities (List.take 1 securities),
        expect  = Http.expectJson securitiesDecoder,
        timeout = Nothing,
        withCredentials = False
      } |> Http.send msg
