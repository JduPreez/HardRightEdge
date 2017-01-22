open System
open System.Windows.Forms
open HardRightEdge
open HardRightEdge.Services.Data
open HardRightEdge.Services
open HardRightEdge.Services.Domain
open HardRightEdge.Services.DomainServices

open RDotNet
open RProvider
open RProvider.graphics
 
[<EntryPoint>]
[<STAThread>]
let main argv =

  Application.EnableVisualStyles()
  Application.SetCompatibleTextRenderingDefault false
  
  ////DbAdmin.prepare Db.Name 
  
  //use form = new SecurityListForm ()

  let gsk = DomainServices.getFinancialSecurity "GSK.L"



  //if gsk.IsSome then
  //  form.Securities <- [ gsk.Value ]
        
  (*let x =  {  id = Some 1L;
              name = "GSK.L";
              previousName = None;
              prices = [];
              dataProviders = Seq.empty }
        
  let y = saveFinancialSecurity x*)
 
  //Application.Run(form);

  let widgets = [ 3; 8; 12; 15; 19; 18; 18; 20; ]
  let sprockets = [ 5; 4; 6; 7; 12; 9; 5; 6; ]   

  (*R.par(
    namedParams [
      "mfrow", box [ 2; 2; ]
    ]) |> ignore

  R.plot(namedParams["x", box widgets;
                    "main", box "Stock 2"]) |> ignore  

  R.lines(
    namedParams [      
      "x", box widgets;
      "main", box "Stock 2";
      "type", box "o"; 
      "pch", box 22;
      "lty", box 2;
      "col", box "red" ]) |> ignore

  R.plot(sprockets) |> ignore

  R.lines(
    namedParams [      
      "x", box sprockets;
      "main", box "Stock 2";
      "type", box "o"; 
      "pch", box 22;
      "lty", box 2;
      "col", box "red" ]) |> ignore*)

  0