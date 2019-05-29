module Main exposing (Model, getPortfolioCmd, init, initialModel, main, onBlurWithTargetValue, update, view, viewContent, viewItem)

import Api exposing (..)
import Browser
import Browser.Navigation as Nav
import Domain exposing (..)
import File exposing (File)
import File.Select as Select
import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (on, onClick, targetValue)
import Http
import Json.Decode as Json
import Routing exposing (Route)
import Url exposing (Url)

type alias Model =
  { securities:     List Security,
    security:       Maybe Security,
    errors:         List String,
    key:            Nav.Key,
    url:            Url}

initialModel : Url -> Nav.Key -> Model
initialModel url key =
  { securities = [],
    security = Nothing,
    errors = [],
    url = url,
    key = key }

getPortfolioCmd : Cmd Msg
getPortfolioCmd = getPortfolio

init : () -> Url -> Nav.Key -> (Model, Cmd Msg)
init flags url key =
  (initialModel url key, getPortfolioCmd)

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

    ShowPortfolio res ->
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
      let _ = Debug.log "EditSymbol" (symbol, platform, security)
      in ({ model
            | securities =
              updateIn model.securities
              security
              platform
              symbol },
          Cmd.none)

    SavePortfolio securities ->
      let _ = Debug.log "SavePortfolio" securities
      in
      (model, savePortfolio securities)

    --HandleSaved savedRes ->
    --  case savedRes of
    --    Result.Ok securities ->
    --      -- TODO:  Maybe optimise this to better handle individual
    --      --        securities
    --      ({  model | securities = securities },
    --          Cmd.none)

    --    Result.Err err ->
    --      let _ = Debug.log "Error saving artist" err
    --      in
    --      (model, Cmd.none)

    RequestFile -> 
      (model, 
        Select.file ["application/vnd.ms-excel", "text/csv"] SelectedFile)
    
    SelectedFile file -> 
      let _ = Debug.log "SelectedFile" ""
      in
      (model, uploadPortfolioFile file)

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
                              [ class "navbar-link",
                                href "#import", 
                                onClick <| RequestFile ]
                              [ text "Import" ]
                          ],
                        li [ class "navbar-item" ]
                          [ a
                              [ class "navbar-link",
                                href "#save",
                                onClick <| SavePortfolio model.securities ]
                              [ text "Save" ]
                          ],
                        li [ class "navbar-item" ]
                          [ a
                              [ class "navbar-link",
                                href "#cancel" ]
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
                [ th [] [ text "Security" ],
                  th []
                    [ text "Symbol",
                      br [] [],
                      text "Saxo/DMA" ],
                  th []
                    [ text "Symbol",
                      br [] [],
                      text "Yahoo! Finance" ]
                ]
            ],
          tbody [] (List.map viewItem model.securities) ]

viewItem : Security -> Html Msg
viewItem security =
    tr [ onClick <| EditSecurity security ]
        [ td [] [ text security.name ],
          td []
            [ --text (security.platforms |> symbol Saxo),
              input
                [ type_ "text",
                  value (security.platforms |> symbol Saxo),
                  onBlurWithTargetValue <| EditSymbol security Saxo ]
                []
            ],
          td []
            [ --text (security.platforms |> symbol Yahoo),
              input
                [ type_ "text",
                  value (security.platforms |> symbol Yahoo),
                  onBlurWithTargetValue <| EditSymbol security Yahoo ]
                [] ]]

main : Program () Model Msg
main =
    Browser.application
      { init = init,
        view = view,
        update = update,
        subscriptions = \_ -> Sub.none,
        onUrlChange = ChangedUrl,
        onUrlRequest = ClickedLink }