# HardRightEdge

HardRightEdge is an investment portfolio management toolset written in F#. It imports your trades from Saxo and EasyEquities trade files. Support for other brokers and to build your own import templates will be added as needed or requested), and market data from Yahoo Finance into a PostgreSQL database.

From there it provides a bunch of Open R statistics, and various F# FsLab scripts (https://fslab.org/) to analyse and visualise your porfolio performance.

If you have questions or comments, contact me on Twitter at [jacquesdp](https://www.twitter.com/jacquesdp).

## Requirements

1. x64 Windows (when .NET Core supports Type Providers I will look add cross platform support)
2. PostgreSQL 9.6
3. F# 4.5 (FSharp.Core, 4.5.2.0)
4. .NET Framework 4.7.1
5. Open R (statistics)
6. Visual Studio 2017
7. Node.js v10.15.3

## Build and Run
1. Open HRE.sln, build and run. You're Suave HTTP back-end server is now up and running.
2. In your Windows command prompt:
  * Go to hardrightedge-ui
  * Run build.bat
  * Run start.bat
3. Open your browser and go to http://127.0.0.1:8081/app.html

## Running tests

Unit tests are written in XUnit. Just make sure to grab the Visual Studio XUnit test runner, and you're good to go.
