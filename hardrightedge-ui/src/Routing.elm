module Routing exposing (Route(..), fromUrl, href, replaceUrl)

import Browser
import Browser.Navigation as Nav
import Html exposing (Attribute)
import Html.Attributes as Attr
import Url exposing (Url)
import Url.Parser as Parser exposing ((</>), Parser, oneOf, s, string)

type Route
    = Portfolio
    | Security Int
    | NotFound

urlParser : Parser.Parser (Int -> a) a
urlParser =
  Parser.custom "SECURITY" (\str -> String.toInt str)

parser : Parser (Route -> a) a
parser =
  oneOf
    [ Parser.map Portfolio Parser.top,
      Parser.map Security (s "portfolio" </> urlParser),
      Parser.map Portfolio (s "portfolio") ]

href : Route -> Attribute msg
href targetRoute =
  Attr.href (routeToString targetRoute)


replaceUrl : Nav.Key -> Route -> Cmd msg
replaceUrl key route =
  Nav.replaceUrl key (routeToString route)

fromUrl : Url -> Maybe Route
fromUrl url =
  -- The RealWorld spec treats the fragment like a path.
  -- This makes it *literally* the path, so we can proceed
  -- with parsing as if it had been a normal path all along.
  { url | path = Maybe.withDefault "" url.fragment, fragment = Nothing }
    |> Parser.parse parser

routeToString : Route -> String
routeToString page =
  let
    pieces =
      case page of
        Portfolio -> []
        Security securityId -> ["security", String.fromInt securityId]
        NotFound -> []
    in
    "#/" ++ String.join "/" pieces