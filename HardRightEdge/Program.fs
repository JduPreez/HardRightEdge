open System
open System.Windows.Forms
open HardRightEdge.Services.Data
open HardRightEdge.Services
 
[<EntryPoint>]
[<STAThread>]
let main argv =

  Application.EnableVisualStyles()
  Application.SetCompatibleTextRenderingDefault false
  
  DbAdmin.prepare Db.Name 
  
  use form = new Form()

  let x = DomainServices.getFinancialSecurity "GSK.L" 
 
  Application.Run(form);
  0