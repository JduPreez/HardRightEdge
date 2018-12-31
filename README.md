# HardRightEdge

HardRightEdge is an investment portfolio management toolset written in F#. It imports your trades from Saxo and EasyEquities trade files (support for other brokers will be added as needed or requested), and market data from Yahoo Finance into a PostgreSQL database.

From there it provides a bunch of Open R statistics, and various F# FsLab scripts (https://fslab.org/) to analyse and visualise your porfolio performance.

## Requirements

1. x64 Windows (when .NET Core supports Type Providers I will look add cross platform support)
2. PostgreSQL
3. F# 4.0 (FSharp.Core, 4.4.0.0)
4. .NET Framework 4.6.1
5. Open R (statistics)

## Running tests

Unit tests are written in XUnit. Just grab the Visual Studio XUnit test runner, and you're good to go.
