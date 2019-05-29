module Api exposing (baseUrl, encodeSecurities, encodeSecurity, encodeSecurityPlatform, getPortfolio, uploadPortfolioFile, platformDecoder, savePortfolio, securitiesDecoder, securityDecoder, securityPlatformDecoder, securityPlatformsDecoder)

import Domain exposing (..)
import File exposing (File)
import Http exposing (header)
import Json.Decode as JsonD
import Json.Encode as JsonE


baseUrl : String
baseUrl =
    "http://localhost:8080"


platformDecoder : JsonD.Decoder (Maybe Platform)
platformDecoder =
    JsonD.int
    |> JsonD.andThen (\id -> JsonD.succeed (platform id))


securityPlatformDecoder : JsonD.Decoder SecurityPlatform
securityPlatformDecoder =
  JsonD.map3 SecurityPlatform
      (JsonD.maybe <| JsonD.field "shareId" JsonD.int)
      (JsonD.field "platform" platformDecoder)
      (JsonD.field "symbol" JsonD.string)


securityPlatformsDecoder : JsonD.Decoder (List SecurityPlatform)
securityPlatformsDecoder =
  JsonD.list securityPlatformDecoder


securitiesDecoder : JsonD.Decoder (List Security)
securitiesDecoder =
  JsonD.list securityDecoder


securityDecoder : JsonD.Decoder Security
securityDecoder =
  JsonD.map3 Security
    (JsonD.maybe <| JsonD.field "id" JsonD.int)
    (JsonD.field "name" JsonD.string)
    (JsonD.field "platforms" <| securityPlatformsDecoder)


encodeSecurityPlatform : SecurityPlatform -> JsonE.Value
encodeSecurityPlatform securityPlatform =
  JsonE.object
    [ ( "securityId"
      , case securityPlatform.securityId of
            Nothing ->
                JsonE.null

            Just secId ->
                JsonE.int secId
      )
    , ( "platform"
      , case securityPlatform.platform of
            Nothing ->
                JsonE.null

            Just platform ->
                JsonE.int (platformId platform)
      )
    , ( "symbol", JsonE.string securityPlatform.symbol ) ]


encodeSecurity : Security -> JsonE.Value
encodeSecurity security =
    JsonE.object
        [ ( "id",
            case security.id of
                Nothing ->
                    JsonE.null

                Just id ->
                    JsonE.int id
          ),
          ( "name", JsonE.string security.name ),
          ( "previousName", JsonE.null ),
          ( "prices", JsonE.list (\x -> JsonE.null) [] ),
          ( "platforms", JsonE.list encodeSecurityPlatform security.platforms),
          ( "currency", JsonE.null ) ]


encodeSecurities : List Security -> JsonE.Value
encodeSecurities securities = JsonE.list encodeSecurity securities

getPortfolio : Cmd Msg
getPortfolio =
  Http.get 
    { url = baseUrl ++ "/portfolio",
      expect = Http.expectJson ShowPortfolio securitiesDecoder }

-- TODO: Save both platforms: Saxo (working currently) and Yahoo! (TODO)
savePortfolio : List Security -> Cmd Msg
savePortfolio securities =
  let
    x =
        encodeSecurities securities
    _ =
        Debug.log "JSON" x
  in
  Http.request
    { method = "PUT",
      headers = [ header "Accept" "application/json" ],
      url = baseUrl ++ "/portfolio",
      body = Http.jsonBody (encodeSecurities securities),
      expect = Http.expectJson ShowPortfolio securitiesDecoder,
      timeout = Nothing,
      tracker = Nothing }

uploadPortfolioFile : File -> Cmd Msg
uploadPortfolioFile file =
  let _ = Debug.log "uploadPortfolioFile " (File.mime file)
  in
  Http.request
    { method = "POST",
      headers = [ header "Accept" "application/json" ],
      url = baseUrl ++ "/portfolio/file",
      body = Http.fileBody file,
      expect = Http.expectJson ShowPortfolio securitiesDecoder,
      timeout = Nothing,
      tracker = Nothing }