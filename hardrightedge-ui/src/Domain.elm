module Domain exposing (..)

import List.Extra exposing (..)

type Platform 
  = Yahoo
  | Google
  | Saxo

platform: Int -> Maybe Platform
platform id =
  case id of
  1 -> Just Yahoo
  2 -> Just Google
  3 -> Just Saxo
  _ -> Nothing

type alias SecurityPlatform = {
  securityId: Maybe Int,
  platform:   Maybe Platform,
  symbol:     String }

symbol: Platform -> List SecurityPlatform -> String
symbol platform securityPlatforms =
  securityPlatforms
  |> find (\ sp -> case sp.platform of
                Just platform -> platform == Saxo
                Nothing -> False)
  |> Maybe.withDefault  { securityId   = Nothing,
                          platform  = Nothing,
                          symbol    = "" }
  |> (\ sp -> sp.symbol)

type alias Security = {  
  id:           Maybe Int,
  name:         String,
  platforms:    List SecurityPlatform }

updateIn: List Security -> Security -> Platform -> String -> List Security
updateIn securities security platform symbol =
  let updatedSec = { security | platforms =  
                                  security.platforms                            
                                  |> updateIf (\ sp -> sp.platform == Just platform) 
                                              (\ sp -> { sp | symbol = symbol }) }
  in 
    replaceIf (\ s -> s == security) updatedSec securities

type Route
    = SecuritiesRoute
    | SecurityRoute Int
    | NotFoundRoute