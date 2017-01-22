namespace HardRightEdge

open System
open System.Drawing
open System.Reactive
open System.Windows.Forms
open System.ComponentModel
open System.Reactive.Linq
open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable
open HardRightEdge.Services.Domain

type public SecurityListForm () as form =
  inherit Form()

  let securities = ref List.empty<Stock>

  let observation : IDisposable option ref = ref None

  let securityListView = new ListView()

  let updateSecurityListView (security: Stock) =
    securityListView.Items.Add (new ListViewItem([| security.name |])) |> ignore

  let initControls =
    securityListView.Columns.Add("name", "Name") |> ignore
    securityListView.Columns.Add("year1", "1Y") |> ignore

  let subscribe () = form.Securities
                      |> List.sortBy (fun s -> s.name)
                      |> toObservable 
                      |> subscribeWithCallbacks updateSecurityListView ignore ignore
                      |> Some
  do
    initControls

    form.Text <- "HardRightEdge"
    form.Controls.Add securityListView

  member form.Securities
    with get () = !securities
    and set sec = 
      let obs = !observation
      
      if obs.IsSome then obs.Value.Dispose ()

      securities := sec

      observation := subscribe ()      

  override form.Dispose _ = match !observation with
                            | Some obs -> obs.Dispose ()
                            | _ -> ()
   