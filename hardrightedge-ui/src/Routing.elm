module Routing exposing (..)

import Navigation exposing (Location)
import Domain exposing (Route(..))
import UrlParser exposing (..)


matchers : Parser (Route -> a) a
matchers =
    oneOf
        [ map SecuritiesRoute top
        , map SecurityRoute (s "securities" </> int)
        , map SecuritiesRoute (s "Security")
        ]


parseLocation : Location -> Route
parseLocation location =
    case (parseHash matchers location) of
        Just route ->
            route

        Nothing ->
            NotFoundRoute