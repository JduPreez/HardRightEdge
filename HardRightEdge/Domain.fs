module HardRightEdge.Domain

open System

let default' (deflt: unit -> 't) = deflt()

let priceStartDate () = DateTime.Now.AddMonths(-4)

let priceEndDate () = DateTime.Now.AddDays(-1.0)
  
type SharePrice = {
  id:         int64 option;
  shareId:    int64 option;
  date:       DateTime; 
  openp:      double;
  high:       double; 
  low:        double;
  close:      double;
  adjClose:   float option;
  volume:     int64 }

type Platform =
| Yahoo = 1
| Google = 2

type SharePlatform = {
  shareId: int64 option; 
  platform: Platform; 
  symbol: string }

type Share = {  
  id: int64 option; 
  name: string; 
  previousName: string option;
  prices: SharePrice list;
  platforms: SharePlatform seq; }