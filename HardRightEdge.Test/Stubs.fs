module HardRightEdge.Test.Stubs

open HardRightEdge.Domain
open System

let random = Random()

let testShare name symbolSaxo symbolYahoo = { id = None
                                              name =  match name with
                                                      | Some n -> n
                                                      | _ -> "GlaxoSmithKline Plc"
                                              previousName = None
                                              prices = [ for i in 1..10 ->      
                                                            { id          = None
                                                              securityId  = None 
                                                              date        = DateTime.Now.AddDays(-1.0)
                                                              openp       = random.NextDouble() * float(random.Next(100))
                                                              high        = random.NextDouble() * float(random.Next(100))
                                                              low         = random.NextDouble() * float(random.Next(100)) 
                                                              close       = random.NextDouble() * float(random.Next(100)) 
                                                              adjClose    = None
                                                              volume      = int64(random.Next()) }]
                                              currency = None
                                              platforms = seq { yield { securityId  = None 
                                                                        platform    = Platform.Saxo
                                                                        symbol      = match symbolSaxo with
                                                                                      | Some s -> s 
                                                                                      | _ -> "GSK:xlon" }
                                                                          
                                                                yield { securityId  = None
                                                                        platform    = Platform.Yahoo
                                                                        symbol      = match symbolYahoo with
                                                                                      | Some s -> s
                                                                                      | _ -> "GSK.L" } } }

let testShares () = 
  [ testShare None None None 
    testShare (Some "BMW") (Some "BMW:xetr") (Some "BMW.DE")
    testShare (Some "Vestas Wind Systems") (Some "VWS:xcse") (Some "VWS.CO") ]

let testSharePrices shrId = seq { for x in 1 .. 5 -> 
                                    { id          = None  
                                      securityId  = shrId 
                                      date        = DateTime.Now 
                                      openp       = double (random.Next())
                                      high        = double (random.Next()) 
                                      low         = double (random.Next()) 
                                      close       = double (random.Next()) 
                                      adjClose    = None
                                      volume      = int64 (random.Next()) } } |> Seq.toList

