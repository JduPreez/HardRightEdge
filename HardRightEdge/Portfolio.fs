namespace HardRightEdge

open System
open System.Reactive
open System.Reactive.Linq
open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable

open RDotNet
open RProvider
open RProvider.graphics

open HardRightEdge.Services.Domain

module Portfolio =

  module Model =

    let private models' = ref List.empty<Stock>

    let get () = !models' 
                  |> List.sortBy (fun s -> s.name)
                  |> toObservable

    let with' modls =
      models' := List.append !models' modls
      get ()

  module View =

    let element (stock: Stock) =
      let closingPrices = List.map (fun p -> p.close) stock.prices
      
      R.plot(namedParams["x", box closingPrices;
                        "main", box "Stock 2"]) |> ignore

      R.lines(
        namedParams [      
          "x", box closingPrices;
          "main", box "Stock 2";
          "type", box "o"; 
          "pch", box 22;
          "lty", box 2;
          "col", box "red" ]) |> ignore

    let scene () =
      R.par(
        namedParams [
          "mfrow", box [ 2; 2; ]
        ]) |> ignore

      element

  module Controller =

    let private observation : IDisposable option ref = ref None

    let private subscribe modls (onNext : Stock -> unit) = 
      match !observation with
      | None -> observation :=  modls
                                |> subscribeWithCallbacks onNext ignore ignore
                                |> Some
                ()
      | _ -> ()

    // TODO: Change this into function compositions for chain
    // of view >> controller <| stocks
    let show (scene: unit -> (Stock -> unit)) (stocks : IObservable<Stock>) =
      let element = scene ()

      for stock in stocks do
        element stock

      subscribe stocks element

        //securityListView.Items.Add (new ListViewItem([| security.name |])) |> ignore