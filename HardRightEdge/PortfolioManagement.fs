namespace HardRightEdge

open System
open System.Windows.Forms
open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable
open HardRightEdge.Services.Infrastructure.Common
open HardRightEdge.Services.Domain
open HardRightEdge.Services.DomainServices
open HardRightEdge.Services.DomainServices.DataProviderStock

module PortfolioManagement =

  (*module Model =

    let private models' = ref List.empty<Stock>

    let get () = !models' 
                  |> List.sortBy (fun s -> s.name)
                  |> toObservable

    let with' modls =
      models' := List.append !models' modls
      get ()*)

  module View =

    module Columns = 
      let Id = "ID"
      let YahooId = "YahooId"
      let Yahoo = "Yahoo"
      let GoogleId = "GoogleId"      
      let Google = "Google"
      let Name = "Name"

    type public ViewForm () as form =
      inherit Form()

      let securities = ref List.empty<Stock>

      let observation : IDisposable option ref = ref None
        
      let portfolioGridView = 
        let portfolioGridView = new DataGridView (Dock = DockStyle.Fill)
        portfolioGridView.AutoGenerateColumns <- false

        // ID column
        let idColmn = new DataGridViewTextBoxColumn (Visible = false)
        idColmn.Visible <- false
        idColmn.Name <- Columns.Id
        idColmn.HeaderText <- Columns.Id
        portfolioGridView.Columns.Add idColmn |> ignore

        // Name column
        let nameColmn = new DataGridViewTextBoxColumn ()
        nameColmn.HeaderText <- Columns.Name
        nameColmn.Name <- Columns.Name
        portfolioGridView.Columns.Add nameColmn |> ignore

        // Yahoo provider ID column
        let yahooIdColmn = new DataGridViewTextBoxColumn (Visible = false)
        yahooIdColmn.Name <- Columns.YahooId
        yahooIdColmn.HeaderText <- Columns.YahooId
        portfolioGridView.Columns.Add yahooIdColmn |> ignore

        // Yahoo symbol column
        let yahooColmn = new DataGridViewTextBoxColumn ()
        yahooColmn.HeaderText <- Columns.Yahoo
        yahooColmn.Name <- Columns.Yahoo
        portfolioGridView.Columns.Add yahooColmn |> ignore

        // Google provider ID column
        let googleIdColmn = new DataGridViewTextBoxColumn (Visible = false)
        googleIdColmn.Name <- Columns.GoogleId
        googleIdColmn.HeaderText <- Columns.GoogleId
        portfolioGridView.Columns.Add googleIdColmn |> ignore

        // Google symbol column
        let googleColmn = new DataGridViewTextBoxColumn ()
        googleColmn.HeaderText <- Columns.Google
        googleColmn.Name <- Columns.Google
        portfolioGridView.Columns.Add googleColmn |> ignore

        portfolioGridView

      let subscribe () = form.Securities
                          |> List.sortBy (fun s -> s.name)
                          |> toObservable 
                          |> subscribeWithCallbacks (fun m -> form.updateSecurityListView m |> ignore) ignore ignore
                          |> Some
      do
        form.Text <- "HardRightEdge"
        form.Controls.Add portfolioGridView

      member form.updateSecurityListView (stock: Stock) =
        let gridRow = portfolioGridView.Rows.[portfolioGridView.Rows.Add ()]
        gridRow.Cells.[Columns.Id].Value        <- toNullable stock.id
        gridRow.Cells.[Columns.Name].Value      <- stock.name
        gridRow.Cells.[Columns.YahooId].Value   <- stock |> DataProviderStock.get DataProvider.Yahoo  |> stockId
        gridRow.Cells.[Columns.Yahoo].Value     <- stock |> DataProviderStock.get DataProvider.Yahoo  |> symbol
        gridRow.Cells.[Columns.GoogleId].Value  <- stock |> DataProviderStock.get DataProvider.Google |> stockId
        gridRow.Cells.[Columns.Google].Value    <- stock |> DataProviderStock.get DataProvider.Google |> symbol

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

    let viewForm = new ViewForm ()
    
    let portfolioManagementScene () = viewForm :> Form, viewForm.updateSecurityListView