module Domain exposing (..)

type alias Share = {  
  id:           Maybe Int,
  name:         String }

type Route
    = SharesRoute
    | ShareRoute Int
    | NotFoundRoute