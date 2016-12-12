open System
open System.Windows.Forms
open HardRightEdge.Services.Data
open HardRightEdge.Services
open HardRightEdge.Services.Domain
open HardRightEdge.Services.DomainServices
 
[<EntryPoint>]
[<STAThread>]
let main argv =

  Application.EnableVisualStyles()
  Application.SetCompatibleTextRenderingDefault false
  
  //DbAdmin.prepare Db.Name 
  
  use form = new Form()

  //let x = DomainServices.getFinancialSecurity "GSK.L" 
        
  let x =  {  id = Some 1L;
              name = "GSK.L";
              previousName = None;
              prices = [];
              dataProviders = Seq.empty }
        
  let y = saveFinancialSecurity x
 
  Application.Run(form);
  0