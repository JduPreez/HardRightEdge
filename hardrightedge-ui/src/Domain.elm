module Domain exposing (Msg(..), Platform(..), Security, SecurityPlatform, platform, platformId, symbol, updateIn)

import Bytes exposing (Bytes)
import Browser
import Domain.Currencies as Currencies exposing (..)
import File exposing (File)
import Http
import Iso8601 exposing (..)
import List.Extra exposing (..)
import Routing exposing (Route(..))
import Time
import Url exposing (Url)

type Msg
  = ChangedUrl Url 
  | ClickedLink Browser.UrlRequest
  | ShowPortfolio (Result Http.Error (List Security))
  | SavePortfolio (List Security)
  --| HandleSaved (Result Http.Error (List Security))
  | EditSecurity Security
  | EditSymbol Security Platform String
  | RequestFile
  | SelectedFile File
  --| LoadedFile (Result Http.Error (List Security))

type Platform
  = Yahoo
  | Google
  | Saxo

type alias SecurityPlatform =
  { securityId: Maybe Int,
    platform:   Maybe Platform,
    symbol:     String }

type alias Security =
  { id:         Maybe Int,
    name:       String,
    platforms:  List SecurityPlatform }

type alias TradePattern =
  { id:           Maybe Int,
    name:         String,
    subPatterns:  TradePatterns }

type TradePatterns = TradePatterns (List TradePattern)

type alias ProfitLoss =
  { profitTarget:                     Float,
    -- Calculated
    profitTargetTotal:                Float, 
    profitTargetTotalInclCosts:       Float,
    profitTargetPercent:              Float,
    profitTargetPercentInclCosts:     Float,
    profitLoss:                       Float,
    profitLossInclCosts:              Float,
    profitLossPercentRisk:            Float }

type alias Risk =
  { stop:                             Float,
    firstDeviation:                   Maybe Float,
    -- Calculated
    stopLossTotal:                    Float,
    lossPercent:                      Float }

type alias Trade =
  { openDate:                         Maybe Time.Posix,
    open:                             Float,
    securityId:                       Int,
    tradePattern:                     Maybe TradePattern,
    isShort:                          Bool,
    quantity:                         Float,
    close:                            Float,
    closeDate:                        Maybe Time.Posix,
    currency:                         Currencies.Currency,
    cost:                             Float,
    interestPerDay:                   Float,
    interestTotal:                    Float, -- Calculated
    profitLoss:                       ProfitLoss,
    profitLossHomeCurrency:           Maybe ProfitLoss,
    risk:                             Risk,
    riskHomeCurrency:                 Risk,
    homeCurrency:                     Currencies.Currency,
    conversionRate:                   Float }

platform : Int -> Maybe Platform
platform id =
  case id of
    1 -> Just Yahoo
    2 -> Just Google
    3 -> Just Saxo
    _ -> Nothing

platformId : Platform -> Int
platformId platfrm =
  case platfrm of
    Yahoo   -> 1
    Google  -> 2
    Saxo    -> 3

symbol : Platform -> List SecurityPlatform -> String
symbol platfrm securityPlatforms =
  securityPlatforms
    |> find
      (\sp ->
          case sp.platform of
            Just p  -> p == Saxo
            Nothing -> False)
    |> Maybe.withDefault
      { securityId = Nothing,
        platform = Nothing,
        symbol = "" }
    |> (\sp -> sp.symbol)

updateIn : List Security -> Security -> Platform -> String -> List Security
updateIn securities security platfrm symbl =
    let
        updatedSec =
            { security
                | platforms =
                    security.platforms
                        |> updateIf (\sp -> sp.platform == Just platfrm)
                            (\sp -> { sp | symbol = symbl }) }
    in
    setIf (\s -> s == security) updatedSec securities