module Main exposing (..)

import Html exposing (..)
import Html.Attributes exposing (class)
import Http
import Domain exposing (..)
import Api exposing (..)
import Navigation exposing (Location)
import Routing

main : Program Never Model Msg
main =
    Navigation.program Show
        { init = init
        , view = view
        , update = update
        , subscriptions = \_ -> Sub.none }

-- MODEL
type alias Model = {
  shares: List Share,
  share:  Maybe Share,
  errors: List String }

init : Location -> ( Model, Cmd Msg )
init location =
    let
        currentRoute =
            Routing.parseLocation location
    in
        (initialModel currentRoute, getSharesCmd)

initialModel : Route -> Model
initialModel route =
    { shares = [],
      share = Nothing,
      errors = [] }

type Msg 
  = Show        Location
  | ShowShares  (Result Http.Error (List Share))
  --| EditShare   Int
  --| SaveShare   Share

getSharesCmd : Cmd Msg
getSharesCmd = Api.getShares ShowShares

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    Show location -> (model, getSharesCmd)

    ShowShares res ->
      case res of
        Result.Ok shares ->
          ( { model | shares = shares },
            Cmd.none )

        Result.Err err ->
          let _ = Debug.log "Error retrieving artist" err
          in
              (model, Cmd.none)

view: Model -> Html Msg
view model =
  table [ class "is-striped" ]
    [ thead []
      [ tr []
        [ th [] [ text "Share Company" ],
          th [] [ 
            text "Symbol", 
            br [] [], 
            text "Saxo/DMA" ],
          th [] [ 
            text "Symbol", 
            br [] [], 
            text "Yahoo! Finance" ]]],
      tbody [] (List.map viewItem model.shares)]

viewItem: Share -> Html Msg
viewItem share =
  tr []
        [ td [] [ text "-" ],
          td [] [ text "-" ],
          td [] [ text "-" ]]