module Main exposing (Model, Msg(..), getPortfolioCmd, init, initialModel, main, onBlurWithTargetValue, update, view, viewContent, viewItem)

import Api exposing (..)
import Browser
import Browser.Navigation as Nav
import Domain exposing (..)
import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (on, onClick, targetValue)
import Http
import Json.Decode as Json
import Routing exposing (Route)
import Url exposing (Url)

type alias Model =
    { securities  : List Security,
      security    : Maybe Security,
      errors      : List String,
      key         : Nav.Key,
      url         : Url}

getPortfolioCmd : Cmd Msg
getPortfolioCmd = Api.getPortfolio ShowSecurities

init : () -> Url -> Nav.Key -> ( Model, Cmd Msg )
init flags url key =
  (initialModel url key, getPortfolioCmd)

initialModel : Url -> Nav.Key -> Model
initialModel url key =
  { securities = [],
    security = Nothing,
    errors = [],
    url = url,
    key = key }

-- Upload application/vnd.ms-excel, text/csv

type Msg
  = ChangedUrl Url 
  | ClickedLink Browser.UrlRequest
  | ShowSecurities (Result Http.Error (List Security))
  | EditSecurity Security
  | EditSymbol Security Platform String
  | SaveSecurities (List Security)
  | HandleSaved (Result Http.Error (List Security))

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    ClickedLink urlRequest ->
      case urlRequest of
        Browser.Internal url ->
          (model, Nav.pushUrl model.key (Url.toString url))

        Browser.External href -> 
          (model, Nav.load href)

    ChangedUrl url -> (model, Cmd.none)

    ShowSecurities res ->
      case res of
        Result.Ok securities ->
          ({  model | securities = securities },
              Cmd.none)

        Result.Err err ->
          let _ = Debug.log "Error retrieving artist" err
          in
          (model, Cmd.none)

    EditSecurity security ->
      let _ = Debug.log "EditSecurity" security
      in
      (model, Cmd.none)

    EditSymbol security platform symbol ->
      let _ = Debug.log "EditSymbol" ( symbol, platform, security )
      in ({ model
            | securities =
              updateIn model.securities
              security
              platform
              symbol },
          Cmd.none)

    SaveSecurities securities ->
      let _ = Debug.log "SaveSecurities" securities
      in
      (model, saveSecurities securities HandleSaved)

    HandleSaved savedRes ->
      case savedRes of
        Result.Ok securities ->
          -- TODO:  Maybe optimise this to better handle individual
          --        securities
          ({  model | securities = securities },
              Cmd.none)

        Result.Err err ->
          let _ = Debug.log "Error saving artist" err
          in
          (model, Cmd.none)


onBlurWithTargetValue : (String -> msg) -> Attribute msg
onBlurWithTargetValue value =
    on "blur" (Json.map value targetValue)


view : Model -> Browser.Document Msg
view model =
  { title = "HardRightEdge",
    body =
      [ div [ class "container" ]
          [ div [ class "navbar-spacer" ] []
          , nav [ class "navbar" ]
              [ div [ class "container" ]
                  [ ul [ class "navbar-list" ]
                      [ li [ class "navbar-item" ]
                          [ a
                              [ class "navbar-link"
                              , href "#home"
                              ]
                              [ text "HardRightEdge" ]
                          ]
                      , li [ class "navbar-item" ]
                          [ a
                              [ class "navbar-link"
                              , href "#save"
                              , onClick <| SaveSecurities model.securities
                              ]
                              [ text "Save" ]
                          ]
                      , li [ class "navbar-item" ]
                          [ a
                              [ class "navbar-link"
                              , href "#cancel"
                              ]
                              [ text "Cancel" ]
                          ]
                      ]
                  ]
              ]
          , div [ class "row main" ]
              [ div [ class "twelve columns" ]
                  [ viewContent model ]
              ]
          ]
      ]
  }


viewContent : Model -> Html Msg
viewContent model =
    table [ class "is-striped" ]
        [ thead []
            [ tr []
                [ th [] [ text "Security" ]
                , th []
                    [ text "Symbol"
                    , br [] []
                    , text "Saxo/DMA"
                    ]
                , th []
                    [ text "Symbol"
                    , br [] []
                    , text "Yahoo! Finance"
                    ]
                ]
            ]
        , tbody [] (List.map viewItem model.securities)
        ]


viewItem : Security -> Html Msg
viewItem security =
    tr [ onClick <| EditSecurity security ]
        [ td [] [ text security.name ]
        , td []
            [ --text (security.platforms |> symbol Saxo),
              input
                [ type_ "text"
                , value (security.platforms |> symbol Saxo)
                , onBlurWithTargetValue <| EditSymbol security Saxo
                ]
                []
            ]
        , td []
            [ --text (security.platforms |> symbol Yahoo),
              input
                [ type_ "text"
                , value (security.platforms |> symbol Yahoo)
                , onBlurWithTargetValue <| EditSymbol security Yahoo
                ]
                []
            ]
        ]

main : Program () Model Msg
main =
    Browser.application
      { init = init,
        view = view,
        update = update,
        subscriptions = \_ -> Sub.none,
        onUrlChange = ChangedUrl,
        onUrlRequest = ClickedLink }