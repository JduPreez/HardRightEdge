open System
open System.Windows.Forms
open HardRightEdge
open HardRightEdge.Services.Data
open HardRightEdge.Services
open HardRightEdge.Services.Domain
open HardRightEdge.Services.DomainServices
open HardRightEdge.Presentation.Controller
open HardRightEdge.Presentation.Model
open HardRightEdge.PortfolioManagement.View

open RDotNet
open RProvider
open RProvider.graphics
 
[<EntryPoint>]
[<STAThread>]
let main argv =

  Application.EnableVisualStyles()
  Application.SetCompatibleTextRenderingDefault false
  
  DbAdmin.prepare Db.Name

  let r = Random ()
  let stocks = [ for i in 1 .. 4 ->
                  { id = Some (int64 i)
                    name = "GSK.L2"
                    previousName = None
                    prices = [for x in 1L .. 100L ->
                                { id = Some x
                                  stockId = Some (int64 i)
                                  date = DateTime.Now
                                  openp = 0.0
                                  high = 0.0
                                  low = 0.0
                                  close = double (r.Next(1, 100))
                                  adjClose = 0.0
                                  volume = 0L }]
                    dataProviders = Seq.empty } ]

  use form = show2<Form, Stock> portfolioManagementScene << with' <| stocks

  //let gsk = DomainServices.getFinancialSecurity "GSK.L"

  //if gsk.IsSome then
  //  form.Securities <- [ gsk.Value ]
        
  (*let x =  {  id = Some 1L;
              name = "GSK.L";
              previousName = None;
              prices = [];
              dataProviders = Seq.empty }
        
  let y = saveFinancialSecurity x*)
 
  Application.Run(form);

  0