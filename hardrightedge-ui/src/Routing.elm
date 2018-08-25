module Routing exposing (..)

import Navigation exposing (Location)
import Domain exposing (Route(..))
import UrlParser exposing (..)


matchers : Parser (Route -> a) a
matchers =
    oneOf
        [ map SharesRoute top
        , map ShareRoute (s "shares" </> int)
        , map SharesRoute (s "Share")
        ]


parseLocation : Location -> Route
parseLocation location =
    case (parseHash matchers location) of
        Just route ->
            route

        Nothing ->
            NotFoundRoute