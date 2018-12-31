module HardRightEdge.Test.Stubs

open HardRightEdge.Domain
open System

let random = Random()

let testShare = { id = None
                  name = "GlaxoSmithKline Plc"
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
                                            symbol      = "GSK:xlon" }
                                              
                                    yield { securityId  = None
                                            platform    = Platform.Yahoo
                                            symbol      = "GSK.L" } } }

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

