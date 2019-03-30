module Main exposing (..)

import Html exposing (..)
import Html.Events exposing (onClick, on, targetValue)
import Html.Attributes exposing (..)
import Http
import Domain exposing (..)
import Api exposing (..)
import Navigation exposing (Location)
import Routing
import Json.Decode as Json

main : Program Never Model Msg
main =
    Navigation.program Show
        { init = init
        , view = view
        , update = update
        , subscriptions = \_ -> Sub.none }

-- MODEL
type alias Model = {
  securities: List Security,
  security:  Maybe Security,
  errors: List String }

init : Location -> ( Model, Cmd Msg )
init location =
    let
        currentRoute =
            Routing.parseLocation location
    in
        (initialModel currentRoute, getPortfolioCmd)

initialModel : Route -> Model
initialModel route =
    { securities = [],
      security = Nothing,
      errors = [] }

type Msg 
  = Show            Location
  | ShowSecurities  (Result Http.Error (List Security))
  | EditSecurity    Security
  | EditSymbol      Security Platform String
  | SaveSecurities  (List Security)
  | HandleSaved     (Result Http.Error (List Security))
  

getPortfolioCmd : Cmd Msg
getPortfolioCmd = Api.getPortfolio ShowSecurities

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    Show location -> (model, getPortfolioCmd)

    ShowSecurities res ->
      case res of
        Result.Ok securities ->
          ( { model | securities = securities },
            Cmd.none )

        Result.Err err ->
          let _ = Debug.log "Error retrieving artist" err
          in
            (model, Cmd.none)

    EditSecurity security ->
      let _ = Debug.log "EditSecurity" security
      in
        (model, Cmd.none)

    EditSymbol security platform symbol ->
      let _ = Debug.log "EditSymbol" (symbol, platform, security)
      in
        ( { model | securities = updateIn model.securities 
                                          security 
                                          platform 
                                          symbol }, Cmd.none)

    SaveSecurities securities ->
      let _ = Debug.log "SaveSecurities" securities
      in
        (model, saveSecurities securities HandleSaved)

    HandleSaved res ->
      case res of
        Result.Ok securities ->
          -- TODO:  Maybe optimise this to better handle individual
          --        securities
          ( { model | securities = securities },
            Cmd.none)

        Result.Err err ->
          let _ = Debug.log "Error saving artist" err
          in
            (model, Cmd.none)


onBlurWithTargetValue : (String -> msg) -> Attribute msg
onBlurWithTargetValue value =
  on "blur" (Json.map value targetValue)

view: Model -> Html Msg
view model =
  div [ class "container" ]
    [ div [ class "navbar-spacer" ] [],
      nav [ class "navbar" ]
        [ div [ class "container" ] 
            [ ul [ class "navbar-list" ] 
                [ li [ class "navbar-item" ] 
                    [ a [ class "navbar-link",
                          href "#home" ] 
                        [ text "HardRightEdge" ] ],
                  li [ class "navbar-item" ] 
                    [ a [ class "navbar-link",
                          href "#save",
                          onClick <| SaveSecurities model.securities ] 
                        [ text "Save" ] ],
                  li [ class "navbar-item" ] 
                    [ a [ class "navbar-link",
                          href "#cancel" ] 
                        [ text "Cancel" ] ] ] ] ],
      div [ class "row main" ]
        [ div [ class "twelve columns" ]
            [ viewContent model ] ] ]

viewContent: Model -> Html Msg
viewContent model =
    table [ class "is-striped" ]
      [ thead []
        [ tr []
          [ th [] [ text "Security" ],
            th [] [ 
              text "Symbol", 
              br [] [], 
              text "Saxo/DMA" ],
            th [] [ 
              text "Symbol", 
              br [] [], 
              text "Yahoo! Finance" ]]],
        tbody [] (List.map viewItem model.securities)]

viewItem: Security -> Html Msg
viewItem security =
  tr [ onClick <| EditSecurity security ]
        [ td [] [ text security.name ],
          td [] [ --text (security.platforms |> symbol Saxo), 
                  input [ type_ "text", 
                          value (security.platforms |> symbol Saxo),
                          onBlurWithTargetValue <| EditSymbol security Saxo ] [] ],
          td [] [ --text (security.platforms |> symbol Yahoo), 
                  input [ type_ "text", 
                          value (security.platforms |> symbol Yahoo),
                          onBlurWithTargetValue <| EditSymbol security Yahoo ] [] ]]