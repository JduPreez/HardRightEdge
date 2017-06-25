open System
open System.Windows.Forms
open HardRightEdge.Services.Data
open HardRightEdge.Services.Domain
open HardRightEdge.Presentation.Controller
open HardRightEdge.Presentation.Model
open HardRightEdge.PortfolioManagement.View

[<EntryPoint>]
[<STAThread>]
let main argv =

  Application.EnableVisualStyles()
  Application.SetCompatibleTextRenderingDefault false
  
  DbAdmin.prepare Db.Name

  let r = Random ()
  let stocks = [ 
      { id = Some 1L
        name = "GlaxoSmithKline"
        previousName = None
        prices = [for x in 1L .. 100L ->
                    { id = None
                      stockId = Some x
                      date = DateTime.Now
                      openp = 0.0
                      high = 0.0
                      low = 0.0
                      close = double (r.Next(1, 100))
                      adjClose = 0.0
                      volume = 0L } ]
        dataProviders = [ { stockId = None
                            dataProvider = DataProvider.Yahoo
                            symbol = "GSK.L"
                          } ] }
      { id = Some 2L
        name = "Abcam"
        previousName = None
        prices = [for x in 1L .. 100L ->
                    { id = None
                      stockId = Some x
                      date = DateTime.Now
                      openp = 0.0
                      high = 0.0
                      low = 0.0
                      close = double (r.Next(1, 100))
                      adjClose = 0.0
                      volume = 0L } ]
        dataProviders = [ { stockId = None
                            dataProvider = DataProvider.Yahoo
                            symbol = "ABC.L"
                          } ] }
      { id = Some 3L
        name = "China Sunsine Chemical"
        previousName = None
        prices = [for x in 1L .. 100L ->
                    { id = None
                      stockId = Some x
                      date = DateTime.Now
                      openp = 0.0
                      high = 0.0
                      low = 0.0
                      close = double (r.Next(1, 100))
                      adjClose = 0.0
                      volume = 0L } ]
        dataProviders = [ { stockId = None
                            dataProvider = DataProvider.Yahoo
                            symbol = "CH8.SI"
                          } ] }
      { id = Some 4L;
        name = "IG Group Holdings"
        previousName = None
        prices = [for x in 1L .. 100L ->
                    { id = None
                      stockId = Some x
                      date = DateTime.Now
                      openp = 0.0
                      high = 0.0
                      low = 0.0
                      close = double (r.Next(1, 100))
                      adjClose = 0.0
                      volume = 0L } ]
        dataProviders = [ { stockId = None
                            dataProvider = DataProvider.Yahoo
                            symbol = "IGG.L"
                          } ] } ]

  use form = show2<Form, Stock> portfolioManagementScene << with' <| stocks

  // TODO 2: Get real data from for various stocks
  // TODO 3: Open R chart from winform & chart stocks

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