module Domain exposing (Platform(..), Security, SecurityPlatform, platform, platformId, symbol, updateIn)

import List.Extra exposing (..)
import Routing exposing (Route(..))

type Platform
    = Yahoo
    | Google
    | Saxo


platform : Int -> Maybe Platform
platform id =
    case id of
        1 ->
            Just Yahoo

        2 ->
            Just Google

        3 ->
            Just Saxo

        _ ->
            Nothing


platformId : Platform -> Int
platformId platfrm =
    case platfrm of
        Yahoo ->
            1

        Google ->
            2

        Saxo ->
            3


type alias SecurityPlatform =
    { securityId : Maybe Int
    , platform : Maybe Platform
    , symbol : String
    }


symbol : Platform -> List SecurityPlatform -> String
symbol platfrm securityPlatforms =
    securityPlatforms
        |> find
            (\sp ->
                case sp.platform of
                    Just p  -> p == Saxo
                    Nothing -> False )
        |> Maybe.withDefault
            { securityId = Nothing
            , platform = Nothing
            , symbol = "" }
        |> (\sp -> sp.symbol)


type alias Security =
    { id : Maybe Int
    , name : String
    , platforms : List SecurityPlatform
    }


updateIn : List Security -> Security -> Platform -> String -> List Security
updateIn securities security platfrm symbl =
    let
        updatedSec =
            { security
                | platforms =
                    security.platforms
                        |> updateIf (\sp -> sp.platform == Just platfrm)
                            (\sp -> { sp | symbol = symbl })
            }
    in
    setIf (\s -> s == security) updatedSec securities
