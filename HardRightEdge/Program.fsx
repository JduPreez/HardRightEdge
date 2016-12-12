#I "../packages/RProvider.1.1.20/lib/net40"
#I "../packages/R.NET.Community.FSharp.1.6.5/lib/net40"
#I "../packages/R.NET.Community.1.6.5/lib/net40"
#r "RDotNet.dll"
#r "RDotNet.FSharp.dll"
#r "RDotNet.NativeLibrary.dll"
#r "RProvider.dll"
#r "RProvider.Runtime.dll"
 
open RDotNet
open RProvider
open RProvider.``base``
open RProvider.graphics
open RProvider.stats
open RProvider.zoo
open RProvider.xts
open RProvider.quantmod

open System
open System.Collections.Generic
open System.Net

R.library(package="quantmod")

let args = Dictionary()
[ "Symbols", box (R.c("GSK_L"));
  "src", box "SQLite";
  "dbname", box "bin/Debug/HardRightEdge.db";
  "verbose", box "TRUE"] |> Seq.iter args.Add

R.getSymbols(args)
R.chartSeries(R.get(x="GSK_L"))