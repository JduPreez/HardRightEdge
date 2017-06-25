namespace HardRightEdge

open System
open System.Reactive
open System.Reactive.Linq
open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable

open RDotNet
open RProvider
open RProvider.``base``
open RProvider.graphics
open RProvider.grDevices

open HardRightEdge.Services.Domain

module Portfolio =

  (*module Model =

    let private models' = ref List.empty<Stock>

    let get () = !models' 
                  |> List.sortBy (fun s -> s.name)
                  |> toObservable

    let with' modls =
      models' := List.append !models' modls
      get ()*)

  module View =

    open System.Collections.Generic

    let private charts = ref List.empty<IDictionary<string, obj> * IDictionary<string, obj>>

    let grid (columns: int) (items: int) = 
      let rows = float(items)/float(columns) // Number of rows
      box [ Convert.ToInt32(ceil rows); columns ] // Rows, Columns

    let chart (stock: Stock) =
      // Resize the plot
      R.par(
        namedParams [
          "mfrow", grid 3 ((!charts).Length + 1)
        ]) |> ignore

      // Calculate latest chart
      let closingPrices = List.map (fun p -> p.close) stock.prices
      
      let chartPlot = namedParams["x", box closingPrices;
                                  "main", box "Stock 2"]
      

      let chartLines = namedParams [  "x", box closingPrices;
                                      "main", box "Stock 2";
                                      "type", box "o"; 
                                      "pch", box 22;
                                      "lty", box 2;
                                      "col", box "red" ]
      
      // Add the latest chart to the total list of charts
      let charts' = (chartPlot, chartLines) :: !charts

      // Now redraw all charts. This has to be done to make it
      // feel like R reactively draws the charts, because it wipes the
      // screen after a call to R.plot.
      charts := charts'
      for cp, cl in charts' do
        R.plot cp |> ignore
        R.lines cl |> ignore
      
      R.dev_flush() |> ignore

    let portfolioChartScene () = chart

  (*Moved to HardRightEdge.Presentation.
    module Controller =

    let private observation : IDisposable option ref = ref None

    let private subscribe<'t> modls (onNext : 't -> unit) = 
      match !observation with
      | None -> observation :=  modls
                                |> subscribeWithCallbacks onNext ignore ignore
                                |> Some
                ()
      | _ -> ()

    let show<'t> (scene: unit -> ('t -> unit)) (entity : IObservable<'t>) =
      subscribe<'t> entity (scene ())*)